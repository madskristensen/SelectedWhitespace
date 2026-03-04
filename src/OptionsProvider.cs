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
