using ARUP.IssueTracker.Classes;
using Arup.RestSharp;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace ARUP.IssueTracker.Converters
{
    [ValueConversion(typeof(String), typeof(BitmapImage))]
    public class UriToImageConv : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == "")
                return null;

            RestClient client = new RestClient();
            var request = new RestRequest(value.ToString(), Method.GET);
            try
            {//workaround because I use a class for this XD
                string exeConfigPath = this.GetType().Assembly.Location;
                string dir = Path.GetDirectoryName(exeConfigPath);

                string[] files = System.IO.Directory.GetFiles(dir, "*.config");

                string mainassembly = "";

                foreach (var f in files)
                {
                    if (!f.Contains(this.GetType().Assembly.FullName) && !f.Contains("vshost"))
                    {
                        mainassembly = f.Substring(0, f.Length - 7);
                        break;
                    }
                }


                string username = MySettings.Get("username");
                string password = DataProtector.DecryptData(MySettings.Get("password"));

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return null;

                HttpBasicAuthenticator user = new HttpBasicAuthenticator(username, password);
                client.Authenticator = user;


            }
            catch (Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }


            //
            var response = client.Execute(request);
            return LoadImage(response.RawBytes);

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            throw new NotImplementedException();
        }


        private static BitmapImage LoadImage(byte[] imageData)
        {
            
            try
            {
                if (imageData == null || imageData.Length == 0) return null;
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
            catch { }
            return null;

        }

    }
}
