using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace SelectedWhitespace
{
    /// <summary>
    /// Listens for text view creation and instantiates the line ending whitespace adornment.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class LineEndingWhitespaceAdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            new LineEndingWhitespaceAdornment(textView);
        }
    }
}
