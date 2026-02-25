using System;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace InaraTools
{
    public static partial class InaraParserUtils
    {
        /// <summary>
        /// Parses commodity information from the route box.
        /// </summary>
        private static void ParseCommodityInformation(HtmlNode routeBox, TradeRoute route)
        {
            try
            {
                var buySection = routeBox.SelectSingleNode(".//div[contains(@class,'traderouteboxtoright')]");
                var sellSection = routeBox.SelectSingleNode(".//div[contains(@class,'traderouteboxfromleft')]");
                TradeLeg parsedRoute = new TradeLeg();
                if (buySection != null)
                {
                    parsedRoute.BuyCommodity = ParseCommodityFromSubsection(buySection, "buy");
                }
                else
                {
                    Logger.Logger.Warning("ParseCommodityInformation: Buy section not found");
                }

                if (sellSection != null)
                {
                    parsedRoute.SellCommodity = ParseCommodityFromSubsection(sellSection, "sell");
                }
                else
                {
                    Logger.Logger.Warning("ParseCommodityInformation: Sell section not found");
                }
                route.FirstRoute = parsedRoute;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseCommodityInformation: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a commodity from its subsection in the HTML.
        /// </summary>
        /// <param name="subsection">The buy or sell subsection containing commodity information</param>
        /// <param name="type">The type of commodity ('buy' or 'sell')</param>
        /// <returns>Parsed Commodity object</returns>
        private static Commodity ParseCommodityFromSubsection(HtmlNode subsection, string type)
        {
            var commodity = new Commodity();

            try
            {
                var commodityLink = subsection.SelectSingleNode(".//a[contains(@href,'/elite/commodity/')]");
                if (commodityLink != null)
                {
                    commodity.Name = GetSafeInnerText(commodityLink);
                }
                else
                {
                    Logger.Logger.Warning($"ParseCommodityFromSubsection: No commodity link found in {type} subsection");
                    return commodity;
                }

                var priceNodes = subsection.SelectNodes(".//div[contains(@class,'itempairvalue')]");
                var priceNode = priceNodes != null && priceNodes.Count > 1 ? priceNodes[1] : null;
                var priceText = GetSafeInnerText(priceNode);
                if (priceText != null)
                {
                    var match = Regex.Match(priceText, @"([\d,]+)\s*Cr");
                    if (match.Success)
                    {
                        commodity.Price = ParseInt(match.Groups[1].Value);
                    }
                }

                ParseSubsectionSupplyDemand(subsection, commodity, type);
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseCommodityFromSubsection: Exception parsing {type} commodity: {ex.Message}");
            }

            return commodity;
        }

        /// <summary>
        /// Parses supply and demand information from a specific subsection.
        /// </summary>
        /// <param name="subsection">The buy or sell subsection containing supply/demand information</param>
        /// <param name="commodity">The commodity object to populate</param>
        /// <param name="type">The type of commodity ('buy' or 'sell')</param>
        private static void ParseSubsectionSupplyDemand(HtmlNode subsection, Commodity commodity, string type)
        {
            try
            {
                var itemPairs = subsection.SelectNodes(".//div[contains(@class,'itempaircontainer')]");
                if (itemPairs == null)
                {
                    return;
                }

                foreach (var pair in itemPairs)
                {
                    var label = pair.SelectSingleNode(".//div[contains(@class,'itempairlabel')]");
                    var value = pair.SelectSingleNode(".//div[contains(@class,'itempairvalue')]");

                    if (label != null && value != null)
                    {
                        var labelText = GetSafeInnerText(label).ToLower();
                        var valueText = GetSafeInnerText(value);

                        if (labelText.Contains("supply"))
                        {
                            commodity.Supply = CleanSpecialSymbols(valueText).Replace("?", "");
                        }
                        else if (labelText.Contains("demand"))
                        {
                            commodity.Demand = CleanSpecialSymbols(valueText).Replace("?", "");
                        }
                    }
                }

                var supplyDemandIcons = subsection.SelectNodes(".//div[contains(@class,'supplydemandicon')]");
                if (supplyDemandIcons != null)
                {
                    foreach (var icon in supplyDemandIcons)
                    {
                        var className = GetSafeAttribute(icon, "class");
                        var levelMatch = Regex.Match(className, @"supplydemandicon(\d+)");
                        if (levelMatch.Success)
                        {
                            var level = ParseInt(levelMatch.Groups[1].Value);
                        }
                    }
                }

                Logger.Logger.Debug($"Parsed {type} supply/demand from subsection: Supply: {commodity.Supply}, Demand: {commodity.Demand}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseSubsectionSupplyDemand: Exception parsing {type} supply/demand: {ex.Message}");
            }
        }
    }
}
