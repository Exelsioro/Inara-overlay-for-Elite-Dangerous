using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Logger;   

namespace InaraTools
{
    public class InaraCommunicator
    {
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

            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Read and parse the HTML content
            var data = await response.Content.ReadAsStringAsync();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);

            // Parse trade routes using proper HTML structure
            var tradeRoutes = ParseTradeRoutes(doc);

            Logger.Logger.Info($"Parsed {tradeRoutes.Count} trade routes from Inara");

            // If no routes found, provide fallback sample data for testing
            if (tradeRoutes.Count == 0)
            {
                Logger.Logger.Info("Using fallback data - API returned no results");
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
