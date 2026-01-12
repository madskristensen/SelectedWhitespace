using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Outlining;

namespace SelectedWhitespace
{
    /// <summary>
    /// Renders whitespace characters (spaces, tabs, line endings) only within selected text.
    /// Uses the same color as VS's built-in "Visible Whitespace" setting.
    /// </summary>
    internal sealed class SelectionWhitespaceAdornment
    {
        // Unicode characters matching VS's built-in whitespace visualization
        private const char SpaceDot = '·';      // Middle dot for space (U+00B7)
        private const char TabArrow = '→';      // Rightwards arrow for tab (U+2192)
        private const char CrlfSymbol = '↲';    // Downwards arrow with tip leftwards for CRLF (U+21B2)
        private const char LfSymbol = '↓';      // Downwards arrow for LF (U+2193)
        private const char CrSymbol = '←';      // Leftwards arrow for CR (U+2190)

        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _layer;
        private readonly IOutliningManager _outliningManager;
        private Brush _whitespaceBrush;
        private Typeface _typeface;

        public SelectionWhitespaceAdornment(IWpfTextView view, IOutliningManager outliningManager)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _layer = view.GetAdornmentLayer("SelectionWhitespaceAdornment");
            _outliningManager = outliningManager;

            UpdateWhitespaceBrush();

            _view.Selection.SelectionChanged += OnSelectionChanged;
            _view.LayoutChanged += OnLayoutChanged;
            _view.Closed += OnViewClosed;
        }

        private void UpdateWhitespaceBrush()
        {
            // Use a light-medium gray that's visible on both light and dark themes
            _whitespaceBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            _whitespaceBrush.Freeze();

            // Get the editor's typeface
            var textProperties = _view.FormattedLineSource?.DefaultTextProperties;
            if (textProperties != null)
            {
                _typeface = textProperties.Typeface;
            }
            else
            {
                _typeface = new Typeface("Consolas");
            }
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            _view.Selection.SelectionChanged -= OnSelectionChanged;
            _view.LayoutChanged -= OnLayoutChanged;
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

            if (_view.Selection.IsEmpty)
                return;

            // Update typeface if needed
            var textProperties = _view.FormattedLineSource?.DefaultTextProperties;
            if (textProperties != null)
            {
                _typeface = textProperties.Typeface;
            }

            foreach (var span in _view.Selection.SelectedSpans)
            {
                DrawWhitespaceInSpan(span);
            }
        }

        private void DrawWhitespaceInSpan(SnapshotSpan selectionSpan)
        {
            var snapshot = selectionSpan.Snapshot;
            var text = selectionSpan.GetText();

            // Get all collapsed regions that intersect with this selection span
            var collapsedRegions = _outliningManager?
                .GetCollapsedRegions(selectionSpan)
                .Select(r => r.Extent.GetSpan(snapshot))
                .ToList();

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                char? symbolChar = null;
                int charCount = 1;

                if (c == ' ')
                {
                    symbolChar = SpaceDot;
                }
                else if (c == '\t')
                {
                    symbolChar = TabArrow;
                }
                else if (c == '\r')
                {
                    // Check for CRLF
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        symbolChar = CrlfSymbol;
                        charCount = 2;
                    }
                    else
                    {
                        symbolChar = CrSymbol;
                    }
                }
                else if (c == '\n')
                {
                    symbolChar = LfSymbol;
                }

                if (symbolChar.HasValue)
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

                    DrawWhitespaceGlyph(charSpan, symbolChar.Value);

                    if (charCount == 2)
                    {
                        i++; // Skip the \n in CRLF
                    }
                }
            }
        }

        private void DrawWhitespaceGlyph(SnapshotSpan charSpan, char symbol)
        {
            var geometry = _view.TextViewLines.GetMarkerGeometry(charSpan);
            if (geometry == null)
                return;

            var bounds = geometry.Bounds;
            var fontSize = _view.FormattedLineSource?.DefaultTextProperties?.FontRenderingEmSize ?? 12;

            var textBlock = new TextBlock
            {
                Text = symbol.ToString(),
                FontFamily = _typeface.FontFamily,
                FontSize = fontSize,
                Foreground = _whitespaceBrush,
                // Center the glyph in the character cell
                TextAlignment = TextAlignment.Center,
                Width = bounds.Width,
                Height = bounds.Height
            };

            Canvas.SetLeft(textBlock, bounds.Left);
            Canvas.SetTop(textBlock, bounds.Top);

            _layer.AddAdornment(
                AdornmentPositioningBehavior.TextRelative,
                charSpan,
                null,
                textBlock,
                null);
        }
    }
}
