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
using System.IO;

namespace HOK.RoomsToMass.ToMass
{
    public partial class Form_Parameters : System.Windows.Forms.Form
    {
        private UIApplication m_app;
        private Document m_doc;
        private MassCategory massCategory;
        private DefinitionFile definitionFile = null;
        private Dictionary<string, Definition> defDictionary = new Dictionary<string, Definition>();

        public Dictionary<string, Definition> DefDictionary { get { return defDictionary; } set { defDictionary = value; } }

        public Form_Parameters(UIApplication uiapp, MassCategory category)
        {
            try
            {
                m_app = uiapp;
                m_doc = m_app.ActiveUIDocument.Document;
                massCategory = category;
                InitializeComponent();
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                string definitionPath = Path.GetDirectoryName(currentAssembly) + "/Resources/Mass Shared Parameters.txt";
                using (Transaction trans = new Transaction(m_doc))
                {
                    trans.Start("Open Parameter Definition");
                    m_app.Application.SharedParametersFilename = definitionPath;
                    definitionFile = m_app.Application.OpenSharedParameterFile();
                    trans.Commit();
                }
               
                if (null == definitionFile)
                {
                    MessageBox.Show("Cannot find a definition file for shared Parameters.\n", "Form_Parameters:" + category.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize the form of parameters.\n"+ex.Message, "Form_Parameters:"+category.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void DisplayInfo()
        {
            switch (massCategory)
            {
                case MassCategory.Rooms:
                    labelParameter.Text = "Room Parameters";
                    BuiltInCategory bltCategory = BuiltInCategory.OST_Rooms;
                    DisplayMainParameters(bltCategory);
                    break;
                case MassCategory.Areas:
                    BuiltInCategory bltCategory2 = BuiltInCategory.OST_Areas;
                    DisplayMainParameters(bltCategory2);
                    labelParameter.Text = "Areas Parameters";
                    break;
                case MassCategory.Floors:
                    BuiltInCategory bltCategory3 = BuiltInCategory.OST_Floors;
                    DisplayMainParameters(bltCategory3);
                    labelParameter.Text = "Floors Parameters";
                    break;
            }
        }

        private void DisplayMainParameters(BuiltInCategory mainCategory)
        {
            Element element = null;
            FilteredElementCollector collector = new FilteredElementCollector(m_doc);
            element = collector.OfCategory(mainCategory).WhereElementIsNotElementType().ToElements().First();

            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition.Name.Contains("Extensions.")) { continue; }
                if (param.StorageType != StorageType.ElementId)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = param.Definition.Name;
                    item.Name = param.Definition.Name;
                    item.Tag = param;

                    if (defDictionary.ContainsKey(param.Definition.Name))
                    {
                        listViewMassParameter.Items.Add(item);
                    }
                    else
                    {
                        listViewMainParameter.Items.Add(item);
                    }
                }
            }

            listViewMainParameter.Sort();
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnAdd_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewMainParameter.SelectedItems)
            {
                listViewMainParameter.Items.RemoveByKey(item.Name);
                listViewMassParameter.Items.Add(item);
            }
        }

        private void bttnDelete_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewMassParameter.SelectedItems)
            {
                listViewMassParameter.Items.RemoveByKey(item.Name);
                listViewMainParameter.Items.Add(item);
            }
        }

        private void bttnCreate_Click(object sender, EventArgs e)
        {
            Definition definition=null;
            using (Transaction trans = new Transaction(m_doc))
            {
                trans.Start("Create Shared Parameter");
                try
                {
                    defDictionary.Clear();

                    DefinitionGroups dGroups = definitionFile.Groups;
                    DefinitionGroup dGroup = dGroups.get_Item("HOK Mass Parameters");
                    if (null == dGroup)
                    {
                        dGroup = dGroups.Create("HOK Mass Parameters");
                    }
                    Definitions definitions = dGroup.Definitions;

                    foreach (ListViewItem item in listViewMassParameter.Items)
                    {
                        definition = definitions.get_Item("Mass_" + item.Text);
                        Parameter parameter = item.Tag as Parameter;

                        if (null == definition && null != parameter)
                        {
                            ParameterType paramType = parameter.Definition.ParameterType;
#if RELEASE2013||RELEASE2014
                            definition = definitions.Create("Mass_" + parameter.Definition.Name, paramType);
#elif RELEASE2015
                            ExternalDefinitonCreationOptions options = new ExternalDefinitonCreationOptions("Mass_" + parameter.Definition.Name, paramType);
                            definition = definitions.Create(options);
#endif
                        }

                        if (!defDictionary.ContainsKey(parameter.Definition.Name))
                        {
                            defDictionary.Add(parameter.Definition.Name, definition);
                        }
                    }
                    trans.Commit();
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to set shared parameters.\n" + ex.Message, "Form_Parameters:CreateButtonClick", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    trans.RollBack();
                }
            }

            
        }
    }
}
