using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Formatting;

namespace SelectedWhitespace
{
    /// <summary>
    /// Renders line ending characters throughout the document when VS's "View White Space" setting is enabled.
    /// VS only shows spaces and tabs by default - this extends it to show line endings too.
    /// </summary>
    internal sealed class LineEndingWhitespaceAdornment
    {
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _layer;
        private Brush _whitespaceBrush;
        private Typeface _typeface;
        private bool _isEnabled;

        public LineEndingWhitespaceAdornment(IWpfTextView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _layer = view.GetAdornmentLayer(AdornmentLayers.LineEndingWhitespace);

            UpdateBrushAndTypeface();

            // Check initial state
            _isEnabled = _view.Options.IsVisibleWhitespaceEnabled();

            _view.Options.OptionChanged += OnOptionChanged;
            _view.LayoutChanged += OnLayoutChanged;
            _view.Closed += OnViewClosed;

            if (_isEnabled)
            {
                RedrawAdornments();
                }
            }

            private void UpdateBrushAndTypeface()
            {
                // Use a medium gray with slight transparency for line endings
                _whitespaceBrush = new SolidColorBrush(Color.FromArgb(
                    Constants.LineEndingOpacity,
                    Constants.WhitespaceGrayLevel,
                    Constants.WhitespaceGrayLevel,
                    Constants.WhitespaceGrayLevel));
                _whitespaceBrush.Freeze();

                // Get the editor's typeface
                TextRunProperties textProperties = _view.FormattedLineSource?.DefaultTextProperties;
                _typeface = textProperties?.Typeface ?? new Typeface("Consolas");
            }

            private void OnOptionChanged(object sender, EditorOptionChangedEventArgs e)
            {
                if (e.OptionId == DefaultTextViewOptions.UseVisibleWhitespaceName)
                {
                    _isEnabled = _view.Options.IsVisibleWhitespaceEnabled();
                    RedrawAdornments();
                }
            }

        private void OnViewClosed(object sender, EventArgs e)
        {
            _view.Options.OptionChanged -= OnOptionChanged;
            _view.LayoutChanged -= OnLayoutChanged;
            _view.Closed -= OnViewClosed;
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (!_isEnabled)
                return;

            // Redraw if there are new or reformatted lines
            if (e.NewOrReformattedLines.Count > 0 || e.VerticalTranslation)
            {
                RedrawAdornments();
            }
        }

        private void RedrawAdornments()
        {
            _layer.RemoveAllAdornments();

            if (!_isEnabled)
                return;

            // Update typeface if needed
            TextRunProperties textProperties = _view.FormattedLineSource?.DefaultTextProperties;
            if (textProperties != null)
            {
                _typeface = textProperties.Typeface;
            }

            // Draw line endings for all visible lines
            foreach (ITextViewLine line in _view.TextViewLines)
            {
                DrawLineEndingsForLine(line);
            }
        }

        private void DrawLineEndingsForLine(ITextViewLine line)
        {
            ITextSnapshot snapshot = line.Snapshot;
            SnapshotPoint lineEnd = line.End;

            // Check if this line has a line break
            if (lineEnd.Position >= snapshot.Length)
                return;

            var nextChar = snapshot[lineEnd.Position];
            string symbol = null;
            string tooltip = null;

            if (nextChar == '\r')
            {
                // Check for CRLF
                if (lineEnd.Position + 1 < snapshot.Length && snapshot[lineEnd.Position + 1] == '\n')
                {
                    symbol = Constants.CrlfSymbol;
                    tooltip = Constants.CrlfTooltip;
                }
                else
                {
                    symbol = Constants.CrSymbol;
                    tooltip = Constants.CrTooltip;
                }
            }
            else if (nextChar == '\n')
            {
                symbol = Constants.LfSymbol;
                tooltip = Constants.LfTooltip;
            }

                if (symbol != null)
                {
                    DrawLineEndingGlyph(line, symbol, tooltip);
                }
                    }

                    private void DrawLineEndingGlyph(ITextViewLine line, string symbol, string tooltip)
                    {
                        // Position at the end of the line text
                        var left = line.TextRight;
                        var baseFontSize = _view.FormattedLineSource?.DefaultTextProperties?.FontRenderingEmSize ?? 12;
                        var fontSize = baseFontSize + Constants.LineEndingFontSizeOffset;

                        // Adjust top position to align baseline with the text
                        // The smaller font needs to be pushed down to align
                        var top = line.TextTop + (baseFontSize - fontSize);

                        var textBlock = new TextBlock
                        {
                            Text = symbol,
                            FontFamily = _typeface.FontFamily,
                            FontSize = fontSize,
                            FontStyle = FontStyles.Italic,
                            Foreground = _whitespaceBrush,
                            ToolTip = tooltip
                        };

                        Canvas.SetLeft(textBlock, left);
                        Canvas.SetTop(textBlock, top);

                        _layer.AddAdornment(
                            AdornmentPositioningBehavior.TextRelative,
                            line.Extent,
                            null,
                            textBlock,
                            null);
                    }
                }
            }
