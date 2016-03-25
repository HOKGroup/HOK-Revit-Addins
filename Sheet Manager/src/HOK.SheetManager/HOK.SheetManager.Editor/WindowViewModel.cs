using HOK.SheetManager.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.SheetManager.Editor
{
    internal class WindowViewModel
    {
        private EditorViewModel editorViewModel;

        public EditorViewModel EditorView { get { return editorViewModel; } set { editorViewModel = value; } }

        public WindowViewModel()
        {
            editorViewModel = new EditorViewModel();
        }
    }
}
