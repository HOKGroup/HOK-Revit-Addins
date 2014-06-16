using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.AVFManager.GenericForms
{
    public enum AnalysisCategory { UrbanPlanning = 0, Architecture, InteriorDesign, Custom }
    
    public partial class CommandForm : Form
    {
        private AnalysisCategory analysisCategory;

        public AnalysisCategory SelectedAnalysisCategory { get { return analysisCategory; } set { analysisCategory = value; } }

        public CommandForm()
        {
            InitializeComponent();
        }

        private void bttnUrban_Click(object sender, EventArgs e)
        {
            analysisCategory = AnalysisCategory.UrbanPlanning;
            this.DialogResult = DialogResult.OK;
        }

        private void bttnArchitecture_Click(object sender, EventArgs e)
        {
            analysisCategory = AnalysisCategory.Architecture;
            this.DialogResult = DialogResult.OK;
        }

        private void bttnInterior_Click(object sender, EventArgs e)
        {
            analysisCategory = AnalysisCategory.InteriorDesign;
            this.DialogResult = DialogResult.OK;
        }

        private void bttnCustom_Click(object sender, EventArgs e)
        {
            analysisCategory = AnalysisCategory.Custom;
            this.DialogResult = DialogResult.OK;
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
