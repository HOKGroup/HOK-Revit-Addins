using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.ToMass
{
    public static class MassUtils
    {
        public static string GetValidCategoryName(string catName)
        {
            string validName = catName;
            if (validName.Contains('\\')) { validName = validName.Replace('\\', '_'); }
            if (validName.Contains(':')) { validName = validName.Replace(':', '_'); }
            if (validName.Contains('{')) { validName = validName.Replace('{', '_'); }
            if (validName.Contains('}')) { validName = validName.Replace('}', '_'); }
            if (validName.Contains('[')) { validName = validName.Replace('[', '_'); }
            if (validName.Contains(']')) { validName = validName.Replace(']', '_'); }
            if (validName.Contains('|')) { validName = validName.Replace('|', '_'); }
            if (validName.Contains(';')) { validName = validName.Replace(';', '_'); }
            if (validName.Contains('<')) { validName = validName.Replace('<', '_'); }
            if (validName.Contains('>')) { validName = validName.Replace('>', '_'); }
            if (validName.Contains('?')) { validName = validName.Replace('?', '_'); }
            if (validName.Contains('\'')) { validName = validName.Replace('\'', '_'); }
            if (validName.Contains('~')) { validName = validName.Replace('~', '_'); }
            return validName;
        }

        public static System.Drawing.Color ConvertToSystemColor(Autodesk.Revit.DB.Color revitColor)
        {
            System.Drawing.Color systemColor = System.Drawing.Color.FromArgb(revitColor.Red, revitColor.Green, revitColor.Blue);
            return systemColor;
        }

        public static Autodesk.Revit.DB.Color ConvertToRevitColor(System.Drawing.Color systemColor)
        {
            Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(systemColor.R, systemColor.G, systemColor.B);
            return revitColor;
        }

        public static Autodesk.Revit.DB.Color GenerateRandomColor()
        {
            Random random = new Random();
            Autodesk.Revit.DB.Color randomColor = new Autodesk.Revit.DB.Color((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
            return randomColor;
        }

        public static string GetParamValueAsString(string paramName, Dictionary<string, Parameter> parameters)
        {
            string paramValue = "";
            if (parameters.ContainsKey(paramName))
            {
                Parameter param = parameters[paramName];
                if (param.HasValue)
                {
                    switch (param.StorageType)
                    {
                        case StorageType.Double:
                            paramValue = ((int)param.AsDouble()).ToString();
                            break;
                        case StorageType.Integer:
                            paramValue = param.AsInteger().ToString();
                            break;
                        case StorageType.String:
                            paramValue = param.AsString();
                            break;
                    }
                }
            }
            return paramValue;
        }

        public static FamilyInstance FindMassById(Document doc, int id)
        {
            FamilyInstance familyinstance = null;
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                List<FamilyInstance> massRooms = collector.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Mass).Cast<FamilyInstance>().ToList();
                var query = from element in massRooms
                            where element.Symbol.Family.Name == id.ToString()
                            select element;
                if (query.Count() > 0)
                {
                    familyinstance = query.First() as FamilyInstance;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return familyinstance;
        }
    }

    public class MassProperties
    {
        public int HostElementId { get; set; }
        public FamilyInstance MassFamilyInstance { get; set; }
    }
}
