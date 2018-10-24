using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HOK.SmartBCF.Utils
{
    public static class ImageUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapImage(string imageName, int size)
        {
            var image = new BitmapImage();
           
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the embedded resource image.\n" + ex.Message, "Load Bitmap Image", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return image;
        }
    }
}