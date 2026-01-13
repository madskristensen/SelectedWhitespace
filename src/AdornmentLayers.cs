using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace SelectedWhitespace
{
    /// <summary>
    /// Defines the adornment layers used by this extension.
    /// </summary>
    internal static class AdornmentLayers
    {
        public const string SelectionWhitespace = "SelectionWhitespaceAdornment";
        public const string LineEndingWhitespace = "LineEndingWhitespaceAdornment";

#pragma warning disable 649 // Field is never assigned to

        /// <summary>
        /// Defines the adornment layer for selection whitespace.
        /// Ordered after selection so adornments appear on top.
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name(SelectionWhitespace)]
        [Order(After = PredefinedAdornmentLayers.Selection)]
        private static AdornmentLayerDefinition _selectionWhitespaceLayer;

        /// <summary>
        /// Defines the adornment layer for line ending whitespace.
        /// Ordered after text so adornments appear on top.
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name(LineEndingWhitespace)]
        [Order(After = PredefinedAdornmentLayers.Text)]
        private static AdornmentLayerDefinition _lineEndingWhitespaceLayer;

#pragma warning restore 649
    }
}
