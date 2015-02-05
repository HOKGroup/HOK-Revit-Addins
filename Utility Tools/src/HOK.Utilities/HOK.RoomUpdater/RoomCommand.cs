using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.RoomUpdater
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class RoomCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;

                RoomUpdaterWindow roomUpdater = new RoomUpdaterWindow(m_app);
                if (roomUpdater.ShowDialog() == true)
                {
                }

                List<Category> categoryList = new List<Category>();
                foreach (Category category in m_doc.Settings.Categories)
                {
                    if (category.HasMaterialQuantities)
                    {
#if RELEASE2015
                        if (category.CategoryType == CategoryType.Model)
                        {
                            categoryList.Add(category);
                        }
#else
                        categoryList.Add(category);
#endif
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to run Room Upater.\n" + ex.Message, "Room Updater", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return Result.Succeeded;
        }
    }
}
