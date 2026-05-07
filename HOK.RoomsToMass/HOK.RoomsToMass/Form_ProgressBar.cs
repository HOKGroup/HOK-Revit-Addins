using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace HOK.RoomsToMass
{
    public partial class Form_ProgressBar : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProgressBar CurProgressBar { get { return progressBar1; } set { progressBar1 = value; } }
        [DefaultValueAttribute("")]
        public string LabelText { get { return statusLabel.Text; } set { statusLabel.Text = value; } }
        [DefaultValueAttribute(100)]
        public int MaxValue { get { return progressBar1.Maximum; } set { progressBar1.Maximum = value; } }
        [DefaultValueAttribute(0)]
        public int CurValue { get { return progressBar1.Value; } set { progressBar1.Value = value; } }
        [DefaultValueAttribute(1)]
        public int Step { get { return progressBar1.Step; } set { progressBar1.Step = value; } }
        [DefaultValueAttribute(1)]
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
