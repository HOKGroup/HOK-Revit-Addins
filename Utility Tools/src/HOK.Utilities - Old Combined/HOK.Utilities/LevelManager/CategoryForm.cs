using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace HOK.Utilities.LevelManager
{
    public partial class CategoryForm : System.Windows.Forms.Form
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private Dictionary<string/*catName*/, List<Element>> elementDictionary = new Dictionary<string, List<Element>>();
        private Dictionary<string/*catName*/, List<Element>> selectedElements = new Dictionary<string, List<Element>>();

        public Dictionary<string, List<Element>> SelectedElements { get { return selectedElements; } set { selectedElements = value; } }

        private BuiltInCategory[] systemCategories = new BuiltInCategory[]
        {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_ShaftOpening,
            BuiltInCategory.OST_Stairs, 
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_Ceilings,
            BuiltInCategory.OST_Roofs,
            BuiltInCategory.OST_Railings,
            BuiltInCategory.OST_BuildingPad,
            BuiltInCategory.OST_CableTray,
            BuiltInCategory.OST_Conduit,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_FlexDuctCurves,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_Rooms,
            BuiltInCategory.OST_IOSModelGroups
        };

        public CategoryForm(UIApplication application)
        {
            m_app = application;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            DisplayCategories();
        }

        private void DisplayCategories()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                IList<Element> familyInstances = collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType().ToElements().ToList();

                foreach (Element elem in familyInstances)
                {
                    Parameter levelparam = elem.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                    Parameter baseLevelParam = elem.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);

                    if (null != elem.Category)
                    {
                        if (null == levelparam && null == baseLevelParam) { continue; }
                        if (null != levelparam) { if (levelparam.IsReadOnly) { continue; } }
                        if (null != baseLevelParam) { if (baseLevelParam.IsReadOnly) { continue; } }
                        
                        if (elementDictionary.ContainsKey(elem.Category.Name))
                        {
                            elementDictionary[elem.Category.Name].Add(elem);
                        }
                        else
                        {
                            List<Element> elements = new List<Element>();
                            elements.Add(elem);
                            elementDictionary.Add(elem.Category.Name, elements);
                        }
                    }
                }

                foreach (BuiltInCategory biltCat in systemCategories)
                {
                    CollectSystemFamilies(biltCat);
                }

                foreach (string catName in elementDictionary.Keys)
                {
                    ListViewItem item = new ListViewItem();
                    item.Name = catName;
                    item.Text = catName;
                    item.Tag = elementDictionary[catName];
                    listViewCategory.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the lists of categories.\n"+ex.Message, "CategoryForm : DisplayCategories", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectSystemFamilies(BuiltInCategory biltCat)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elementList = new List<Element>();
                IList<Element> elements = collector.OfCategory(biltCat).WhereElementIsNotElementType().ToElements().ToList();
                if (elements.Count > 0)
                {
                    elementList.AddRange(elements);
                    string catName = elements.First().Category.Name;
                    if (!elementDictionary.ContainsKey(catName))
                    {
                        elementDictionary.Add(catName, elementList);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect system families.\n"+ex.Message, "Collect System Families", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (ListViewItem item in listViewCategory.CheckedItems)
                {
                    string catName = item.Text;
                    List<Element> elements = new List<Element>();
                    elements = item.Tag as List<Element>;
                    if (!selectedElements.ContainsKey(catName))
                    {
                        selectedElements.Add(catName, elements);
                    }
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to select categories.\n"+ex.Message, "Select Categories", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            if (buttonCheck.ImageIndex == 0)
            {
                buttonCheck.ImageIndex = 1;
                foreach (ListViewItem item in listViewCategory.Items)
                {
                    item.Checked = true;
                }
            }
            else if(buttonCheck.ImageIndex==1)
            {
                buttonCheck.ImageIndex = 0;
                foreach (ListViewItem item in listViewCategory.Items)
                {
                    item.Checked = false;
                }
            }
        }
    }
}
