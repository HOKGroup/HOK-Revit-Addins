using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using HOK.RoomsToMass.ToMass;
using Autodesk.Revit.DB;

namespace HOK.RoomsToMass.DataTransfer
{
    public partial class Form_DataTransfer : System.Windows.Forms.Form
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private MassCategory massCategory;
        private INIDataManager iniDataManager;
        private Dictionary<Element/*Room*/, FamilyInstance/*roomMass*/> roomDictionary = new Dictionary<Element, FamilyInstance>();
        private Dictionary<Element/*Area*/, FamilyInstance/*areaMass*/> areaDictionary = new Dictionary<Element, FamilyInstance>();
        private Dictionary<Element/*Floor*/, FamilyInstance/*floorMass*/> floorDictionary = new Dictionary<Element, FamilyInstance>();
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();

        public Form_DataTransfer(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = uiapp.ActiveUIDocument.Document;
            InitializeComponent();
            FindMassInstance();
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private MassCategory GetCategory()
        {
            MassCategory category = MassCategory.Rooms;
            if (radioButtonRoom.Checked)
            {
                category = MassCategory.Rooms;
            }
            else if (radioButtonArea.Checked)
            {
                category = MassCategory.Areas;
            }
            else if (radioButtonFloor.Checked)
            {
                category = MassCategory.Floors;
            }

            return category;
        }

        private void FindMassInstance()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<Element> massInstances = collector.OfCategory(BuiltInCategory.OST_Mass).WhereElementIsNotElementType().ToElements().ToList();

                foreach (Element instance in massInstances)
                {
                    FamilyInstance fi = instance as FamilyInstance;
                    if (null != fi)
                    {
                        int objectId = 0;
                        if (fi.Symbol.Family.Name.Length > 5 && int.TryParse(fi.Symbol.Family.Name, out objectId))
                        {
                            ElementId elementId = new ElementId(objectId);
                            Element element = m_doc.GetElement(elementId);
                            if (null != element)
                            {
                                switch (element.Category.Name)
                                {
                                    case "Rooms":
                                        roomDictionary.Add(element, fi);
                                        break;
                                    case "Areas":
                                        areaDictionary.Add(element, fi);
                                        break;
                                    case "Floors":
                                        floorDictionary.Add(element, fi);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to find Mass Instances. \n"+ex.Message, "Form_DataTransfer:FindMassInstance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void bttnToMass_Click(object sender, EventArgs e)
        {
            massCategory = GetCategory();

            iniDataManager = new INIDataManager(m_app, massCategory);
            defDictionary = new Dictionary<string, Definition>();
            defDictionary = iniDataManager.DefDictionary;

            string parameters = "";
            foreach (string param in defDictionary.Keys)
            {
                parameters += "["+param+"]  ";
            }

            bool result = false;
            switch (massCategory)
            {
                case MassCategory.Rooms:
                    result=TransferToMass(roomDictionary);
                    break;
                case MassCategory.Areas:
                    result=TransferToMass(areaDictionary);
                    break;
                case MassCategory.Floors:
                    result=TransferToMass(floorDictionary);
                    break;
            }

            if (result)
            {
                DialogResult dr = MessageBox.Show("Parameters values have been successfully transferred from "+massCategory.ToString()+" to Mass.\n"+parameters, "Updated Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dr == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        //Room/Area/Floor >> Mass
        private bool TransferToMass(Dictionary<Element, FamilyInstance> mapDictionary)
        {
            try
            {
                foreach (Element mainElement in mapDictionary.Keys)
                {
                    FamilyInstance massInstance = mapDictionary[mainElement];

                    foreach (string paramName in defDictionary.Keys)
                    {
                        ExternalDefinition extDefinition = defDictionary[paramName] as ExternalDefinition;
#if RELEASE2013||RELEASE2014
                        Parameter mainParameter = mainElement.get_Parameter(paramName);
                        Parameter massParameter = massInstance.get_Parameter("Mass_" + paramName);
#elif RELEASE2015 || RELEASE2016
                        Parameter mainParameter = mainElement.LookupParameter(paramName);
                        Parameter massParameter = massInstance.LookupParameter("Mass_" + paramName);
#endif
                        if (null != mainParameter && null != massParameter && !massParameter.IsReadOnly)
                        {
                            using (Transaction trans = new Transaction(m_doc))
                            {
                                trans.Start("Set Parameter");
                                switch (mainParameter.StorageType)
                                {
                                    case StorageType.Double:
                                        massParameter.Set(mainParameter.AsDouble());
                                        break;
                                    case StorageType.Integer:
                                        massParameter.Set(mainParameter.AsInteger());
                                        break;
                                    case StorageType.String:
                                        massParameter.Set(mainParameter.AsString());
                                        break;
                                }
                                trans.Commit();
                            }
                        }
                        else if (null != mainParameter && null == massParameter) //create Mass Parameter
                        {
                            Family family = massInstance.Symbol.Family;
                            Document familyDoc = m_doc.EditFamily(family);

                            if (null != familyDoc && familyDoc.IsFamilyDocument)
                            {
                                using (Transaction fTrans = new Transaction(familyDoc))
                                {
                                    fTrans.Start("Add Parameter");
                                    FamilyParameter fParam = familyDoc.FamilyManager.AddParameter(extDefinition, BuiltInParameterGroup.INVALID, true);
                                    switch (fParam.StorageType)
                                    {
                                        case StorageType.Double:
                                            familyDoc.FamilyManager.Set(fParam, mainParameter.AsDouble());
                                            break;
                                        case StorageType.Integer:
                                            familyDoc.FamilyManager.Set(fParam, mainParameter.AsInteger());
                                            break;
                                        case StorageType.String:
                                            familyDoc.FamilyManager.Set(fParam, mainParameter.AsString());
                                            break;
                                    }

                                    familyDoc.LoadFamily(m_doc, new FamilyOption());
                                    fTrans.Commit();
                                }
                                familyDoc.Close(true);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to transfer data from "+massCategory.ToString()+" to Mass.\n" + ex.Message, "Form_DataTrnasfer:TransferToMass", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void bttnToRoom_Click(object sender, EventArgs e)
        {
            massCategory = GetCategory();

            iniDataManager = new INIDataManager(m_app, massCategory);
            defDictionary = new Dictionary<string, Definition>();
            defDictionary = iniDataManager.DefDictionary;

            string parameters = "";
            foreach (string param in defDictionary.Keys)
            {
                parameters += "[" + param + "]  ";
            }

            bool result = false;
            switch (massCategory)
            {
                case MassCategory.Rooms:
                    result=TransferToRoom(roomDictionary);
                    break;
                case MassCategory.Areas:
                    result=TransferToRoom(areaDictionary);
                    break;
                case MassCategory.Floors:
                    result=TransferToRoom(floorDictionary);
                    break;
            }

            if (result)
            {
                DialogResult dr = MessageBox.Show("Parameters values have been successfully transferred from Mass to" + massCategory.ToString() + ".\n" + parameters, "Updated Parameters", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dr == DialogResult.OK)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }
        
        //Mass >> Room/Area/Floor
        private bool TransferToRoom(Dictionary<Element, FamilyInstance> mapDictionary)
        {
            try
            {
                foreach (Element mainElement in mapDictionary.Keys)
                {
                    FamilyInstance massInstance = mapDictionary[mainElement];

                    foreach (string paramName in defDictionary.Keys)
                    {
                        ExternalDefinition extDefinition = defDictionary[paramName] as ExternalDefinition;

#if RELEASE2013||RELEASE2014
                        Parameter mainParameter = mainElement.get_Parameter(paramName);
                        Parameter massParameter = massInstance.get_Parameter("Mass_" + paramName);
#elif RELEASE2015 || RELEASE2016
                        Parameter mainParameter = mainElement.LookupParameter(paramName);
                        Parameter massParameter = massInstance.LookupParameter("Mass_" + paramName);
#endif

                        if (null != mainParameter && null != massParameter && !mainParameter.IsReadOnly)
                        {
                            using (Transaction trans = new Transaction(m_doc))
                            {
                                trans.Start("Set Parameter");
                                switch (mainParameter.StorageType)
                                {
                                    case StorageType.Double:
                                        mainParameter.Set(massParameter.AsDouble());
                                        break;
                                    case StorageType.Integer:
                                        mainParameter.Set(massParameter.AsInteger());
                                        break;
                                    case StorageType.String:
                                        mainParameter.Set(massParameter.AsString());
                                        break;
                                }
                                trans.Commit();
                            }
                        }
                        else if (null != mainParameter && null == massParameter) //create Mass Parameter
                        {
                            Family family = massInstance.Symbol.Family;
                            Document familyDoc = m_doc.EditFamily(family);

                            if (null != familyDoc && familyDoc.IsFamilyDocument)
                            {
                                using (Transaction fTrans = new Transaction(familyDoc))
                                {
                                    fTrans.Start("Add Parameter");
                                    FamilyParameter fParam = familyDoc.FamilyManager.AddParameter(extDefinition, BuiltInParameterGroup.INVALID, true);
                                    switch (fParam.StorageType)
                                    {
                                        case StorageType.Double:
                                            familyDoc.FamilyManager.Set(fParam, mainParameter.AsDouble());
                                            break;
                                        case StorageType.Integer:
                                            familyDoc.FamilyManager.Set(fParam, mainParameter.AsInteger());
                                            break;
                                        case StorageType.String:
                                            familyDoc.FamilyManager.Set(fParam, mainParameter.AsString());
                                            break;
                                    }

                                    familyDoc.LoadFamily(m_doc, new FamilyOption());
                                    fTrans.Commit();
                                }
                                familyDoc.Close(true);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to transfer data from Mass to "+massCategory.ToString()+"\n" + ex.Message, "Form_DataTrnasfer:TransferToRoom", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}
