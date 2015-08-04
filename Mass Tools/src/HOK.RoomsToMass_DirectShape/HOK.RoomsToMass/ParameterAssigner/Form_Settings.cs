using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public partial class Form_Settings : Form
    {
        private double ratio = 0;
        private bool followHost = false;

        public double Ratio { get { return ratio; } set { ratio = value; } }
        public bool FollowHost { get { return followHost; } set { followHost = value; } }

        public Form_Settings(double overlappingRatio, bool follow)
        {
            InitializeComponent();
            textBoxRatio.Text = overlappingRatio.ToString();
            checkBoxHost.Checked = follow;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckValidation())
                {
                    followHost = checkBoxHost.Checked;
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to apply the settings.\n" + ex.Message, "Form_Settings:buttonOK_Click", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool CheckValidation()
        {
            bool result = true;
            try
            {
                if(double.TryParse(textBoxRatio.Text, out ratio))
                {
                    if (ratio > 0 && ratio < 1)
                    {
                        result = true;
                    }
                    else if (ratio == 1)
                    {
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show("The ratio vlalue should between 0 and 1.", "Invalid Ratio Value", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }
                }
                else
                {
                    MessageBox.Show("The ratio vlalue should between 0 and 1.", "Invalid Ratio Value", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check validation.\n"+ex.Message, "Form_Settings:CheckValidation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogFileManager.AppendLog("CheckValidation", ex.Message);
                result = false;
            }
            return result;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
