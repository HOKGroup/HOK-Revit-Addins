using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.RoomsToMass
{
    public partial class Form_ProgressBar : Form
    {
        public ProgressBar CurProgressBar { get { return progressBar1; } set { progressBar1 = value; } }
        public string LabelText { get { return statusLabel.Text; } set { statusLabel.Text = value; } }
        public int MaxValue { get { return progressBar1.Maximum; } set { progressBar1.Maximum = value; } }
        public int CurValue { get { return progressBar1.Value; } set { progressBar1.Value = value; } }
        public int Step { get { return progressBar1.Step; } set { progressBar1.Step = value; } }
        public string LabelCount { get { return labelCount.Text; } set { labelCount.Text = value; } }

        public Form_ProgressBar()
        {
            InitializeComponent();
            progressBar1.Minimum = 1;
            progressBar1.Step = 1;
        }
        public void PerformStep()
        {
            progressBar1.PerformStep();
        }

        private void bttnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
