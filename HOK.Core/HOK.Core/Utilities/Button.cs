using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace HOK.Core.Utilities
{
    public static class ButtonUtil
    {
        /// <summary>
        /// Retrieves the BitmapImage from given assembly by its name and namespace. 
        /// Image must live in Resources folder.
        /// </summary>
        /// <param name="assembly">Assembly object to retrieve image from.</param>
        /// <param name="nameSpace">Namespace.</param>
        /// <param name="imageName">Image name with its extension.</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapImage(Assembly assembly, string nameSpace, string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = nameSpace + ".Resources.";
                var stream = assembly.GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return image;
        }

        ///// <summary>
        ///// Converts Bitmap to BitmapSource that can be used with WPF.
        ///// </summary>
        ///// <param name="bitmap">Bitmap image to convert.</param>
        ///// <returns></returns>
        //public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        //{
        //    if (bitmap == null) return null;

        //    var bitmapImage = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), 
        //        IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        //    return bitmapImage;
        //}
    }
}
