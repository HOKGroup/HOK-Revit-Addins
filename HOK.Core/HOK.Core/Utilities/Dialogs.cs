using Microsoft.Win32;

namespace HOK.Core.Utilities
{
    public static class Dialogs
    {
        /// <summary>
        /// Opens Dialog with filters to allow image selection only.
        /// </summary>
        /// <returns></returns>
        public static string SelectImageFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".jpg",
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png"
            };
            var result = dialog.ShowDialog();

            return result != true ? string.Empty : dialog.FileName;
        }
    }
}
