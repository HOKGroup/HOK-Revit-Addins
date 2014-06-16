using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using System.IO;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Forms;
using RevitDBManager.GenericForms;
using System.Drawing.Imaging;

namespace RevitDBManager.Classes
{
    public class ThumbnailCreator
    {
        private Document doc;
        private ImageList imgList;
        private Dictionary<string, Dictionary<int, ElementTypeProperties>> elementTypes = new Dictionary<string, Dictionary<int, ElementTypeProperties>>();
        private form_ProgressBar progressForm;
        private string imageFolder = "";
        private Image defaultImage = null;

        public ThumbnailCreator(Document document, ImageList imageList, Dictionary<string, Dictionary<int, ElementTypeProperties>> dictionary)
        {
            doc = document;
            imgList = imageList;
            elementTypes = dictionary;

            progressForm = new form_ProgressBar();
            progressForm.Text = "Opening Setup.. ";
            progressForm.MaxValue = SetMaxValue();
            progressForm.LabelText = "Collecting Preview Images from Element Types..";
            progressForm.Show();

            CreateImageFolder();
            CollectImages();
        }

        private int SetMaxValue()
        {
            int maxVal = 0;
            foreach (string category in elementTypes.Keys)
            {
                maxVal += elementTypes[category].Count;
            }
            return maxVal;
        }

        private void CreateImageFolder()
        {
            string masterFilePath = "";
            if (doc.IsWorkshared) { masterFilePath = doc.GetWorksharingCentralModelPath().ToString(); }
            else { masterFilePath = doc.PathName; }

            imageFolder = Path.GetDirectoryName(masterFilePath) +@"/"+ Path.GetFileNameWithoutExtension(masterFilePath) + "_TypeImages";
            if (!File.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }
        }

        private void CollectImages()
        {
            try
            {
                progressForm.Refresh();
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                defaultImage = (Bitmap)Image.FromFile(Path.GetDirectoryName(currentAssembly) + "/Resources/default.bmp", true);

                foreach (string category in elementTypes.Keys)
                {
                    foreach (int typeId in elementTypes[category].Keys)
                    {
                        ElementType elementType = elementTypes[category][typeId].ElementTypeObject;
                        imgList.Images.Add(typeId.ToString(), GetPreviewImage(elementType));
                        progressForm.PerformStep();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect preview images. \n" + ex.Message, "ThumbnailCreator Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Image CheckExist(int typeId)
        {
            Image typeImage = null;
            try
            {
                string imageFilePath = Path.Combine(imageFolder, typeId.ToString() + ".jpg");
                if (File.Exists(imageFilePath))
                {
                    typeImage = (Bitmap)Image.FromFile(Path.Combine(imageFolder, typeId.ToString() + ".jpg"), true);
                    typeImage.Tag = typeId;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to get image from file. \n" + ex.Message, "ThumbnailCreator Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return typeImage;
        }

        private Image GetPreviewImage(ElementType elementType)
        {
            Image image = null;
            try
            {
                int typeId = elementType.Id.IntegerValue;
                System.Drawing.Size size = new System.Drawing.Size(48, 48);
                image = CheckExist(typeId);
                if (null != image)
                {
                    image.Tag = typeId;
                    return image;
                }
                else
                {
                    image = elementType.GetPreviewImage(size);
                    if (null != image)
                    {
                        image.Save(Path.Combine(imageFolder, typeId.ToString() + ".jpg"));
                        image.Tag = typeId;
                        return image;
                    }
                    else
                    {
                        defaultImage.Tag = typeId;
                        return defaultImage;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a preview image. \n" + ex.Message,"ThumbnailCreator Error",MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return image;
        }

        private void SavePreviewImage(Image image, int typeId)
        {
            image.Save(Path.Combine(imageFolder, typeId.ToString() + ".jpg"));
        }


        public ImageList GetImageList()
        {
            return imgList;
        }
    }
}
