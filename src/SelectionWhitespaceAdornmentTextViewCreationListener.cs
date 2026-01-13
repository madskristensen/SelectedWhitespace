using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;

namespace SelectedWhitespace
{
    /// <summary>
    /// Listens for text view creation and instantiates the selection whitespace adornment.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class SelectionWhitespaceAdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Import]
        private IOutliningManagerService OutliningManagerService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            IOutliningManager outliningManager = OutliningManagerService?.GetOutliningManager(textView);
            new SelectionWhitespaceAdornment(textView, outliningManager);
        }
    }
}
