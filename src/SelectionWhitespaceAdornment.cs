using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Outlining;

namespace SelectedWhitespace
{
    /// <summary>
    /// Renders whitespace characters (spaces, tabs, line endings) only within selected text.
    /// Automatically disables when VS's built-in "View White Space" is enabled.
    /// </summary>
    internal sealed class SelectionWhitespaceAdornment
    {
        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _layer;
        private readonly IOutliningManager _outliningManager;
        private Brush _whitespaceBrush;
        private Brush _lineEndingBrush;
        private Typeface _typeface;

        public SelectionWhitespaceAdornment(IWpfTextView view, IOutliningManager outliningManager)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _layer = view.GetAdornmentLayer(AdornmentLayers.SelectionWhitespace);
            _outliningManager = outliningManager;

            UpdateWhitespaceBrush();

            _view.Selection.SelectionChanged += OnSelectionChanged;
            _view.LayoutChanged += OnLayoutChanged;
            _view.Options.OptionChanged += OnOptionChanged;
            _view.Closed += OnViewClosed;
        }

        private void UpdateWhitespaceBrush()
        {
            // Use a medium gray that's visible on both light and dark themes
            _whitespaceBrush = new SolidColorBrush(Color.FromRgb(
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel));
            _whitespaceBrush.Freeze();

            // Line endings are slightly transparent
            _lineEndingBrush = new SolidColorBrush(Color.FromArgb(
                Constants.LineEndingOpacity,
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel,
                Constants.WhitespaceGrayLevel));
            _lineEndingBrush.Freeze();

            // Get the editor's typeface
            TextRunProperties textProperties = _view.FormattedLineSource?.DefaultTextProperties;
            _typeface = textProperties?.Typeface ?? new Typeface("Consolas");
        }

        private void OnOptionChanged(object sender, EditorOptionChangedEventArgs e)
        {
            // When View White Space is toggled, redraw (or clear) adornments
            if (e.OptionId == DefaultTextViewOptions.UseVisibleWhitespaceName)
            {
                RedrawAdornments();
            }
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            _view.Selection.SelectionChanged -= OnSelectionChanged;
            _view.LayoutChanged -= OnLayoutChanged;
            _view.Options.OptionChanged -= OnOptionChanged;
            _view.Closed -= OnViewClosed;
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            RedrawAdornments();
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot || e.VerticalTranslation)
            {
                RedrawAdornments();
            }
        }

        private void RedrawAdornments()
        {
            _layer.RemoveAllAdornments();

            // Don't show selection whitespace when VS's built-in "View White Space" is enabled
            // (VS shows spaces/tabs, and LineEndingWhitespaceAdornment shows line endings)
            if (_view.Options.IsVisibleWhitespaceEnabled())
                return;

            if (_view.Selection.IsEmpty)
                return;

            // Update typeface if needed
            TextRunProperties textProperties = _view.FormattedLineSource?.DefaultTextProperties;
            if (textProperties != null)
            {
                _typeface = textProperties.Typeface;
            }

            foreach (SnapshotSpan span in _view.Selection.SelectedSpans)
            {
                DrawWhitespaceInSpan(span);
            }
        }

        private void DrawWhitespaceInSpan(SnapshotSpan selectionSpan)
        {
            ITextSnapshot snapshot = selectionSpan.Snapshot;
            var text = selectionSpan.GetText();

            // Get all collapsed regions that intersect with this selection span
            var collapsedRegions = _outliningManager?
                .GetCollapsedRegions(selectionSpan)
                .Select(r => r.Extent.GetSpan(snapshot))
                .ToList();

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                string symbol = null;
                var charCount = 1;
                var isLineEnding = false;

                if (c == ' ')
                {
                    symbol = Constants.SpaceDot.ToString();
                }
                else if (c == '\t')
                {
                    symbol = Constants.TabArrow.ToString();
                }
                else if (c == '\r')
                {
                    isLineEnding = true;
                    // Check for CRLF
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        symbol = Constants.CrlfSymbol;
                        charCount = 2;
                    }
                    else
                    {
                        symbol = Constants.CrSymbol;
                    }
                }
                else if (c == '\n')
                {
                    isLineEnding = true;
                    symbol = Constants.LfSymbol;
                }

                if (symbol != null)
                {
                    var charPosition = selectionSpan.Start.Position + i;

                    // Skip if this position is inside a collapsed region
                    if (collapsedRegions != null && collapsedRegions.Any(r => r.Contains(charPosition)))
                    {
                        if (charCount == 2)
                            i++;
                        continue;
                    }

                    var charSpan = new SnapshotSpan(snapshot, charPosition, 1);

                                        DrawWhitespaceGlyph(charSpan, symbol, isLineEnding);

                                        if (charCount == 2)
                                        {
                                            i++; // Skip the \n in CRLF
                                        }
                                    }
                                }
                            }

                            private void DrawWhitespaceGlyph(SnapshotSpan charSpan, string symbol, bool isLineEnding)
                            {
                                Geometry geometry = _view.TextViewLines.GetMarkerGeometry(charSpan);
                                if (geometry == null)
                                    return;

                                Rect bounds = geometry.Bounds;
                                var baseFontSize = _view.FormattedLineSource?.DefaultTextProperties?.FontRenderingEmSize ?? 12;
                                var fontSize = baseFontSize;

                                // Line endings are smaller and use transparent brush
                                if (isLineEnding)
                                {
                                    fontSize += Constants.LineEndingFontSizeOffset;
                                }

                                var textBlock = new TextBlock
                                {
                                    Text = symbol,
                                    FontFamily = _typeface.FontFamily,
                                    FontSize = fontSize,
                                    FontStyle = isLineEnding ? FontStyles.Italic : FontStyles.Normal,
                                    Foreground = isLineEnding ? _lineEndingBrush : _whitespaceBrush,
                                    // Center the glyph in the character cell
                                    TextAlignment = TextAlignment.Center,
                                    Width = bounds.Width,
                                    Height = bounds.Height
                                };

                                // Adjust top position for smaller font to align baseline
                                var top = isLineEnding ? bounds.Top + (baseFontSize - fontSize) : bounds.Top;

                                Canvas.SetLeft(textBlock, bounds.Left);
                                Canvas.SetTop(textBlock, top);

                                _layer.AddAdornment(
                                    AdornmentPositioningBehavior.TextRelative,
                                    charSpan,
                                    null,
                                    textBlock,
                                    null);
                            }
                        }
                    }
