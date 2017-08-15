using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.ElementWrapers;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;

namespace HOK.MissionControl.LinksManager.ImportsTab
{
    public class ImportsModel
    {
        private readonly Document _doc;
        public ObservableCollection<CadLinkTypeWrapper> Imports { get; set; }

        public ImportsModel(Document doc)
        {
            _doc = doc;
            CollectImports();
        }

        /// <summary>
        /// Deletes all of the specified DWG imports.
        /// </summary>
        /// <param name="imports">DWGs to be deleted.</param>
        /// <returns>List of deleted DWGs.</returns>
        public List<CadLinkTypeWrapper> Delete(List<CadLinkTypeWrapper> imports)
        {
            var deleted = new List<CadLinkTypeWrapper>();
            using (var trans = new Transaction(_doc, "Delete Imports"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Deleting DWGs:", imports.Count);

                foreach (var import in imports)
                {
                    StatusBarManager.StepForward();
                    try
                    {
                        _doc.Delete(import.Id);
                        deleted.Add(import);
                    }
                    catch (Exception e)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
                    }
                }

                StatusBarManager.FinalizeProgress();
                trans.Commit();
            }

            return deleted;
        }

        /// <summary>
        /// Collects all of the imported DWGs.
        /// </summary>
        private void CollectImports()
        {
            var cadLinksDic = new FilteredElementCollector(_doc)
                .OfClass(typeof(CADLinkType))
                .Cast<CADLinkType>()
                .Select(x => new CadLinkTypeWrapper(x))
                .ToDictionary(key => key.Id, value => value);

            var imports = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .ToList();

            foreach (var ii in imports)
            {
                var id = ii.GetTypeId();
                if (cadLinksDic[id].Instances == 0)
                {
                    cadLinksDic[id].IsViewSpecific = ii.ViewSpecific;
                    cadLinksDic[id].IsLinked = ii.IsLinked;
                }
                cadLinksDic[id].Instances = cadLinksDic[id].Instances + 1;
            }

            Imports = new ObservableCollection<CadLinkTypeWrapper>(cadLinksDic.Values.OrderBy(x => x.Name));
        }
    }
}
