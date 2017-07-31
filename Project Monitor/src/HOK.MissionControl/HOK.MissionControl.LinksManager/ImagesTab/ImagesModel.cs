using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;

namespace HOK.MissionControl.LinksManager.ImagesTab
{
    public class ImagesModel
    {
        private readonly Document _doc;
        public ObservableCollection<ImageWrapper> Images { get; set; }

        public ImagesModel(Document doc)
        {
            _doc = doc;
            CollectImages();
        }

        private void CollectImages()
        {
            var images = new Dictionary<ElementId, ImageWrapper>();

            var allPlacedImageIds = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_RasterImages)
                .Select(x => x.GetTypeId())
                .ToList();

            foreach (var id in allPlacedImageIds)
            {
                if (!images.ContainsKey(id))
                {
                    var type = (ImageType)_doc.GetElement(id);
                    if (type == null) continue;

                    var wrapper = new ImageWrapper
                    {
                        Name = type.Name,
                        FilePath = type.Path,
                        Instances = 1
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

            Images = new ObservableCollection<ImageWrapper>(images.Values);
        }
    }
}
