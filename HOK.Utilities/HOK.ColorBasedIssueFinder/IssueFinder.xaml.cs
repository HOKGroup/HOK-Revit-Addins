using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.ColorBasedIssueFinder.IssueFinderLib;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Nice3point.Revit.Toolkit.External.Handlers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HOK.ColorBasedIssueFinder
{
    /// <summary>
    /// Interaction logic for IssueFinderWindow.xaml
    /// </summary>
    public partial class IssueFinder : Window
    {
        public string directoryPath;
        public string openFilePath;
        private UIApplication uiApp;
        private Document doc;
        private Bitmap bitmapImage;

        // Default error colours
        private System.Drawing.Color errorColor1 = System.Drawing.Color.FromArgb(127, 191, 255);
        private System.Drawing.Color errorColor2 = System.Drawing.Color.FromArgb(255, 127, 127);

        private readonly ActionEventHandler _externalHandler = new ActionEventHandler();
        private readonly AsyncEventHandler _asyncExternalHandler = new AsyncEventHandler();

        #region Helper Functions

        private void ExportWorldCoords(int pixelSize)
        {
            // Get the crop box for the current view
            var currView = GetActiveViewPlan(doc);

            // Get the UIView of the current doc
            UIView uiView = uiApp.ActiveUIDocument.GetOpenUIViews().FirstOrDefault(v => v.ViewId == currView.Id);

            XYZ cropMin, cropMax;

            var zoomCorners = uiView.GetZoomCorners();

            cropMin = zoomCorners[0];
            cropMax = zoomCorners[1];

            // Calculating scale
            int imageWidth = pixelSize;  // Adjust to match PixelSize above
            double worldWidth = cropMax.X - cropMin.X; // Real-world width
            double pixelSizeX = worldWidth / imageWidth; // A

            int imageHeight = (int)(imageWidth * (cropMax.Y - cropMin.Y) / (cropMax.X - cropMin.X));
            double pixelSizeY = (cropMin.Y - cropMax.Y) / imageHeight; // E (negative)

            // Ref point
            double c = cropMin.X + (pixelSizeX / 2);
            double f = cropMax.Y + (pixelSizeY / 2);

            // Generating the world file content
            string worldFileContent = $"{pixelSizeX}\n0.0\n0.0\n{pixelSizeY}\n{c}\n{f}";

            // Saving the world file
            if (directoryPath != "")
            {
                var outputPath = directoryPath + $"/{txtBxViewName.Content.ToString()}_revit_world.wld";
                File.WriteAllText(outputPath, worldFileContent);
            }
            else
            {
                MessageBox.Show("Please select a folder to export the world file to");
            }

        }
        private ViewPlan GetActiveViewPlan(Document doc)
        {
            return doc.ActiveView as ViewPlan;
        }

        public static IEnumerable<Rect> GetColorRectangles(Bitmap src, System.Drawing.Color color)
        {
            var rects = new List<Rect>();
            var points = new List<System.Windows.Point>();
            var srcRec = new System.Drawing.Rectangle(0, 0, src.Width, src.Height);
            var srcData = src.LockBits(srcRec, ImageLockMode.ReadOnly, src.PixelFormat);
            var srcBuff = new byte[srcData.Stride * srcData.Height];
            var pixSize = System.Drawing.Image.GetPixelFormatSize(src.PixelFormat) / 8;

            Marshal.Copy(srcData.Scan0, srcBuff, 0, srcBuff.Length);
            src.UnlockBits(srcData);

            Rect GetColorRectangle()
            {
                var curX = points.First().X;
                var curY = points.First().Y + 1;
                var maxX = points.Max(p => p.X);

                for (var y = curY; y < src.Height; y++)
                    for (var x = curX; x <= maxX; x++)
                    {
                        var pos = (y * srcData.Stride) + (x * pixSize);
                        var blue = srcBuff[(int)pos];
                        var green = srcBuff[(int)(pos + 1)];
                        var red = srcBuff[(int)(pos + 2)];

                        if (System.Drawing.Color.FromArgb(red, green, blue).ToArgb().Equals(color.ToArgb()))
                            points.Add(new System.Windows.Point(x, y));
                        else
                            break;
                    }

                var p1 = points.First();
                var p2 = points.Last();

                return new Rect(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
            }

            for (var y = 0; y < src.Height; y++)
            {
                for (var x = 0; x < src.Width; x++)
                {
                    var pos = (y * srcData.Stride) + (x * pixSize);
                    var blue = srcBuff[pos];
                    var green = srcBuff[pos + 1];
                    var red = srcBuff[pos + 2];

                    if (System.Drawing.Color.FromArgb(red, green, blue).ToArgb().Equals(color.ToArgb()))
                    {
                        var p = new System.Windows.Point(x, y);

                        if (!rects.Any(r => new Rect(r.X - 2, r.Y - 2,
                            r.Width + 4, r.Height + 4).Contains(p)))
                            points.Add(p);
                    }
                }

                if (points.Any())
                {
                    var rect = GetColorRectangle();
                    rects.Add(rect);
                    points.Clear();
                }
            }

            return rects;
        }
        #endregion

        public IssueFinder(UIApplication UiApp)
        {
            InitializeComponent();
            uiApp = UiApp;
            doc = uiApp.ActiveUIDocument.Document;
            txtBxViewName.Content = uiApp.ActiveUIDocument.ActiveView.Name;
            directoryPath = "";
            // Setting the default colors
            colPkErrorColor1.SelectedColor = new System.Windows.Media.Color() { R = errorColor1.R, G = errorColor1.G, B = errorColor1.B, A = 255};
            colPkErrorColor2.SelectedColor = new System.Windows.Media.Color() { R = errorColor2.R, G = errorColor2.G, B = errorColor2.B, A = 255 };
        }

        private void DirectoryBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFileDlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                directoryBox.Text = openFileDlg.SelectedPath;
            }
            directoryPath = directoryBox.Text;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if(directoryPath != "")
            {
                var viewExportOptions = new ImageExportOptions()
                {
                    ZoomType = ZoomFitType.Zoom,
                    Zoom = 100,
                    ExportRange = ExportRange.VisibleRegionOfCurrentView,
                    ShadowViewsFileType = ImageFileType.BMP,
                    HLRandWFViewsFileType = ImageFileType.BMP,
                    ImageResolution = ImageResolution.DPI_600,
                    FilePath = directoryPath + "/" + txtBxViewName.Content.ToString() + ".bmp"
                };
                uiApp.ActiveUIDocument.Document.ExportImage(viewExportOptions);

                // Get the final image width
                int pixelSize = 1;
                using (var fileStream = new FileStream(viewExportOptions.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var image = System.Drawing.Image.FromStream(fileStream, false, false))
                    {
                        pixelSize = image.Width;
                    }
                }
                ExportWorldCoords(pixelSize);
                MessageBox.Show("Export successful");
            }
            else
            {
                MessageBox.Show("Please select a folder to export the image to");
            }
        }

        private void directoryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            directoryPath = directoryBox.Text;
        }

        private void txtBxLoadImage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var openFileDlg = new System.Windows.Forms.OpenFileDialog();
            openFileDlg.Filter = "Bitmap Images|*.bmp|JSON Results File|*_results.json";
            openFileDlg.Multiselect = false;
            var result = openFileDlg.ShowDialog();
            if (result.ToString() != string.Empty)
            {
                txtBxLoadImage.Text = openFileDlg.FileName;
            }
            openFilePath = txtBxLoadImage.Text;
        }

        private void txtBxLoadImage_TextChanged(object sender, TextChangedEventArgs e)
        {
            openFilePath = txtBxLoadImage.Text;
            if(txtBxLoadImage.Text.EndsWith("_results.json"))
            {
                colPkErrorColor1.IsEnabled = false;
                colPkErrorColor2.IsEnabled = false;
                lblErrorColor1.Content = "Error Color 1 (Disabled)";
                lblErrorColor2.Content = "Error Color 2 (Disabled)";
            }
            else
            {
                colPkErrorColor1.IsEnabled = true;
                colPkErrorColor2.IsEnabled = true;
                lblErrorColor1.Content = "Error Color 1";
                lblErrorColor2.Content = "Error Color 2";
            }
        }

        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            // Ensure that the list is cleared first
            lstBxErrors.Items.Clear();
            if(openFilePath.EndsWith(".bmp"))
            {
                bitmapImage = (Bitmap)System.Drawing.Image.FromFile(openFilePath);
                WorldFileRevit wFile = new WorldFileRevit(openFilePath.Replace(".bmp", "_revit_world.wld"));
                // Get the rectangles for both error colors
                var errorRects1 = GetColorRectangles(bitmapImage, errorColor1);
                var errorRects2 = GetColorRectangles(bitmapImage, errorColor2);

                // Populate the listbox with ErrorAreas
                List<ErrorArea> errorAreas1 = new List<ErrorArea>();
                List<ErrorArea> errorAreas2 = new List<ErrorArea>();

                for (int i = 0; i < errorRects1.Count(); i++)
                {
                    errorAreas1.Add(new ErrorArea("Error Area1 " + i, errorRects1.ElementAt(i), errorColor1, wFile));
                }
                for (int i = 0; i < errorRects2.Count(); i++)
                {
                    errorAreas2.Add(new ErrorArea("Error Area2 " + i, errorRects2.ElementAt(i), errorColor2, wFile));
                }

                // Populate the list box
                foreach (var area in errorAreas1)
                {
                    lstBxErrors.Items.Add(area);
                }
                foreach (var area in errorAreas2)
                {
                    lstBxErrors.Items.Add(area);
                }

                bitmapImage.Dispose();
            }
            else if (openFilePath.EndsWith(".json"))
            {
                // Deserialize the json file to IssueFinderLib.Results and convert it to error areas
                WorldFileRevit wFile = new WorldFileRevit(openFilePath.Replace("_results.json", "_revit_world.wld"));
                var resultsJson = File.ReadAllText(openFilePath);
                var results = JsonConvert.DeserializeObject<Results>(resultsJson);
                List<ErrorArea> errorAreas = new List<ErrorArea>();

                // Go through the ErrorRects and convert them to ErrorAreas
                foreach (var errorRect in results.ErrorRects)
                {
                    errorAreas.Add(new ErrorArea(errorRect, wFile));
                }
                foreach (var area in errorAreas)
                {
                    lstBxErrors.Items.Add(area);
                }
                return;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("No errors found.");
                return;
            }
        }

        [RelayCommand]
        private async Task ShowErrorArea()
        {
            if (lstBxErrors.Items.Count < 1 || lstBxErrors.SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("Please select an error area from the list");
                return;
            }

            ErrorArea selectedArea = (ErrorArea)lstBxErrors.SelectedItem;

            // First, get the centerpoint of the rectangle of the selected item
            var selectedRect = selectedArea.Rectangle;
            var bottomLeft = selectedRect.BottomLeft; //Min of bounding box
            var topRight = selectedRect.TopRight; //Max of bounding box

            // Convert the cropBox Min and Max to Revit Coords
            var cropBoxMin = selectedArea.WorldFile.PixelToWorld(new System.Windows.Point(bottomLeft.X, bottomLeft.Y));
            var cropBoxMax = selectedArea.WorldFile.PixelToWorld(new System.Windows.Point(topRight.X, topRight.Y));

            await _asyncExternalHandler.RaiseAsync(application =>
            {
                // Set the view to the error area
                using (Transaction tr = new Transaction(doc, "Set View for " + doc.ActiveView.Name))
                {
                    tr.Start();
                    try
                    {
                        ViewPlan activeView = GetActiveViewPlan(doc);
                        bool cropBoxActive = activeView.CropBoxActive;
                        activeView.CropBoxActive = true;

                        var cropBox = activeView.CropBox;
                        if (cropBox == null)
                        {
                            TaskDialog.Show("Error", "CropBox is not available.");
                            return;
                        }
                        cropBox.Min = cropBoxMin;
                        cropBox.Max = cropBoxMax;
                        activeView.CropBox = cropBox;
                        UIView currUIView = uiApp.ActiveUIDocument.GetOpenUIViews().FirstOrDefault(v => v.ViewId == activeView.Id);
                        currUIView.ZoomToFit();
                        var zoomValue = Convert.ToDouble(txtBxZoomScale.Text.ToString());
                        currUIView.Zoom(1.0 / (zoomValue / 100.00));
                        if (!cropBoxActive)
                        {
                            activeView.CropBoxActive = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                        tr.RollBack();
                        return;
                    }
                    tr.Commit();
                }
            });

            


        }
        private void btnLoadErrorArea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task task = ShowErrorArea();
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void colPkErrorColor1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            // Set the color for error color 1
            errorColor1 = System.Drawing.Color.FromArgb(colPkErrorColor1.SelectedColor.Value.R, colPkErrorColor1.SelectedColor.Value.G, colPkErrorColor1.SelectedColor.Value.B);
        }

        private void colPkErrorColor2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            // Set the color for error color 2
            errorColor2 = System.Drawing.Color.FromArgb(colPkErrorColor2.SelectedColor.Value.R, colPkErrorColor2.SelectedColor.Value.G, colPkErrorColor2.SelectedColor.Value.B);
        }

        private void txtBxZoomScale_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
