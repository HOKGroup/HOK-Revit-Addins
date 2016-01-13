using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace RTVSettingFileGenerator
{
    public class SettingFileGenerator
    {
        private string rvtDirectory = "";
        private string xmlDirectory = "";
        private List<string> rvtFiles = new List<string>();
        private List<string> templateContents = new List<string>();

        public string RvtDirectory { get { return rvtDirectory; } set { rvtDirectory = value; } }
        public string XmlDirectory { get { return xmlDirectory; } set { xmlDirectory = value; } }
        public List<string> RvtFiles {get{return rvtFiles;}set{rvtFiles = value;}}
        public List<string> TemplateContents { get { return templateContents; } set { templateContents = value; } }

        public SettingFileGenerator(string rvt, string xml)
        {
            rvtDirectory = rvt;
            xmlDirectory = xml;

            rvtFiles = ReadListOfFiles();
            templateContents = ReadTemplate();
            if(rvtFiles.Count> 0 && templateContents.Count> 0)
            {
                WriteTemplate();
            }
        }

        private List<string> ReadListOfFiles()
        {
            List<string> fileList = new List<string>();
            try
            {
                string[] fileArray = Directory.GetFiles(rvtDirectory, "*.rvt", SearchOption.AllDirectories);
                if (fileArray.Length > 0)
                {
                    fileList.AddRange(fileArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot read the list of files.\n" + ex.Message, "Read List of Files", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return fileList;
        }

        private List<string> ReadTemplate()
        {
            List<string> contents = new List<string>();
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string prefix = typeof(SettingFileGenerator).Namespace + ".Template.";
                Stream stream = assembly.GetManifestResourceStream(prefix + "Settings.xml");

                using (StreamReader sr = new StreamReader(stream))
                {
                    while(sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        contents.Add(line);
                    }
                }
                stream.Close();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return contents;
        }

        private bool WriteTemplate()
        {
            bool result = false;
            try
            {
                int count = 0;
                foreach (string rvtFile in rvtFiles)
                {
                    string xmlFileName = Path.GetFileNameWithoutExtension(rvtFile);
                    string filePath = Path.Combine(xmlDirectory, xmlFileName+".xml");

                    bool created = WriteTemplateFile(rvtFile, filePath);
                    if (created) { count++; }
                }

                if (count == rvtFiles.Count)
                {
                    MessageBox.Show(count + " xml files are created in the directory, " + xmlDirectory, "Files are Created!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return result;
        }

        private bool WriteTemplateFile(string rvtFile, string xmlPath)
        {
            bool fileCreated = false;
            try
            {
                if (File.Exists(xmlPath))
                {
                    File.Delete(xmlPath);
                }

                if (!File.Exists(xmlPath))
                {
                    using (StreamWriter sw = File.CreateText(xmlPath))
                    {
                        foreach (string line in templateContents)
                        {
                            string copiedLine = line;
                            if (line.Contains("<Filename>"))
                            {
                                string newLine = "<Filename>" + rvtFile + "</Filename>";
                                int startIndex = line.IndexOf('<');
                                string spaceStr = copiedLine.Remove(startIndex);
                                newLine = spaceStr + newLine;
                                sw.WriteLine(newLine);
                            }
                            else if (line.Contains("<GUID>"))
                            {
                                string newLine = "<GUID>" + Guid.NewGuid().ToString() + "</GUID>";
                                int startIndex = line.IndexOf('<');
                                string spaceStr = copiedLine.Remove(startIndex);
                                newLine = spaceStr + newLine;
                                sw.WriteLine(newLine);
                            }
                            else if (line.Contains("<ViewName>"))
                            {
                                string newLine = "<ViewName>" + Path.GetFileNameWithoutExtension(rvtFile) + "</ViewName>";
                                int startIndex = line.IndexOf('<');
                                string spaceStr = copiedLine.Remove(startIndex);
                                newLine = spaceStr + newLine;
                                sw.WriteLine(newLine);
                            }
                            else
                            {
                                sw.WriteLine(line);
                            }
                        }
                        fileCreated = true;
                        sw.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                string message = ex.Message;
            }
            return fileCreated;
        }

       
    }
}
