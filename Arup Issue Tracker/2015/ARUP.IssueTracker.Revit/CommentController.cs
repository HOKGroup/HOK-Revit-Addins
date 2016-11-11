using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARUP.IssueTracker.Windows;
using System.Windows;
using Autodesk.Revit.DB;
using System.IO;
using Autodesk.Revit.UI;
using ARUP.IssueTracker.Classes.BCF2;
using System.Xml.Serialization;
using System.Windows.Controls;

namespace ARUP.IssueTracker.Revit
{
    public class CommentController : ICommentController
    {
        private RevitWindow window;
        private Document doc;
        private UIDocument uidoc;

        public CommentController(RevitWindow window) 
        {
            this.window = window;
            this.uidoc = window.uiapp.ActiveUIDocument;
            this.doc = window.uiapp.ActiveUIDocument.Document;
        }

        public override Tuple<string, string> getSnapshotAndViewpoint(int elemCheck)
        {
            if (!(uidoc.ActiveView is View3D || uidoc.ActiveView is ViewSheet || uidoc.ActiveView is ViewPlan || uidoc.ActiveView is ViewSection || uidoc.ActiveView is ViewDrafting))
            {
                MessageBox.Show("I'm sorry,\nonly 3D and 2D views are supported.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string tempPath = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
            string issueFolder = Path.Combine(tempPath, Guid.NewGuid().ToString());
            if (!Directory.Exists(issueFolder))
                Directory.CreateDirectory(issueFolder);

            Guid viewpointGuid = Guid.NewGuid();
            string snapshotFilePath = Path.Combine(issueFolder, string.Format("{0}.png", viewpointGuid.ToString()));
            string viewpointFilePath = Path.Combine(issueFolder, string.Format("{0}.bcfv", viewpointGuid.ToString()));

            // save snapshot
            ImageExportOptions options = new ImageExportOptions();
            options.FilePath = snapshotFilePath;
            options.HLRandWFViewsFileType = ImageFileType.PNG;
            options.ShadowViewsFileType = ImageFileType.PNG;
            options.ExportRange = ExportRange.VisibleRegionOfCurrentView;
            options.ZoomType = ZoomFitType.FitToPage;
            options.ImageResolution = ImageResolution.DPI_72;
            options.PixelSize = 1000;
            doc.ExportImage(options);

            // save viewpoint
            VisualizationInfo vi = window.generateViewpoint(elemCheck);
            XmlSerializer serializerV = new XmlSerializer(typeof(VisualizationInfo));
            Stream writerV = new FileStream(viewpointFilePath, FileMode.Create);
            serializerV.Serialize(writerV, vi);
            writerV.Close();

            return new Tuple<string, string>(snapshotFilePath, viewpointFilePath);
        }
    
        public override void comboVisuals_SelectionChanged(object sender, SelectionChangedEventArgs e, AddComment addCommentWindow)
        {
            try
            {
                System.Windows.Controls.ComboBox comboVisuals = sender as System.Windows.Controls.ComboBox;
                if (doc.ActiveView.DisplayStyle.ToString() != comboVisuals.SelectedValue.ToString())
                {
                    switch (comboVisuals.SelectedIndex)
                    {
                        case 0:
                            doc.ActiveView.DisplayStyle = DisplayStyle.FlatColors;
                            break;
                        case 1:
                            doc.ActiveView.DisplayStyle = DisplayStyle.HLR;
                            break;
                        case 2:
                            doc.ActiveView.DisplayStyle = DisplayStyle.Realistic;
                            break;
                        case 3:
                            doc.ActiveView.DisplayStyle = DisplayStyle.RealisticWithEdges;
                            break;
                        case 4:
                            doc.ActiveView.DisplayStyle = DisplayStyle.Rendering;
                            break;
                        case 5:
                            doc.ActiveView.DisplayStyle = DisplayStyle.Shading;
                            break;
                        case 6:
                            doc.ActiveView.DisplayStyle = DisplayStyle.ShadingWithEdges;
                            break;
                        case 7:
                            doc.ActiveView.DisplayStyle = DisplayStyle.Wireframe;
                            break;
                        default:
                            break;
                    }

                }
                uidoc.RefreshActiveView();
                addCommentWindow.captureModelViewpointButton_Click(null, null);
            }
            catch (System.Exception ex1)
            {
                TaskDialog.Show("Error!", "exception: " + ex1);
            }
        }
}
}
