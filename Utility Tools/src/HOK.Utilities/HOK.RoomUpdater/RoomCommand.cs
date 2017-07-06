using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;

namespace HOK.RoomUpdater
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class RoomCommand : IExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                m_app = commandData.Application;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog("HOK.RoomUpdater.RoomCommand: Started.");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-RoomUpdater", m_doc));

                var roomUpdater = new RoomUpdaterWindow(m_app);
                if (roomUpdater.ShowDialog() == true)
                {
                }

                var categoryList = new List<Category>();
                foreach (Category category in m_doc.Settings.Categories)
                {
                    if (!category.HasMaterialQuantities) continue;

                    if (category.CategoryType == CategoryType.Model)
                    {
                        categoryList.Add(category);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog("HOK.RoomUpdater.RoomCommand: " + ex.Message);
            }
            Log.AppendLog("HOK.RoomUpdater.RoomCommand: Ended.");
            return Result.Succeeded;
        }
    }
}
