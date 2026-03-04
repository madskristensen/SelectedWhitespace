using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelectedWhitespace
{
    /// <summary>
    /// Helper for creating whitespace glyph adornments.
    /// </summary>
    internal static class WhitespaceGlyphFactory
    {
        public static Brush CreateWhitespaceBrush(WhitespaceOptions options)
        {
            byte grayLevel = options?.WhitespaceGrayLevel ?? Constants.WhitespaceGrayLevel;
            byte opacity = options?.WhitespaceOpacity ?? Constants.WhitespaceOpacity;

            var brush = new SolidColorBrush(Color.FromArgb(opacity, grayLevel, grayLevel, grayLevel));
            brush.Freeze();
            return brush;
        }

        public static Brush CreateLineEndingBrush(WhitespaceOptions options)
        {
            byte grayLevel = options?.WhitespaceGrayLevel ?? Constants.WhitespaceGrayLevel;
            byte opacity = options?.LineEndingOpacity ?? Constants.LineEndingOpacity;

            var brush = new SolidColorBrush(Color.FromArgb(opacity, grayLevel, grayLevel, grayLevel));
            brush.Freeze();
            return brush;
        }

        /// <summary>
        /// Creates a TextBlock for displaying a whitespace glyph.
        /// </summary>
        public static TextBlock CreateGlyph(
            string symbol,
            Typeface typeface,
            double baseFontSize,
            bool isLineEnding,
            Brush foreground,
            double? width = null,
            double? height = null,
            string tooltip = null,
            double leftMargin = 2)
        {
            var fontSize = baseFontSize;
            if (isLineEnding)
            {
                fontSize += Constants.LineEndingFontSizeOffset;
            }

            var textBlock = new TextBlock
            {
                Text = symbol,
                FontFamily = typeface.FontFamily,
                FontSize = fontSize,
                FontStyle = isLineEnding ? FontStyles.Italic : FontStyles.Normal,
                Foreground = foreground,
            };

            if (leftMargin > 0)
            {
                textBlock.Margin = new Thickness(leftMargin, 0, 0, 0);
            }

            if (width.HasValue)
            {
                textBlock.Width = width.Value;
                textBlock.TextAlignment = TextAlignment.Center;
            }

            if (height.HasValue)
            {
                textBlock.Height = height.Value;
            }

            if (tooltip != null)
            {
                textBlock.ToolTip = tooltip;
            }

            return textBlock;
        }

        /// <summary>
        /// Calculates the top offset to align a smaller font with the baseline.
        /// </summary>
        public static double GetBaselineAlignmentOffset(double baseFontSize, bool isLineEnding)
        {
            if (!isLineEnding)
                return 0;

            var fontSize = baseFontSize + Constants.LineEndingFontSizeOffset;
            return baseFontSize - fontSize;
        }
    }
}
