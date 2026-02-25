using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace InaraTools
{
    public static partial class InaraParserUtils
    {
        private static void ParseDistanceToStars(HtmlNode routeBox, TradeRoute route)
        {
            var routes = routeBox.SelectNodes(".//div[contains(.,'Station distance')]//div[contains(@class,'itempaircontainer')]");
            if (routes == null || routes.Count < 2)
            {
                return;
            }

            var toFirstStationDistance = routes[0];
            var toSecondStationDistance = routes[1];

            if (toFirstStationDistance != null)
            {
                var distanceTextToFirst = GetSafeInnerText(toFirstStationDistance.SelectSingleNode(".//span[contains(@class, 'minor')]"));
                route.CardHeader.FromStation.DistanceFromStar = ParseDoubleLy(distanceTextToFirst);
            }

            if (toSecondStationDistance != null)
            {
                var distanceTextToSedond = GetSafeInnerText(toSecondStationDistance.SelectSingleNode(".//span[contains(@class, 'minor')]"));
                route.CardHeader.ToStation.DistanceFromStar = ParseDoubleLy(distanceTextToSedond);
            }
        }

        /// <summary>
        /// Parses station information from a station link, extracting name, system, and distance data.
        /// </summary>
        /// <param name="stationLink">The station link element</param>
        /// <param name="routeBox">The route container for context</param>
        /// <param name="isFromStation">True if this is the from station, false for to station</param>
        /// <returns>Parsed Station object</returns>
        private static Station ParseStationFromLink(HtmlNode stationLink, HtmlNode routeBox, bool isFromStation)
        {
            var station = new Station();

            try
            {
                var parentText = CleanSpecialSymbols(GetSafeInnerText(stationLink.ParentNode));

                if (!string.IsNullOrEmpty(parentText) && parentText.Contains("|"))
                {
                    var parts = parentText.Split('|');
                    if (parts.Length >= 2)
                    {
                        station.Name = CleanSpecialSymbols(string.Join(' ', parts[0].Trim().Split(' ').Skip(1)));
                        station.System = CleanSpecialSymbols(parts[1].Trim());
                    }
                    else
                    {
                        station.Name = CleanSpecialSymbols(parentText.Trim());
                    }
                }
                else
                {
                    var linkText = GetSafeInnerText(stationLink);
                    if (!string.IsNullOrEmpty(linkText))
                    {
                        var parts = linkText.Split('|');
                        if (parts.Length >= 2)
                        {
                            station.Name = CleanSpecialSymbols(parts[0].Trim());
                            station.System = CleanSpecialSymbols(parts[1].Trim());
                        }
                        else
                        {
                            station.Name = CleanSpecialSymbols(linkText.Trim());
                        }
                    }
                }

                var stationIcon = stationLink.SelectSingleNode(".//div[contains(@class,'stationicon')]");
                if (stationIcon != null)
                {
                    var styleValue = stationIcon.GetAttributeValue("style", string.Empty);
                    var iconStyle = styleValue.Split("background-position:").Skip(1).FirstOrDefault()?.Split("px").FirstOrDefault()?.TrimStart();
                    station.StationType = GetStationTypeFromIcon(iconStyle ?? string.Empty);
                }

                var blackMarketIcon = stationLink.ParentNode?.SelectSingleNode(".//div[contains(@class,'blackmarketicon')]");
                if (blackMarketIcon != null)
                {
                    station.StationType += "(Black Market)";
                }

                station.StationDistanceLs = ParseStationDistanceFromContext(stationLink, routeBox, isFromStation);

                Logger.Logger.Debug($"Parsed station: {station.Name} in {station.System}, Distance: {station.StationDistanceLs} Ls");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseStationFromLink: Exception parsing station: {ex.Message}");
            }

            return station;
        }

        /// <summary>
        /// Gets station type from icon position.
        /// </summary>
        public static string GetStationTypeFromIcon(string iconPosition)
        {
            return iconPosition switch
            {
                "-26" => "Outpost",
                "-13" => "Starport",
                "-156" => "Starport",
                "-169" => "Starport",
                "-52" => "Outpost",
                "-780" => "Planetary",
                "-104" => "Mega Ship",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Parses station distance in light seconds (Ls).
        /// Returns 0.0 only after logging parsing failure.
        /// </summary>
        private static double ParseStationDistance(string distanceText)
        {
            if (string.IsNullOrWhiteSpace(distanceText))
            {
                Logger.Logger.LogNumericParsingFailure("ParseStationDistance", distanceText ?? "<null>", "Input is null or whitespace");
                return 0.0;
            }

            try
            {
                var match = Regex.Match(distanceText, @"([\d,]+)\s*Ls");
                if (match.Success)
                {
                    var numericText = match.Groups[1].Value.Replace(",", "");
                    if (double.TryParse(numericText, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    {
                        if (result < 0)
                        {
                            Logger.Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Negative distance value: {result}");
                            return 0.0;
                        }
                        if (result > 1000000)
                        {
                            Logger.Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Unreasonably large distance: {result} Ls");
                            return result;
                        }
                        return result;
                    }
                    else
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Failed to parse numeric part: '{numericText}'");
                    }
                }
                else
                {
                    Logger.Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, "No Ls distance pattern found");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Exception: {ex.Message}");
            }

            return 0.0;
        }

        /// <summary>
        /// Parses station distance from context by finding the nearest "Station distance" itempaircontainer
        /// using preceding-sibling:: or index parameter.
        /// </summary>
        /// <param name="stationLink">The station link element</param>
        /// <param name="routeBox">The route container for context</param>
        /// <param name="isFromStation">True if this is the from station, false for to station</param>
        /// <returns>Station distance in light seconds (Ls)</returns>
        private static double ParseStationDistanceFromContext(HtmlNode stationLink, HtmlNode routeBox, bool isFromStation)
        {
            try
            {
                var stationDistanceContainer = stationLink.SelectSingleNode(".//preceding-sibling::div[contains(@class,'itempaircontainer')][.//div[contains(@class,'itempairlabel')][contains(text(),'Station distance')]]");
                if (stationDistanceContainer != null)
                {
                    var distanceValue = stationDistanceContainer.SelectSingleNode(".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]");
                    if (distanceValue != null)
                    {
                        var distanceText = GetSafeInnerText(distanceValue);
                        Logger.Logger.Debug($"ParseStationDistanceFromContext: Found station distance using preceding-sibling: {distanceText}");
                        return ParseStationDistance(distanceText);
                    }
                }

                var distanceNodes = routeBox.SelectNodes(".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]");
                if (distanceNodes != null && distanceNodes.Count > 0)
                {
                    var distanceIndex = isFromStation ? 0 : (distanceNodes.Count > 1 ? 1 : 0);
                    if (distanceIndex < distanceNodes.Count)
                    {
                        var distanceText = GetSafeInnerText(distanceNodes[distanceIndex]);
                        Logger.Logger.Debug($"ParseStationDistanceFromContext: Found station distance using index {distanceIndex}: {distanceText}");
                        return ParseStationDistance(distanceText);
                    }
                }

                var allStationDistanceContainers = routeBox.SelectNodes(".//div[contains(@class,'itempaircontainer')][.//div[contains(@class,'itempairlabel')][contains(text(),'Station distance')]]");
                if (allStationDistanceContainers != null && allStationDistanceContainers.Count > 0)
                {
                    var containerIndex = isFromStation ? 0 : (allStationDistanceContainers.Count > 1 ? 1 : 0);
                    if (containerIndex < allStationDistanceContainers.Count)
                    {
                        var container = allStationDistanceContainers[containerIndex];
                        var distanceValue = container.SelectSingleNode(".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]");
                        if (distanceValue != null)
                        {
                            var distanceText = GetSafeInnerText(distanceValue);
                            Logger.Logger.Debug($"ParseStationDistanceFromContext: Found station distance using container index {containerIndex}: {distanceText}");
                            return ParseStationDistance(distanceText);
                        }
                    }
                }

                Logger.Logger.Debug($"ParseStationDistanceFromContext: No station distance found for {(isFromStation ? "from" : "to")} station");
                return 0.0;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseStationDistanceFromContext: Exception parsing station distance: {ex.Message}");
                return 0.0;
            }
        }
    }
}
