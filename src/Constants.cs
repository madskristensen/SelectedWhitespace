namespace SelectedWhitespace
{
    /// <summary>
    /// Constants for whitespace visualization.
    /// </summary>
    internal static class Constants
    {
        // Whitespace symbols
        public const string SpaceSymbol = "·";       // Middle dot for space (U+00B7)
        public const string TabSymbol = "→";         // Rightwards arrow for tab (U+2192)
        public const string CrlfSymbol = "CRLF";     // Carriage Return + Line Feed (Windows)
        public const string LfSymbol = "LF";         // Line Feed (Unix/macOS)
        public const string CrSymbol = "CR";         // Carriage Return (Classic Mac)

        // Tooltips for line endings
        public const string CrlfTooltip = "CRLF (Windows)";
        public const string LfTooltip = "LF (Unix/macOS)";
        public const string CrTooltip = "CR (Classic Mac)";

        // Default color for whitespace glyphs (medium gray)
        public const byte WhitespaceGrayLevel = 128;
        public const byte WhitespaceOpacity = 255;
        public const byte LineEndingOpacity = 140;  // More transparent (0-255)
        public const double LineEndingFontSizeOffset = -2.0;  // Slightly smaller than code text
        public const double LineEndingLeftMarginScale = 2.0 / 3.0;  // Scales with font size (12pt => 8px)

        // Redraw/performance safeguards
        public const int SelectionRedrawDelayMilliseconds = 40;
        public const bool ShowLineEndingMarkersInSelection = true;
        public const bool ShowLineEndingMarkersWhenViewWhitespaceEnabled = true;
        public const int MaximumGlyphsPerRedraw = 12000;
        public const int MaximumFileLengthForAdornmentRendering = 2_000_000;
        public const int MaximumSelectionLengthForAdornmentRendering = 200_000;

        // Whitespace filtering behavior
        public const bool ShowOnlyMultipleWhitespaceRuns = true;
        public const int MinimumWhitespaceRunLength = 2;

        // Context-aware whitespace filtering
        public const bool EnableContextAwareFiltering = true;
        public const bool ShowIndentationWhitespaceRuns = false;
        public const bool ShowInlineWhitespaceRuns = true;
        public const bool ShowTrailingWhitespaceRuns = true;
    }
}
