using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HOK.SmartBCF.Utils
{
    public static class ImageUtil
    {
        public static byte[] GetImageArray(string fileName)
        {
            byte[] imgArray = null;
            try
            {
                if (File.Exists(fileName))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Image image = Image.FromFile(fileName);
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        imgArray = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return imgArray;
        }

        public static string ConvertToImageFile(byte[] imgArray)
        {
            string imageFile = "";
            try
            {
                string tempImg = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SmartBCF", System.IO.Path.GetTempFileName() + ".png");
                using (FileStream fs = new FileStream(tempImg, FileMode.Create, FileAccess.Write))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(imgArray);
                    bw.Close();
                    fs.Close();
                }

                if (File.Exists(tempImg))
                {
                    imageFile = tempImg;
                }
                else
                {
                    MessageBox.Show("The image of snapshot cannot be created as an file.", "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return imageFile;
        }

    }
}
