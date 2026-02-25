using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logger;   

namespace InaraTools
{
    public class InaraCommunicator
    {
        private const int MaxRetryAttempts = 3;
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20);
        private static readonly Uri InaraBaseUri = new("https://inara.cz/");
        private static readonly SemaphoreSlim WarmupLock = new(1, 1);
        private static DateTimeOffset lastWarmupAt = DateTimeOffset.MinValue;
        private static HttpClient httpClient = CreateHttpClient();

        public static async Task<List<TradeRoute>> SearchInaraTradeRoutes(TradeRouteSearchParams searchParams)
        {
            // Prepare parameters to match exact INARA form structure
            var parameters = new Dictionary<string, string>
            {
                { "ps1", searchParams.NearStarSystem }, // Near star system
                { "pi10", searchParams.CargoCapacity.ToString() }, // Cargo capacity
                { "pi2", GetDistanceParameterValue(searchParams.MaxRouteDistance?.ToString()) }, // Max route distance
                { "pi5", GetAgeParameterValue(searchParams.MaxPriceAge ?.ToString()) }, // Max price age
                { "pi3", GetLandingPadParameterValue(searchParams.MinLandingPad) }, // Min landing pad
                { "pi9", GetStationDistanceParameterValue(searchParams.MaxStationDistance.ToString()) }, // Max station distance
                { "pi4", searchParams.UseSurfaceStations.ToString() }, // Use surface stations
                { "pi14", GetPowerPlayParameterValue(searchParams.SourceStationPower) }, // Source station Power
                { "pi15", GetPowerPlayParameterValue(searchParams.TargetStationPower) }, // Target station Power
                { "pi7", GetSupplyParameterValue(searchParams.MinSupply) }, // Min supply
                { "pi12", GetDemandParameterValue(searchParams.MinDemand) }, // Min demand
                { "pi1", searchParams.OrderBy.ToString() }, // Order by
            };

            // Add checkbox parameters only if checked
            if (searchParams.IncludeRoundTrips)
            {
                parameters.Add("pi8", "1"); // Include round trips
            }
            if (searchParams.DisplayPowerplayBonuses)
            {
                parameters.Add("pi11", "1"); // Display Powerplay bonuses
            }

            // Create query string
            var queryString = new StringBuilder();
            foreach (var param in parameters)
            {
                if (queryString.Length > 0)
                    queryString.Append("&");
                queryString.AppendFormat($"{param.Key}={WebUtility.UrlEncode(param.Value)}");
            }

            // Complete URL
            var url = $"https://inara.cz/elite/market-traderoutes/?{queryString}";

            await EnsureWarmSessionAsync();
            using HttpResponseMessage response = await SendWithRetryAsync(url);

            string? dataFromFallbackClient = null;
            if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                LogNonSuccessResponse(response, "primary");
                dataFromFallbackClient = await TryFetchHtmlWithMinimalClientAsync(url);
                if (!string.IsNullOrWhiteSpace(dataFromFallbackClient))
                {
                    Logger.Logger.Info("INARA fallback client succeeded after 503 from primary client.");
                }
            }

            if (string.IsNullOrWhiteSpace(dataFromFallbackClient))
            {
                EnsureSuccessOrThrowWithDetails(response);
            }

            // Read and parse the HTML content
            var data = dataFromFallbackClient ?? await response.Content.ReadAsStringAsync();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);

            // Parse trade routes using proper HTML structure
            var tradeRoutes = ParseTradeRoutes(doc);

            Logger.Logger.Info($"Parsed {tradeRoutes.Count} trade routes from Inara");

            // If no routes found, provide fallback sample data for testing
            if (tradeRoutes.Count == 0)
            {
                Logger.Logger.Debug("INARA response parsed successfully but returned no trade routes.");
            }

            // Log example trade route if any found
            if (tradeRoutes.Count > 0)
            {
                var example = tradeRoutes[0];
                //Logger.Logger.Info($"Example route: {example.CardHeader.FromStation.Name} ({example.CardHeader.FromStation.System}) → {example.CardHeader.ToStation.Name} ({example.CardHeader.ToStation.System}), {example.FirstRoute.BuyCommodity.Name}: {example.FirstRoute.BuyCommodity.Price} → {example.FirstRoute.SellCommodity.Price}, Profit: {example.FirstRoute.ProfitPerUnit}/t, Distance: {example.FirstRoute.RouteDistance:F1} Ly");
            }
            foreach(TradeRoute tradeRoute in tradeRoutes)
            {
                tradeRoute.CargoCapacity = searchParams.CargoCapacity;
            }
            return tradeRoutes;
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                PooledConnectionLifetime = TimeSpan.FromMinutes(10)
            };

            var client = new HttpClient(handler)
            {
                Timeout = RequestTimeout,
                BaseAddress = InaraBaseUri
            };

            client.DefaultRequestHeaders.UserAgent.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ED-Inara-Overlay", "2.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            return client;
        }

        private static async Task EnsureWarmSessionAsync()
        {
            if (DateTimeOffset.UtcNow - lastWarmupAt < TimeSpan.FromMinutes(30))
            {
                return;
            }

            await WarmupLock.WaitAsync();
            try
            {
                if (DateTimeOffset.UtcNow - lastWarmupAt < TimeSpan.FromMinutes(30))
                {
                    return;
                }

                using var warmupRequest = new HttpRequestMessage(HttpMethod.Get, InaraBaseUri);
                using var warmupResponse = await httpClient.SendAsync(warmupRequest);
                lastWarmupAt = DateTimeOffset.UtcNow;

                if (!warmupResponse.IsSuccessStatusCode)
                {
                    Logger.Logger.Debug($"INARA warm-up returned {(int)warmupResponse.StatusCode}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Debug($"INARA warm-up failed: {ex.Message}");
            }
            finally
            {
                WarmupLock.Release();
            }
        }

        private static async Task<HttpResponseMessage> SendWithRetryAsync(string url)
        {
            HttpResponseMessage? lastResponse = null;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    if (response.StatusCode == HttpStatusCode.ServiceUnavailable && attempt < MaxRetryAttempts)
                    {
                        await EnsureWarmSessionAsync();
                    }

                    if (!IsTransientStatusCode(response.StatusCode) || attempt == MaxRetryAttempts)
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            LogNonSuccessResponse(response, $"attempt {attempt}");
                        }
                        return response;
                    }

                    lastResponse = response;
                    var delay = GetRetryDelay(attempt);
                    Logger.Logger.Debug($"INARA returned {(int)response.StatusCode} on attempt {attempt}/{MaxRetryAttempts}. Retrying in {delay.TotalMilliseconds} ms.");
                    await Task.Delay(delay);
                }
                catch (HttpRequestException ex) when (attempt < MaxRetryAttempts)
                {
                    lastException = ex;
                    var delay = GetRetryDelay(attempt);
                    Logger.Logger.Debug($"INARA request failed on attempt {attempt}/{MaxRetryAttempts}: {ex.Message}. Retrying in {delay.TotalMilliseconds} ms.");
                    await Task.Delay(delay);
                }
                catch (TaskCanceledException ex) when (attempt < MaxRetryAttempts)
                {
                    lastException = ex;
                    var delay = GetRetryDelay(attempt);
                    Logger.Logger.Debug($"INARA request timed out on attempt {attempt}/{MaxRetryAttempts}. Retrying in {delay.TotalMilliseconds} ms.");
                    await Task.Delay(delay);
                }
            }

            if (lastResponse != null)
            {
                return lastResponse;
            }

            throw lastException ?? new HttpRequestException("INARA request failed after retries.");
        }

        private static async Task<string?> TryFetchHtmlWithMinimalClientAsync(string url)
        {
            using var client = new HttpClient
            {
                Timeout = RequestTimeout
            };

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    if (!IsTransientStatusCode(response.StatusCode) || attempt == 2)
                    {
                        LogNonSuccessResponse(response, $"fallback attempt {attempt}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Debug($"INARA fallback request failed on attempt {attempt}/2: {ex.Message}");
                    if (attempt == 2)
                    {
                        return null;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }

            return null;
        }

        private static bool IsTransientStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.ServiceUnavailable
                || statusCode == HttpStatusCode.BadGateway
                || statusCode == HttpStatusCode.GatewayTimeout
                || (int)statusCode == 429;
        }

        private static TimeSpan GetRetryDelay(int attempt)
        {
            // 1st retry: 1s, 2nd retry: 2s
            return TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
        }

        private static void LogNonSuccessResponse(HttpResponseMessage response, string stage)
        {
            string? server = null;
            string? cfRay = null;

            if (response.Headers.TryGetValues("Server", out var serverValues))
            {
                server = string.Join(",", serverValues);
            }

            if (response.Headers.TryGetValues("cf-ray", out var cfValues))
            {
                cfRay = string.Join(",", cfValues);
            }

            Logger.Logger.Warning($"INARA non-success ({stage}): {(int)response.StatusCode} {response.StatusCode}; server={server ?? "n/a"}; cf-ray={cfRay ?? "n/a"}");
        }

        private static void EnsureSuccessOrThrowWithDetails(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string retryAfter = "n/a";
            if (response.Headers.TryGetValues("Retry-After", out var values))
            {
                retryAfter = string.Join(",", values);
            }

            string message = $"INARA returned {(int)response.StatusCode} ({response.StatusCode}). Retry-After: {retryAfter}.";
            throw new HttpRequestException(message, null, response.StatusCode);
        }

        private static List<TradeRoute> ParseTradeRoutes(HtmlAgilityPack.HtmlDocument doc)
        {
            return InaraParserUtils.ParseTradeRoutes(doc);
        }

        // Parameter conversion methods to match INARA form values
        private static string GetDistanceParameterValue(string? distance)
        {
            if (string.IsNullOrEmpty(distance)) return "80";
            return distance.Replace(" Ly", "").Trim();
        }

        private static string GetAgeParameterValue(string? age)
        {
            if (string.IsNullOrEmpty(age)) return "72";
            return age switch
            {
                "8 hours" => "8",
                "16 hours" => "16",
                "1 day" => "24",
                "2 days" => "48",
                "3 days" => "72",
                _ => "72"
            };
        }

        private static string GetLandingPadParameterValue(int index)
        {
            return index switch
            {
                0 => "1", // Small
                1 => "2", // Medium
                2 => "3", // Large
                _ => "1"
            };
        }

        private static string GetStationDistanceParameterValue(string? distance)
        {
            if (string.IsNullOrEmpty(distance) || distance == "Any") return "0";
            return distance.Replace(" Ls", "").Trim();
        }

        private static string GetPowerPlayParameterValue(int index)
        {
            if (index == 0) return ""; // "Any"

            // Map to actual INARA power values based on the form structure
            return index switch
            {
                1 => "2",  // Aisling Duval
                2 => "10", // Archon Delaine
                3 => "4",  // Arissa Lavigny-Duval
                4 => "1",  // Denton Patreus
                5 => "3",  // Edmund Mahon
                6 => "5",  // Felicia Winters
                7 => "12", // Jerome Archer
                8 => "7",  // Li Yong-Rui
                9 => "13", // Nakato Kaine
                10 => "9", // Pranav Antal
                11 => "11", // Yuri Grom
                12 => "8", // Zemina Torval
                _ => ""
            };
        }

        private static string GetSupplyParameterValue(int index)
        {
            return index switch
            {
                0 => "0",     // Any
                1 => "100",   // 100 Units
                2 => "500",   // 500 Units
                3 => "1000",  // 1,000 Units
                4 => "2500",  // 2,500 Units
                5 => "5000",  // 5,000 Units
                6 => "10000", // 10,000 Units
                7 => "50000", // 50,000 Units
                _ => "0"
            };
        }

        private static string GetDemandParameterValue(int index)
        {
            return index switch
            {
                0 => "0",     // Any
                1 => "100",   // 100 Units or unlimited
                2 => "500",   // 500 Units or unlimited
                3 => "1000",  // 1,000 Units or unlimited
                4 => "2500",  // 2,500 Units or unlimited
                5 => "5000",  // 5,000 Units or unlimited
                6 => "10000", // 10,000 Units or unlimited
                7 => "50000", // 50,000 Units or unlimited
                _ => "0"
            };
        }
    }
}
