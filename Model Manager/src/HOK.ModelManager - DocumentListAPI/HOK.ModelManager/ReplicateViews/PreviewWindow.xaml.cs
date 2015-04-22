using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Threading;

namespace HOK.ModelManager.ReplicateViews
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
     
        private List<PreviewMap> previewMapList = new List<PreviewMap>();
        private string tempFolder = "";
        private int pageNum = 0;
        private int totalPage = 0;
        private PreviewControl viewControl = null;
        private bool createSheet = false;

        public List<PreviewMap> PreviewMapList { get { return previewMapList; } set { previewMapList = value; } }
        public bool CreateSheet { get { return createSheet; } set { createSheet = value; } }

        public PreviewWindow(List<PreviewMap> previewMaps, bool sheetCreation)
        {
            previewMapList = previewMaps;
            createSheet = sheetCreation;
            InitializeComponent();
            pageNum++;
            totalPage = previewMaps.Count;
            tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ModelManager");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            DisplayPreviews(previewMapList[pageNum-1]);
        }

        private void DisplayPreviews(PreviewMap preview)
        {
            try
            {
                textBoxPage.Text = pageNum + " of " + totalPage;
                checkBoxSkip.IsChecked = false;
                if (!preview.IsEnabled) { checkBoxSkip.IsChecked = true; }
                buttonNext.IsEnabled = true;
                buttonBack.IsEnabled = true;
                if (pageNum == previewMapList.Count)
                {
                    buttonNext.IsEnabled = false;
                    buttonUpdateViews.IsEnabled = true;
                }
                if (pageNum == 1)
                {
                    buttonBack.IsEnabled = false;
                }

                imageRecipient.Source = null;
                if (null != preview.RecipientModelInfo && null != preview.RecipientViewProperties)
                {
                    RecipientModelName.Content = preview.RecipientModelInfo.DocTitle;
                    RecipientView.Content = preview.RecipientViewProperties.ViewName;
                    RecipientSheetName.Content = (!string.IsNullOrEmpty(preview.RecipientViewProperties.SheetName)) ? preview.RecipientViewProperties.SheetName : "None";

                    if (!string.IsNullOrEmpty(preview.ViewLinkInfo.DestImagePath2))
                    {
                        imageRecipient.Source = new BitmapImage(new Uri(preview.ViewLinkInfo.DestImagePath2));
                    }
                }
                else if (null == preview.RecipientViewProperties)
                {
                    RecipientModelName.Content = preview.RecipientModelInfo.DocTitle;
                    RecipientView.Content = "To Be Created";
                    if (createSheet && !string.IsNullOrEmpty(preview.SourceViewProperties.SheetName))
                    {
                        RecipientSheetName.Content = "To Be Created";
                    }
                    else
                    {
                        RecipientSheetName.Content = "None";
                    }
                }

                if (null != preview.SourceModelInfo && null != preview.SourceViewProperties)
                {
                    SourceModelName.Content = preview.SourceModelInfo.DocTitle;
                    SourceView.Content = preview.SourceViewProperties.ViewName;
                    SourceSheetName.Content = (!string.IsNullOrEmpty(preview.SourceViewProperties.SheetName)) ? preview.SourceViewProperties.SheetName : "None";

                    if (null != viewControl)
                    {
                        viewControl.Dispose();
                    }
                    viewControl = new PreviewControl(preview.SourceModelInfo.Doc, new ElementId(preview.SourceViewProperties.ViewId));
                    contentSource.Content = viewControl;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display preview Images.\n"+ex.Message, "Display Previews", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void buttonUpdateViews_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UpdateViews())
                {
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update views.\n"+ex.Message, "Update Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdateAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UpdateViews())
                {
                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update all drafting views.\n"+ex.Message, "Update All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private bool UpdateViews()
        {
            bool result = false;
            try
            {
                progressBar.Visibility = System.Windows.Visibility.Visible;
                statusLable.Visibility = System.Windows.Visibility.Visible;
                statusLable.Text = "Duplicating Views . . .";

                progressBar.Minimum = 0;
                progressBar.Maximum = previewMapList.Count;
                progressBar.Value = 0;

                double value = 0;

                UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);
                DuplicateUtils.errorMessage = new StringBuilder();
                List<PreviewMap> updatedList = new List<PreviewMap>();
                for (int i = 0; i <previewMapList.Count; i++)
                {
                    value += 1;
                    PreviewMap previewMap = DuplicateUtils.DuplicateView(previewMapList[i],createSheet);
                    updatedList.Add(previewMap);
                    Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                }

                if (DuplicateUtils.errorMessage.Length > 0)
                {
                    MessageBox.Show("Following drafting views contains problems.\n\n" + DuplicateUtils.errorMessage.ToString(), "Errors in Duplicating Views", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                previewMapList = new List<PreviewMap>();
                previewMapList = updatedList;
                //exportView one by one
                ImageExportOptions option = new ImageExportOptions();
                option.HLRandWFViewsFileType = ImageFileType.PNG;
                option.ImageResolution = ImageResolution.DPI_300;
                option.ShouldCreateWebSite = false;
                option.ExportRange = ExportRange.SetOfViews;

                List<ElementId> viewIds = new List<ElementId>();
                foreach (PreviewMap preview in previewMapList)
                {
                    viewIds.Clear();
                    if (null != preview.RecipientViewProperties)
                    {
                        if (preview.RecipientViewProperties.ViewId > 0)
                        {
                            string tempFileName = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "png");
                            string tempImageFile = System.IO.Path.Combine(tempFolder, tempFileName);

                            viewIds.Add(new ElementId(preview.RecipientViewProperties.ViewId));
                            option.FilePath = tempImageFile;
                            option.SetViewsAndSheets(viewIds);
                            if (ImageExportOptions.IsValidFileName(tempImageFile))
                            {
                                string imageFileName = ExportDraftingView(preview.RecipientModelInfo.Doc, option);

                                if (!string.IsNullOrEmpty(preview.ViewLinkInfo.DestImagePath1))
                                {
                                    if (File.Exists(preview.ViewLinkInfo.DestImagePath1))
                                    {
                                        File.Delete(preview.ViewLinkInfo.DestImagePath1);
                                    }
                                }
                                if (!string.IsNullOrEmpty(preview.ViewLinkInfo.DestImagePath2))
                                {
                                    preview.ViewLinkInfo.DestImagePath1 = preview.ViewLinkInfo.DestImagePath2;
                                }
                                preview.ViewLinkInfo.DestImagePath2 = imageFileName;
                            }
                        }
                    }
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update drafting views.\n" + ex.Message, "Update Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }


        private List<string> ExportDraftingView(Document recipientDoc, List<ElementId> viewIds)
        {
            List<string> fileNames = new List<string>();
            try
            {
                string tempFileName = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "png");
                string tempImageFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempFileName);

                ImageExportOptions option = new ImageExportOptions();
                option.FilePath = tempImageFile;
                option.HLRandWFViewsFileType = ImageFileType.PNG;
                option.ImageResolution = ImageResolution.DPI_300;
                option.ShouldCreateWebSite = false;
                option.SetViewsAndSheets(viewIds);
                option.ExportRange = ExportRange.SetOfViews;

                if (ImageExportOptions.IsValidFileName(tempImageFile))
                {
                    recipientDoc.ExportImage(option);
                }

                fileNames = Directory.GetFiles(System.IO.Path.GetTempPath(), string.Format("{0}*.*", System.IO.Path.GetFileNameWithoutExtension(tempFileName))).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export draftingView to image.\n" + ex.Message, "ExportDraftingView", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return fileNames;
        }

        private string ExportDraftingView(Document recipientDoc, ImageExportOptions option)
        {
            string fileName = "";
            try
            {
                recipientDoc.ExportImage(option);

                string[] fileNames = Directory.GetFiles(tempFolder, string.Format("{0}*.*", System.IO.Path.GetFileNameWithoutExtension(option.FilePath)));
                if (fileNames.Length > 0)
                {
                    fileName = fileNames[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to export draftingView to image.\n" + ex.Message, "ExportDraftingView", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return fileName;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = pageNum - 1;
                PreviewMap preview = previewMapList[index];
                if ((bool)checkBoxSkip.IsChecked) { preview.IsEnabled = false; }
                else { preview.IsEnabled = true; }
                
                previewMapList.RemoveAt(index);
                previewMapList.Insert(index, preview);

                pageNum++;
                preview = previewMapList[pageNum-1];
                DisplayPreviews(preview);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to move the page forward.\n"+ex.Message, "Move Page Forward", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = pageNum - 1;
                PreviewMap preview = previewMapList[index];
                if ((bool)checkBoxSkip.IsChecked) { preview.IsEnabled = false; }
                else { preview.IsEnabled = true; }

                previewMapList.RemoveAt(index);
                previewMapList.Insert(index, preview);

                pageNum--;
                preview = previewMapList[pageNum-1];
                DisplayPreviews(preview);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to move the page backward.\n"+ex.Message, "Move page Backward", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (null != viewControl)
            {
                viewControl.Dispose();
            }
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (null != viewControl)
            {
                viewControl.Dispose();
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //export image
                ImageExportOptions option = new ImageExportOptions();
                option.HLRandWFViewsFileType = ImageFileType.PNG;
                option.ImageResolution = ImageResolution.DPI_300;
                option.ShouldCreateWebSite = false;
                option.ExportRange = ExportRange.SetOfViews;

                List<ElementId> viewIds = new List<ElementId>();
                int index = pageNum - 1;
                PreviewMap preview = previewMapList[index];
                if (null != preview.RecipientViewProperties)
                {
                    if (preview.RecipientViewProperties.ViewId > 0)
                    {
                        string tempFileName = System.IO.Path.ChangeExtension(System.IO.Path.GetRandomFileName(), "png");
                        string tempImageFile = System.IO.Path.Combine(tempFolder, tempFileName);

                        viewIds.Add(new ElementId(preview.RecipientViewProperties.ViewId));
                        option.FilePath = tempImageFile;
                        option.SetViewsAndSheets(viewIds);
                        if (ImageExportOptions.IsValidFileName(tempImageFile))
                        {
                            string imageFileName = ExportDraftingView(preview.RecipientModelInfo.Doc, option);
                            if (!string.IsNullOrEmpty(preview.ViewLinkInfo.DestImagePath1))
                            {
                                if (File.Exists(preview.ViewLinkInfo.DestImagePath1))
                                {
                                    File.Delete(preview.ViewLinkInfo.DestImagePath1);
                                }
                            }
                            if (!string.IsNullOrEmpty(preview.ViewLinkInfo.DestImagePath2))
                            {
                                preview.ViewLinkInfo.DestImagePath1 = preview.ViewLinkInfo.DestImagePath2;
                            }
                            preview.ViewLinkInfo.DestImagePath2 = imageFileName;
                            imageRecipient.Source = new BitmapImage(new Uri(preview.ViewLinkInfo.DestImagePath2));
                        }
                    }
                    previewMapList.RemoveAt(index);
                    previewMapList.Insert(index, preview);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to refresh the preview image.\n"+ex.Message, "Refresh Preview Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
