using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator.Tasks
{
    public class TaskAssistantModel
    {
        public FamilyItem Family { get; set; }
        public FamilyTask Task { get; set; }

        public TaskAssistantModel(FamilyItem family, FamilyTask task)
        {
            Family = family;
            Task = task;
        }

        public void SubmitEdits()
        {
        }

        public void EditFamily()
        {
            // (Konrad) We are in a modeless dialog context. The only way to talk back to
            // Revit is via Idling Event (or External Event but that's a different story).
            AppCommand.EnqueueTask(app =>
            {
                var doc = app.ActiveUIDocument.Document;
                if (doc == null) return;

                var family = new FilteredElementCollector(doc).OfClass(typeof(Family)).FirstOrDefault(x => x.Id.IntegerValue == Family.elementId);
                if (family == null) return;

                var famDoc = doc.EditFamily((Family)family);
            });
        }
    }
}
