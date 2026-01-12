using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;

namespace SelectedWhitespace
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class SelectionWhitespaceAdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        // Disable "Field is never assigned to..." compiler warning since the field is assigned by MEF
#pragma warning disable 649, IDE0044

        /// <summary>
        /// Defines the adornment layer for the whitespace adornment.
        /// This layer is ordered after the selection layer so our adornments appear on top.
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("SelectionWhitespaceAdornment")]
        [Order(After = PredefinedAdornmentLayers.Selection)]
        private AdornmentLayerDefinition _editorAdornmentLayer;

        /// <summary>
        /// Service to get outlining manager for detecting collapsed regions.
        /// </summary>
        [Import]
        private IOutliningManagerService OutliningManagerService { get; set; }

#pragma warning restore 649, IDE0044

        /// <summary>
        /// Called when a text view is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed.</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            var outliningManager = OutliningManagerService?.GetOutliningManager(textView);
            // Create the adornment - it will attach itself to the view's events
            new SelectionWhitespaceAdornment(textView, outliningManager);
        }
    }
}
