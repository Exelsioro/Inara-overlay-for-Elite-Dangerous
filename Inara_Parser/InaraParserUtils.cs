using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Xml;
using HtmlAgilityPack;

namespace Inara_Parser
{
    public static class InaraParserUtils
    {
        #region Selector Constants

        /// <summary>
        /// XPath selector for trade route boxes containing route information
        /// </summary>
        public const string RouteBoxSelector = "//div[@class='mainblock traderoutebox']";

        /// <summary>
        /// XPath selector for station links within route boxes
        /// </summary>
        public const string StationLinkSelector = ".//a[contains(@href,'/elite/station-market/')]";

        /// <summary>
        /// XPath selector for commodity links
        /// </summary>
        public const string CommodityLinkSelector = ".//a[contains(@href,'/elite/commodity/')]";

        /// <summary>
        /// XPath selector for station distance (Ls from star)
        /// </summary>
        public const string StationDistanceSelector = ".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]";

        /// <summary>
        /// XPath selector for route distance (Ly between systems)
        /// </summary>
        public const string RouteDistanceSelector = ".//div[text()='Route distance']/following-sibling::div";

        /// <summary>
        /// XPath selector for system distance (distance from current location)
        /// </summary>
        public const string SystemDistanceSelector = ".//div[text()='Distance']/following-sibling::div";

        /// <summary>
        /// XPath selector for last updated timestamp
        /// </summary>
        public const string UpdatedSelector = ".//div[text()='Updated']/following-sibling::div";

        /// <summary>
        /// XPath selector for profit per unit
        /// </summary>
        public const string ProfitPerUnitSelector = ".//div[@class='traderouteboxprofit']//span[contains(@class,'biggest')]";

        /// <summary>
        /// XPath selector for station icons
        /// </summary>
        public const string StationIconSelector = ".//div[contains(@class,'stationicon')]";

        /// <summary>
        /// XPath selector for supply/demand icons
        /// </summary>
        public const string SupplyDemandIconSelector = ".//div[contains(@class,'supplydemandicon')]";

        /// <summary>
        /// XPath selector for black market indicator
        /// </summary>
        public const string BlackMarketSelector = ".//div[contains(@class,'blackmarketicon')]";

        /// <summary>
        /// XPath selector for "From" label in trade route sections
        /// </summary>
        public const string FromLabelXPath = ".//div[contains(@class,'itempairlabel')][text()='From']";

        /// <summary>
        /// XPath selector for "To" label in trade route sections
        /// </summary>
        public const string ToLabelXPath = ".//div[contains(@class,'itempairlabel')][text()='To']";

        /// <summary>
        /// XPath selector for buy section on the right side of trade route box
        /// </summary>
        public const string BuySectionXPath = ".//div[@class='traderouteboxtoright']";

        /// <summary>
        /// XPath selector for sell section on the left side of trade route box
        /// </summary>
        public const string SellSectionXPath = ".//div[@class='traderouteboxfromleft']";

        /// <summary>
        /// XPath selector for trade route subsection containers
        /// </summary>
        public const string RouteSubsectionXPath = ".//div[contains(@class,'traderoute-subsection')]";

        /// <summary>
        /// XPath selector for profit section in trade route boxes
        /// </summary>
        public const string ProfitSectionXPath = ".//div[@class='traderouteboxprofit']";

        /// <summary>
        /// XPath selector for item pair containers (label-value pairs)
        /// </summary>
        public const string ItemPairContainerXPath = ".//div[contains(@class,'itempaircontainer')]";

        /// <summary>
        /// XPath selector for item pair labels
        /// </summary>
        public const string ItemPairLabelXPath = ".//div[contains(@class,'itempairlabel')]";

        /// <summary>
        /// XPath selector for item pair values
        /// </summary>
        public const string ItemPairValueXPath = ".//div[contains(@class,'itempairvalue')]";

        #endregion

        #region HTML Node Finding Methods

        /// <summary>
        /// Finds the first HTML node within the given container that has a class containing the specified text.
        /// </summary>
        /// <param name="container">The container node to search within</param>
        /// <param name="classContains">The text that should be contained in the class attribute</param>
        /// <returns>The first matching HtmlNode, or null if not found</returns>
        public static HtmlNode? Find(HtmlNode container, string classContains)
        {
            if (container == null)
            {
                Logger.Warning($"InaraParserUtils.Find: Container node is null");
                return null;
            }

            if (string.IsNullOrWhiteSpace(classContains))
            {
                Logger.Warning($"InaraParserUtils.Find: classContains parameter is null or empty");
                return null;
            }

            try
            {
                var selector = $".//*[contains(@class,'{classContains}')]";
                return container.SelectSingleNode(selector);
            }
            catch (Exception ex)
            {
                Logger.Warning($"InaraParserUtils.Find: Failed to find node with class containing '{classContains}': {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Text Cleaning and Parsing Methods

        /// <summary>
        /// Cleans numeric text by removing common formatting characters and extracting numbers.
        /// </summary>
        /// <param name="text">The text to clean</param>
        /// <returns>Cleaned numeric text, or empty string if input is null/empty</returns>
        public static string CleanNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            try
            {
                // Remove common formatting: commas, spaces, currency symbols, units
                var cleaned = Regex.Replace(text.Trim(), @"[,\s₹$€£¥₩\t\r\n]", "");

                // Remove common units (Ly, CR, etc.)
                cleaned = Regex.Replace(cleaned, @"(?i)\b(ly|cr|credits?|units?)\b", "");

                // Extract numbers including decimals and negative signs
                var match = Regex.Match(cleaned, @"-?\d+\.?\d*");
                if (match.Success)
                {
                    return match.Value;
                }

                Logger.Warning($"InaraParserUtils.CleanNumber: No numeric value found in text '{text}'");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Warning($"InaraParserUtils.CleanNumber: Failed to clean number from text '{text}': {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Parses an integer from the given text, with failsafe error handling.
        /// Returns 0 only after logging critical extraction failure.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <returns>Parsed integer value, or 0 if parsing fails</returns>
        public static int ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.LogNumericParsingFailure("ParseInt", text ?? "<null>", "Input is null or whitespace");
                return 0;
            }

            try
            {
                var cleanedText = CleanNumber(text);
                if (string.IsNullOrEmpty(cleanedText))
                {
                    Logger.LogNumericParsingFailure("ParseInt", text, "No numeric value found after cleaning");
                    return 0;
                }

                if (int.TryParse(cleanedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                {
                    return result;
                }

                // Try parsing as double first, then convert to int (handles cases like "1.5" -> 1)
                if (double.TryParse(cleanedText, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
                {
                    // Check for overflow/underflow before conversion
                    if (doubleResult > int.MaxValue)
                    {
                        Logger.LogNumericParsingFailure("ParseInt", text, $"Value {doubleResult} exceeds int.MaxValue");
                        return int.MaxValue;
                    }
                    if (doubleResult < int.MinValue)
                    {
                        Logger.LogNumericParsingFailure("ParseInt", text, $"Value {doubleResult} below int.MinValue");
                        return int.MinValue;
                    }
                    return (int)Math.Round(doubleResult);
                }

                Logger.LogNumericParsingFailure("ParseInt", text, $"Failed to parse as int or double (cleaned: '{cleanedText}')");
                return 0;
            }
            catch (OverflowException ex)
            {
                Logger.LogNumericParsingFailure("ParseInt", text, $"Overflow exception: {ex.Message}");
                return 0;
            }
            catch (FormatException ex)
            {
                Logger.LogNumericParsingFailure("ParseInt", text, $"Format exception: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Logger.LogNumericParsingFailure("ParseInt", text, $"Unexpected exception: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Parses a double representing light-years (Ly) from the given text, with failsafe error handling.
        /// Returns 0.0 only after logging critical extraction failure.
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <returns>Parsed double value, or 0.0 if parsing fails</returns>
        public static double ParseDoubleLy(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.LogNumericParsingFailure("ParseDoubleLy", text ?? "<null>", "Input is null or whitespace");
                return 0.0;
            }

            try
            {
                var cleanedText = CleanNumber(text);
                if (string.IsNullOrEmpty(cleanedText))
                {
                    Logger.LogNumericParsingFailure("ParseDoubleLy", text, "No numeric value found after cleaning");
                    return 0.0;
                }

                if (double.TryParse(cleanedText, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                {
                    // Check for overflow conditions
                    if (double.IsPositiveInfinity(result))
                    {
                        Logger.LogNumericParsingFailure("ParseDoubleLy", text, "Value is positive infinity");
                        return double.MaxValue;
                    }
                    if (double.IsNegativeInfinity(result))
                    {
                        Logger.LogNumericParsingFailure("ParseDoubleLy", text, "Value is negative infinity");
                        return double.MinValue;
                    }
                    if (double.IsNaN(result))
                    {
                        Logger.LogNumericParsingFailure("ParseDoubleLy", text, "Value is NaN");
                        return 0.0;
                    }

                    // Validate reasonable range for distances in light-years
                    if (result < 0)
                    {
                        Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Negative distance value: {result}");
                        return 0.0;
                    }

                    return result;
                }

                Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Failed to parse as double (cleaned: '{cleanedText}')");
                return 0.0;
            }
            catch (OverflowException ex)
            {
                Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Overflow exception: {ex.Message}");
                return 0.0;
            }
            catch (FormatException ex)
            {
                Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Format exception: {ex.Message}");
                return 0.0;
            }
            catch (Exception ex)
            {
                Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Unexpected exception: {ex.Message}");
                return 0.0;
            }
        }

        #endregion

        #region Station Name Extraction

        /// <summary>
        /// Extracts station name from an HTML node, with failsafe error handling.
        /// Returns null only after logging critical extraction failure with class and outerHtml snippet.
        /// Looks for text content in anchor tags or direct text content.
        /// </summary>
        /// <param name="node">The HTML node to extract station name from</param>
        /// <returns>Extracted station name, or null if extraction fails</returns>
        public static string? ExtractStationName(HtmlNode node)
        {
            if (node == null)
            {
                Logger.LogCriticalExtractionFailure("ExtractStationName", "<null>", "<null>", "Input node is null");
                return null;
            }

            try
            {
                // First try to find an anchor tag within the node
                var anchorNode = node.SelectSingleNode(".//a");
                if (anchorNode != null)
                {
                    var anchorText = anchorNode.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(anchorText))
                    {
                        return anchorText;
                    }
                }

                // If no anchor found, try direct text content
                var directText = node.InnerText?.Trim();
                if (!string.IsNullOrEmpty(directText))
                {
                    return directText;
                }

                // Try getting text from title attribute
                var titleText = node.GetAttributeValue("title", "")?.Trim();
                if (!string.IsNullOrEmpty(titleText))
                {
                    return titleText;
                }

                // Log critical extraction failure with context before returning null
                var nodeClass = node.GetAttributeValue("class", "<no-class>");
                var outerHtml = node.OuterHtml ?? "<no-html>";
                Logger.LogCriticalExtractionFailure("ExtractStationName", nodeClass, outerHtml, "No station name found in any extraction method");
                return null;
            }
            catch (Exception ex)
            {
                var nodeClass = node?.GetAttributeValue("class", "<no-class>") ?? "<exception-getting-class>";
                var outerHtml = node?.OuterHtml ?? "<exception-getting-html>";
                Logger.LogCriticalExtractionFailure("ExtractStationName", nodeClass, outerHtml, $"Exception: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Safely gets inner text from an HTML node with null checking.
        /// </summary>
        /// <param name="node">The HTML node to get text from</param>
        /// <returns>Inner text of the node, or empty string if node is null</returns>
        public static string GetSafeInnerText(HtmlNode? node)
        {
            return node?.InnerText?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Safely gets an attribute value from an HTML node with null checking.
        /// </summary>
        /// <param name="node">The HTML node to get attribute from</param>
        /// <param name="attributeName">The name of the attribute</param>
        /// <returns>Attribute value, or empty string if node is null or attribute doesn't exist</returns>
        public static string GetSafeAttribute(HtmlNode? node, string attributeName)
        {
            return node?.GetAttributeValue(attributeName, string.Empty) ?? string.Empty;
        }

        #endregion

        #region Trade Route Parsing

        /// <summary>
        /// Parses trade routes from an HTML string containing Inara trade route data.
        /// </summary>
        /// <param name="htmlContent">The HTML content to parse</param>
        /// <returns>List of parsed trade routes, or empty list if parsing fails</returns>
        public static List<TradeRoute> ParseTradeRoutes(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                Logger.Warning("InaraParserUtils.ParseTradeRoutes: HTML content is null or empty");
                return new List<TradeRoute>();
            }

            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlContent);
                return ParseTradeRoutes(doc);
            }
            catch (Exception ex)
            {
                Logger.Warning($"InaraParserUtils.ParseTradeRoutes: Failed to parse HTML content: {ex.Message}");
                return new List<TradeRoute>();
            }
        }

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
                Logger.Warning("InaraParserUtils.ParseTradeRoutes: HtmlDocument is null");
                return routes;
            }

            try
            {
                Logger.Info("Starting HTML parsing of INARA response");
                Logger.Debug($"Response data length: {document.DocumentNode.OuterHtml.Length} characters");

                // Find trade route boxes using the correct selector for new structure
                var routeBoxes = document.DocumentNode.SelectNodes("//div[contains(@class,'mainblock') and contains(@class,'traderoutebox')]");

                if (routeBoxes == null)
                {
                    Logger.Warning("No trade route elements found with mainblock traderoutebox selector");
                    return routes;
                }

                Logger.Info($"Found {routeBoxes.Count} trade route boxes to parse");

                foreach (var box in routeBoxes)
                {
                    var route = ParseSingleTradeRoute(box);
                    if (route != null)
                    {
                        routes.Add(route);
                    }
                }

                Logger.Info($"Successfully parsed {routes.Count} trade routes");
            }
            catch (Exception ex)
            {
                Logger.Warning($"InaraParserUtils.ParseTradeRoutes: Exception parsing trade routes: {ex.Message}");
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

                // Check if this is a best-routes page by detecting absence of leg containers
                // INARA "best trade routes" list is always one-way and uses the root box directly
                var hasLegContainers = HasLegContainerStructure(routeBox);
                var stationLinks = routeBox.SelectNodes(".//a[contains(@href,'/elite/station-market/')]");
                if (stationLinks != null && stationLinks.Count >= 2)
                {
                    route.CardHeader.FromStation = ParseStationFromLink(stationLinks[0], routeBox, true);
                    route.CardHeader.ToStation = ParseStationFromLink(stationLinks[1], routeBox, false);
                }
                else
                {
                    Logger.Warning("ParseSingleTradeRoute: Could not find expected station links");
                    return null;
                }

                if (!hasLegContainers)
                {
                    // INARA best trade routes: bypass CollectLegContainers, parse directly from root box
                    Logger.Debug("ParseSingleTradeRoute: Detected best-routes page structure - parsing as one-way route from root box");

                    // Parse FROM and TO stations directly from the route box

                    // Parse commodity information
                    ParseCommodityInformation(routeBox, route);

                    // Parse route-level information (profit, distances, timestamps)
                    ParseRouteInformation(routeBox, route);

                    // Parse profit information from the profit section
                    ParseProfitInformation(routeBox, route);

                    Logger.Debug($"Successfully parsed best-routes one-way route: {route.CardHeader.FromStation.Name} ({route.CardHeader.FromStation.System}) → {route.CardHeader.ToStation.Name} ({route.CardHeader.ToStation.System}), {route.FirstRoute.BuyCommodity.Name}, Profit: {route.FirstRoute.ProfitPerUnit} CR/t");
                }
                else
                {
                    // Traditional route parsing with leg containers (round-trip support guarded)
                    Logger.Debug("ParseSingleTradeRoute: Detected traditional route structure with leg containers");
                    // Step 1: Collect all leg containers, ordered as they appear
                    var legContainers = CollectLegContainersV2(routeBox);

                    if (legContainers.Count == 4)
                    {
                        // Round-trip detected - parse all 4 leg containers
                        Logger.Info("ParseSingleTradeRoute: Round-trip detected with 4 leg containers - parsing as round trip");

                        // Parse outbound leg (first two containers)
                        List<HtmlNode> outboundContainers = new List<HtmlNode>();
                        outboundContainers.Add(legContainers.ElementAt(0));
                        outboundContainers.Add(legContainers.ElementAt(2));
                        outboundContainers.Add(routeBox.SelectSingleNode(".//div[@class='traderouteboxprofit']//span[contains(@class,'biggest')]"));
                        if (!ParseOutboundLegs(outboundContainers, route))
                        {
                            Logger.Warning("ParseSingleTradeRoute: Failed to parse outbound legs");
                            return null;
                        }

                        // Parse return leg (last two containers)
                        List<HtmlNode> returnContainers = new List<HtmlNode>();
                        returnContainers.Add(legContainers.ElementAt(1));
                        returnContainers.Add(legContainers.ElementAt(3));
                        returnContainers.Add(routeBox.SelectSingleNode(".//div[@class='traderouteboxprofit']//span[contains(@class,'biggest')]"));
                        route.SecondRoute = ParseInboundLegs(returnContainers);

                        if (route.SecondRoute == null)
                        {
                            Logger.Warning("ParseSingleTradeRoute: Failed to parse return legs - treating as one-way");
                            // Continue as one-way route
                        }
                        else
                        {
                            route.IsRoundTrip = true;
                            Logger.Debug($"Successfully parsed round-trip route: {route.CardHeader.FromStation.Name} ↔ {route.CardHeader.ToStation.Name}");
                        }

                        return route; // Return early for round trips
                    }

                    if (legContainers.Count == 2)
                    {
                        // Two leg containers → one-way route
                        Logger.Debug("ParseSingleTradeRoute: Detected one-way trip with 2 leg containers");

                        // Parse commodity information
                        ParseCommodityInformation(routeBox, route);

                        // Parse route-level information (profit, distances, timestamps)
                        ParseRouteInformation(routeBox, route);

                        // Parse profit information from the profit section
                        ParseProfitInformation(routeBox, route);

                        Logger.Debug($"Successfully parsed one-way route: {route.CardHeader.FromStation.Name} ({route.CardHeader.FromStation.System}) → {route.CardHeader.ToStation.Name} ({route.CardHeader.ToStation.System}), {route.FirstRoute.BuyCommodity.Name}, Profit: {route.FirstRoute.ProfitPerUnit} CR/t");
                    }
                    else
                    {
                        Logger.Warning($"ParseSingleTradeRoute: Unexpected number of leg containers: {legContainers.Count}");
                        return null;
                    }
                }

                // Validate essential data
                if (string.IsNullOrEmpty(route.CardHeader.FromStation?.Name) || string.IsNullOrEmpty(route.CardHeader.ToStation?.Name))
                {
                    Logger.Warning("ParseSingleTradeRoute: Missing essential station data");
                    return null;
                }

                return route;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseSingleTradeRoute: Exception parsing route: {ex.Message}");
                return null;
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
                // Extract station name and system from the parent text of the <a> (contains "|")
                var parentText = CleanSpecialSymbols(GetSafeInnerText(stationLink.ParentNode));

                if (!string.IsNullOrEmpty(parentText) && parentText.Contains("|"))
                {
                    // The structure is "StationName | SystemName"
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
                    // Fallback to link text if parent doesn't contain "|"
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

                // Extract station icon/type information (same XPath, class unchanged)
                var stationIcon = stationLink.SelectSingleNode(".//div[contains(@class,'stationicon')]");
                if (stationIcon != null)
                {
                    var iconStyle = GetSafeAttribute(stationIcon, "style");
                    station.StationIconKey = ExtractIconPosition(iconStyle);
                    station.StationType = GetStationTypeFromIcon(station.StationIconKey);
                }

                // Check for black market indicator
                var blackMarketIcon = stationLink.ParentNode?.SelectSingleNode(".//div[contains(@class,'blackmarketicon')]");
                if (blackMarketIcon != null)
                {
                    station.StationType += " (Black Market)";
                }

                // Grab distance from the nearest "Station distance" itempaircontainer using preceding-sibling:: or index parameter
                station.StationDistanceLs = ParseStationDistanceFromContext(stationLink, routeBox, isFromStation);

                Logger.Debug($"Parsed station: {station.Name} in {station.System}, Distance: {station.StationDistanceLs} Ls");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseStationFromLink: Exception parsing station: {ex.Message}");
            }

            return station;
        }

        /// <summary>
        /// Parses commodity information from the route box.
        /// </summary>
        private static void ParseCommodityInformation(HtmlNode routeBox, TradeRoute route)
        {
            try
            {
                // Locate buy and sell subsections using the specified XPath selectors
                var buySection = routeBox.SelectSingleNode(".//div[contains(@class,'traderouteboxtoright')]");
                var sellSection = routeBox.SelectSingleNode(".//div[contains(@class,'traderouteboxfromleft')]");
                TradeLeg parsedRoute = new TradeLeg();
                // Parse buy commodity from buy section
                if (buySection != null)
                {
                    parsedRoute.BuyCommodity = ParseCommodityFromSubsection(buySection, "buy");
                }
                else
                {
                    Logger.Warning("ParseCommodityInformation: Buy section not found");
                }

                // Parse sell commodity from sell section
                if (sellSection != null)
                {
                    parsedRoute.SellCommodity = ParseCommodityFromSubsection(sellSection, "sell");
                }
                else
                {
                    Logger.Warning("ParseCommodityInformation: Sell section not found");
                }
                route.FirstRoute = parsedRoute;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseCommodityInformation: Exception: {ex.Message}");
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
                // Find commodity link within this subsection
                var commodityLink = subsection.SelectSingleNode(".//a[contains(@href,'/elite/commodity/')]");
                if (commodityLink != null)
                {
                    // Get commodity name from link text
                    commodity.Name = GetSafeInnerText(commodityLink);
                }
                else
                {
                    Logger.Warning($"ParseCommodityFromSubsection: No commodity link found in {type} subsection");
                    return commodity;
                }

                // Find price information - look for first number before "Cr"
                var priceText = GetSafeInnerText(subsection.SelectNodes(".//div[contains(@class,'itempairvalue')]")[1]);
                if (priceText != null)
                {
                    var match = Regex.Match(priceText, @"([\d,]+)\s*Cr");
                    if (match.Success)
                    {
                        commodity.Price = ParseInt(match.Groups[1].Value);
                    }
                }

                // Find supply/demand information within the same subsection
                ParseSubsectionSupplyDemand(subsection, commodity, type);

                //Logger.Debug($"Parsed {type} commodity from subsection: {commodity.Name}, Price: {commodity.Price}, Supply: {commodity.Supply}, Demand: {commodity.Demand}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseCommodityFromSubsection: Exception parsing {type} commodity: {ex.Message}");
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
                // Find supply and demand values within this specific subsection
                var itemPairs = subsection.SelectNodes(".//div[contains(@class,'itempaircontainer')]");
                if (itemPairs == null) return;

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

                // Find supply/demand level icons within this subsection
                var supplyDemandIcons = subsection.SelectNodes(".//div[contains(@class,'supplydemandicon')]");
                if (supplyDemandIcons != null)
                {
                    foreach (var icon in supplyDemandIcons)
                    {
                        var className = GetSafeAttribute(icon, "class");
                        // Extract supply/demand level from class name (e.g., "supplydemandicon3")
                        var levelMatch = Regex.Match(className, @"supplydemandicon(\d+)");
                        if (levelMatch.Success)
                        {
                            var level = ParseInt(levelMatch.Groups[1].Value);
                            // This could be used to set supply/demand quality indicators
                        }
                    }
                }

                Logger.Debug($"Parsed {type} supply/demand from subsection: Supply: {commodity.Supply}, Demand: {commodity.Demand}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseSubsectionSupplyDemand: Exception parsing {type} supply/demand: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses route-level information like profit, distances, and timestamps.
        /// </summary>
        private static void ParseRouteInformation(HtmlNode routeBox, TradeRoute route)
        {
            try
            {
                // Parse profit per unit - targeting the biggest span in traderouteboxprofit
                var profitNode = routeBox.SelectSingleNode(".//div[@class='traderouteboxprofit']//span[contains(@class,'biggest')]");
                if (profitNode != null)
                {
                    var profitText = GetSafeInnerText(profitNode);
                    route.FirstRoute.ProfitPerUnit = ParseInt(profitText);
                }

                // Parse route distance
                var routeDistanceNode = routeBox.SelectSingleNode(".//div[text()='Route distance']/following-sibling::div");
                if (routeDistanceNode != null)
                {
                    var distanceText = GetSafeInnerText(routeDistanceNode);
                    route.FirstRoute.RouteDistance = ParseDoubleLy(distanceText);
                }

                // Parse last updated timestamp
                var updatedNode = routeBox.SelectSingleNode(".//div[text()='Updated']/following-sibling::div");
                if (updatedNode != null)
                {
                    route.LastUpdate = GetSafeInnerText(updatedNode);
                }

                //Logger.Debug($"Parsed route info: Profit: {route.FirstRoute.ProfitPerUnit}, Distance: {route.FirstRoute.RouteDistance} Ly, Updated: {route.LastUpdate}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseRouteInformation: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts icon position from a style attribute.
        /// </summary>
        public static string ExtractIconPosition(string styleAttribute)
        {
            try
            {
                var match = Regex.Match(styleAttribute, @"background-position:\s*(-?\d+)px\s+(-?\d+)px");
                if (match.Success)
                {
                    return $"{match.Groups[1].Value},{match.Groups[2].Value}";
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"ExtractIconPosition: Exception: {ex.Message}");
            }
            return "";
        }

        /// <summary>
        /// Gets station type from icon position.
        /// </summary>
        public static string GetStationTypeFromIcon(string iconPosition)
        {
            return iconPosition switch
            {
                "-26,0" => "Starport",
                "-52,0" => "Outpost",
                "-78,0" => "Planetary",
                "-104,0" => "Mega Ship",
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
                Logger.LogNumericParsingFailure("ParseStationDistance", distanceText ?? "<null>", "Input is null or whitespace");
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
                        // Validate reasonable range for station distances (light seconds)
                        if (result < 0)
                        {
                            Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Negative distance value: {result}");
                            return 0.0;
                        }
                        if (result > 1000000) // Reasonable upper bound for station distances
                        {
                            Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Unreasonably large distance: {result} Ls");
                            return result; // Still return it but log the warning
                        }
                        return result;
                    }
                    else
                    {
                        Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Failed to parse numeric part: '{numericText}'");
                    }
                }
                else
                {
                    Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, "No Ls distance pattern found");
                }
            }
            catch (Exception ex)
            {
                Logger.LogNumericParsingFailure("ParseStationDistance", distanceText, $"Exception: {ex.Message}");
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
                // Method 1: Look for "Station distance" label in itempaircontainer using preceding-sibling::
                var stationDistanceContainer = stationLink.SelectSingleNode(".//preceding-sibling::div[contains(@class,'itempaircontainer')][.//div[contains(@class,'itempairlabel')][contains(text(),'Station distance')]]");
                if (stationDistanceContainer != null)
                {
                    var distanceValue = stationDistanceContainer.SelectSingleNode(".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]");
                    if (distanceValue != null)
                    {
                        var distanceText = GetSafeInnerText(distanceValue);
                        Logger.Debug($"ParseStationDistanceFromContext: Found station distance using preceding-sibling: {distanceText}");
                        return ParseStationDistance(distanceText);
                    }
                }

                // Method 2: Find all distance nodes in the route box and use index parameter
                var distanceNodes = routeBox.SelectNodes(".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]");
                if (distanceNodes != null && distanceNodes.Count > 0)
                {
                    var distanceIndex = isFromStation ? 0 : (distanceNodes.Count > 1 ? 1 : 0);
                    if (distanceIndex < distanceNodes.Count)
                    {
                        var distanceText = GetSafeInnerText(distanceNodes[distanceIndex]);
                        Logger.Debug($"ParseStationDistanceFromContext: Found station distance using index {distanceIndex}: {distanceText}");
                        return ParseStationDistance(distanceText);
                    }
                }

                // Method 3: Look for nearest "Station distance" itempaircontainer in the route context
                var allStationDistanceContainers = routeBox.SelectNodes(".//div[contains(@class,'itempaircontainer')][.//div[contains(@class,'itempairlabel')][contains(text(),'Station distance')]]");
                if (allStationDistanceContainers != null && allStationDistanceContainers.Count > 0)
                {
                    // Use index based on whether this is from or to station
                    var containerIndex = isFromStation ? 0 : (allStationDistanceContainers.Count > 1 ? 1 : 0);
                    if (containerIndex < allStationDistanceContainers.Count)
                    {
                        var container = allStationDistanceContainers[containerIndex];
                        var distanceValue = container.SelectSingleNode(".//div[contains(@class,'itempairvalue')][contains(text(),'Ls')]");
                        if (distanceValue != null)
                        {
                            var distanceText = GetSafeInnerText(distanceValue);
                            Logger.Debug($"ParseStationDistanceFromContext: Found station distance using container index {containerIndex}: {distanceText}");
                            return ParseStationDistance(distanceText);
                        }
                    }
                }

                Logger.Debug($"ParseStationDistanceFromContext: No station distance found for {(isFromStation ? "from" : "to")} station");
                return 0.0;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseStationDistanceFromContext: Exception parsing station distance: {ex.Message}");
                return 0.0;
            }
        }

        /// <summary>
        /// Parses profit information from the profit section of the route box.
        /// </summary>
        private static void ParseProfitInformation(HtmlNode routeBox, TradeRoute route)
        {
            try
            {
                // Look for the profit section
                var profitSection = routeBox.SelectSingleNode(".//div[contains(@class,'traderouteboxprofit')]");
                if (profitSection == null)
                {
                    Logger.Debug("ParseProfitInformation: No profit section found");
                    return;
                }

                // Parse profit per unit from the profit section
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

                Logger.Debug($"ParseProfitInformation: Extracted profit per unit: {route.FirstRoute.ProfitPerUnit}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseProfitInformation: Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a single trade leg from a subsection node (e.g., "to-right", "from-left", etc.).
        /// Returns a TradeLeg filled exactly like the current one-way path.
        /// </summary>
        /// <param name="legNode">The HTML node containing the leg subsection information</param>
        /// <param name="isOutbound">True if this is an outbound leg, false for return leg</param>
        /// <returns>Parsed TradeLeg object, or null if parsing fails</returns>
        public static TradeLeg? ParseLeg(HtmlNode legNode, bool isOutbound)
        {
            if (legNode == null)
            {
                Logger.Warning("ParseLeg: Input leg node is null");
                return null;
            }

            try
            {
                var leg = new TradeLeg();

                // Parse commodity information for this leg
                ParseLegCommodityInformation(legNode, leg);

                // Parse leg-level information (profit, distances, timestamps)
                ParseLegRouteInformation(legNode, leg); //move this function higher

                // Parse profit information from the leg's profit section
                ParseLegProfitInformation(legNode, leg); //move this function higher

                return leg;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseLeg: Exception parsing {(isOutbound ? "outbound" : "return")} leg: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses commodity information from a leg node, using the new subsection-based approach.
        /// </summary>
        /// <param name="legNode">The leg node containing commodity information</param>
        /// <param name="leg">The TradeLeg object to populate</param>
        private static void ParseLegCommodityInformation(HtmlNode legNode, TradeLeg leg)
        {
            try
            {
                // Locate buy and sell subsections within the leg node
                var sellSection = legNode.SelectSingleNode(".//div[contains(@class,'traderouteboxfromright')]");
                var buySection = legNode.SelectSingleNode(".//div[contains(@class,'traderouteboxtoleft')]");

                // Parse buy commodity from buy section
                if (buySection != null)
                {
                    leg.BuyCommodity = ParseCommodityFromSubsection(buySection, "buy");
                }
                else
                {
                    Logger.Warning("ParseLegCommodityInformation: Buy section not found in leg");
                }

                // Parse sell commodity from sell section
                if (sellSection != null)
                {
                    leg.SellCommodity = ParseCommodityFromSubsection(sellSection, "sell");
                }
                else
                {
                    Logger.Warning("ParseLegCommodityInformation: Sell section not found in leg");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseLegCommodityInformation: Exception: {ex.Message}");
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
                // Parse profit per unit for this leg - targeting the biggest span in traderouteboxprofit
                var profitNode = legNode.SelectSingleNode(".//div[@class='traderouteboxprofit']//span[contains(@class,'biggest')]");
                if (profitNode != null)
                {
                    var profitText = GetSafeInnerText(profitNode);
                    leg.ProfitPerUnit = ParseInt(profitText);
                }

                // Parse route distance for this leg
                var routeDistanceNode = legNode.SelectSingleNode(".//div[text()='Route distance']/following-sibling::div");
                if (routeDistanceNode != null)
                {
                    var distanceText = GetSafeInnerText(routeDistanceNode);
                    leg.RouteDistance = ParseDoubleLy(distanceText);
                }

                // Parse last updated timestamp for this leg
                var updatedNode = legNode.SelectSingleNode(".//div[text()='Updated']/following-sibling::div");
                if (updatedNode != null)
                {
                    leg.LastUpdate = GetSafeInnerText(updatedNode);
                }

                Logger.Debug($"Parsed leg route info: Profit: {leg.ProfitPerUnit}, Distance: {leg.RouteDistance} Ly, Updated: {leg.LastUpdate}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseLegRouteInformation: Exception: {ex.Message}");
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
                // Look for the profit section within this leg
                var profitSection = legNode.SelectSingleNode(".//div[contains(@class,'traderouteboxprofit')]");
                if (profitSection == null)
                {
                    Logger.Debug("ParseLegProfitInformation: No profit section found in leg");
                    return;
                }

                // Parse profit per unit from the leg's profit section
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

                Logger.Debug($"ParseLegProfitInformation: Extracted profit per unit: {leg.ProfitPerUnit}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseLegProfitInformation: Exception: {ex.Message}");
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
                // Look for common leg container patterns in INARA HTML structure
                // This may need adjustment based on actual HTML structure
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
                        Logger.Debug($"CollectLegContainers: Found {nodes.Count} containers with selector: {selector}");
                        break; // Use the first selector that finds containers
                    }
                }

                // If no specific leg containers found, try to split based on station links
                if (legContainers.Count == 0)
                {
                    var stationLinks = routeBox.SelectNodes(".//a[contains(@href,'/elite/station-market/')]");
                    if (stationLinks != null)
                    {
                        // For round trips, we expect pairs of station links to represent legs
                        // This is a fallback approach when specific leg containers aren't identified
                        Logger.Debug($"CollectLegContainers: Fallback to station link analysis. Found {stationLinks.Count} station links");

                        // Group station links into pairs to simulate leg containers
                        for (int i = 0; i < stationLinks.Count - 1; i += 2)
                        {
                            if (i + 1 < stationLinks.Count)
                            {
                                // Create a virtual leg container that encompasses both stations
                                var commonParent = FindCommonParent(stationLinks[i], stationLinks[i + 1]);
                                if (commonParent != null)
                                {
                                    legContainers.Add(commonParent);
                                }
                            }
                        }
                    }
                }

                Logger.Debug($"CollectLegContainers: Total leg containers found: {legContainers.Count}");
            }
            catch (Exception ex)
            {
                Logger.Warning($"CollectLegContainers: Exception collecting leg containers: {ex.Message}");
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
                Logger.Debug("CollectLegContainersV2: Starting deterministic leg container collection");

                // Define the four specific leg selectors in the expected order
                var legSelectors = new Dictionary<string, string>
                {
                    { "to-right", ".//div[contains(@class,'traderouteboxtoright')]"},
                    { "from-right", ".//div[contains(@class,'traderouteboxfromright')]"},
                    { "from-left", ".//div[contains(@class,'traderouteboxfromleft')]"},
                    { "to-left", ".//div[contains(@class,'traderouteboxtoleft')]"}
                };

                // Try to find all four leg types
                var foundLegs = new Dictionary<string, HtmlNode>();

                foreach (var legSelector in legSelectors)
                {
                    var legType = legSelector.Key;
                    var selector = legSelector.Value;

                    var legNode = routeBox.SelectSingleNode(selector);
                    if (legNode != null)
                    {
                        foundLegs[legType] = legNode;
                        Logger.Debug($"CollectLegContainersV2: Found {legType} leg node");
                    }
                    else
                    {
                        Logger.Debug($"CollectLegContainersV2: Missing {legType} leg node with selector: {selector}");
                    }
                }

                // Check if we have all four legs for a round trip
                if (foundLegs.ContainsKey("to-right") && foundLegs.ContainsKey("from-right") &&
                    foundLegs.ContainsKey("from-left") && foundLegs.ContainsKey("to-left"))
                {
                    // Full round trip - return in fixed order [to-right, from-right, from-left, to-left]
                    legContainers.Add(foundLegs["to-right"]);
                    legContainers.Add(foundLegs["from-right"]);
                    legContainers.Add(foundLegs["from-left"]);
                    legContainers.Add(foundLegs["to-left"]);

                    Logger.Info($"CollectLegContainersV2: Found complete round trip with 4 legs: [to-right, from-right, from-left, to-left]");
                }
                else if (foundLegs.ContainsKey("to-right") && foundLegs.ContainsKey("from-left"))
                {
                    // One-way route - return [to-right, from-right]
                    legContainers.Add(foundLegs["to-right"]);
                    legContainers.Add(foundLegs["from-left"]);

                    Logger.Info($"CollectLegContainersV2: Found one-way route with 2 legs: [to-right, from-right]");

                    // Log missing legs for debugging
                    if (!foundLegs.ContainsKey("from-right"))
                    {
                        Logger.Warning("CollectLegContainersV2: Expected from-left leg missing for potential round trip");
                    }
                    if (!foundLegs.ContainsKey("to-left"))
                    {
                        Logger.Warning("CollectLegContainersV2: Expected to-left leg missing for potential round trip");
                    }
                }
                else
                {
                    // Extensive logging when expected legs are missing
                    Logger.Warning("CollectLegContainersV2: Could not find minimum required legs for valid route");

                    if (!foundLegs.ContainsKey("to-right"))
                    {
                        Logger.Warning("CollectLegContainersV2: Critical - to-right leg missing. This is required for any valid route.");
                    }
                    if (!foundLegs.ContainsKey("from-right"))
                    {
                        Logger.Warning("CollectLegContainersV2: Critical - from-right leg missing. This is required for any valid route.");
                    }

                    // Log what we did find
                    foreach (var foundLeg in foundLegs)
                    {
                        Logger.Info($"CollectLegContainersV2: Found leg: {foundLeg.Key}");
                    }

                    // Fallback to original method if we can't find the expected structure
                    Logger.Info("CollectLegContainersV2: Falling back to original CollectLegContainers method");
                    return CollectLegContainers(routeBox);
                }

                Logger.Info($"CollectLegContainersV2: Successfully collected {legContainers.Count} leg containers in deterministic order");
            }
            catch (Exception ex)
            {
                Logger.Warning($"CollectLegContainersV2: Exception during deterministic leg collection: {ex.Message}");
                Logger.Info("CollectLegContainersV2: Falling back to original CollectLegContainers method due to exception");
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

                // Collect all parents of node1
                while (current != null)
                {
                    parents1.Add(current);
                    current = current.ParentNode;
                }

                // Find first common parent in node2's ancestry
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
                Logger.Warning($"FindCommonParent: Exception finding common parent: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Parses outbound legs (indices 0 & 1) into the main TradeRoute object.
        /// </summary>
        /// <param name="outboundNodes">List of outbound leg nodes</param>
        /// <param name="route">TradeRoute object to populate</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        private static bool ParseOutboundLegs(List<HtmlNode> outboundNodes, TradeRoute route)
        {
            try
            {
                if (outboundNodes.Count != 2)
                {
                    Logger.Warning($"ParseOutboundLegs: Expected 2 outbound nodes, got {outboundNodes.Count}");
                    return false;
                }

                // Combine both outbound nodes for parsing
                var combinedContainer = CreateCombinedContainer(outboundNodes);

                // Parse commodity information for outbound legs
                ParseCommodityInformation(combinedContainer, route);

                // Parse route-level information (profit, distances, timestamps)
                ParseRouteInformation(combinedContainer, route); //move this function higher

                // Parse profit information from the profit section
                ParseProfitInformation(combinedContainer, route);//move this function higher

                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseOutboundLegs: Exception parsing outbound legs: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Parses inbound legs (indices 2 & 3) into a new TradeLeg object.
        /// </summary>
        /// <param name="inboundNodes">List of inbound leg nodes</param>
        /// <returns>Parsed TradeLeg object, or null if parsing fails</returns>
        private static TradeLeg? ParseInboundLegs(List<HtmlNode> inboundNodes)
        {
            try
            {
                if (inboundNodes.Count != 2)
                {
                    Logger.Warning($"ParseInboundLegs: Expected 2 inbound nodes, got {inboundNodes.Count}");
                    return null;
                }

                // Combine both inbound nodes for parsing
                var combinedContainer = CreateCombinedContainer(inboundNodes);

                // Parse the combined inbound legs using the existing ParseLeg method
                var returnLeg = ParseLeg(combinedContainer, false);

                return returnLeg;
            }
            catch (Exception ex)
            {
                Logger.Warning($"ParseInboundLegs: Exception parsing inbound legs: {ex.Message}");
                return null;
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
                // Create a virtual container that includes all the nodes
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
                Logger.Warning($"CreateCombinedContainer: Exception creating combined container: {ex.Message}");
                // Fallback to first node if combination fails
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
                // Check for the presence of specific leg container classes
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
                        Logger.Debug($"HasLegContainerStructure: Found leg container with selector: {selector}");
                        return true;
                    }
                }

                Logger.Debug("HasLegContainerStructure: No leg container structure found - assuming best-routes page");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning($"HasLegContainerStructure: Exception checking for leg containers: {ex.Message}");
                // Default to false (treat as best-routes page) on error
                return false;
            }
        }
        private static string CleanSpecialSymbols(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Удаляем невидимые символы, включая VARIATION SELECTOR и другие спецсимволы Unicode
            var cleaned = Regex.Replace(input, @"[\p{C}\p{M}\u200B-\u200D\uFE00-\uFE0F]", "");
            // \p{C} — управляющие и неотображаемые символы, \p{M} — комбинирующие символы
            //return CleanEnd(cleaned);
            return CleanEnd(Regex.Replace(cleaned, @"[^\w\s\-\.\,\:\;\(\)\[\]\{\}\&\#\@\!\?\+\=\/\\\'\""\|]", ""));
        }
        private static string CleanEnd(string input)
        {
            if (input == null) return null;
            return new string(input.TrimEnd().Reverse()
                .SkipWhile(c => char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol)
                .Reverse().ToArray());
        }
        #endregion
    }
}
