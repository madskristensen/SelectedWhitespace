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
        private static readonly Brush _whitespaceBrush;
        private static readonly Brush _lineEndingBrush;

        static WhitespaceGlyphFactory()
        {
            _whitespaceBrush = new SolidColorBrush(Color.FromRgb(
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel));
            _whitespaceBrush.Freeze();

            _lineEndingBrush = new SolidColorBrush(Color.FromArgb(
                Constants.LineEndingOpacity,
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel));
            _lineEndingBrush.Freeze();
        }

        public static Brush WhitespaceBrush => _whitespaceBrush;
        public static Brush LineEndingBrush => _lineEndingBrush;

        /// <summary>
        /// Creates a TextBlock for displaying a whitespace glyph.
        /// </summary>
        public static TextBlock CreateGlyph(
            string symbol,
            Typeface typeface,
            double baseFontSize,
            bool isLineEnding,
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
                Foreground = isLineEnding ? _lineEndingBrush : _whitespaceBrush,
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
