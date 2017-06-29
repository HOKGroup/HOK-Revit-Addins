using System.Reflection;
using System.Windows.Media.Imaging;

namespace HOK.MissionControl.Utils
{
    public static class ButtonUtilities
    {
        public static BitmapImage LoadBitmapImage(string imageName)
        {
            var image = new BitmapImage();
            try
            {
                var prefix = typeof(AppCommand).Namespace + ".Resources.";
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(prefix + imageName);

                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
            }
            catch
            {
                // ignored
            }
            return image;
        }
    
    }
}
