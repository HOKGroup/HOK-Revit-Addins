using System;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Tools.Communicator
{
    public enum RequestId
    {
        None = 0,
        EditFamily = 1
    }

    public class CommunicatorRequest
    {
        private int _request = (int) RequestId.None;

        public RequestId Take()
        {
            return (RequestId) Interlocked.Exchange(ref _request, (int) RequestId.None);
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref _request, (int) request);
        }
    }

    public class CommunicatorRequestHandler : IExternalEventHandler
    {
        public FamilyItem FamilyItem { get; set; }

        public CommunicatorRequest Request { get; } = new CommunicatorRequest();

        public string GetName()
        {
            return "Task External Event";
        }

        public void Execute(UIApplication app)
        {
            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                    {
                        return;
                    }
                    case RequestId.EditFamily:
                    {
                        EditFamily(app);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Method to open family for editing.
        /// </summary>
        /// <param name="app">Current UI Application.</param>
        public void EditFamily(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            if (doc == null || doc.IsFamilyDocument) return;

            var family = new FilteredElementCollector(doc).OfClass(typeof(Family)).FirstOrDefault(x => x.Id.IntegerValue == FamilyItem.elementId);
            if (family == null) return;

            var famDoc = doc.EditFamily((Family)family);
            string filePath;
            var storedPath = famDoc.PathName;
            if (File.Exists(storedPath))
            {
                filePath = storedPath;
            }
            else
            {
                var myDocPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                filePath = Path.Combine(myDocPath, famDoc.Title);
                if(File.Exists(filePath)) File.Delete(filePath);
                famDoc.SaveAs(filePath);
            }

            famDoc.Close(false);
            app.OpenAndActivateDocument(
                ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath),
                new OpenOptions(),
                false);
        }
    }
}
