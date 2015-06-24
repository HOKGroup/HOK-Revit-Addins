using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.CameraDuplicator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class CameraCommand:IExternalCommand
    {
        private UIApplication m_app = null;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            if (m_app.Application.Documents.Size > 1)
            {
                CameraWindow cameraWindow = new CameraWindow(m_app);
                if (true == cameraWindow.ShowDialog())
                {
                    cameraWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("Please open more than two Revit documents before running this tool.\n A source model and a recipient model are required.", "Opened Revit Documents Required!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            

            return Result.Succeeded;
        }
    }

    public class CameraViewInfo
    {
        private int viewId = -1;
        private string viewName = "";
        private int linkedViewId = -1;
        private bool linked = false;
        private ViewOrientation3D orientation = null;
        private bool isCropBoxOn = false;
        private BoundingBoxXYZ cropBox = null;
        private bool isSectionBoxOn = false;
        private BoundingBoxXYZ sectionBox = null;
        private RenderingSettings rendering = null;
        private DisplayStyle display = DisplayStyle.Undefined;
       
        private Dictionary<string, Parameter> viewParameters = new Dictionary<string, Parameter>();
        private bool isSelected = false;
        
        public int ViewId { get { return viewId; } set { viewId = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }
        public int LinkedViewId { get { return linkedViewId; } set { linkedViewId = value; } }
        public bool Linked { get { return linked; } set { linked = value; } }
        public ViewOrientation3D Orientation { get { return orientation; } set { orientation = value; } }
        public bool IsCropBoxOn { get { return isCropBoxOn; } set { isCropBoxOn = value; } }
        public BoundingBoxXYZ CropBox { get { return cropBox; } set { cropBox = value; } }
        public bool IsSectionBoxOn { get { return isSectionBoxOn; } set { isSectionBoxOn = value; } }
        public BoundingBoxXYZ SectionBox { get { return sectionBox; } set { sectionBox = value; } }
        public RenderingSettings Rendering { get { return rendering; } set { rendering = value; } }
        public DisplayStyle Display { get { return display; } set { display = value; } }
        public Dictionary<string, Parameter> ViewParameters { get { return viewParameters; } set { viewParameters = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public CameraViewInfo(View3D view3d)
        {
            viewId = view3d.Id.IntegerValue;
            viewName = view3d.ViewName;
            orientation = view3d.GetOrientation();
            isCropBoxOn = view3d.CropBoxActive;
            cropBox = view3d.CropBox;
#if RELEASE2013
            sectionBox = view3d.SectionBox;
#elif RELEASE2014||RELEASE2015
            isSectionBoxOn = view3d.IsSectionBoxActive;
            sectionBox = view3d.GetSectionBox();
#endif

            rendering = view3d.GetRenderingSettings();
            display = view3d.DisplayStyle;
            GetParameters(view3d.Parameters);
        }

        public CameraViewInfo(CameraViewInfo vInfo)
        {
            this.ViewId = vInfo.ViewId;
            this.ViewName = vInfo.ViewName;
            this.LinkedViewId = vInfo.LinkedViewId;
            this.Orientation = vInfo.Orientation;
            this.CropBox = vInfo.CropBox;
            this.Rendering = vInfo.Rendering;
            this.Display = vInfo.Display;
            this.ViewParameters = vInfo.ViewParameters;
            this.IsSelected = vInfo.IsSelected;
        }

        private void GetParameters(ParameterSet parameters)
        {
            try
            {
                foreach (Parameter param in parameters)
                {
                    string paramName = param.Definition.Name;
                    if (paramName.Contains(".Extensions")) { continue; }
                    if (param.StorageType == StorageType.None) { continue; }
                    if (!viewParameters.ContainsKey(paramName) && param.HasValue)
                    {
                        viewParameters.Add(paramName, param);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

    public class CameraViewMap
    {
        private string sourceModelId = "";
        private int sourceViewId = -1;
        private string recipientModelId = "";
        private int recipientViewId = -1;

        public string SourceModelId { get { return sourceModelId; } set { sourceModelId = value; } }
        public int SourceViewId { get { return sourceViewId; } set { sourceViewId = value; } }
        public string RecipientModelId { get { return recipientModelId; } set { recipientModelId = value; } }
        public int RecipientViewId { get { return recipientViewId; } set { recipientViewId = value; } }

        public CameraViewMap()
        {
        }
    }

    public class ModelInfo
    {
        private string modelId = "";
        private string modelName = "";
        private Document modelDoc = null;
        private Dictionary<int/*viewId*/, CameraViewInfo> cameraViews = new Dictionary<int, CameraViewInfo>();

        public string ModelId { get { return modelId; } set { modelId = value; } }
        public string ModelName { get { return modelName; } set { modelName = value; } }
        public Document ModelDoc { get { return modelDoc; } set { modelDoc = value; } }
        public Dictionary<int, CameraViewInfo> CameraViews { get { return cameraViews; } set { cameraViews = value; } }

        public ModelInfo(Document doc, string id)
        {
            modelDoc = doc;
            modelName = doc.Title;
            modelId = id;

            GetCameraViews();
        }

        private void GetCameraViews()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(modelDoc);
                List<View3D> threeDViews = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                foreach (View3D view in threeDViews)
                {
                    if (view.ViewType != ViewType.ThreeD) { continue; }
                    if (view.IsTemplate) { continue; }
                    if (view.IsPerspective)
                    {
                        CameraViewInfo viewInfo = new CameraViewInfo(view);
                        if (!cameraViews.ContainsKey(viewInfo.ViewId))
                        {
                            cameraViews.Add(viewInfo.ViewId, viewInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get camera views.\n" + ex.Message, "Get Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
    }
}
