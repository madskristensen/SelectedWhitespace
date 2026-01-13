namespace SelectedWhitespace
{
    /// <summary>
    /// Constants for whitespace visualization.
    /// </summary>
    internal static class Constants
    {
        // Whitespace symbols
        public const char SpaceDot = '·';           // Middle dot for space (U+00B7)
        public const char TabArrow = '→';           // Rightwards arrow for tab (U+2192)
        public const string CrlfSymbol = "\\r\\n";  // Backslash-escaped representation
        public const string LfSymbol = "\\n";       // Backslash-escaped representation
        public const string CrSymbol = "\\r";       // Backslash-escaped representation

        // Tooltips for line endings
        public const string CrlfTooltip = "CRLF (Windows)";
        public const string LfTooltip = "LF (Unix/macOS)";
        public const string CrTooltip = "CR (Classic Mac)";

        // Default color for whitespace glyphs (medium gray)
        public const byte WhitespaceGrayLevel = 128;
        public const byte LineEndingOpacity = 140;  // More transparent (0-255)
        public const double LineEndingFontSizeOffset = -3.0;  // 3pt smaller
        public const double LineEndingLeftMargin = 7.0;  // Pixels to offset line endings from selection
    }
}
