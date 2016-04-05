using Autodesk.Revit.DB;
using HOK.Keynote.ClassModels;
using HOK.Keynote.REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteManager
{
    public static class KeynoteDatabase
    {
        public static string[] rvtVersions = new string[] { "2013", "2014", "2015", "2016" };

        public static string GetCurrentVersion()
        {
            return System.DateTime.Now.ToString("MM-yyyy");
        }

        public static bool IsValidRevitVersion(string versionNumber)
        {
            return rvtVersions.Contains(versionNumber);
        }

        public static void LoadKeynoteEntries(IDictionary<string, string>  refMap, ref KeyBasedTreeEntriesLoadContent entriesContent)
        {
            try
            {
                string versionNumber = refMap["VersionNumber"];
                string projectName = refMap["ProjectName"];

                if (!IsValidRevitVersion(versionNumber))
                {
                    throw new ArgumentOutOfRangeException("Version Number", versionNumber, "The spcified version cannot be found in the database");
                }

                if (null == entriesContent)
                {
                    throw new ArgumentNullException("entries load content");
                }

                if (projectName == "Test")
                {
                    CreateKeynoteEntries(versionNumber, ref entriesContent);
                }
                else if (projectName == "LocalDB")
                {
                    //insert keynotedate
                    //List<KeynoteInfo> keynoteData = CreateKeynoteData();
                    //HttpStatusCode status;
                    //string jsonResponse, errorMessage;
                    //status = ServerUtil.PostBatch(out jsonResponse, out errorMessage, "keynotes", keynoteData);
                    

                   //Read Keynote Entries from mongoDB
                    List<KeynoteInfo> keynotes = ServerUtil.GetKeynotes("keynotes");

                }
                else if (projectName == "CloudDB")
                {
                    List<KeynoteInfo> keynotes = ServerUtil.GetKeynotes("keynotes");
                    CreateKeynoteEntries(versionNumber, keynotes, ref entriesContent);
                }

            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void CreateKeynoteEntries(string versionNumber, ref KeyBasedTreeEntriesLoadContent entriesContent)
        {
            try
            {

                switch (versionNumber)
                {
                    case "2016":
                        //read from database
                        entriesContent.AddEntry(new KeynoteEntry("01", "", "Dienstleistungen, Produktionen"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01", "01", "Fuhrparkkosten"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.01", "01.01", "Frachten"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.01.01", "01.01.01", "Eigene Fracht"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.01.02", "01.01.01", "Fremdfracht"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.01.99", "01.01.01", "Sonstige - Fracht"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.02", "01.01", "Fuhrparkleistungen"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.02.01", "01.01.02", "Eigener Fuhrpark"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.02.02", "01.01.02", "Fremder Fuhrpark"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.02.99", "01.01.02", "Sonstige - Fuhrparkleistung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.03", "01.01", "Kran und Stapler"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.03.01", "01.01.03", "Kranentladung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.03.02", "01.01.03", "Staplerkosten"));
                        entriesContent.AddEntry(new KeynoteEntry("01.01.03.99", "01.01.03", "Sonstiger - Kran und Stapler"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02", "01", "Handwerkliche Leistungen"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01", "01.02", "Allgemeine Bauarbeiten"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.01", "01.02.01", "Dachdeckungsarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.02", "01.02.01", "Elektroinstallation"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.03", "01.02.01", "Erdarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.04", "01.02.01", "Klempnerarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.05", "01.02.01", "Rohbau"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.06", "01.02.01", "Sanitär- und Heizungsbauarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.07", "01.02.01", "Trockenbauarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.08", "01.02.01", "Verglasungsarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.01.99", "01.02.01", "Sonstige - allgem. Bauarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.02", "01.02", "Belagsarbeiten"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.02.01", "01.02.02", "Asphaltarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.02.02", "01.02.02", "Estricharbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.02.03", "01.02.02", "Mineralgemischeinbringung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.02.04", "01.02.02", "Natursteinarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.02.99", "01.02.02", "Sonstige - Belagsarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.03", "01.02", "Betonarbeiten"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.03.01", "01.02.03", "Betoninstandhaltung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.03.02", "01.02.03", "Betonsanierung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.03.99", "01.02.03", "Sonstige - Betonarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.04", "01.02", "Montage"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.04.01", "01.02.04", "Dachmontage"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.04.02", "01.02.04", "Fenstermontage"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.04.03", "01.02.04", "Montagewand-/Unterdeckenmontage"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.04.04", "01.02.04", "Türenmontage"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.04.99", "01.02.04", "Sonstige - Montage"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05", "01.02", "Verlegung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.01", "01.02.05", "Fliesenverlegung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.02", "01.02.05", "Parkett- und Laminatverlegung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.03", "01.02.05", "Pflasterarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.04", "01.02.05", "Plattenverlegung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.05", "01.02.05", "PVC-Verlegung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.06", "01.02.05", "Tapezierarbeit"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.07", "01.02.05", "Teppichverlegung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.08", "01.02.05", "Wand- und Deckenvertäfelung"));
                        entriesContent.AddEntry(new KeynoteEntry("01.02.05.99", "01.02.05", "Sonstige - Verlegung"));
                        break;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static void CreateKeynoteEntries(string versionNumber, List<KeynoteInfo> keynoteInfoList, ref KeyBasedTreeEntriesLoadContent entriesContent)
        {
            try
            {
                foreach (KeynoteInfo info in keynoteInfoList)
                {
                    entriesContent.AddEntry(new KeynoteEntry(info.key, info.parentKey, info.description));
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public static List<KeynoteInfo> CreateKeynoteData()
        {
            List<KeynoteInfo> infoList = new List<KeynoteInfo>();
            try
            {
                infoList.Add(new KeynoteInfo("82118519-8bec-4d22-8dff-fa2910e30588", "00", "", "DIVISION 00 - PROJECT REQUIREMENTS", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                infoList.Add(new KeynoteInfo("7259577e-9730-4313-8dac-ecee6e954dda", "001000", "00", "SECTION 001000 - PROJECT NOTE LEGEND", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                infoList.Add(new KeynoteInfo("96f6b271-2008-41e1-8283-8a6458d866c4", "001000.A01", "001000", "CUSTOM PROJECT NOTES HERE", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                infoList.Add(new KeynoteInfo("254a90b4-5668-4f45-8000-a64553e4e9d4", "01", "", "DIVISION 01 - GENERAL REQUIREMENTS", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                infoList.Add(new KeynoteInfo("502896ff-1a20-42f9-a0ae-88126362758c", "011000", "01", "SECTION 011000 - SUMMARY", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                infoList.Add(new KeynoteInfo("b23a9cfd-729d-4991-a8f4-8ad0a29727fa", "011000.G01", "011000", "SEISMIC CATEGORY A-C:", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                infoList.Add(new KeynoteInfo("32363e22-0e68-44a2-a6d5-0fc219f3625d", "011000.G02", "011000", "SEISMIC CATEGORY B-C:", "02234469-3d3d-44f3-9aea-0eb1a5d286f7"));
                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return infoList;
        }

    }
}
