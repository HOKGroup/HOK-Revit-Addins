using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {

        static private Regex reProject = new Regex(@"[\\|/](\d{2}\.\d{5}\.\d{2})\s+(.+?(?=[\\|/]))");
        static private Regex reServer = new Regex(@"^\\\\group\\hok\\(.+?(?=\\))|[\\|/](\w{2,3})-\d{2}svr(\.group\.hok\.com)?[\\|/]|^[rR]:\\(\w{2,3})\\", RegexOptions.IgnoreCase);

        static void Main(string[] args)
        {
            String newCase1 = @"RSN://NY-18SVR/2016/13.07051.00 LaGuardia Terminal B/Structure/SXXXXXXXX-3D_CA-CENTRAL.rvt";
            String newCase2 = @"RSN://STL-18SVR/2015/15.01001.00/Revit-AC/STLStadium-HOK-LA-Central.rvt";
            String existingExample1 = @"R:\SF\PROJECTS\2014\14.04024.00 SF Traffic and Forensic Lab\E-BIM\Software\Revit-AC\SFTCFL-HOK-AR-central.rvt";
            String existingExample2 = @"\\group\HOK\CHI\IPD\14.03021.00 WRWA Terminal Expansion\Software\Revit-AC\WRWATE-HOK-AR-central.rvt";

            String theSource = existingExample2;

            //Project Name
            var nameResult = reProject.Match(theSource);

            if (nameResult.Success)
            {
                Console.WriteLine("Project Name = " + nameResult.Groups[2].ToString());
                Console.WriteLine("Project Number = " + nameResult.Groups[1].ToString());
                //Project Number is in nameResult.Groups[1] if needed
            }

            //File Location
            var flResult = reServer.Match(theSource);
            if (flResult.Success)
            {
                if (!String.IsNullOrEmpty(flResult.Groups[4].ToString()))
                {
                    Console.WriteLine("File Location POS4 = " + flResult.Groups[4].ToString());
                    //rec.FileLocation = flResult.Groups[2].ToString();
                }
                else if (!String.IsNullOrEmpty(flResult.Groups[2].ToString()))
                {
                    Console.WriteLine("File Location POS2 = " + flResult.Groups[2].ToString());
                    //rec.FileLocation = flResult.Groups[1].ToString();
                }
                else
                {
                    Console.WriteLine("File Location POS1 = " + flResult.Groups[1].ToString());
                }
            }
            Console.Write("Finished processing, press any key to exit>");
            Console.ReadKey();
        }
    }
}
