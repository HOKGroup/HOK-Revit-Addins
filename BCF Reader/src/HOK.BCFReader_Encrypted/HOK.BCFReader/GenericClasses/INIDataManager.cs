using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.IO;
using System.Windows.Forms;

namespace HOK.BCFReader.GenericClasses
{
    public class INIDataManager
    {
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document m_doc;
        private Dictionary<string/*topicId*/, BCF> bcfFiles = new Dictionary<string, BCF>();
        private Dictionary<string/*filePath*/, Dictionary<string, BCF>> refDictionary = new Dictionary<string, Dictionary<string, BCF>>();
        private string iniPath = "";
        private string[] splitter = new string[] { "##" };
        private string selectedFilePath = "";
        private string masterFilePath = "";

        public Dictionary<string, BCF> BCFFiles { get { return bcfFiles; } set { bcfFiles = value; } }
        public Dictionary<string, Dictionary<string, BCF>> RefDictionary { get { return refDictionary; } set { refDictionary = value; } }
        public string SelectedFilePath { get { return selectedFilePath; } set { selectedFilePath = value; } }
        public string INIPath { get { return iniPath; } set { iniPath = value; } }
        public string MasterFilePath { get { return masterFilePath; } set { masterFilePath = value; } }

        public INIDataManager(UIApplication uiapp)
        {
            m_app = uiapp.Application;
            m_doc = uiapp.ActiveUIDocument.Document;
        }

        public bool FindINI()
        {
            bool result = false;
            try
            {
                if (m_doc.IsWorkshared && null != m_doc.GetWorksharingCentralModelPath())
                {
                    masterFilePath = m_doc.GetWorksharingCentralModelPath().ToString();
                }
                else
                {
                    masterFilePath = m_doc.PathName;
                }

                if (!string.IsNullOrEmpty(masterFilePath))
                {
                    iniPath = masterFilePath.Replace(".rvt", "_BCF.ini");
                    if (File.Exists(iniPath))
                    {
                        result = true;
                    }
                }
                else
                {
                    MessageBox.Show("Please save the current Revit project file before running the BCFReader.", "File Not Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find INI file. \n" + ex.Message, "INIDataManager : FindINI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return result;
        }

        public void ReadINI()
        {
            try
            {
                if (File.Exists(iniPath))
                {
                    using (StreamReader sr = new StreamReader(iniPath))
                    {
                        string line;
                        Dictionary<string/*topicId*/, BCF> tempDictionary = new Dictionary<string, BCF>();
                        BCF bcf = new BCF();
                        bcf.MarkUp = new BCFMarkUp();
                        bcf.MarkUp.MarkUpTopic = new Topic();
                        List<Comment> commentList = new List<Comment>();
                        string bcfPath = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Contains("****"))
                            {
                                if (bcfPath != null)
                                {
                                    if (bcf.IssueNumber != null && commentList.Count > 0)
                                    {
                                        bcf.MarkUp.Comments = commentList;
                                        tempDictionary.Add(bcf.MarkUp.MarkUpTopic.TopicGuid, bcf);
                                    }

                                    refDictionary.Add(bcfPath, tempDictionary);
                                    tempDictionary = new Dictionary<string, BCF>();
                                    bcf = new BCF();
                                    bcf.MarkUp = new BCFMarkUp();
                                    bcf.MarkUp.MarkUpTopic = new Topic();
                                    commentList = new List<Comment>();
                                }
                            }
                            else if (line.Contains("BCFFile##"))//BCFFile##Directory
                            {
                                string[] bcfInfo = line.Split(splitter, StringSplitOptions.None);
                                if (bcfInfo.Length == 2)
                                {
                                    bcfPath = bcfInfo[1];
                                }
                            }
                            else if (line.Contains("Issue##"))//Issue##TopicID##Title##IssueNumber
                            {
                                if (bcf.IssueNumber != null && commentList.Count>0)
                                {
                                    bcf.MarkUp.Comments = commentList;
                                    tempDictionary.Add(bcf.MarkUp.MarkUpTopic.TopicGuid, bcf);
                                    bcf = new BCF();
                                    bcf.MarkUp = new BCFMarkUp();
                                    bcf.MarkUp.MarkUpTopic = new Topic();
                                    commentList = new List<Comment>();
                                }
                                string[] issueInfo = line.Split(splitter, StringSplitOptions.None);
                                if (issueInfo.Length == 4)
                                {
                                    bcf.MarkUp.MarkUpTopic.TopicGuid = issueInfo[1];
                                    bcf.MarkUp.MarkUpTopic.Title = issueInfo[2];
                                    bcf.IssueNumber = issueInfo[3];
                                }
                            }
                            else if (line.Contains("Comment##")) //Comment##CommentID##CommentString##Action
                            {
                                string[] commentInfo = line.Split(splitter, StringSplitOptions.None);
                                if (commentInfo.Length == 4)
                                {
                                    Comment comment = new Comment();
                                    comment.CommentGuid = commentInfo[1];
                                    comment.CommentString = commentInfo[2];
                                    comment.Action = commentInfo[3];
                                    commentList.Add(comment);
                                }
                            }
                        }
                        sr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read ini file.\n" + iniPath + "\n" + ex.Message, "INIDataManager:ReadINI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void WriteINI()
        {
            try
            {
                if (!File.Exists(iniPath))
                {
                    using (FileStream fileStream = File.Create(iniPath))
                    {
                        fileStream.Close();
                    }
                }

                //delete all
                string tempFile = Path.GetTempFileName();
                using (StreamReader sr = new StreamReader(iniPath))
                {
                    using (StreamWriter sw = new StreamWriter(tempFile))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            sw.WriteLine("");
                        }
                    }
                }

                File.Delete(iniPath);
                File.Move(tempFile, iniPath);
                FileStream fs = File.Open(iniPath, FileMode.Create);

                if (refDictionary.ContainsKey(selectedFilePath))
                {
                    refDictionary.Remove(selectedFilePath);
                }
                refDictionary.Add(selectedFilePath, bcfFiles);

                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string sp = "##";
                    foreach (string filePath in refDictionary.Keys)
                    {
                        Dictionary<string, BCF> tempBcfFiles = new Dictionary<string, BCF>();
                        tempBcfFiles = refDictionary[filePath];
                        sw.WriteLine("BCFFile" + sp + filePath);
                        foreach (string topicId in tempBcfFiles.Keys)
                        {
                            BCF bcf = tempBcfFiles[topicId];
                            sw.WriteLine("Issue" + sp + topicId + sp + bcf.MarkUp.MarkUpTopic.Title + sp + bcf.IssueNumber);
                            foreach (Comment comment in bcf.MarkUp.Comments)
                            {
                                sw.WriteLine("Comment" + sp + comment.CommentGuid + sp + comment.CommentString + sp + comment.Action);
                            }
                        }
                        sw.WriteLine("********************************************************");
                    }
                    sw.Close();
                }
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write ini file.\n"+ex.Message, "INIDataManager:WriteINI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
