using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace InaraTools
{
    public static partial class InaraParserUtils
    {
        #region Text Cleaning and Parsing Methods

        public static string CleanNumber(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            try
            {
                var cleaned = Regex.Replace(text.Trim(), @"[,\sв‚№$в‚¬ВЈВҐв‚©\t\r\n]", "");
                cleaned = Regex.Replace(cleaned, @"(?i)\b(ly|cr|credits?|units?)\b", "");
                var match = Regex.Match(cleaned, @"-?\d+\.?\d*");
                if (match.Success)
                {
                    return match.Value;
                }

                Logger.Logger.Warning($"InaraParserUtils.CleanNumber: No numeric value found in text '{text}'");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Logger.Warning($"InaraParserUtils.CleanNumber: Failed to clean number from text '{text}': {ex.Message}");
                return string.Empty;
            }
        }

        public static int ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.Logger.LogNumericParsingFailure("ParseInt", text ?? "<null>", "Input is null or whitespace");
                return 0;
            }

            try
            {
                var cleanedText = CleanNumber(text);
                if (string.IsNullOrEmpty(cleanedText))
                {
                    Logger.Logger.LogNumericParsingFailure("ParseInt", text, "No numeric value found after cleaning");
                    return 0;
                }

                if (int.TryParse(cleanedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                {
                    return result;
                }

                if (double.TryParse(cleanedText, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleResult))
                {
                    if (doubleResult > int.MaxValue)
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseInt", text, $"Value {doubleResult} exceeds int.MaxValue");
                        return int.MaxValue;
                    }

                    if (doubleResult < int.MinValue)
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseInt", text, $"Value {doubleResult} below int.MinValue");
                        return int.MinValue;
                    }

                    return (int)Math.Round(doubleResult);
                }

                Logger.Logger.LogNumericParsingFailure("ParseInt", text, $"Failed to parse as int or double (cleaned: '{cleanedText}')");
                return 0;
            }
            catch (OverflowException ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseInt", text, $"Overflow exception: {ex.Message}");
                return 0;
            }
            catch (FormatException ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseInt", text, $"Format exception: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseInt", text, $"Unexpected exception: {ex.Message}");
                return 0;
            }
        }

        public static double ParseDoubleLy(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text ?? "<null>", "Input is null or whitespace");
                return 0.0;
            }

            try
            {
                var cleanedText = CleanNumber(text);
                if (string.IsNullOrEmpty(cleanedText))
                {
                    Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, "No numeric value found after cleaning");
                    return 0.0;
                }

                if (double.TryParse(cleanedText, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                {
                    if (double.IsPositiveInfinity(result))
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, "Value is positive infinity");
                        return double.MaxValue;
                    }

                    if (double.IsNegativeInfinity(result))
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, "Value is negative infinity");
                        return double.MinValue;
                    }

                    if (double.IsNaN(result))
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, "Value is NaN");
                        return 0.0;
                    }

                    if (result < 0)
                    {
                        Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Negative distance value: {result}");
                        return 0.0;
                    }

                    return result;
                }

                Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Failed to parse as double (cleaned: '{cleanedText}')");
                return 0.0;
            }
            catch (OverflowException ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Overflow exception: {ex.Message}");
                return 0.0;
            }
            catch (FormatException ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Format exception: {ex.Message}");
                return 0.0;
            }
            catch (Exception ex)
            {
                Logger.Logger.LogNumericParsingFailure("ParseDoubleLy", text, $"Unexpected exception: {ex.Message}");
                return 0.0;
            }
        }

        #endregion

        #region Helper Methods

        public static string GetSafeInnerText(HtmlNode? node)
        {
            return node?.InnerText?.Trim() ?? string.Empty;
        }

        public static string GetSafeAttribute(HtmlNode? node, string attributeName)
        {
            return node?.GetAttributeValue(attributeName, string.Empty) ?? string.Empty;
        }

        private static string CleanSpecialSymbols(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var cleaned = Regex.Replace(input, @"[\p{C}\p{M}\u200B-\u200D\uFE00-\uFE0F]", "");
            return CleanEnd(Regex.Replace(cleaned, @"[^\w\s\-\.\,\:\;\(\)\[\]\{\}\&\#\@\!\?\+\=\/\\\'\""\|]", ""));
        }

        private static string CleanEnd(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            return new string(input.TrimEnd().Reverse()
                .SkipWhile(c => char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol)
                .Reverse().ToArray());
        }

        #endregion
    }
}
