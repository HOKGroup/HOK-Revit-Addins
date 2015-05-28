using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private ViewAnalysisManager analysisManager = null;
        private AnalysisSettings settings = new AnalysisSettings();

        public AnalysisSettings Settings { get { return settings; } set { settings = value; } }
        public MainWindow(UIApplication uiapp, List<Room> rooms )
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;
            selectedRooms = rooms;
            AbortFlag.SetAbortFlag(false);

            InitializeComponent();

            DisplaySettings();
            this.Title = "LEED EQc 8.2 - View Analysis v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void DisplaySettings()
        {
            try
            {
                settings = DataStorageUtil.ReadAnalysisSettings(m_doc);
                textBoxDataFile.Text = settings.DataFileName;
                sliderResolution.Value = (int)settings.Resolution;
                radioButtonParameter.IsChecked = settings.ExteriorWallByParameter;
                checkBoxOverwrite.IsChecked = settings.OverwriteData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display settings for View Analysis.\n"+ex.Message, "Display Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void buttonAnalysis_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SaveSettings())
                {
                    ViewAnalysisManager manager = new ViewAnalysisManager(m_app, selectedRooms, settings);
                    bool result = manager.RunViewAnalysis(progressBar, statusLable);
                    if (result)
                    {
                        this.DialogResult = true;
                    }
                }  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start View Analysis.\n"+ex.Message, "View Analysis - Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
                settings.OverwriteData = (bool)checkBoxOverwrite.IsChecked;

                if (DataStorageUtil.UpdateAnalysisSettings(m_doc, settings))
                {
                    saved = true;
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save analysis settings.\n" + ex.Message, "Save Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return saved;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Would you like to stop processing the View Analysis?", "Cancellation - View Analysis", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                AbortFlag.SetAbortFlag(true);
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

        public DataResolution Resolution { get { return resolution; } set { resolution = value; } }
        public double Interval { get { return interval; } set { interval = value; } }
        public string DataFileName { get { return dataFileName; } set { dataFileName = value; } }
        public bool OverwriteData { get { return overwriteData; } set { overwriteData = value; } }
        public bool ExteriorWallByParameter { get { return exteriorWallByParameter; } set { exteriorWallByParameter = value; } }

        public AnalysisSettings()
        {

        }

        public AnalysisSettings(AnalysisSettings settings)
        {
            this.Resolution = settings.Resolution;
            this.Interval = settings.Interval;
            this.DataFileName = settings.DataFileName;
            this.ExteriorWallByParameter = settings.ExteriorWallByParameter;
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
