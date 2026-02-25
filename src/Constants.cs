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
        public const string CrlfSymbol = "CRLF";    // Carriage Return + Line Feed (Windows)
        public const string LfSymbol = "LF";        // Line Feed (Unix/macOS)
        public const string CrSymbol = "CR";        // Carriage Return (Classic Mac)

        // Tooltips for line endings
        public const string CrlfTooltip = "CRLF (Windows)";
        public const string LfTooltip = "LF (Unix/macOS)";
        public const string CrTooltip = "CR (Classic Mac)";

        // Default color for whitespace glyphs (medium gray)
        public const byte WhitespaceGrayLevel = 128;
        public const byte LineEndingOpacity = 140;  // More transparent (0-255)
        public const double LineEndingFontSizeOffset = -2.0;  // Slightly smaller than code text
        public const double LineEndingLeftMargin = 8.0;  // Pixels to offset line endings from selection
    }
}
