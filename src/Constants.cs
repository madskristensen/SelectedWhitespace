namespace SelectedWhitespace
{
    /// <summary>
    /// Constants for whitespace visualization.
    /// </summary>
    internal static class Constants
    {
        // Unicode characters matching VS's built-in whitespace visualization
        public const char SpaceDot = '·';      // Middle dot for space (U+00B7)
        public const char TabArrow = '→';      // Rightwards arrow for tab (U+2192)
        public const char CrlfSymbol = '↲';    // Downwards arrow with tip leftwards for CRLF (U+21B2)
        public const char LfSymbol = '↓';      // Downwards arrow for LF (U+2193)
        public const char CrSymbol = '←';      // Leftwards arrow for CR (U+2190)

        // Tooltips for line endings
        public const string CrlfTooltip = "CRLF (Windows)";
        public const string LfTooltip = "LF (Unix/macOS)";
        public const string CrTooltip = "CR (Classic Mac)";

        // Default color for whitespace glyphs (medium gray)
        public const byte WhitespaceGrayLevel = 128;
    }
}
