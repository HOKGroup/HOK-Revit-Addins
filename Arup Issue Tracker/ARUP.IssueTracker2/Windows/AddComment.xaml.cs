using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ARUP.IssueTracker.Windows
{
    /// <summary>
    /// Interaction logic for AddComment.xaml
    /// </summary>
    public partial class AddComment : Window
    {
        public string snapshotFolder;
        public string snapshotFilePath;
        public string viewpointFilePath;
        public string attachmentFilePath;
        
        private ICommentController commentController;
        private bool isBcf = false;

        public AddComment(ICommentController commentController, bool isBcf)
        {          
            InitializeComponent();
            this.commentController = commentController;
            this.isBcf = isBcf;

            if (commentController != null) 
            {
                if (commentController.client == AuthoringTool.Revit)
                {                    
                    captureModelViewpointButton.Visibility = System.Windows.Visibility.Visible;
                    comboVisuals.ItemsSource = commentController.visuals;
                }
                else if (commentController.client == AuthoringTool.Navisworks)
                {
                    captureModelViewpointButton.Visibility = System.Windows.Visibility.Visible;
                }
                else if (commentController.client == AuthoringTool.None)
                {

                }
            }            

            snapshotFolder = Path.Combine(Path.GetTempPath(), "BCFtemp", Path.GetRandomFileName());
            if (!Directory.Exists(snapshotFolder))
                Directory.CreateDirectory(snapshotFolder);
            
        }
        private void OKBtnClick(object sender, RoutedEventArgs e)
        {
            if (comment.Text == "")
            {
                MessageBox.Show("Please write a comment or cancel.", "No comment", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
        }
        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void updateImage(string snapshotPath)
        {
            try
            {
                SnapshotImg.Source = null;
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.UriSource = new Uri(snapshotPath);
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                source.EndInit();
                SnapshotImg.Source = source;

                PathLabel.Content = "none";
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }

        }

        //LOAD EXTERNAL IMAGE
        private void Button_LoadImage(object sender, RoutedEventArgs e)
        {
            none.Visibility = System.Windows.Visibility.Collapsed;
            all.Visibility = System.Windows.Visibility.Collapsed;
            selected.Visibility = System.Windows.Visibility.Collapsed;
            comboVisuals.Visibility = System.Windows.Visibility.Collapsed;
            all.IsEnabled = false;
            selected.IsEnabled = false;
            none.IsEnabled = false;
            comboVisuals.IsEnabled = false;

            Microsoft.Win32.OpenFileDialog openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
            //  openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //openFileDialog1.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            openFileDialog1.RestoreDirectory = true;
            Nullable<bool> result = openFileDialog1.ShowDialog(); // Show the dialog.

            if (result == true) // Test result.
            {
                try
                {
                    // check overwrite
                    snapshotFilePath = Path.Combine(snapshotFolder, Path.GetFileName(openFileDialog1.FileName));
                    if (File.Exists(snapshotFilePath))
                    {
                        var overwriteFileResult = MessageBox.Show(string.Format("{0} already cached. Do you want to overwrite it?", Path.GetFileName(openFileDialog1.FileName)), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (overwriteFileResult == MessageBoxResult.No) 
                        {
                            return;
                        }
                    }

                    // check if it is an image
                    string extension = Path.GetExtension(snapshotFilePath).ToLower();
                    if (extension != ".jpg" && extension != ".jpeg" && extension != ".gif" && extension != ".bmp" && extension != ".png")
                    {
                        if (!isBcf)
                        {
                            // not an image actually
                            SaveImageData(File.ReadAllBytes(openFileDialog1.FileName), snapshotFilePath);
                            PathLabel.Content = openFileDialog1.FileName;
                            SnapshotImg.Source = null;
                            attachmentFilePath = snapshotFilePath;
                            snapshotFilePath = null;
                            return;
                        }
                        else 
                        {
                            SnapshotImg.Source = null;
                            attachmentFilePath = null;
                            snapshotFilePath = null;
                            MessageBox.Show("We only support image files for BCF!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }                        
                    }

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(openFileDialog1.FileName);
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    image.EndInit();
                    BitmapSource a = ConvertBitmapTo96DPI(image);
                    int width = (int)a.Width;
                    int height = (int)a.Height;
                    if (width > 1500 || image.Height > 1500)
                    {
                        string size = width.ToString() + "x" + height.ToString();
                        int newWidth = 1500;
                        float scale = (float)newWidth / ((float)width / (float)height);
                        int newHeight = Convert.ToInt32(scale);

                        MessageBoxResult answer = MessageBox.Show("Image size is " + size + ", "
                            + "such a big image could increase A LOT the BCF file size. "
                        + "Do you want me to resize it to " + newWidth.ToString() + "x" + newHeight.ToString() + " for you?", "Attention!",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (answer == MessageBoxResult.Yes)
                        {
                            width = newWidth;
                        }

                        /*
                        TaskDialog mainDialog = new TaskDialog("Attention!");
                        mainDialog.MainInstruction = "Attention!";
                        mainDialog.MainContent =
                            "Image size is " + size + ", "
                            + "such a big image could increase A LOT the BCF file size. "
                        + "Do you want me to resize it to " + newWidth.ToString() + "x" + newHeight.ToString() + " for you?";
                        mainDialog.CommonButtons = TaskDialogCommonButtons.No ^ TaskDialogCommonButtons.Yes;
                        mainDialog.DefaultButton = TaskDialogResult.Yes;
                        TaskDialogResult tResult = mainDialog.Show();
                        
                        //ONLY IF NECESSARY I RESIZE
                        if (TaskDialogResult.Yes == tResult)
                        {
                           width = newHeight;
                        }*/
                    }
                    byte[] imageBytes = LoadImageData(openFileDialog1.FileName);
                    ImageSource imageSource = CreateImage(imageBytes, width, 0);
                    imageBytes = GetEncodedImageData(imageSource, ".jpg");  
                    SaveImageData(imageBytes, snapshotFilePath);
                    //VISUALIZE IMAGE
                    BitmapImage source = new BitmapImage();
                    source.BeginInit();
                    source.UriSource = new Uri(snapshotFilePath);
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    source.EndInit();
                    SnapshotImg.Source = source;
                    PathLabel.Content = openFileDialog1.FileName;
                }
                catch (System.Exception ex1)
                {
                    MessageBox.Show("exception: " + ex1, "Error!");
                }
            }
        }

        public static BitmapSource ConvertBitmapTo96DPI(BitmapImage bitmapImage)
        {
            try
            {
                double dpi = 96;
                int width = bitmapImage.PixelWidth;
                int height = bitmapImage.PixelHeight;

                int stride = width * 4; // 4 bytes per pixel
                byte[] pixelData = new byte[stride * height];
                bitmapImage.CopyPixels(pixelData, stride, 0);

                return BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
            }

            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
            return null;
        }

        private static byte[] LoadImageData(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] imageBytes = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();
                return imageBytes;
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
            return null;
        }

        private static ImageSource CreateImage(byte[] imageData, int decodePixelWidth, int decodePixelHeight)
        {
            try
            {
                if (imageData == null) return null;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                if (decodePixelWidth > 0)
                {
                    result.DecodePixelWidth = decodePixelWidth;
                }
                if (decodePixelHeight > 0)
                {
                    result.DecodePixelHeight = decodePixelHeight;
                }
                result.StreamSource = new MemoryStream(imageData);
                result.CreateOptions = BitmapCreateOptions.None;
                result.CacheOption = BitmapCacheOption.Default;
                result.EndInit();
                return result;
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
            return null;
        }

        private static void SaveImageData(byte[] imageData, string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Create,
                FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(imageData);
                bw.Close();
                fs.Close();
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
        }

        internal byte[] GetEncodedImageData(ImageSource image, string preferredFormat)
        {
            try
            {
                byte[] result = null;
                BitmapEncoder encoder = null;
                switch (preferredFormat.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                        encoder = new JpegBitmapEncoder();
                        break;
                    case ".bmp":
                        encoder = new BmpBitmapEncoder();
                        break;
                    case ".png":
                        encoder = new PngBitmapEncoder();
                        break;
                    case ".tif":
                    case ".tiff":
                        encoder = new TiffBitmapEncoder();
                        break;
                    case ".gif":
                        encoder = new GifBitmapEncoder();
                        break;
                    case ".wmp":
                        encoder = new WmpBitmapEncoder();
                        break;
                }
                if (image is BitmapSource)
                {
                    MemoryStream stream = new MemoryStream();
                    encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
                    encoder.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    result = new byte[stream.Length];
                    BinaryReader br = new BinaryReader(stream);
                    br.Read(result, 0, (int)stream.Length);
                    br.Close();
                    stream.Close();
                }
                return result;
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
            return null;
        }

        private void EditSnapshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(snapshotFilePath))
                {
                    //edit snapshot
                    string editSnap = "mspaint";
                    Process paint = new Process();
                    // paint.Start(editSnap, "\"" + snapshot + "\"");
                    ProcessStartInfo paintInfo = new ProcessStartInfo(editSnap, "\"" + snapshotFilePath + "\"");

                    paintInfo.UseShellExecute = false;
                    //navisworksStartInfo.RedirectStandardOutput = true;

                    paint.StartInfo = paintInfo;
                    paint.Start();

                    paint.WaitForExit();
                    Refresh_Click(null, null);


                }
                else
                    MessageBox.Show("Snapshot is not a valit image, please try again.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage source = new BitmapImage();
                source.BeginInit();
                source.UriSource = new Uri(snapshotFilePath);
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                source.EndInit();
                SnapshotImg.Source = source;
            }
            catch (System.Exception ex1)
            {
                MessageBox.Show("exception: " + ex1, "Error!");
            }
        }

        public void captureModelViewpointButton_Click(object sender, RoutedEventArgs e)
        {
            none.Visibility = System.Windows.Visibility.Visible;
            all.Visibility = System.Windows.Visibility.Visible;
            selected.Visibility = System.Windows.Visibility.Visible;
            comboVisuals.Visibility = System.Windows.Visibility.Visible;
            all.IsEnabled = true;
            selected.IsEnabled = true;
            none.IsEnabled = true;
            comboVisuals.IsEnabled = true;
            if (commentController != null) 
            {
                int elemCheck = 2;
                if (all.IsChecked.Value)
                    elemCheck = 0;
                else if (selected.IsChecked.Value)
                    elemCheck = 1;
                var paths = commentController.getSnapshotAndViewpoint(elemCheck);
                snapshotFilePath = paths.Item1;
                viewpointFilePath = paths.Item2;
                updateImage(paths.Item1);
            }
        }

        private void comboVisuals_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (commentController != null)
            {
                commentController.comboVisuals_SelectionChanged(sender, e, this);
            }
        }

    }
}
