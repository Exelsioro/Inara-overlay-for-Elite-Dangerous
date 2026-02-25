using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace InaraTools
{
    public static partial class InaraParserUtils
    {
        /// <summary>
        /// Parses route-level information like profit, distances, and timestamps.
        /// </summary>
        private static void ParseRouteInformation(HtmlNode routeBox, TradeRoute route)
        {
            try
            {
                var profitNode = routeBox.SelectSingleNode("//div[contains(@class,'itempairvalue')]//span[contains(@class,'major')]");
                if (profitNode != null)
                {
                    var profitText = GetSafeInnerText(profitNode);
                    route.FirstRoute.ProfitPerUnit = ParseInt(profitText);
                }

                var updatedNode = routeBox.SelectSingleNode(".//div[text()='Updated']/following-sibling::div");
                if (updatedNode != null)
                {
                    route.LastUpdate = GetSafeInnerText(updatedNode);
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseRouteInformation: Exception: {ex.Message}");
            }
        }

        public static void ParseRouteDistance(HtmlNode routeBox, TradeRoute route)
        {
            var routeDistanceNode = routeBox.SelectSingleNode("//div[div[text()='Route distance']]//span[contains(@class,'bigger')]");
            if (routeDistanceNode != null)
            {
                var distanceText = GetSafeInnerText(routeDistanceNode);
                route.RouteDistance = ParseDoubleLy(distanceText);
            }
        }

        public static void ParseRouteTotalProfit(HtmlNode routeBox, TradeRoute route)
        {
            var totalProfitPerTripNode = routeBox.SelectSingleNode("//div[div//text()[contains(.,'Profit per trip')]]/div[@class='itempairvalue itempairvalueright']");
            if (totalProfitPerTripNode != null)
            {
                var profitText = GetSafeInnerText(totalProfitPerTripNode);
                var match = Regex.Match(profitText, @"([\d,]+)");
                if (match.Success)
                {
                    route.TotalProfitPerTrip = ParseInt(match.Groups[1].Value);
                }
            }
        }

        /// <summary>
        /// Parses profit information from the profit section of the route box.
        /// </summary>
        private static void ParseProfitInformation(HtmlNode routeBox, TradeRoute route)
        {
            try
            {
                var profitSection = routeBox.SelectSingleNode(".//div[contains(@class,'traderouteboxprofit')]");
                if (profitSection == null)
                {
                    Logger.Logger.Debug("ParseProfitInformation: No profit section found");
                    return;
                }

                var profitPerUnitNode = profitSection.SelectSingleNode(".//div[contains(@class,'itempairvalue')][contains(text(),'Cr')]");
                if (profitPerUnitNode != null)
                {
                    var profitText = GetSafeInnerText(profitPerUnitNode);
                    var match = Regex.Match(profitText, @"([\d,]+)");
                    if (match.Success)
                    {
                        route.FirstRoute.ProfitPerUnit = ParseInt(match.Groups[1].Value);
                    }
                }

                Logger.Logger.Debug($"ParseProfitInformation: Extracted profit per unit: {route.FirstRoute.ProfitPerUnit}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseProfitInformation: Exception: {ex.Message}");
            }
        }
    }
}
