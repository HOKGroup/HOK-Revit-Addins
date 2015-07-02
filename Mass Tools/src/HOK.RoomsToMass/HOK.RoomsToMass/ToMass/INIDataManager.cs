using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.DB.Architecture;

namespace HOK.RoomsToMass.ToMass
{
    public enum MassCategory
    {
        Rooms = 1,
        Areas = 2,
        Floors = 3
    }

    public class INIDataManager
    {
        private Autodesk.Revit.ApplicationServices.Application m_app;
        private Document doc;
        private string iniPath = "";
        private string massFolder = "";
        private string[] splitter = new string[] { "##" };
        private string[] starSplitter = new string[] { "**" };
        private Dictionary<int, XYZ> roomCenters = new Dictionary<int, XYZ>();//existing rooms: to find discrepancy of new rooms
        private Dictionary<int, double> areaAreas = new Dictionary<int, double>();//existing areas: to find discrepancy of new areas
        private Dictionary<int, XYZ> floorCenters = new Dictionary<int, XYZ>();//exisitng floors: to find discrepancy of new floors
        private Dictionary<int, RoomProperties> createdRooms = new Dictionary<int, RoomProperties>();
        private Dictionary<int, AreaProperties> createdAreas = new Dictionary<int, AreaProperties>();
        private Dictionary<int, FloorProperties> createdFloors = new Dictionary<int, FloorProperties>();
        private List<int> placedRooms = new List<int>();
        private List<int> placedAreas = new List<int>();
        private List<int> placedFloors = new List<int>();
        private List<int> roomDiscrepancy = new List<int>();
        private List<int> areaDiscrepancy = new List<int>();
        private List<int> floorDiscrepancy = new List<int>();
        private List<string> roomSharedParameters = new List<string>();
        private List<string> areaSharedParameters = new List<string>();
        private List<string> floorSharedParameters = new List<string>();
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();
        private MassCategory massCategory;
        private DefinitionFile definitionFile;

        public string MassFolder { get { return massFolder; } set { massFolder = value; } }
        public Dictionary<int, XYZ> RoomCenters { get { return roomCenters; } set { roomCenters = value; } }
        public Dictionary<int, double> AreaAreas { get { return areaAreas; } set { areaAreas = value; } }
        public Dictionary<int, XYZ> FloorCenters { get { return floorCenters; } set { floorCenters = value; } }
        public Dictionary<int, RoomProperties> CreatedRooms { get { return createdRooms; } set { createdRooms = value; } }
        public Dictionary<int, AreaProperties> CreatedAreas { get { return createdAreas; } set { createdAreas = value; } }
        public Dictionary<int, FloorProperties> CreatedFloors { get { return createdFloors; } set { createdFloors = value; } }
        public List<int> PlacedRooms { get { return placedRooms; } set { placedRooms = value; } }
        public List<int> PlacedAreas { get { return placedAreas; } set { placedAreas = value; } }
        public List<int> PlacedFloors { get { return placedFloors; } set { placedFloors = value; } }
        public List<int> RoomDiscrepancy { get { return roomDiscrepancy; } set { roomDiscrepancy = value; } }
        public List<int> AreaDiscrepancy { get { return areaDiscrepancy; } set { areaDiscrepancy = value; } }
        public List<int> FloorDiscrepancy { get { return floorDiscrepancy; } set { floorDiscrepancy = value; } }
        public List<string> RoomSharedParameters { get { return roomSharedParameters; } set { roomSharedParameters = value; } }
        public List<string> AreaSharedParameters { get { return areaSharedParameters; } set { areaSharedParameters = value; } }
        public List<string> FloorSharedParameters { get { return floorSharedParameters; } set { floorSharedParameters = value; } }
        public Dictionary<string, Definition> DefDictionary { get { return defDictionary; } set { defDictionary = value; } }

        public INIDataManager(UIApplication uiapp, MassCategory category)
        {
            m_app = uiapp.Application;
            doc = uiapp.ActiveUIDocument.Document;
            massCategory = category;

            if (FindINI())
            {
                ReadINI();
                StoreSharedParams();
                if (roomCenters.Count > 0)
                {
                    FindDiscrepancyOfRoom();
                }
                if (areaAreas.Count > 0)
                {
                    FindDiscrepancyOfArea();
                }
                if (floorCenters.Count > 0)
                {
                    FindDiscrepancyOfFloor();
                }
            }
            else
            {
                MessageBox.Show("The INI file cannot be found.\n Please see if the Revit project file has a file location.", "INI Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool FindINI()
        {
            bool result = false;
            try
            {
                string masterFilePath = "";
                if (doc.IsWorkshared)
                {
                    ModelPath modelPath = doc.GetWorksharingCentralModelPath();
                    masterFilePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                    if (string.IsNullOrEmpty(masterFilePath))
                    {
                        masterFilePath = doc.PathName;
                    }
                }
                else
                {
                    masterFilePath = doc.PathName;
                }

                if (!string.IsNullOrEmpty(masterFilePath))
                {
                    iniPath = masterFilePath.Replace(".rvt", "_mass.ini");
                    if (!File.Exists(iniPath))
                    {
                        FileStream fs = File.Create(iniPath);

                        TaskDialog taskdialog = new TaskDialog("Mass from Room");
                        taskdialog.MainInstruction = "Select a directory for the Mass families.";
                        taskdialog.MainContent = "If you choose the option for the default directory,\n the new folder will be created in the same directory as the Revit project.";
                        taskdialog.AllowCancellation = true;
                        taskdialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Select a directory.");
                        taskdialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Default Directory.");

                        TaskDialogResult tResult = taskdialog.Show();

                        if (TaskDialogResult.CommandLink1 == tResult)
                        {
                            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                            folderBrowserDialog.Description = "Select a directory that you want to use as the default.";
                            folderBrowserDialog.ShowNewFolderButton = true;

                            if (DialogResult.OK == folderBrowserDialog.ShowDialog())
                            {
                                massFolder = folderBrowserDialog.SelectedPath;
                            }
                        }
                        else if (TaskDialogResult.CommandLink2 == tResult)
                        {
                            massFolder = Path.GetDirectoryName(masterFilePath) + @"\RoomMass";
                            if (!Directory.Exists(massFolder))
                            {
                                Directory.CreateDirectory(massFolder);
                            }
                        }

                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("MassFolder:" + massFolder);
                        }
                        fs.Close();
                    }

                    if (File.Exists(iniPath))
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find INI file. \n" + ex.Message, "INIDataManager : FindINI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        private void ReadINI()
        {
            try
            {
                if (File.Exists(iniPath))
                {
                    using (StreamReader sr = new StreamReader(iniPath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] strRoomInfo = line.Split(splitter, StringSplitOptions.None);
                            if (strRoomInfo.Length == 5)
                            {
                                string source = strRoomInfo[0];
                                int id = int.Parse(strRoomInfo[1]);
                                double x = double.Parse(strRoomInfo[2]);
                                double y = double.Parse(strRoomInfo[3]);
                                double z = double.Parse(strRoomInfo[4]);
                                XYZ centroid = new XYZ(x, y, z);
                                switch (source)
                                {
                                    case "Room":
                                        if (!roomCenters.ContainsKey(id)) { roomCenters.Add(id, centroid); }
                                        break;
                                    case "Floor":
                                        if (!floorCenters.ContainsKey(id)) { floorCenters.Add(id, centroid); }
                                        break;
                                }
                            }
                            else if (strRoomInfo.Length == 3)
                            {
                                string source = strRoomInfo[0];
                                int id = int.Parse(strRoomInfo[1]);
                                double area = double.Parse(strRoomInfo[2]);

                                if (source == "Area" && !areaAreas.ContainsKey(id))
                                {
                                    areaAreas.Add(id, area);
                                }
                            }
                            else if (line.Contains("MassFolder:"))
                            {
                                massFolder = line.Replace("MassFolder:", "");
                            }
                            else if (line.Contains("RoomSharedParameter"))
                            {
                                string[] paramInfo = line.Split(starSplitter, StringSplitOptions.None);
                                if (paramInfo.Length > 1)
                                {
                                    for (int i = 1; i < paramInfo.Length; i++)
                                    {
                                        roomSharedParameters.Add(paramInfo[i]);
                                    }
                                }
                            }
                            else if (line.Contains("AreaSharedParameter"))
                            {
                                string[] paramInfo = line.Split(starSplitter, StringSplitOptions.None);
                                if (paramInfo.Length > 1)
                                {
                                    for (int i = 1; i < paramInfo.Length; i++)
                                    {
                                        areaSharedParameters.Add(paramInfo[i]);
                                    }
                                }
                            }
                            else if (line.Contains("FloorSharedParameter"))
                            {
                                string[] paramInfo = line.Split(starSplitter, StringSplitOptions.None);
                                if (paramInfo.Length > 1)
                                {
                                    for (int i = 1; i < paramInfo.Length; i++)
                                    {
                                        floorSharedParameters.Add(paramInfo[i]);
                                    }
                                }
                            }
                        }
                        sr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read ini file.\n" + iniPath + "\n" + ex.Message, "INIDataManager:ReadINI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StoreSharedParams()
        {
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Store Shared Parameters");
                try
                {
                    string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                    string definitionPath = Path.GetDirectoryName(currentAssembly) + "/Resources/Mass Shared Parameters.txt";
                    m_app.SharedParametersFilename = definitionPath;
                    definitionFile = m_app.OpenSharedParameterFile();

                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    Element element = null;
                    List<string> activeSharedParams = new List<string>();

                    switch (massCategory)
                    {
                        case MassCategory.Rooms:
                            element = collector.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements().First();
                            activeSharedParams = roomSharedParameters;
                            break;
                        case MassCategory.Areas:
                            element = collector.OfCategory(BuiltInCategory.OST_Areas).WhereElementIsNotElementType().ToElements().First();
                            activeSharedParams = areaSharedParameters;
                            break;
                        case MassCategory.Floors:
                            element = collector.OfCategory(BuiltInCategory.OST_Floors).WhereElementIsNotElementType().ToElements().First();
                            activeSharedParams = floorSharedParameters;
                            break;
                    }

                    if (null != definitionFile)
                    {
                        DefinitionGroups dGroups = definitionFile.Groups;
                        DefinitionGroup dGroup = dGroups.get_Item("HOK Mass Parameters");
                        if (null == dGroup)
                        {
                            dGroup = dGroups.Create("HOK Mass Parameters");
                        }
                        Definitions definitions = dGroup.Definitions;
                        Definition definition;

                        foreach (string paramName in activeSharedParams)
                        {
                            definition = definitions.get_Item("Mass_" + paramName);
                            if (null == definition)
                            {
#if RELEASE2013||RELEASE2014
                                Parameter param = element.get_Parameter(paramName);
                                definition = definitions.Create("Mass_" + param.Definition.Name, param.Definition.ParameterType);
#elif RELEASE2015 
                                Parameter param = element.LookupParameter(paramName);
                                ExternalDefinitonCreationOptions options = new ExternalDefinitonCreationOptions("Mass_" + param.Definition.Name, param.Definition.ParameterType);
                                definition = definitions.Create(options);
#elif RELEASE2016
                                Parameter param = element.LookupParameter(paramName);
                                ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions("Mass_" + param.Definition.Name, param.Definition.ParameterType);
                                definition = definitions.Create(options);
#endif
                            }
                            if (null != definition && !defDictionary.ContainsKey(paramName))
                            {
                                defDictionary.Add(paramName, definition);
                            }
                        }
                    }
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to store shared parameters.\n" + ex.Message, "Form_RoomMass:StoreSharedParams", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }
        }

        public void FindDiscrepancyOfRoom()
        {
            try
            {
                //find placed mass
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IList<Element> roomMass = collector.OfCategory(BuiltInCategory.OST_Mass).WhereElementIsNotElementType().ToElements().ToList();

                foreach (Element element in roomMass)
                {
                    FamilyInstance fi = element as FamilyInstance;
                    if (null != fi)
                    {
                        int roomId = 0;
                        if (fi.Symbol.Family.Name.Length > 5 && int.TryParse(fi.Symbol.Family.Name, out roomId))
                        {
                            //compare existing centroid
                            if (roomCenters.ContainsKey(roomId))
                            {
                                ElementId elementId = new ElementId(roomId);
                                Element roomElement = doc.GetElement(elementId);
                                Room room = roomElement as Room;
                                RoomProperties rp = new RoomProperties(doc, room);
                                if (rp.RoomCenter.IsAlmostEqualTo(roomCenters[roomId]))
                                {
                                    placedRooms.Add(roomId);
                                    continue;
                                }
                                else
                                {
                                    roomDiscrepancy.Add(roomId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find discrepancy\n" + ex.Message, "INIDataManager:FindDiscrepancyOfRoom", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void FindDiscrepancyOfArea()
        {
            try
            {
                //find placed mass
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IList<Element> areaMass = collector.OfCategory(BuiltInCategory.OST_Mass).WhereElementIsNotElementType().ToElements().ToList();

                foreach (Element element in areaMass)
                {
                    FamilyInstance fi = element as FamilyInstance;
                    if (null != fi)
                    {
                        int areaId = 0;
                        if (fi.Symbol.Family.Name.Length > 5 && int.TryParse(fi.Symbol.Family.Name, out areaId))
                        {
                            //compare existing centroid
                            if (areaAreas.ContainsKey(areaId))
                            {
                                ElementId elementId = new ElementId(areaId);
                                Element areaElement = doc.GetElement(elementId);
                                Area area = areaElement as Area;
                                double areaValue = Math.Round(area.Area, 3, MidpointRounding.ToEven);

                                if (areaValue == areaAreas[areaId])
                                {
                                    placedAreas.Add(areaId);
                                    continue;
                                }
                                else
                                {
                                    areaDiscrepancy.Add(areaId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find discrepancy\n" + ex.Message, "INIDataManager:FindDiscrepancyOfArea", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void FindDiscrepancyOfFloor()
        {
            try
            {
                //find placed mass
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IList<Element> floorMass = collector.OfCategory(BuiltInCategory.OST_Mass).WhereElementIsNotElementType().ToElements().ToList();

                foreach (Element element in floorMass)
                {
                    FamilyInstance fi = element as FamilyInstance;
                    if (null != fi)
                    {
                        int floorId = 0;
                        if (fi.Symbol.Family.Name.Length > 5 && int.TryParse(fi.Symbol.Family.Name, out floorId))
                        {
                            //compare existing centroid
                            if (floorCenters.ContainsKey(floorId))
                            {
                                ElementId elementId = new ElementId(floorId);
                                Element floorElement = doc.GetElement(elementId);
                                Floor floor = floorElement as Floor;
                                FloorProperties fp = new FloorProperties(doc, floor);
                                if (fp.FloorCenter.IsAlmostEqualTo(floorCenters[floorId]))
                                {
                                    placedFloors.Add(floorId);
                                    continue;
                                }
                                else
                                {
                                    floorDiscrepancy.Add(floorId);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find discrepancy\n" + ex.Message, "INIDataManager:FindDiscrepancyOfFloor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //after mass created
        public void WriteINI()
        {
            try
            {
                if (File.Exists(iniPath))
                {
                    //delete all
                    string tempFile = Path.GetTempFileName();
                    using (StreamReader sr = new StreamReader(iniPath))
                    {
                        using (StreamWriter sw = new StreamWriter(tempFile))
                        {
                            string line;

                            while ((line = sr.ReadLine()) != null)
                            {
                                sw.WriteLine("");
                            }
                        }
                    }
                    File.Delete(iniPath);
                    File.Move(tempFile, iniPath);

                    FileStream fs = File.Open(iniPath, FileMode.Create);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string sp = "##";
                        sw.WriteLine("MassFolder:" + massFolder);
                        sw.WriteLine(WriteParamInfo());

                        foreach (int roomId in createdRooms.Keys)
                        {
                            XYZ roomCenter = createdRooms[roomId].RoomCenter;
                            sw.WriteLine("Room" + sp + roomId + sp + roomCenter.X + sp + roomCenter.Y + sp + roomCenter.Z);
                        }

                        foreach (int roomId in roomCenters.Keys)
                        {
                            if (!createdRooms.ContainsKey(roomId))
                            {
                                XYZ roomCenter = roomCenters[roomId];
                                sw.WriteLine("Room" + sp + roomId + sp + roomCenter.X + sp + roomCenter.Y + sp + roomCenter.Z);
                            }
                        }

                        foreach (int areaId in createdAreas.Keys)
                        {
                            double area = createdAreas[areaId].Area;
                            sw.WriteLine("Area" + sp + areaId + sp + area);
                        }

                        foreach (int areaId in areaAreas.Keys)
                        {
                            if (!createdAreas.ContainsKey(areaId))
                            {
                                double area = areaAreas[areaId];
                                sw.WriteLine("Area" + sp + areaId + sp + area);
                            }
                        }

                        foreach (int floorId in createdFloors.Keys)
                        {
                            XYZ floorCenter = createdFloors[floorId].FloorCenter;
                            sw.WriteLine("Floor" + sp + floorId + sp + floorCenter.X + sp + floorCenter.Y + sp + floorCenter.Z);
                        }

                        foreach (int floorId in floorCenters.Keys)
                        {
                            if (!createdFloors.ContainsKey(floorId))
                            {
                                XYZ floorCenter = floorCenters[floorId];
                                sw.WriteLine("Floor" + sp + floorId + sp + floorCenter.X + sp + floorCenter.Y + sp + floorCenter.Z);
                            }
                        }
                        sw.Close();
                    }
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write ini file. \n" + ex.Message, "INIDataManager:WriteINI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private string WriteParamInfo()
        {
            string paramLines = "";
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                string roomParams = "RoomSharedParameter";
                string areaParams = "AreaSharedParameter";
                string floorParams = "FloorSharedParameter";
                string updatedParams = "";
                foreach (string paramName in defDictionary.Keys)
                {
                    updatedParams += "**" + paramName;
                }
                foreach (string paramName in roomSharedParameters)
                {
                    roomParams += "**" + paramName;
                }
                foreach (string paramName in areaSharedParameters)
                {
                    areaParams += "**" + paramName;
                }
                foreach (string paramName in floorSharedParameters)
                {
                    floorParams += "**" + paramName;
                }

                switch (massCategory)
                {
                    case MassCategory.Rooms:
                        roomParams = "RoomSharedParameter" + updatedParams;
                        break;
                    case MassCategory.Areas:
                        areaParams = "AreaSharedParameter" + updatedParams;
                        break;
                    case MassCategory.Floors:
                        areaParams = "FloorSharedParameter" + updatedParams;
                        break;
                }
                strBuilder.AppendLine(roomParams);
                strBuilder.AppendLine(areaParams);
                strBuilder.AppendLine(floorParams);

                paramLines = strBuilder.ToString();
                return paramLines;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write parameter information.\n" + ex.Message, "INIDataManager:WriteParamInfo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return paramLines;
            }
        }
    }
}
