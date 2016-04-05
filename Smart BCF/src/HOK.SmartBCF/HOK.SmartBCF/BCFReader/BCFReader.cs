using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using HOK.SmartBCF.Schemas;
using Version = HOK.SmartBCF.Schemas.Version;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.BCFReader
{
    public static class BCFReader
    {
        public static ProgressBar progressBar = null;
        public static TextBlock statusLabel = null;

        public static BCFZIP Read(string bcfPath)
        {
            BCFZIP bcfZip = new BCFZIP(bcfPath);
            try
            {
                Dictionary<string /*topicId*/, Dictionary<string/*fileName*/, VisualizationInfo>> tempVisInfoHolder = new Dictionary<string, Dictionary<string, VisualizationInfo>>();
                Dictionary<string/*topicId*/, Dictionary<string/*fileName*/, ComponentExtensionInfo>> tempExtInfoHolder = new Dictionary<string, Dictionary<string, ComponentExtensionInfo>>();
                Dictionary<string /*topicId*/, Dictionary<string/*fileName*/, byte[]>> tempFileContentHoder = new Dictionary<string, Dictionary<string, byte[]>>();

                bcfZip = ReadRawData(bcfPath, out tempVisInfoHolder, out tempFileContentHoder, out tempExtInfoHolder);

                bcfZip = MapContents(bcfZip, tempVisInfoHolder, tempFileContentHoder, tempExtInfoHolder);

                bcfZip.Markups = new ObservableCollection<Markup>(bcfZip.Markups.OrderBy(o => o.Topic.Index).ToList());
                if (bcfZip.Markups.Count > 0)
                {
                    bcfZip.SelectedMarkup = 0;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read BCF.\n" + ex.Message, "Read BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfZip;
        }

        private static BCFZIP ReadRawData(string bcfPath, out Dictionary<string, Dictionary<string, VisualizationInfo>> tempVisInfoHolder, 
            out Dictionary<string, Dictionary<string, byte[]>> tempFileContentHoder, out Dictionary<string, Dictionary<string, ComponentExtensionInfo>> tempExtInfoHolder)
        {
            BCFZIP bcfZip = new BCFZIP(bcfPath);
            tempVisInfoHolder = new Dictionary<string, Dictionary<string, VisualizationInfo>>();
            tempFileContentHoder = new Dictionary<string, Dictionary<string, byte[]>>();
            tempExtInfoHolder = new Dictionary<string, Dictionary<string, ComponentExtensionInfo>>();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(bcfPath))
                {
                    ProgressManager.InitializeProgress("Gathering information from the BCF file...", archive.Entries.Count);
                    double value = 0;

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        value++;
                        ProgressManager.StepForward();
                        string topicId = entry.ExtractGuidFolderName();

                        if (entry.FullName.EndsWith(".bcfp", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(ProjectExtension));
                            XmlReader reader = XmlReader.Create(entry.Open());
                            if (serializer.CanDeserialize(reader))
                            {
                                bcfZip.ProjectFile = (ProjectExtension)serializer.Deserialize(reader);
                                continue;
                            }
                        }
                        else if (entry.FullName.EndsWith(".version", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(Version));
                            XmlReader reader = XmlReader.Create(entry.Open());
                            if (serializer.CanDeserialize(reader))
                            {
                                bcfZip.VersionFile = (Version)serializer.Deserialize(reader);
                                continue;
                            }
                        }
                        else if (entry.FullName.EndsWith(".color", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(RevitExtensionInfo));
                            XmlReader reader = XmlReader.Create(entry.Open());
                            if (serializer.CanDeserialize(reader))
                            {
                                bcfZip.ExtensionColor = (RevitExtensionInfo)serializer.Deserialize(reader);
                                continue;
                            }
                        }
                        else if (!string.IsNullOrEmpty(topicId) && entry.FullName.EndsWith(".bcf", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(Markup));
                            XmlReader reader = XmlReader.Create(entry.Open());
                            if (serializer.CanDeserialize(reader))
                            {
                                Markup markup = (Markup)serializer.Deserialize(reader);
                                bcfZip.Markups.Add(markup);
                            }
                        }
                        else if (!string.IsNullOrEmpty(topicId) && entry.FullName.EndsWith(".bcfv", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(VisualizationInfo));
                            XmlReader reader = XmlReader.Create(entry.Open());
                            if (serializer.CanDeserialize(reader))
                            {
                                VisualizationInfo visInfo = (VisualizationInfo)serializer.Deserialize(reader);
                                if (tempVisInfoHolder.ContainsKey(topicId))
                                {
                                    if (!tempVisInfoHolder[topicId].ContainsKey(entry.Name))
                                    {
                                        tempVisInfoHolder[topicId].Add(entry.Name, visInfo);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, VisualizationInfo> visInfoDictionary = new Dictionary<string, VisualizationInfo>();
                                    visInfoDictionary.Add(entry.Name, visInfo);
                                    tempVisInfoHolder.Add(topicId, visInfoDictionary);
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(topicId) && entry.FullName.EndsWith(".bcfvx", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(ComponentExtensionInfo));
                            XmlReader reader = XmlReader.Create(entry.Open());
                            if (serializer.CanDeserialize(reader))
                            {
                                ComponentExtensionInfo extInfo = (ComponentExtensionInfo)serializer.Deserialize(reader);
                                if (tempExtInfoHolder.ContainsKey(topicId))
                                {
                                    if (!tempExtInfoHolder[topicId].ContainsKey(entry.Name))
                                    {
                                        tempExtInfoHolder[topicId].Add(entry.Name, extInfo);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, ComponentExtensionInfo> extInfoDictionary = new Dictionary<string, ComponentExtensionInfo>();
                                    extInfoDictionary.Add(entry.Name, extInfo);
                                    tempExtInfoHolder.Add(topicId, extInfoDictionary);
                                }
                            }
                        }
                        else
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                entry.Open().CopyTo(ms);
                                byte[] byteArray = ms.ToArray();
                                if (tempFileContentHoder.ContainsKey(topicId))
                                {
                                    if (!tempFileContentHoder[topicId].ContainsKey(entry.Name))
                                    {
                                        tempFileContentHoder[topicId].Add(entry.Name, byteArray);
                                    }
                                }
                                else
                                {
                                    Dictionary<string, byte[]> fileContents = new Dictionary<string, byte[]>();
                                    fileContents.Add(entry.Name, byteArray);
                                    tempFileContentHoder.Add(topicId, fileContents);
                                }
                            }
                        }
                    }
                    ProgressManager.FinalizeProgress();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read data from BCF.\n"+ex.Message, "Read Raw Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return bcfZip;

        }

        private static BCFZIP MapContents(BCFZIP bcfzip, Dictionary<string, Dictionary<string, VisualizationInfo>> tempVisInfoHolder, 
            Dictionary<string, Dictionary<string, byte[]>> tempFileContentHolder, Dictionary<string, Dictionary<string, ComponentExtensionInfo>> tempExtInfoHolder)
        {
            BCFZIP mappedBCF = null;
            try
            {
                Dictionary<string/*guid*/, RevitExtension> extensions = new Dictionary<string, RevitExtension>();
                foreach (RevitExtension ext in bcfzip.ExtensionColor.Extensions)
                {
                    if (!extensions.ContainsKey(ext.Guid))
                    {
                        extensions.Add(ext.Guid, ext);
                    }
                }

                for (int i = 0; i < bcfzip.Markups.Count; i++)
                {
                    Markup markup = bcfzip.Markups[i];
                    string topicId = markup.Topic.Guid;

                    //BimSnippet
                    BimSnippet bimSnippet = markup.Topic.BimSnippet;
                    if (!string.IsNullOrEmpty(bimSnippet.Reference) && !bimSnippet.isExternal)
                    {
                        if (tempFileContentHolder.ContainsKey(topicId))
                        {
                            if (tempFileContentHolder[topicId].ContainsKey(bimSnippet.Reference))
                            {
                                bimSnippet.FileContent = tempFileContentHolder[topicId][bimSnippet.Reference];
                            }
                        }
                    }
                    bimSnippet.TopicGuid = topicId;
                    markup.Topic.BimSnippet = bimSnippet;

                    //DocumentReferences
                    List<TopicDocumentReferences> docList = new List<TopicDocumentReferences>();
                    foreach (TopicDocumentReferences doc in markup.Topic.DocumentReferences)
                    {
                        TopicDocumentReferences docRef = doc;
                        docRef.TopicGuid = topicId;
                        if (!string.IsNullOrEmpty(docRef.ReferencedDocument) && !docRef.isExternal)
                        {
                            if (tempFileContentHolder.ContainsKey(topicId))
                            {
                                if (tempFileContentHolder[topicId].ContainsKey(docRef.ReferencedDocument))
                                {
                                    docRef.FileContent = tempFileContentHolder[topicId][docRef.ReferencedDocument];
                                }
                            }
                        }
                        docList.Add(docRef);
                    }
                    markup.Topic.DocumentReferences = docList;

                    if (markup.Viewpoints.Count > 0)
                    {
                        ViewPoint firstViewpoint = markup.Viewpoints.First();

                        for (int j = 0; j < markup.Comment.Count; j++)
                        {
                            string viewPointGuid = markup.Comment[j].Viewpoint.Guid;
                            if (string.IsNullOrEmpty(viewPointGuid))
                            {
                                markup.Comment[j].Viewpoint.Guid = firstViewpoint.Guid;
                            }
                        }
                    }

                    //viewpoints
                    for (int j = 0; j < markup.Viewpoints.Count; j++)
                    {
                        ViewPoint viewpoint = markup.Viewpoints[j];
                        //bitmap
                        if (tempVisInfoHolder.ContainsKey(topicId))
                        {
                            if (tempVisInfoHolder[topicId].ContainsKey(viewpoint.Viewpoint))
                            {
                                VisualizationInfo visInfo = tempVisInfoHolder[topicId][viewpoint.Viewpoint];
                                visInfo.ViewPointGuid = viewpoint.Guid;
                                List<VisualizationInfoBitmaps> bitmapList = new List<VisualizationInfoBitmaps>();
                                foreach (VisualizationInfoBitmaps bitmap in visInfo.Bitmaps)
                                {
                                    VisualizationInfoBitmaps visBitmap = bitmap;
                                    if (!string.IsNullOrEmpty(bitmap.Reference))
                                    {
                                        if (tempFileContentHolder.ContainsKey(topicId))
                                        {
                                            if (tempFileContentHolder[topicId].ContainsKey(bitmap.Reference))
                                            {
                                                visBitmap.BitmapImage = tempFileContentHolder[topicId][bitmap.Reference];
                                            }
                                        }
                                    }
                                    visBitmap.ViewPointGuid = viewpoint.Guid;
                                    bitmapList.Add(visBitmap);
                                }
                                visInfo.Bitmaps = bitmapList;

                                string viewpointExt = viewpoint.Viewpoint.Replace(".bcfv", ".bcfvx");
                                if (tempExtInfoHolder.ContainsKey(topicId))
                                {
                                    if (tempExtInfoHolder[topicId].ContainsKey(viewpointExt))
                                    {
                                        ComponentExtensionInfo extInfo = tempExtInfoHolder[topicId][viewpointExt];
                                        if (visInfo.Components.Count > 0)
                                        {
                                            foreach (ComponentExtension compExt in extInfo.Extensions)
                                            {
                                                var compFound = from comp in visInfo.Components where comp.IfcGuid == compExt.IfcGuid select comp;
                                                if (compFound.Count() > 0)
                                                {
                                                    int compIndex = visInfo.Components.IndexOf(compFound.First());
                                                    visInfo.Components[compIndex].ElementName = compExt.ElementName;
                                                    visInfo.Components[compIndex].Action = (extensions.ContainsKey(compExt.ActionGuid)) ? extensions[compExt.ActionGuid] : new RevitExtension();
                                                    visInfo.Components[compIndex].Responsibility = (extensions.ContainsKey(compExt.ResponsibilityGuid)) ? extensions[compExt.ResponsibilityGuid] : new RevitExtension();
                                                }
                                            }
                                        }
                                    }
                                }
                               
                                markup.Viewpoints[j].VisInfo = visInfo;
                            }
                        }
                        //snapshot
                        if (tempFileContentHolder.ContainsKey(topicId))
                        {
                            if (tempFileContentHolder[topicId].ContainsKey(viewpoint.Snapshot))
                            {
                                markup.Viewpoints[j].SnapshotImage = tempFileContentHolder[topicId][viewpoint.Snapshot];
                                if (null == markup.TopicImage && null != markup.Viewpoints[j].SnapshotImage)
                                {
                                    markup.TopicImage = markup.Viewpoints[j].SnapshotImage;
                                }
                            }
                        }
                    }

                    if (markup.Viewpoints.Count > 0) { markup.SelectedViewpoint = markup.Viewpoints.First(); }
                    bcfzip.Markups[i] = markup;
                }
                mappedBCF = bcfzip;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create maps from BCF contents.\n" + ex.Message, "Mapping BCF Contents", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return mappedBCF;
        }

    }
}
