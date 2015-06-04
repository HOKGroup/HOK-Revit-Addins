using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace HOK.ViewAnalysis
{
    public enum DataResolution
    {
        Low =0, Medium =1, High =2
    }

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UIApplication m_app;
        private Document m_doc;
        private List<Room> selectedRooms = new List<Room>();
        private AnalysisSettings settings = new AnalysisSettings();
        private AnalysisDataCollection analysisDataCollection = new AnalysisDataCollection();

        public AnalysisSettings Settings { get { return settings; } set { settings = value; } }
        public AnalysisDataCollection DataCollection { get { return analysisDataCollection; } set { analysisDataCollection = value; } }

        public MainWindow(UIApplication uiapp, List<Room> rooms )
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            selectedRooms = rooms;
            AbortFlag.SetAbortFlag(false);

            InitializeComponent();
            settings = DataStorageUtil.ReadAnalysisSettings(m_doc);

            DisplaySettings();
            this.Title = "LEED EQc 8.2 - View Analysis v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void DisplaySettings()
        {
            try
            {
                textBoxDataFile.Text = settings.DataFileName;
                sliderResolution.Value = (int)settings.Resolution;
                radioButtonParameter.IsChecked = settings.ExteriorWallByParameter;
                checkBoxLinkedModel.IsChecked = settings.IncludeLinkedModel;
                chekcBoxRecalculate.IsChecked = settings.OverwriteData;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to display settings for View Analysis.\n"+ex.Message, "Display Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonAnalysis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SaveSettings())
                {
                    ViewAnalysisManager manager = new ViewAnalysisManager(m_app, selectedRooms, settings, analysisDataCollection);
                    bool result = manager.RunViewAnalysis(progressBar, statusLable);
                    if (result)
                    {
                        if (SaveResultData(manager.RoomDictionary))
                        {
                            this.DialogResult = true;
                        }
                        
                    }
                }  
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to start View Analysis.\n"+ex.Message, "View Analysis - Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool SaveResultData(Dictionary<int,RoomData> roomDictionary)
        {
            bool saved = false;
            try
            {
                if (!string.IsNullOrEmpty(settings.DataFileName))
                {
                    analysisDataCollection = new AnalysisDataCollection();

                    foreach (int roomId in roomDictionary.Keys)
                    {
                        RoomData roomData = roomDictionary[roomId];
                        int index = analysisDataCollection.AnalysisDataList.FindIndex(o => o.RoomId == roomId);
                        if (settings.OverwriteData)
                        {
                            if (index > -1) { analysisDataCollection.AnalysisDataList.RemoveAt(index); }
                            AnalysisData aData = new AnalysisData();
                            aData.RoomId = roomData.RoomId;
                            aData.RoomArea = roomData.RoomArea;
                            aData.VisibleArea = roomData.AreaWithViews;
                            aData.RoomFace = FMEDataUtil.ConvertRevitFaceToFMEArea(roomData.RoomFace);
                            aData.PointValues = FMEDataUtil.ConvertToFMEPointList(roomData.PointDataList);
                            analysisDataCollection.AnalysisDataList.Add(aData);
                        }
                        else if(index == -1)
                        {
                            //add new room data only if it doesn't exist
                            AnalysisData aData = new AnalysisData();
                            aData.RoomId = roomData.RoomId;
                            aData.RoomArea = roomData.RoomArea;
                            aData.VisibleArea = roomData.AreaWithViews;
                            aData.RoomFace = FMEDataUtil.ConvertRevitFaceToFMEArea(roomData.RoomFace);
                            aData.PointValues = FMEDataUtil.ConvertToFMEPointList(roomData.PointDataList);
                            analysisDataCollection.AnalysisDataList.Add(aData);
                        }
                    }

                    if (analysisDataCollection.AnalysisDataList.Count > 0)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(AnalysisDataCollection));
                        StreamWriter writer = new StreamWriter(settings.DataFileName);
                        serializer.Serialize(writer, analysisDataCollection);
                        writer.Close();
                        saved = true;
                    }
                }
                else
                {
                    saved = true;
                }

            }
            catch (Exception ex)
            {
                saved = false;
                System.Windows.MessageBox.Show("Failed to save analysis data.\n" + ex.Message, "Save Result Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private bool SaveSettings()
        {
            bool saved = false;
            try
            {
                settings.DataFileName = textBoxDataFile.Text;

                double sliderVal = sliderResolution.Value;
                settings.Resolution = (DataResolution)sliderVal;
                switch (settings.Resolution)
                {
                    case DataResolution.Low:
                        settings.Interval = 2;
                        break;
                    case DataResolution.Medium:
                        settings.Interval = 1;
                        break;
                    case DataResolution.High:
                        settings.Interval = 0.5;
                        break;
                }

                settings.ExteriorWallByParameter = (bool)radioButtonParameter.IsChecked;
                settings.IncludeLinkedModel = (bool)checkBoxLinkedModel.IsChecked;
                settings.OverwriteData = (bool)chekcBoxRecalculate.IsChecked;

                if (DataStorageUtil.UpdateAnalysisSettings(m_doc, settings))
                {
                    saved = true;
                }
                
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to save analysis settings.\n" + ex.Message, "Save Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Would you like to stop processing the View Analysis?", "Cancellation - View Analysis", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                AbortFlag.SetAbortFlag(true);
            }
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "xml files (*.xml)|*.xml";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.Title = "Save Analysis Data";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    settings.DataFileName = saveFileDialog.FileName;
                    textBoxDataFile.Text = settings.DataFileName;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("A data file cannot be saved in the file path.\n" + ex.Message, "Save Data File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void textBoxDataFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxDataFile.Text))
                {
                    if (File.Exists(textBoxDataFile.Text))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(AnalysisDataCollection));
                        FileStream fs = new FileStream(textBoxDataFile.Text, FileMode.Open);
                        XmlReader reader = XmlReader.Create(fs);
                        if (serializer.CanDeserialize(reader))
                        {
                            analysisDataCollection = (AnalysisDataCollection)serializer.Deserialize(reader);
                        }
                        reader.Close();
                        fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Failed to read point data from the xml file.\n"+ex.Message, "Read Result Data File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }

    public class AnalysisSettings
    {
        private DataResolution resolution = DataResolution.Medium;
        private double interval = 1;
        private string dataFileName = "";
        private bool overwriteData = false;
        private bool exteriorWallByParameter = false;
        private bool includeLinkedModel = false;

        public DataResolution Resolution { get { return resolution; } set { resolution = value; } }
        public double Interval { get { return interval; } set { interval = value; } }
        public string DataFileName { get { return dataFileName; } set { dataFileName = value; } }
        public bool OverwriteData { get { return overwriteData; } set { overwriteData = value; } }
        public bool ExteriorWallByParameter { get { return exteriorWallByParameter; } set { exteriorWallByParameter = value; } }
        public bool IncludeLinkedModel { get { return includeLinkedModel; } set { includeLinkedModel = value; } }

        public AnalysisSettings()
        {

        }

        public AnalysisSettings(AnalysisSettings settings)
        {
            this.Resolution = settings.Resolution;
            this.Interval = settings.Interval;
            this.DataFileName = settings.DataFileName;
            this.ExteriorWallByParameter = settings.ExteriorWallByParameter;
            this.IncludeLinkedModel = settings.IncludeLinkedModel;
        }
    }

    public static class AbortFlag
    {
        private static bool abortFlag = false;

        public static bool GetAbortFlag()
        {
            return abortFlag;
        }

        public static void SetAbortFlag(bool abort)
        {
            abortFlag = abort;
        }

    }
}
