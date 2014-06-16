using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;

namespace HOK.ModelManager.ReplicateViews
{
    public class ModelViewManager
    {
        private UIApplication m_app;

        public ModelViewManager(UIApplication uiapp)
        {
            m_app = uiapp;
        }
    }
}
