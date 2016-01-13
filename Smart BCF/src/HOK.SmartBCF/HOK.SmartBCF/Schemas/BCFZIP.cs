using HOK.SmartBCF.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace HOK.SmartBCF.Schemas
{
    public class BCFZIP : INotifyPropertyChanged 
    {
        private string fileId = "";
        private string zipFileName = "";
        private string zipFilePath = "";
        private string uploadedBy = "";
        private DateTime uploadedDate = DateTime.Now;
        private DateTime creationDate = DateTime.Now;
        private ObservableCollection<Markup> markups = new ObservableCollection<Markup>();
        private RevitExtensionInfo extensionColor = new RevitExtensionInfo();
        private ProjectExtension projectFile = new ProjectExtension();
        private Version versionFile = new Version();

        private int selectedMarkup = 0;
        private bool isPrimary = false;

        public string FileId { get { return fileId; } set { fileId = value; NotifyPropertyChanged("FileId"); } }
        public string ZipFileName { get { return zipFileName; } set { zipFileName = value; NotifyPropertyChanged("ZipFileName"); } }
        public string ZipFilePath { get { return zipFilePath; } set { zipFilePath = value; NotifyPropertyChanged("ZipFilePath"); } }
        public string UploadedBy { get { return uploadedBy; } set { uploadedBy = value; NotifyPropertyChanged("UploadedBy"); } }
        public DateTime UploadedDate { get { return uploadedDate; } set { uploadedDate = value; NotifyPropertyChanged("UploadedDate"); } }
        public DateTime CreationDate { get { return creationDate; } set { creationDate = value; NotifyPropertyChanged("CreationDate"); } }
        public ObservableCollection<Markup> Markups { get { return markups; } set { markups = value; NotifyPropertyChanged("Markups"); } }
        public RevitExtensionInfo ExtensionColor { get { return extensionColor; } set { extensionColor = value; NotifyPropertyChanged("ExtensionColor"); } }
        public ProjectExtension ProjectFile { get { return projectFile; } set { projectFile = value; NotifyPropertyChanged("ProjectFile"); } }
        public Version VersionFile { get { return versionFile; } set { versionFile = value; NotifyPropertyChanged("VersionFile"); } }

        public int SelectedMarkup { get { return selectedMarkup; } set { selectedMarkup = value; NotifyPropertyChanged("SelectedMarkup"); } }
        public bool IsPrimary { get { return isPrimary; } set { isPrimary = value; NotifyPropertyChanged("IsPrimary"); } }

        public BCFZIP()
        {
            GetDefaultExtension();
        }

        public BCFZIP(string filePath)
        {
            fileId = Guid.NewGuid().ToString();
            zipFilePath = filePath;
            zipFileName = Path.GetFileName(zipFilePath);
            uploadedBy = System.Environment.UserName;
            FileInfo fi = new FileInfo(zipFilePath);
            creationDate = fi.CreationTime;
            GetDefaultExtension();
        }

        private void GetDefaultExtension()
        {
            try
            {
                //BCF Action
                ObservableCollection<RevitExtension> extensions = new ObservableCollection<RevitExtension>();
                extensions.Add(new RevitExtension(Guid.Empty.ToString(), "", "", GetColorArray(255, 255, 255) /*white*/));

                extensions.Add(new RevitExtension("022c627d-450a-42eb-aae7-9c42548c084f", "BCF_Action", "DELETE", GetColorArray(255, 99, 71) /*tomato*/));
                extensions.Add(new RevitExtension("1b300b68-1a86-4c50-ae2e-0e144754ce95", "BCF_Action", "MOVE", GetColorArray(100, 149, 237)/*corn flower blue*/));
                extensions.Add(new RevitExtension("4b022058-4833-43cd-ae01-2d3cf3873058", "BCF_Action", "ADD", GetColorArray(60, 179, 113)/*medium sea green*/));
                extensions.Add(new RevitExtension("e8efc0aa-1b3f-4c6d-8dab-b866534d87d4", "BCF_Action", "CHANGE TYPE", GetColorArray(255, 215, 0)/*gold*/));
                
                //BCF Responsibility
                extensions.Add(new RevitExtension("ac7cacab-d80b-4a8d-87b2-4cbc985d0542", "BCF_Responsibility", "ARCHITECTURE", GetColorArray(30, 144, 255)/*dodger blue*/));
                extensions.Add(new RevitExtension("fdc1b079-25a5-4b3e-a27a-4aa1b59d1faf", "BCF_Responsibility", "STRUCTURE", GetColorArray(154, 205, 50)/*yellow green*/));
                extensions.Add(new RevitExtension("43f0d813-0b37-415e-8983-e78f9591fe9d", "BCF_Responsibility", "MEP", GetColorArray(205, 133, 63)/*peru*/));
                extensionColor.Extensions = extensions;
            }

            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private byte[] GetColorArray(int r, int g, int b)
        {
            byte[] colorBytes = new byte[3];
            colorBytes[0] = (byte)r;
            colorBytes[1] = (byte)g;
            colorBytes[2] = (byte)b;
            return colorBytes;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
