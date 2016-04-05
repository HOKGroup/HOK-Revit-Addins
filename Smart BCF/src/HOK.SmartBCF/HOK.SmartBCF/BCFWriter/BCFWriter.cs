using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using HOK.SmartBCF.Schemas;
using Version = HOK.SmartBCF.Schemas.Version;
using HOK.SmartBCF.Utils;

namespace HOK.SmartBCF.BCFWriter
{
    public static class BCFWriter
    {
        public static bool Write(string bcfPath, BCFZIP bcf)
        {
            bool written = false;
            try
            {
                ProgressManager.InitializeProgress("Writing BCF..", bcf.Markups.Count);

                string tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SmartBCF", bcf.FileId);
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }

                //Create root directory
                Directory.CreateDirectory(tempDirectory);

                //Project File
                string projectFilePath = System.IO.Path.Combine(tempDirectory, "project.bcfp");
                using (FileStream stream = new FileStream(projectFilePath, FileMode.Create))
                {
                    XmlSerializer projectSerializer = new XmlSerializer(typeof(ProjectExtension));
                    projectSerializer.Serialize(stream, bcf.ProjectFile);
                    stream.Close();
                }

                //Version File
                string versionFilePath = System.IO.Path.Combine(tempDirectory, "bcf.version");
                using (FileStream stream = new FileStream(versionFilePath, FileMode.Create))
                {
                    XmlSerializer versionSerializer = new XmlSerializer(typeof(Version));
                    versionSerializer.Serialize(stream, bcf.VersionFile);
                    stream.Close();
                }

                //Color File
                string colorFilePath = System.IO.Path.Combine(tempDirectory, "extension.color");
                using (FileStream stream = new FileStream(colorFilePath, FileMode.Create))
                {
                    XmlSerializer colorSerializer = new XmlSerializer(typeof(RevitExtensionInfo));
                    colorSerializer.Serialize(stream, bcf.ExtensionColor);
                    stream.Close();
                }
                
                //Markup and Viewpoint
                XmlSerializer markupSerializer = new XmlSerializer(typeof(Markup));
                XmlSerializer visInfoSerializer = new XmlSerializer(typeof(VisualizationInfo));
                XmlSerializer extInfoSerializer = new XmlSerializer(typeof(ComponentExtensionInfo));
                foreach (Markup markup in bcf.Markups)
                {
                    ProgressManager.StepForward();

                    string topicDirectory = System.IO.Path.Combine(tempDirectory, markup.Topic.Guid);
                    Directory.CreateDirectory(topicDirectory);

                    string markupFilePath = System.IO.Path.Combine(topicDirectory, "markup.bcf");
                    using (FileStream stream = new FileStream(markupFilePath, FileMode.Create))
                    {
                        markupSerializer.Serialize(stream, markup);
                        stream.Close();
                    }

                    //Viewpoint
                    foreach (ViewPoint vp in markup.Viewpoints)
                    {
                        //Snapshot
                        if (!string.IsNullOrEmpty(vp.Snapshot) && null != vp.SnapshotImage)
                        {
                            string snapshotPath = System.IO.Path.Combine(topicDirectory, vp.Snapshot);
                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(vp.SnapshotImage)))
                            {
                                image.Save(snapshotPath, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }

                        if (!string.IsNullOrEmpty(vp.Viewpoint))
                        {
                            //Visinfo
                            string visInfoPath = System.IO.Path.Combine(topicDirectory, vp.Viewpoint);
                            if (null != vp.VisInfo)
                            {
                                VisualizationInfo visInfo = vp.VisInfo;
                                using (FileStream stream = new FileStream(visInfoPath, FileMode.Create))
                                {
                                    visInfoSerializer.Serialize(stream, visInfo);
                                    stream.Close();
                                }

                                string extensionPath = visInfoPath.Replace(".bcfv", ".bcfvx");
                                ComponentExtensionInfo extInfo = new ComponentExtensionInfo();
                                extInfo.ViewpointGuid = vp.Guid;
                                var revitComponents = from comp in visInfo.Components
                                                      where (null != comp.Action) && (null!=comp.Responsibility)
                                                      select comp;
                                if (revitComponents.Count() > 0)
                                {
                                    var componentsToWrite = from comp in revitComponents
                                                            where (comp.Action.Guid != Guid.Empty.ToString()) || (comp.Responsibility.Guid != Guid.Empty.ToString()) || (!string.IsNullOrEmpty(comp.ElementName))
                                                            select comp;
                                    if (componentsToWrite.Count() > 0)
                                    {
                                        ObservableCollection<ComponentExtension> compExtensions = new ObservableCollection<ComponentExtension>();
                                        List<Component> componentList = revitComponents.ToList();
                                        foreach (Component comp in componentList)
                                        {
                                            compExtensions.Add(new ComponentExtension(comp));
                                        }
                                        extInfo.Extensions = compExtensions;
                                        using (FileStream stream = new FileStream(extensionPath, FileMode.Create))
                                        {
                                            extInfoSerializer.Serialize(stream, extInfo);
                                            stream.Close();
                                        }
                                    }
                                }

                                //Bitmap
                                if (vp.VisInfo.Bitmaps.Count > 0)
                                {
                                    foreach (VisualizationInfoBitmaps bitmap in visInfo.Bitmaps)
                                    {
                                        if (!string.IsNullOrEmpty(bitmap.Reference) && null != bitmap.BitmapImage)
                                        {
                                            string bitmapPath = System.IO.Path.Combine(topicDirectory, bitmap.Reference);
                                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(bitmap.BitmapImage)))
                                            {
                                                if (bitmap.Bitmap == BitmapFormat.JPG)
                                                {
                                                    image.Save(bitmapPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                }
                                                else if (bitmap.Bitmap == BitmapFormat.PNG)
                                                {
                                                    image.Save(bitmapPath, System.Drawing.Imaging.ImageFormat.Png);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ZipFile.CreateFromDirectory(tempDirectory, bcfPath, CompressionLevel.Fastest, false);
                if (File.Exists(bcfPath))
                {
                    written = true;
                }
                ProgressManager.FinalizeProgress();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write the BCF file.\n" + ex.Message, "Write BCF", MessageBoxButton.OK, MessageBoxImage.Warning);
                ProgressManager.FinalizeProgress();
            }
            return written;
        }
    }
}
