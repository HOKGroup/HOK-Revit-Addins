using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.KeynoteEditor.UserControls;
using HOK.KeynoteEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteEditor
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command:IExternalCommand
    {
        private UIApplication m_app = null;
        private Document m_doc = null;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                KeynotePanel keynoteWindow = new KeynotePanel();
                KeynoteConfiguration configuration = DataStorageUtil.GetConfiguration(m_doc);
                configuration.ProjectId = "4b5dcbfe-c438-4fb8-a9f9-cbc7537e8fc9";
                configuration.KeynoteSetId = "02234469-3d3d-44f3-9aea-0eb1a5d286f7";
                keynoteWindow.DataContext = new KeynoteViewModel(m_app, configuration);
                if ((bool)keynoteWindow.ShowDialog())
                {

                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return Result.Succeeded;
        }
    }
}
