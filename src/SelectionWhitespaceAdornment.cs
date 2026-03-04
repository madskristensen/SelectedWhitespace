using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Outlining;

namespace SelectedWhitespace
{
    /// <summary>
    /// Renders whitespace characters (spaces, tabs, line endings) only within selected text.
    /// Automatically disables when VS's built-in "View White Space" is enabled.
    /// </summary>
    internal sealed class SelectionWhitespaceAdornment
    {
        private enum WhitespaceRunContext
        {
            Indentation,
            Inline,
            Trailing
        }

        private readonly IWpfTextView _view;
        private readonly IAdornmentLayer _layer;
        private readonly IOutliningManager _outliningManager;
        private readonly HashSet<int> _activeLineTags = new HashSet<int>();
        private readonly DispatcherTimer _selectionRedrawTimer;
        private Typeface _typeface;
        private Brush _whitespaceBrush = Brushes.Gray;
        private Brush _lineEndingBrush = Brushes.Gray;
        private int _remainingGlyphBudget;
        private string _spaceSymbol = Constants.SpaceSymbol;
        private string _tabSymbol = Constants.TabSymbol;
        private string _crlfSymbol = Constants.CrlfSymbol;
        private string _lfSymbol = Constants.LfSymbol;
        private string _crSymbol = Constants.CrSymbol;

        public SelectionWhitespaceAdornment(IWpfTextView view, IOutliningManager outliningManager)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _layer = view.GetAdornmentLayer(AdornmentLayers.SelectionWhitespace);
            _outliningManager = outliningManager;
            _selectionRedrawTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(Constants.SelectionRedrawDelayMilliseconds)
            };
            _selectionRedrawTimer.Tick += OnSelectionRedrawTick;

            UpdateTypeface();

            _view.Selection.SelectionChanged += OnSelectionChanged;
            _view.LayoutChanged += OnLayoutChanged;
            _view.Options.OptionChanged += OnOptionChanged;
            _view.Closed += OnViewClosed;
        }

        private void UpdateTypeface()
        {
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
            _selectionRedrawTimer.Stop();
            _selectionRedrawTimer.Tick -= OnSelectionRedrawTick;
            _view.Selection.SelectionChanged -= OnSelectionChanged;
            _view.LayoutChanged -= OnLayoutChanged;
            _view.Options.OptionChanged -= OnOptionChanged;
            _view.Closed -= OnViewClosed;
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ScheduleSelectionRedraw();
        }

        private void OnSelectionRedrawTick(object sender, EventArgs e)
        {
            _selectionRedrawTimer.Stop();
            RedrawAdornments();
        }

        private void ScheduleSelectionRedraw()
        {
            WhitespaceOptions options = WhitespaceOptions.Instance;
            var redrawDelay = options.SelectionRedrawDelayMilliseconds;
            if (redrawDelay <= 0)
            {
                RedrawAdornments();
                return;
            }

            _selectionRedrawTimer.Interval = TimeSpan.FromMilliseconds(redrawDelay);
            _selectionRedrawTimer.Stop();
            _selectionRedrawTimer.Start();
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (_view.Options.IsVisibleWhitespaceEnabled() || _view.Selection.IsEmpty)
                return;

            WhitespaceOptions options = WhitespaceOptions.Instance;
            if (ShouldSkipRendering(options))
            {
                _layer.RemoveAllAdornments();
                _activeLineTags.Clear();
                return;
            }

            UpdateRenderSettings(options);

            if (e.NewSnapshot != e.OldSnapshot)
            {
                RedrawAdornments();
                return;
            }

            if (e.NewOrReformattedLines.Count > 0)
            {
                _remainingGlyphBudget = GetGlyphBudget(options);
                PruneNonVisibleAdornments();

                foreach (ITextViewLine line in e.NewOrReformattedLines)
                {
                    if (_remainingGlyphBudget == 0)
                        break;

                    RefreshSelectionWhitespaceForLine(line, options);
                }
            }
        }

        private void RedrawAdornments()
        {
            _layer.RemoveAllAdornments();
            _activeLineTags.Clear();

            // Don't show selection whitespace when VS's built-in "View White Space" is enabled
            // (VS shows spaces/tabs, and LineEndingWhitespaceAdornment shows line endings)
            if (_view.Options.IsVisibleWhitespaceEnabled())
                return;

            if (_view.Selection.IsEmpty)
                return;

            WhitespaceOptions options = WhitespaceOptions.Instance;
            if (ShouldSkipRendering(options))
                return;

            UpdateTypeface();
            UpdateRenderSettings(options);
            _remainingGlyphBudget = GetGlyphBudget(options);

            foreach (SnapshotSpan span in _view.Selection.SelectedSpans)
            {
                if (_remainingGlyphBudget == 0)
                    break;

                DrawWhitespaceInSpan(span, options);
            }
        }

        private void PruneNonVisibleAdornments()
        {
            if (_activeLineTags.Count == 0)
                return;

            var visibleTags = new HashSet<int>();
            foreach (ITextViewLine line in _view.TextViewLines)
            {
                visibleTags.Add(GetLineTag(line));
            }

            var tagsToRemove = new List<int>();
            foreach (var tag in _activeLineTags)
            {
                if (!visibleTags.Contains(tag))
                {
                    tagsToRemove.Add(tag);
                }
            }

            foreach (var tag in tagsToRemove)
            {
                _layer.RemoveAdornmentsByTag(tag);
                _activeLineTags.Remove(tag);
            }
        }

        private void RefreshSelectionWhitespaceForLine(ITextViewLine line, WhitespaceOptions options)
        {
            var lineTag = GetLineTag(line);
            _layer.RemoveAdornmentsByTag(lineTag);
            _activeLineTags.Remove(lineTag);

            var lineSpan = line.ExtentIncludingLineBreak;
            foreach (SnapshotSpan selectedSpan in _view.Selection.SelectedSpans)
            {
                var intersection = selectedSpan.Intersection(lineSpan);
                if (intersection.HasValue)
                {
                    if (_remainingGlyphBudget == 0)
                        return;

                    DrawWhitespaceInSpan(intersection.Value, options);
                }
            }
        }

        private static int GetLineTag(ITextViewLine line)
        {
            return line.Start.Position;
        }

        private void DrawWhitespaceInSpan(SnapshotSpan selectionSpan, WhitespaceOptions options)
        {
            if (_remainingGlyphBudget == 0)
                return;

            ITextSnapshot snapshot = selectionSpan.Snapshot;
            var text = selectionSpan.GetText();

            // Get all collapsed regions that intersect with this selection span
            var collapsedRegions = _outliningManager?
                .GetCollapsedRegions(selectionSpan)
                ?.Select(r => r.Extent.GetSpan(snapshot))
                ?.OrderBy(r => r.Start.Position)
                ?.ToList();

            var collapsedRegionIndex = 0;
            var minimumWhitespaceRunLength = Math.Max(1, options.MinimumWhitespaceRunLength);

            for (var i = 0; i < text.Length; i++)
            {
                if (_remainingGlyphBudget == 0)
                    return;

                var c = text[i];
                if (c == ' ' || c == '\t')
                {
                    var runStart = i;
                    while (i + 1 < text.Length && (text[i + 1] == ' ' || text[i + 1] == '\t'))
                    {
                        i++;
                    }

                    var runLength = (i - runStart) + 1;
                    var runContext = GetWhitespaceRunContext(text, runStart, i);
                    if (!ShouldRenderWhitespaceRun(runContext, runLength, minimumWhitespaceRunLength, options))
                        continue;

                    for (var runIndex = runStart; runIndex <= i; runIndex++)
                    {
                        if (_remainingGlyphBudget == 0)
                            return;

                        var symbol = text[runIndex] == '\t'
                            ? _tabSymbol
                            : _spaceSymbol;

                        DrawWhitespaceGlyphAtPosition(snapshot, selectionSpan, collapsedRegions, ref collapsedRegionIndex, runIndex, symbol, isLineEnding: false);
                    }
                }
                else if (c == '\r' && options.ShowLineEndingMarkersInSelection)
                {
                    var symbol = _crSymbol;
                    var charCount = 1;

                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        symbol = _crlfSymbol;
                        charCount = 2;
                    }

                    DrawWhitespaceGlyphAtPosition(snapshot, selectionSpan, collapsedRegions, ref collapsedRegionIndex, i, symbol, isLineEnding: true);
                    if (charCount == 2)
                    {
                        i++;
                    }
                }
                else if (c == '\n' && options.ShowLineEndingMarkersInSelection)
                {
                    DrawWhitespaceGlyphAtPosition(snapshot, selectionSpan, collapsedRegions, ref collapsedRegionIndex, i, _lfSymbol, isLineEnding: true);
                }
            }
        }

        private static WhitespaceRunContext GetWhitespaceRunContext(string text, int runStart, int runEnd)
        {
            var isAtLineStart = runStart == 0 || text[runStart - 1] == '\r' || text[runStart - 1] == '\n';
            var isAtLineEnd = runEnd + 1 >= text.Length || text[runEnd + 1] == '\r' || text[runEnd + 1] == '\n';

            if (isAtLineEnd)
                return WhitespaceRunContext.Trailing;

            if (isAtLineStart)
                return WhitespaceRunContext.Indentation;

            return WhitespaceRunContext.Inline;
        }

        private static bool ShouldRenderWhitespaceRun(WhitespaceRunContext runContext, int runLength, int minimumWhitespaceRunLength, WhitespaceOptions options)
        {
            if (options.ShowOnlyMultipleWhitespaceRuns && runLength < minimumWhitespaceRunLength)
                return false;

            if (!options.EnableContextAwareFiltering)
                return true;

            switch (runContext)
            {
                case WhitespaceRunContext.Indentation:
                    return options.ShowIndentationWhitespaceRuns;

                case WhitespaceRunContext.Inline:
                    return options.ShowInlineWhitespaceRuns;

                case WhitespaceRunContext.Trailing:
                    return options.ShowTrailingWhitespaceRuns;

                default:
                    return true;
            }
        }

        private bool DrawWhitespaceGlyphAtPosition(
            ITextSnapshot snapshot,
            SnapshotSpan selectionSpan,
            List<SnapshotSpan> collapsedRegions,
            ref int collapsedRegionIndex,
            int indexInSelection,
            string symbol,
            bool isLineEnding)
        {
            var charPosition = selectionSpan.Start.Position + indexInSelection;

            if (IsInCollapsedRegion(collapsedRegions, ref collapsedRegionIndex, charPosition))
                return false;

            ITextViewLine textViewLine = _view.TextViewLines.GetTextViewLineContainingBufferPosition(
                new SnapshotPoint(snapshot, charPosition));
            if (textViewLine == null)
                return false;

            var lineTag = GetLineTag(textViewLine);

            if (isLineEnding && charPosition != textViewLine.End.Position)
                return false;

            var charSpan = new SnapshotSpan(snapshot, charPosition, 1);
            DrawWhitespaceGlyph(charSpan, symbol, isLineEnding, lineTag);
            _remainingGlyphBudget--;

            return true;
        }

        private static bool IsInCollapsedRegion(List<SnapshotSpan> collapsedRegions, ref int collapsedRegionIndex, int charPosition)
        {
            if (collapsedRegions == null || collapsedRegions.Count == 0)
                return false;

            while (collapsedRegionIndex < collapsedRegions.Count &&
                   collapsedRegions[collapsedRegionIndex].End.Position <= charPosition)
            {
                collapsedRegionIndex++;
            }

            if (collapsedRegionIndex >= collapsedRegions.Count)
                return false;

            SnapshotSpan region = collapsedRegions[collapsedRegionIndex];
            return charPosition >= region.Start.Position && charPosition < region.End.Position;
        }

        private void DrawWhitespaceGlyph(SnapshotSpan charSpan, string symbol, bool isLineEnding, int lineTag)
        {
            Geometry geometry = _view.TextViewLines.GetMarkerGeometry(charSpan);
            if (geometry == null)
                return;

            Rect bounds = geometry.Bounds;
            var baseFontSize = _view.FormattedLineSource?.DefaultTextProperties?.FontRenderingEmSize ?? 12;

            // For line endings, don't constrain width (multi-char symbols like "\r\n" need space)
            // For spaces/tabs, use character width to center the glyph
            // Never constrain height to avoid vertical clipping
            // Add left margin for line endings to offset from selection
            var lineEndingLeftMargin = isLineEnding
                ? Math.Max(0, baseFontSize * Constants.LineEndingLeftMarginScale)
                : 0;

            TextBlock textBlock = WhitespaceGlyphFactory.CreateGlyph(
                                            symbol,
                                            _typeface,
                                            baseFontSize,
                                            isLineEnding,
                                            isLineEnding ? _lineEndingBrush : _whitespaceBrush,
                                            width: isLineEnding ? null : bounds.Width,
                                            leftMargin: lineEndingLeftMargin);

            var top = bounds.Top + WhitespaceGlyphFactory.GetBaselineAlignmentOffset(baseFontSize, isLineEnding);

            Canvas.SetLeft(textBlock, bounds.Left);
            Canvas.SetTop(textBlock, top);

            _layer.AddAdornment(
                AdornmentPositioningBehavior.TextRelative,
                charSpan,
                lineTag,
                textBlock,
                null);

            _activeLineTags.Add(lineTag);
        }

        private bool ShouldSkipRendering(WhitespaceOptions options)
        {
            if (options.MaximumFileLengthForAdornmentRendering > 0 &&
                _view.TextSnapshot.Length > options.MaximumFileLengthForAdornmentRendering)
            {
                return true;
            }

            if (options.MaximumSelectionLengthForAdornmentRendering <= 0)
                return false;

            var totalSelectionLength = 0;
            foreach (SnapshotSpan span in _view.Selection.SelectedSpans)
            {
                totalSelectionLength += span.Length;
                if (totalSelectionLength > options.MaximumSelectionLengthForAdornmentRendering)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateRenderSettings(WhitespaceOptions options)
        {
            _whitespaceBrush = WhitespaceGlyphFactory.CreateWhitespaceBrush(options);
            _lineEndingBrush = WhitespaceGlyphFactory.CreateLineEndingBrush(options);

            _spaceSymbol = GetSymbolOrDefault(options.SpaceSymbol, Constants.SpaceSymbol);
            _tabSymbol = GetSymbolOrDefault(options.TabSymbol, Constants.TabSymbol);
            _crlfSymbol = GetSymbolOrDefault(options.CrlfSymbol, Constants.CrlfSymbol);
            _lfSymbol = GetSymbolOrDefault(options.LfSymbol, Constants.LfSymbol);
            _crSymbol = GetSymbolOrDefault(options.CrSymbol, Constants.CrSymbol);
        }

        private static string GetSymbolOrDefault(string symbol, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(symbol)
                ? defaultValue
                : symbol;
        }

        private static int GetGlyphBudget(WhitespaceOptions options)
        {
            return options.MaximumGlyphsPerRedraw <= 0
                ? int.MaxValue
                : options.MaximumGlyphsPerRedraw;
        }
    }
}
