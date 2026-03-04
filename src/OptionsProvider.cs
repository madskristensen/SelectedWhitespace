using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SelectedWhitespace
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<WhitespaceOptions>
        {
        }
    }

    public class WhitespaceOptions : BaseOptionModel<WhitespaceOptions>
    {
        [Category("Symbols")]
        [DisplayName("Space symbol")]
        [Description("Character used to render spaces in selected text.")]
        [DefaultValue(Constants.SpaceSymbol)]
        public string SpaceSymbol { get; set; } = Constants.SpaceSymbol;

        [Category("Symbols")]
        [DisplayName("Tab symbol")]
        [Description("Character used to render tabs in selected text.")]
        [DefaultValue(Constants.TabSymbol)]
        public string TabSymbol { get; set; } = Constants.TabSymbol;

        [Category("Symbols")]
        [DisplayName("CRLF symbol")]
        [Description("Text used to render CRLF line endings.")]
        [DefaultValue(Constants.CrlfSymbol)]
        public string CrlfSymbol { get; set; } = Constants.CrlfSymbol;

        [Category("Symbols")]
        [DisplayName("LF symbol")]
        [Description("Text used to render LF line endings.")]
        [DefaultValue(Constants.LfSymbol)]
        public string LfSymbol { get; set; } = Constants.LfSymbol;

        [Category("Symbols")]
        [DisplayName("CR symbol")]
        [Description("Text used to render CR line endings.")]
        [DefaultValue(Constants.CrSymbol)]
        public string CrSymbol { get; set; } = Constants.CrSymbol;

        [Category("Appearance")]
        [DisplayName("Whitespace gray level")]
        [Description("Gray level used for whitespace glyph color (0-255).")]
        [DefaultValue(Constants.WhitespaceGrayLevel)]
        public byte WhitespaceGrayLevel { get; set; } = Constants.WhitespaceGrayLevel;

        [Category("Appearance")]
        [DisplayName("Whitespace opacity")]
        [Description("Opacity for space/tab glyphs (0-255).")]
        [DefaultValue(Constants.WhitespaceOpacity)]
        public byte WhitespaceOpacity { get; set; } = Constants.WhitespaceOpacity;

        [Category("Appearance")]
        [DisplayName("Line ending opacity")]
        [Description("Opacity for line-ending glyphs (0-255).")]
        [DefaultValue(Constants.LineEndingOpacity)]
        public byte LineEndingOpacity { get; set; } = Constants.LineEndingOpacity;

        [Category("Line endings")]
        [DisplayName("Show line endings in selection mode")]
        [Description("Controls rendering of line-ending glyphs when whitespace is shown only for selected text.")]
        [DefaultValue(Constants.ShowLineEndingMarkersInSelection)]
        public bool ShowLineEndingMarkersInSelection { get; set; } = Constants.ShowLineEndingMarkersInSelection;

        [Category("Line endings")]
        [DisplayName("Show line endings with View White Space")]
        [Description("Controls rendering of line-ending glyphs when Visual Studio's View White Space is enabled.")]
        [DefaultValue(Constants.ShowLineEndingMarkersWhenViewWhitespaceEnabled)]
        public bool ShowLineEndingMarkersWhenViewWhitespaceEnabled { get; set; } = Constants.ShowLineEndingMarkersWhenViewWhitespaceEnabled;

        [Category("Performance")]
        [DisplayName("Selection redraw delay (ms)")]
        [Description("Delay before redrawing selection whitespace after selection changes.")]
        [DefaultValue(Constants.SelectionRedrawDelayMilliseconds)]
        public int SelectionRedrawDelayMilliseconds { get; set; } = Constants.SelectionRedrawDelayMilliseconds;

        [Category("Performance")]
        [DisplayName("Maximum glyphs per redraw")]
        [Description("Hard cap on whitespace glyphs created in a single redraw. Use 0 to disable.")]
        [DefaultValue(Constants.MaximumGlyphsPerRedraw)]
        public int MaximumGlyphsPerRedraw { get; set; } = Constants.MaximumGlyphsPerRedraw;

        [Category("Performance")]
        [DisplayName("Maximum file length")]
        [Description("Skip whitespace rendering when snapshot length exceeds this value. Use 0 to disable.")]
        [DefaultValue(Constants.MaximumFileLengthForAdornmentRendering)]
        public int MaximumFileLengthForAdornmentRendering { get; set; } = Constants.MaximumFileLengthForAdornmentRendering;

        [Category("Performance")]
        [DisplayName("Maximum selection length")]
        [Description("Skip selection whitespace rendering when selected text length exceeds this value. Use 0 to disable.")]
        [DefaultValue(Constants.MaximumSelectionLengthForAdornmentRendering)]
        public int MaximumSelectionLengthForAdornmentRendering { get; set; } = Constants.MaximumSelectionLengthForAdornmentRendering;

        [Category("Whitespace runs")]
        [DisplayName("Show only multiple whitespace runs")]
        [Description("When enabled, only consecutive whitespace runs with at least the configured minimum length are rendered.")]
        [DefaultValue(Constants.ShowOnlyMultipleWhitespaceRuns)]
        public bool ShowOnlyMultipleWhitespaceRuns { get; set; } = Constants.ShowOnlyMultipleWhitespaceRuns;

        [Category("Whitespace runs")]
        [DisplayName("Minimum whitespace run length")]
        [Description("The minimum number of consecutive spaces/tabs required before a whitespace run is rendered.")]
        [DefaultValue(Constants.MinimumWhitespaceRunLength)]
        public int MinimumWhitespaceRunLength { get; set; } = Constants.MinimumWhitespaceRunLength;

        [Category("Context-aware filtering")]
        [DisplayName("Enable context-aware filtering")]
        [Description("When enabled, whitespace run visibility is controlled separately for indentation, inline, and trailing contexts.")]
        [DefaultValue(Constants.EnableContextAwareFiltering)]
        public bool EnableContextAwareFiltering { get; set; } = Constants.EnableContextAwareFiltering;

        [Category("Context-aware filtering")]
        [DisplayName("Show indentation whitespace runs")]
        [Description("Controls rendering of leading whitespace runs at the beginning of lines.")]
        [DefaultValue(Constants.ShowIndentationWhitespaceRuns)]
        public bool ShowIndentationWhitespaceRuns { get; set; } = Constants.ShowIndentationWhitespaceRuns;

        [Category("Context-aware filtering")]
        [DisplayName("Show inline whitespace runs")]
        [Description("Controls rendering of whitespace runs between non-whitespace characters in a line.")]
        [DefaultValue(Constants.ShowInlineWhitespaceRuns)]
        public bool ShowInlineWhitespaceRuns { get; set; } = Constants.ShowInlineWhitespaceRuns;

        [Category("Context-aware filtering")]
        [DisplayName("Show trailing whitespace runs")]
        [Description("Controls rendering of whitespace runs immediately before a line ending.")]
        [DefaultValue(Constants.ShowTrailingWhitespaceRuns)]
        public bool ShowTrailingWhitespaceRuns { get; set; } = Constants.ShowTrailingWhitespaceRuns;
    }
}
