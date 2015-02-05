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

namespace HOK.WorksetView
{
    public partial class ViewCreatorForm : System.Windows.Forms.Form
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
        private ViewFamilyType view3dFamilyType = null;
        private ViewFamilyType viewPlanFamilyType = null;
       
        private List<string> viewBy = new List<string>() { "Design Options","Phase","Workset"};
        private List<WorksetInfo> worksetInfoList = new List<WorksetInfo>();
        private List<PhaseInfo> phaseInfoList = new List<PhaseInfo>();
        private List<DesignOptionInfo> designOptionInfoList = new List<DesignOptionInfo>();

        public ViewCreatorForm(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            toolStripProgressBar.Visible = false;
            toolStripStatusLabel.Text = "Ready";

            GetSourceInformation();

            comboBoxViewBy.DataSource = viewBy;
            comboBoxViewBy.SelectedIndex = 2;
        }

        private void GetViewFamilyType()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<ViewFamilyType> elements = collector.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>().ToList();
                var types = from type in elements where type.ViewFamily == ViewFamily.ThreeDimensional select type;
                view3dFamilyType = types.First();

                types = from type in elements where type.ViewFamily == ViewFamily.FloorPlan select type;
                viewPlanFamilyType = types.First();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to get view family type.\n"+ex.Message, "Get 3D View Family Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetLevels()
        {
            try
            {
                List<LevelInfo> levelInfoList = new List<LevelInfo>();
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<Element> elements = collector.OfClass(typeof(Level)).WhereElementIsNotElementType().ToElements().ToList();
                foreach (Element element in elements)
                {
                    Level level = element as Level;
                    LevelInfo levelInfo = new LevelInfo(level);
                    levelInfoList.Add(levelInfo);
                }

                levelInfoList = levelInfoList.OrderBy(o => o.LevelName).ToList();
                comboBoxLevel.DataSource = levelInfoList;
                comboBoxLevel.DisplayMember = "LevelName";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get the information of level.\n"+ex.Message, "Get Levels", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetSourceInformation()
        {
            try
            {
                GetViewFamilyType();
                GetLevels();
                GetWorksetInfo();
                GetPhaseInfo();
                GetDesignOptionInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get source information from the Revit document.\n"+ex.Message, "Get Source Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetWorksetInfo()
        {
            try
            {
                FilteredWorksetCollector worksetCollector = new FilteredWorksetCollector(m_doc);
                IList<Workset> worksets = worksetCollector.ToWorksets();

                foreach (Workset workset in worksets)
                {
                    if (workset.Kind == WorksetKind.UserWorkset)
                    {
                        WorksetInfo worksetInfo = new WorksetInfo(workset);
                        worksetInfoList.Add(worksetInfo);
                    }
                }

                worksetInfoList = worksetInfoList.OrderBy(o => o.WorksetName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get workset information from the Revit document.\n" + ex.Message, "Get Workset Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetPhaseInfo()
        {
            try
            {
                PhaseArray phases = m_doc.Phases;
                if (phases.Size > 0)
                {
                    foreach (Phase phase in phases)
                    {
                        PhaseInfo phaseInfo = new PhaseInfo(phase);
                        phaseInfoList.Add(phaseInfo);
                    }
                }

                phaseInfoList = phaseInfoList.OrderBy(o => o.PhaseName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get phase information from the Revit document.\n" + ex.Message, "Get Phase Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetDesignOptionInfo()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                List<DesignOption> options = collector.OfClass(typeof(DesignOption)).ToElements().Cast<DesignOption>().ToList();
                foreach (DesignOption dsOption in options)
                {
                    DesignOptionInfo optionInfo = new DesignOptionInfo(dsOption);
                    designOptionInfoList.Add(optionInfo);
                }

                designOptionInfoList = designOptionInfoList.OrderBy(o => o.DesignOptionName).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get design option information from the Revit document.\n" + ex.Message, "Get Design Option Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                if (ValidateCheck())
                {
                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Visible = true;
                    toolStripStatusLabel.Text = "Creating Views...";
                    ViewCreator.progressBar = toolStripProgressBar;

                    StringBuilder strBuilder = new StringBuilder();
                    List<Autodesk.Revit.DB.View> createdViews = new List<Autodesk.Revit.DB.View>();
                    switch (comboBoxViewBy.SelectedItem.ToString())
                    {
                        case "Workset":
                            strBuilder.AppendLine("Workset views have been created or updated for the follwing worksets.");
                            foreach (WorksetInfo wsInfo in checkedListBoxSource.CheckedItems)
                            {
                                if (radioButton3D.Checked)
                                {
                                    View3D viewCreated = ViewCreator.Create3DView(m_doc, wsInfo, view3dFamilyType, checkBoxOverwrite.Checked);
                                    if (null != viewCreated) { createdViews.Add(viewCreated);}
                                }
                                else if (radioButtonPlan.Checked)
                                {
                                    LevelInfo levelInfo=comboBoxLevel.SelectedValue as LevelInfo;
                                    ViewPlan viewCreated = ViewCreator.CreateFloorPlan(m_doc, wsInfo, viewPlanFamilyType, levelInfo.LevelObj, checkBoxOverwrite.Checked);
                                    if (null != viewCreated) { createdViews.Add(viewCreated); }
                                }
                            }
                            break;
                        case "Phase":
                            strBuilder.AppendLine("Phase views have been created or updated for the follwing phases.");
                            foreach (PhaseInfo pInfo in checkedListBoxSource.CheckedItems)
                            {
                                if (radioButton3D.Checked)
                                {
                                    View3D viewCreated = ViewCreator.Create3DView(m_doc, pInfo, view3dFamilyType, checkBoxOverwrite.Checked);
                                    if (null != viewCreated) { createdViews.Add(viewCreated); }
                                }
                                else if (radioButtonPlan.Checked)
                                {
                                    LevelInfo levelInfo = comboBoxLevel.SelectedValue as LevelInfo;
                                    ViewPlan viewCreated = ViewCreator.CreateFloorPlan(m_doc, pInfo, viewPlanFamilyType, levelInfo.LevelObj, checkBoxOverwrite.Checked);
                                    if (null != viewCreated) { createdViews.Add(viewCreated);  }
                                }
                            }
                            break;
                        case "Design Options":
                            strBuilder.AppendLine("Design option views have been created or updated for the follwing design options.");
                            toolStripProgressBar.Maximum = checkedListBoxSource.CheckedItems.Count;
                            foreach (DesignOptionInfo dInfo in checkedListBoxSource.CheckedItems)
                            {
                                toolStripProgressBar.PerformStep();
                                if (radioButton3D.Checked)
                                {
                                    View3D viewCreated = ViewCreator.Create3DView(m_doc, dInfo, view3dFamilyType, checkBoxOverwrite.Checked);
                                    if (null != viewCreated) { createdViews.Add(viewCreated); }
                                }
                                else if (radioButtonPlan.Checked)
                                {
                                    LevelInfo levelInfo = comboBoxLevel.SelectedValue as LevelInfo;
                                    ViewPlan viewCreated = ViewCreator.CreateFloorPlan(m_doc, dInfo, viewPlanFamilyType, levelInfo.LevelObj, checkBoxOverwrite.Checked);
                                    if (null != viewCreated) { createdViews.Add(viewCreated); }
                                }
                            }
                            break;
                    }

                    if (createdViews.Count > 0)
                    {
                        foreach (Autodesk.Revit.DB.View view in createdViews)
                        {
                            strBuilder.AppendLine(view.Name);
                        }
                        MessageBox.Show(strBuilder.ToString(), "Views Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    
                    toolStripStatusLabel.Text = "Done";
                    toolStripProgressBar.Visible = false;
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create 3d views for each workset.\n"+ex.Message, "Create 3D Views", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool ValidateCheck()
        {
            bool valid = false;
            try
            {
                if (checkedListBoxSource.CheckedItems.Count < 1)
                {
                    MessageBox.Show("Please select at least one item to proceed.", "Select Items", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (radioButtonPlan.Checked)
                {
                    if (comboBoxLevel.SelectedIndex < 0)
                    {
                        MessageBox.Show("Please select a level to create a floor plan.", "Select a Level", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }

                valid = true;     
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate user selection.\n"+ex.Message, "Validate Checks", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return valid;
        }
        
        private void buttonAll_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < checkedListBoxSource.Items.Count; i++)
                {
                    checkedListBoxSource.SetItemChecked(i, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check all.\n"+ex.Message, "Check All", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonNone_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < checkedListBoxSource.Items.Count; i++)
                {
                    checkedListBoxSource.SetItemChecked(i, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check None.\n" + ex.Message, "Check None", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void radioButton3D_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3D.Checked)
            {
                comboBoxLevel.Enabled = false;
            }
        }

        private void radioButtonPlan_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonPlan.Checked)
            {
                comboBoxLevel.Enabled = true;
            }
        }

        private void comboBoxViewBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            { 
                ((ListBox)checkedListBoxSource).DataSource=null;
                switch (comboBoxViewBy.SelectedIndex)
                {
                    case 0: //design option
                        ((ListBox)checkedListBoxSource).DataSource = designOptionInfoList;
                        ((ListBox)checkedListBoxSource).DisplayMember = "DesignOptionName";
                        break;
                    case 1://phase
                        ((ListBox)checkedListBoxSource).DataSource = phaseInfoList;
                        ((ListBox)checkedListBoxSource).DisplayMember = "PhaseName";
                        break;
                    case 2://workset
                        ((ListBox)checkedListBoxSource).DataSource = worksetInfoList;
                        ((ListBox)checkedListBoxSource).DisplayMember = "WorksetName";
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display source items by the selection of View By.\n"+ex.Message, "View By Selection Changed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    public class LevelInfo
    {
        private Level levelObj = null;
        private string levelName = "";
        private ElementId levelId = ElementId.InvalidElementId;

        public Level LevelObj { get { return levelObj; } set { levelObj = value; } }
        public string LevelName { get { return levelName; } set { levelName = value; } }
        public ElementId LevelId { get { return levelId; } set { levelId = value; } }

        public LevelInfo(Level level)
        {
            levelObj = level;
            levelName = level.Name;
            levelId = level.Id;
        }
    }

    public class WorksetInfo
    {
        private Workset worksetObj = null;
        private string worksetName = "";
        private WorksetId worksetId = null;
        
        public Workset WorksetObj { get { return worksetObj; } set { worksetObj = value; } }
        public string WorksetName { get { return worksetName; } set { worksetName = value; } }
        public WorksetId WorksetId { get { return worksetId; } set { worksetId = value; } }
        
        public WorksetInfo(Workset workset)
        {
            worksetObj = workset;
            worksetName = workset.Name;
            worksetId = workset.Id;
        }
    }

    public class PhaseInfo
    {
        private Phase phaseObj = null;
        private string phaseName = "";
        private ElementId phaseId = ElementId.InvalidElementId;

        public Phase PhaseObj { get { return phaseObj; } set { phaseObj = value; } }
        public string PhaseName { get { return phaseName; } set { phaseName = value; } }
        public ElementId PhaseId { get { return phaseId; } set { phaseId = value; } }

        public PhaseInfo(Phase phase)
        {
            phaseObj = phase;
            phaseName = phase.Name;
            phaseId = phase.Id;
        }
    }

    public class DesignOptionInfo
    {
        private DesignOption designOptionObj = null;
        private string designOptionName = "";
        private ElementId designOptionId = ElementId.InvalidElementId;

        public DesignOption DesignOptionObj { get { return designOptionObj; } set { designOptionObj = value; } }
        public string DesignOptionName { get { return designOptionName; } set { designOptionName = value; } }
        public ElementId DesignOptionId { get { return designOptionId; } set { designOptionId = value; } }

        public DesignOptionInfo(DesignOption designOption)
        {
            designOptionObj = designOption;
            designOptionName = designOption.Name;
            designOptionId = designOption.Id;
        }
    }
}
