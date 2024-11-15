using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using Nice3point.Revit.Toolkit.External;

namespace HOK.RoomUpdater
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class RoomCommand : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public override void Execute()
        {
            try
            {
                m_app = Context.UiApplication;
                m_doc = m_app.ActiveUIDocument.Document;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(new AddinLog("Utilities-RoomUpdater", Application.VersionNumber));

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
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            Log.AppendLog(LogMessageType.INFO, "Ended");
        }
    }
}
