using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Utilities.RoomUpdater;


namespace HOK.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class RoomCommand:IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                string dateOfInstalled = RegistryUtil.GetRegistryKey("InstalledOn");
                DateTime installed = DateTime.Parse(dateOfInstalled);
                DateTime expired = installed.AddYears(1);
                if (expired.CompareTo(DateTime.Now) > 0)
                {
                    RoomUpdaterWindow roomUpdater = new RoomUpdaterWindow(m_app);
                    if (roomUpdater.ShowDialog() == true)
                    {
                    }

                }
                else
                {
                    MessageBox.Show("Room Updater has been expired. Please contact HOK buildingSMART team.", "Room Updater Expired", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run Room Upater.\n"+ex.Message, "Room Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return Result.Succeeded;
        }
    }
}
