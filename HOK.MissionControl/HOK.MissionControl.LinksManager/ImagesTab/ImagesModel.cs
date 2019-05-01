using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.ElementWrapers;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;

namespace HOK.MissionControl.LinksManager.ImagesTab
{
    public class ImagesModel
    {
        private readonly Document _doc;
        public ObservableCollection<ImageTypeWrapper> Images { get; set; }

        public ImagesModel(Document doc)
        {
            _doc = doc;
            CollectImages();
        }

        /// <summary>
        /// Deletes selected images from Model.
        /// </summary>
        /// <param name="images">Images to process.</param>
        /// <returns>List of deleted images.</returns>
        public List<ImageTypeWrapper> Delete(List<ImageTypeWrapper> images)
        {
            var deleted = new List<ImageTypeWrapper>();
            using (var trans = new Transaction(_doc, "Delete Images"))
            {
                trans.Start();
                StatusBarManager.InitializeProgress("Deleting Images:", images.Count);

                foreach (var image in images)
                {
                    StatusBarManager.StepForward();
                    try
                    {
                        _doc.Delete(image.Id);
                        deleted.Add(image);
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
        /// Collects all image types in the model.
        /// </summary>
        private void CollectImages()
        {
            var images = new Dictionary<ElementId, ImageTypeWrapper>();
            var allImageTypeIds = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImageType))
                .WhereElementIsElementType()
                .ToElementIds()
                .ToList();

            foreach (var id in allImageTypeIds)
            {
                var instances = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_RasterImages)
                    .Count(x => x.GetTypeId() == id);

                if (!images.ContainsKey(id))
                {
                    var type = (ImageType)_doc.GetElement(id);
                    if (type == null) continue;

                    var wrapper = new ImageTypeWrapper(type)
                    {
                        Instances = instances
                    };
                    images.Add(id, wrapper);
                }
                else
                {
                    var wrapper = images[id];
                    wrapper.Instances = wrapper.Instances + 1;
                    images[id] = wrapper;
                }
            }

            Images = new ObservableCollection<ImageTypeWrapper>(images.Values);
        }
    }
}
