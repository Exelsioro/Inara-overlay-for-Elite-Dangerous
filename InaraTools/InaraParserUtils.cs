using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace InaraTools
{
    public static partial class InaraParserUtils
    {
        #region Trade Route Parsing

        /// <summary>
        /// Parses trade routes from an HtmlDocument containing Inara trade route data.
        /// </summary>
        /// <param name="doc">The HtmlDocument to parse</param>
        /// <returns>List of parsed trade routes, or empty list if parsing fails</returns>
        public static List<TradeRoute> ParseTradeRoutes(HtmlAgilityPack.HtmlDocument document)
        {
            var routes = new List<TradeRoute>();

            if (document == null)
            {
                Logger.Logger.Warning("InaraParserUtils.ParseTradeRoutes: HtmlDocument is null");
                return routes;
            }

            try
            {
                Logger.Logger.Debug("Starting HTML parsing of INARA response");
                Logger.Logger.Debug($"Response data length: {document.DocumentNode.OuterHtml.Length} characters");

                var routeBoxes = document.DocumentNode.SelectNodes("//div[contains(@class,'mainblock') and contains(@class,'traderoutebox')]");

                if (routeBoxes == null)
                {
                    Logger.Logger.Warning("No trade route elements found with mainblock traderoutebox selector");
                    return routes;
                }

                Logger.Logger.Debug($"Found {routeBoxes.Count} trade route boxes to parse");

                foreach (var box in routeBoxes)
                {
                    var route = ParseSingleTradeRoute(box);
                    if (route != null)
                    {
                        routes.Add(route);
                    }
                }

                Logger.Logger.Debug($"Successfully parsed {routes.Count} trade routes");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"InaraParserUtils.ParseTradeRoutes: Exception parsing trade routes: {ex.Message}");
            }

            return routes;
        }

        /// <summary>
        /// Parses a single trade route from an HTML node based on the actual INARA structure.
        /// For INARA "best trade routes" pages, this always treats routes as one-way.
        /// Round-trip logic is bypassed when only the root box exists.
        /// </summary>
        /// <param name="routeBox">The HTML node containing the route information</param>
        /// <returns>Parsed TradeRoute object, or null if parsing fails</returns>
        private static TradeRoute? ParseSingleTradeRoute(HtmlNode routeBox)
        {
            if (routeBox == null)
            {
                return null;
            }

            try
            {
                var route = new TradeRoute();

                var hasLegContainers = HasLegContainerStructure(routeBox);
                var stationLinks = routeBox.SelectNodes(".//a[contains(@href,'/elite/station-market/')]");
                if (stationLinks != null && stationLinks.Count >= 2)
                {
                    route.CardHeader.FromStation = ParseStationFromLink(stationLinks[0], routeBox, true);
                    route.CardHeader.ToStation = ParseStationFromLink(stationLinks[1], routeBox, false);
                }
                else
                {
                    Logger.Logger.Warning("ParseSingleTradeRoute: Could not find expected station links");
                    return null;
                }

                ParseRouteInformation(routeBox, route);
                ParseDistanceToStars(routeBox, route);
                ParseRouteDistance(routeBox, route);
                ParseRouteTotalProfit(routeBox, route);

                if (!hasLegContainers)
                {
                    Logger.Logger.Debug("ParseSingleTradeRoute: Detected best-routes page structure - parsing as one-way route from root box");
                    ParseCommodityInformation(routeBox, route);
                    ParseProfitInformation(routeBox, route);
                    Logger.Logger.Debug($"Successfully parsed best-routes one-way route: {route.CardHeader.FromStation.Name} ({route.CardHeader.FromStation.System}) -> {route.CardHeader.ToStation.Name} ({route.CardHeader.ToStation.System}), {route.FirstRoute.BuyCommodity.Name}, Profit: {route.FirstRoute.ProfitPerUnit} CR/t");
                }
                else
                {
                    Logger.Logger.Debug("ParseSingleTradeRoute: Detected traditional route structure with leg containers");
                    var legContainers = CollectLegContainersV2(routeBox);

                    if (legContainers.Count == 4)
                    {
                        Logger.Logger.Debug("ParseSingleTradeRoute: Round-trip detected with 4 leg containers - parsing as round trip");
                        route.IsRoundTrip = true;

                        List<HtmlNode> outboundContainers = new List<HtmlNode>();
                        outboundContainers.Add(legContainers.ElementAt(0));
                        outboundContainers.Add(legContainers.ElementAt(2));
                        if (!ParseLegs(outboundContainers, route, true))
                        {
                            Logger.Logger.Warning("ParseSingleTradeRoute: Failed to parse outbound legs");
                            return null;
                        }

                        List<HtmlNode> returnContainers = new List<HtmlNode>();
                        returnContainers.Add(legContainers.ElementAt(1));
                        returnContainers.Add(legContainers.ElementAt(3));
                        if (!ParseLegs(returnContainers, route, false))
                        {
                            Logger.Logger.Warning("ParseSingleTradeRoute: Failed to parse outbound legs");
                            return null;
                        }

                        return route;
                    }

                    if (legContainers.Count == 2)
                    {
                        Logger.Logger.Debug("ParseSingleTradeRoute: Detected one-way trip with 2 leg containers");

                        if (!ParseLegs(legContainers, route, true))
                        {
                            Logger.Logger.Warning("ParseSingleTradeRoute: Failed to parse outbound legs");
                            return null;
                        }

                        Logger.Logger.Debug($"Successfully parsed one-way route: {route.CardHeader.FromStation.Name} ({route.CardHeader.FromStation.System}) -> {route.CardHeader.ToStation.Name} ({route.CardHeader.ToStation.System}), {route.FirstRoute.BuyCommodity.Name}, Profit: {route.FirstRoute.ProfitPerUnit} CR/t");
                    }
                    else
                    {
                        Logger.Logger.Warning($"ParseSingleTradeRoute: Unexpected number of leg containers: {legContainers.Count}");
                        return null;
                    }
                }

                if (string.IsNullOrEmpty(route.CardHeader.FromStation?.Name) || string.IsNullOrEmpty(route.CardHeader.ToStation?.Name))
                {
                    Logger.Logger.Warning("ParseSingleTradeRoute: Missing essential station data");
                    return null;
                }

                return route;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseSingleTradeRoute: Exception parsing route: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
