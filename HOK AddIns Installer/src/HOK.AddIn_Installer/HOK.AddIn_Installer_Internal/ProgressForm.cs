using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.AddIn_Installer_Internal
{
    public partial class ProgressForm : Form
    {
        
        public ProgressForm(string text)
        {
            InitializeComponent();
            labelStatus.Text = text;
            progressBarTool.Value = 0;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public int GetMaximumValue()
        {
            return progressBarTool.Maximum;
        }

        public void SetMaximumValue(int maxVal)
        {
            progressBarTool.Maximum = maxVal;
        }

        public void StepForward()
        {
            progressBarTool.PerformStep();
        }
    }
}
