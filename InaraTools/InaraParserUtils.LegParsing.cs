using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace InaraTools
{
    public static partial class InaraParserUtils
    {
        public static bool ParseLeg(HtmlNode legNode, TradeRoute route, bool isOutbound)
        {
            if (legNode == null)
            {
                Logger.Logger.Warning("ParseLeg: Input leg node is null");
                return false;
            }

            try
            {
                var leg = new TradeLeg();

                ParseLegCommodityInformation(legNode, leg, isOutbound);
                ParseLegRouteInformation(legNode, leg);
                ParseLegProfitInformation(legNode, leg);

                if (isOutbound)
                {
                    route.FirstRoute = leg;
                }
                else
                {
                    route.SecondRoute = leg;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseLeg: Exception parsing {(isOutbound ? "outbound" : "return")} leg: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parses commodity information from a leg node, using the new subsection-based approach.
        /// </summary>
        /// <param name="legNode">The leg node containing commodity information</param>
        /// <param name="leg">The TradeLeg object to populate</param>
        private static void ParseLegCommodityInformation(HtmlNode legNode, TradeLeg leg, bool isOutbound)
        {
            try
            {
                HtmlNode? sellSection = null;
                HtmlNode? buySection = null;
                if (isOutbound)
                {
                    buySection = legNode.SelectSingleNode("//div[contains(@class,'traderouteboxtoright')]");
                    sellSection = legNode.SelectSingleNode("//div[contains(@class,'traderouteboxfromleft')]");
                }
                else
                {
                    sellSection = legNode.SelectSingleNode("//div[contains(@class,'traderouteboxfromright')]");
                    buySection = legNode.SelectSingleNode("//div[contains(@class,'traderouteboxtoleft')]");
                }

                if (buySection != null)
                {
                    leg.BuyCommodity = ParseCommodityFromSubsection(buySection, "buy");
                }
                else
                {
                    Logger.Logger.Warning("ParseLegCommodityInformation: Buy section not found in leg");
                }

                if (sellSection != null)
                {
                    leg.SellCommodity = ParseCommodityFromSubsection(sellSection, "sell");
                }
                else
                {
                    Logger.Logger.Warning("ParseLegCommodityInformation: Sell section not found in leg");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseLegCommodityInformation: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses route-level information for a leg, reusing existing parsing utilities.
        /// </summary>
        /// <param name="legNode">The leg node containing route information</param>
        /// <param name="leg">The TradeLeg object to populate</param>
        private static void ParseLegRouteInformation(HtmlNode legNode, TradeLeg leg)
        {
            try
            {
                var profitNode = legNode.SelectSingleNode(".//div[@class='traderouteboxprofit']//span[contains(@class,'biggest')]");
                if (profitNode != null)
                {
                    var profitText = GetSafeInnerText(profitNode);
                    leg.ProfitPerUnit = ParseInt(profitText);
                }

                var updatedNode = legNode.SelectSingleNode(".//div[text()='Updated']/following-sibling::div");
                if (updatedNode != null)
                {
                    leg.LastUpdate = GetSafeInnerText(updatedNode);
                }

                Logger.Logger.Debug($"Parsed leg route info: Profit: {leg.ProfitPerUnit}, Distance: ... Ly, Updated: {leg.LastUpdate}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseLegRouteInformation: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses profit information from a leg's profit section, reusing existing parsing utilities.
        /// </summary>
        /// <param name="legNode">The leg node containing profit information</param>
        /// <param name="leg">The TradeLeg object to populate</param>
        private static void ParseLegProfitInformation(HtmlNode legNode, TradeLeg leg)
        {
            try
            {
                var profitSection = legNode.SelectSingleNode(".//div[contains(@class,'traderouteboxprofit')]");
                if (profitSection == null)
                {
                    Logger.Logger.Debug("ParseLegProfitInformation: No profit section found in leg");
                    return;
                }

                var profitPerUnitNode = profitSection.SelectSingleNode(".//div[contains(@class,'itempairvalue')][contains(text(),'Cr')]");
                if (profitPerUnitNode != null)
                {
                    var profitText = GetSafeInnerText(profitPerUnitNode);
                    var match = Regex.Match(profitText, @"([\d,]+)");
                    if (match.Success)
                    {
                        leg.ProfitPerUnit = ParseInt(match.Groups[1].Value);
                    }
                }

                Logger.Logger.Debug($"ParseLegProfitInformation: Extracted profit per unit: {leg.ProfitPerUnit}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseLegProfitInformation: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Collects all leg containers from a route box, ordered as they appear.
        /// </summary>
        /// <param name="routeBox">The route box HTML node</param>
        /// <returns>List of leg container nodes</returns>
        private static List<HtmlNode> CollectLegContainers(HtmlNode routeBox)
        {
            var legContainers = new List<HtmlNode>();

            try
            {
                var containerSelectors = new[]
                {
                    ".//div[contains(@class,'traderoute-leg')]",
                    ".//div[contains(@class,'leg-container')]",
                    ".//div[contains(@class,'route-segment')]",
                    ".//div[contains(@class,'traderouteitem')]",
                    ".//div[contains(@class,'mainblock')][contains(@class,'traderouteitem')]"
                };

                foreach (var selector in containerSelectors)
                {
                    var nodes = routeBox.SelectNodes(selector);
                    if (nodes != null && nodes.Count > 0)
                    {
                        legContainers.AddRange(nodes);
                        Logger.Logger.Debug($"CollectLegContainers: Found {nodes.Count} containers with selector: {selector}");
                        break;
                    }
                }

                if (legContainers.Count == 0)
                {
                    var stationLinks = routeBox.SelectNodes(".//a[contains(@href,'/elite/station-market/')]");
                    if (stationLinks != null)
                    {
                        Logger.Logger.Debug($"CollectLegContainers: Fallback to station link analysis. Found {stationLinks.Count} station links");

                        for (int i = 0; i < stationLinks.Count - 1; i += 2)
                        {
                            if (i + 1 < stationLinks.Count)
                            {
                                var commonParent = FindCommonParent(stationLinks[i], stationLinks[i + 1]);
                                if (commonParent != null)
                                {
                                    legContainers.Add(commonParent);
                                }
                            }
                        }
                    }
                }

                Logger.Logger.Debug($"CollectLegContainers: Total leg containers found: {legContainers.Count}");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"CollectLegContainers: Exception collecting leg containers: {ex.Message}");
            }

            return legContainers;
        }

        /// <summary>
        /// Deterministic version of CollectLegContainers that looks for specific leg nodes
        /// in a fixed order [to-right, from-right, from-left, to-left] for round trips,
        /// or [to-right, from-right] for one-way routes.
        /// </summary>
        /// <param name="routeBox">The route box HTML node</param>
        /// <returns>List of leg container nodes in deterministic order</returns>
        private static List<HtmlNode> CollectLegContainersV2(HtmlNode routeBox)
        {
            var legContainers = new List<HtmlNode>();

            try
            {
                Logger.Logger.Debug("CollectLegContainersV2: Starting deterministic leg container collection");

                var legSelectors = new Dictionary<string, string>
                {
                    { "to-right", ".//div[contains(@class,'traderouteboxtoright')]"},
                    { "from-right", ".//div[contains(@class,'traderouteboxfromright')]"},
                    { "from-left", ".//div[contains(@class,'traderouteboxfromleft')]"},
                    { "to-left", ".//div[contains(@class,'traderouteboxtoleft')]"}
                };

                var foundLegs = new Dictionary<string, HtmlNode>();

                foreach (var legSelector in legSelectors)
                {
                    var legType = legSelector.Key;
                    var selector = legSelector.Value;

                    var legNode = routeBox.SelectSingleNode(selector);
                    if (legNode != null)
                    {
                        foundLegs[legType] = legNode;
                        Logger.Logger.Debug($"CollectLegContainersV2: Found {legType} leg node");
                    }
                    else
                    {
                        Logger.Logger.Debug($"CollectLegContainersV2: Missing {legType} leg node with selector: {selector}");
                    }
                }

                if (foundLegs.ContainsKey("to-right") && foundLegs.ContainsKey("from-right") &&
                    foundLegs.ContainsKey("from-left") && foundLegs.ContainsKey("to-left"))
                {
                    legContainers.Add(foundLegs["to-right"]);
                    legContainers.Add(foundLegs["from-right"]);
                    legContainers.Add(foundLegs["from-left"]);
                    legContainers.Add(foundLegs["to-left"]);

                    Logger.Logger.Debug("CollectLegContainersV2: Found complete round trip with 4 legs: [to-right, from-right, from-left, to-left]");
                }
                else if (foundLegs.ContainsKey("to-right") && foundLegs.ContainsKey("from-left"))
                {
                    legContainers.Add(foundLegs["to-right"]);
                    legContainers.Add(foundLegs["from-left"]);

                    Logger.Logger.Debug("CollectLegContainersV2: Found one-way route with 2 legs: [to-right, from-right]");

                    if (!foundLegs.ContainsKey("from-right"))
                    {
                        Logger.Logger.Warning("CollectLegContainersV2: Expected from-left leg missing for potential round trip");
                    }
                    if (!foundLegs.ContainsKey("to-left"))
                    {
                        Logger.Logger.Warning("CollectLegContainersV2: Expected to-left leg missing for potential round trip");
                    }
                }
                else
                {
                    Logger.Logger.Warning("CollectLegContainersV2: Could not find minimum required legs for valid route");

                    if (!foundLegs.ContainsKey("to-right"))
                    {
                        Logger.Logger.Warning("CollectLegContainersV2: Critical - to-right leg missing. This is required for any valid route.");
                    }
                    if (!foundLegs.ContainsKey("from-right"))
                    {
                        Logger.Logger.Warning("CollectLegContainersV2: Critical - from-right leg missing. This is required for any valid route.");
                    }

                    foreach (var foundLeg in foundLegs)
                    {
                        Logger.Logger.Debug($"CollectLegContainersV2: Found leg: {foundLeg.Key}");
                    }

                    Logger.Logger.Debug("CollectLegContainersV2: Falling back to original CollectLegContainers method");
                    return CollectLegContainers(routeBox);
                }

                Logger.Logger.Debug($"CollectLegContainersV2: Successfully collected {legContainers.Count} leg containers in deterministic order");
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"CollectLegContainersV2: Exception during deterministic leg collection: {ex.Message}");
                Logger.Logger.Debug("CollectLegContainersV2: Falling back to original CollectLegContainers method due to exception");
                return CollectLegContainers(routeBox);
            }

            return legContainers;
        }

        /// <summary>
        /// Finds the common parent node of two HTML nodes.
        /// </summary>
        /// <param name="node1">First node</param>
        /// <param name="node2">Second node</param>
        /// <returns>Common parent node, or null if not found</returns>
        private static HtmlNode? FindCommonParent(HtmlNode node1, HtmlNode node2)
        {
            try
            {
                var parents1 = new HashSet<HtmlNode>();
                var current = node1.ParentNode;

                while (current != null)
                {
                    parents1.Add(current);
                    current = current.ParentNode;
                }

                current = node2.ParentNode;
                while (current != null)
                {
                    if (parents1.Contains(current))
                    {
                        return current;
                    }
                    current = current.ParentNode;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"FindCommonParent: Exception finding common parent: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Parses outbound legs (indices 0 & 1) into the main TradeRoute object.
        /// </summary>
        /// <param name="outboundNodes">List of outbound leg nodes</param>
        /// <param name="route">TradeRoute object to populate</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        private static bool ParseLegs(List<HtmlNode> outboundNodes, TradeRoute route, bool isOutbound)
        {
            try
            {
                if (outboundNodes.Count != 2)
                {
                    Logger.Logger.Warning($"ParseOutboundLegs: Expected 2 outbound nodes, got {outboundNodes.Count}");
                    return false;
                }

                var combinedContainer = CreateCombinedContainer(outboundNodes);
                return ParseLeg(combinedContainer, route, isOutbound);
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"ParseOutboundLegs: Exception parsing outbound legs: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a combined container from multiple leg nodes for unified parsing.
        /// </summary>
        /// <param name="nodes">List of nodes to combine</param>
        /// <returns>Combined HTML node</returns>
        private static HtmlNode CreateCombinedContainer(List<HtmlNode> nodes)
        {
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                var combinedHtml = "<div class='combined-leg-container'>";

                foreach (var node in nodes)
                {
                    combinedHtml += node.OuterHtml;
                }

                combinedHtml += "</div>";

                doc.LoadHtml(combinedHtml);
                return doc.DocumentNode.FirstChild;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"CreateCombinedContainer: Exception creating combined container: {ex.Message}");
                return nodes.FirstOrDefault() ?? new HtmlAgilityPack.HtmlDocument().CreateElement("div");
            }
        }

        /// <summary>
        /// Checks if the route box has leg container structure (traditional route page)
        /// or if it's a simple structure (best trade routes page).
        /// </summary>
        /// <param name="routeBox">The route box HTML node</param>
        /// <returns>True if leg containers are found, false for simple best-routes structure</returns>
        private static bool HasLegContainerStructure(HtmlNode routeBox)
        {
            try
            {
                var legSelectors = new[]
                {
                    ".//div[contains(@class,'traderouteboxtoright')]",
                    ".//div[contains(@class,'traderouteboxfromright')]",
                    ".//div[contains(@class,'traderouteboxfromleft')]",
                    ".//div[contains(@class,'traderouteboxtoleft')]",
                };

                foreach (var selector in legSelectors)
                {
                    var legNode = routeBox.SelectSingleNode(selector);
                    if (legNode != null)
                    {
                        Logger.Logger.Debug($"HasLegContainerStructure: Found leg container with selector: {selector}");
                        return true;
                    }
                }

                Logger.Logger.Debug("HasLegContainerStructure: No leg container structure found - assuming best-routes page");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"HasLegContainerStructure: Exception checking for leg containers: {ex.Message}");
                return false;
            }
        }
    }
}
