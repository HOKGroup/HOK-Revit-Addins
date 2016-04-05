using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.SmartBCF.AddIn.Util;
using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.UserControls;
using HOK.SmartBCF.Utils;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.SmartBCF.AddIn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WindowViewModel viewModel;
        private BCFHandler m_handler;
        private ExternalEvent m_event;
        
        public MainWindow(ExternalEvent exEvent, BCFHandler handler)
        {
            m_event = exEvent;
            m_handler = handler;
            viewModel = new WindowViewModel(m_event, m_handler);
            DataContext = viewModel;

            //get database file
            string databaseFile = DataStorageUtil.ReadLinkedDatabase(m_handler.ActiveDoc);
            if (!string.IsNullOrEmpty(databaseFile))
            {
                if (File.Exists(databaseFile))
                {
                    bool opened = viewModel.BCFView.OpenDatabase(databaseFile);
                }
            }

            InitializeComponent();
            this.Title = "SmartBCF AddIn v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != viewModel.ComponentView)
            {
                e.Cancel = true;
            }
            else
            {
                string databaseFile = viewModel.BCFView.DatabaseFile;
                if(!string.IsNullOrEmpty(databaseFile))
                {
                    m_handler.DatabaseFile = databaseFile;
                    m_handler.Request.Make(RequestId.StoreToolSettings);
                    m_event.Raise();
                }
            }

            
        }
    }
}
