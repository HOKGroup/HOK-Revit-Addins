using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HOK.AVFManager.GenericClasses;

namespace HOK.AVFManager.GenericForms
{
    public partial class AdvancedSettingsForm : Form
    {
        private List<ComboBox> comboBoxList = new List<ComboBox>();
        private SettingProperties settings;

        public SettingProperties CurrentSettings { get { return settings; } set { settings = value; } }

        public AdvancedSettingsForm(SettingProperties settingProperties)
        {
            settings = settingProperties;
            InitializeComponent();

            comboBoxList.Add(comboBoxConfig1);
            comboBoxList.Add(comboBoxConfig2);
            comboBoxList.Add(comboBoxConfig3);
            comboBoxList.Add(comboBoxConfig4);
            comboBoxList.Add(comboBoxConfig5);

            radioButtonParameters.Checked = true;
            if (null != settings.ParameterName)
            {
                for (int i = 0; i < comboBoxConfig1.Items.Count; i++)
                {
                    if (comboBoxConfig1.Items[i].ToString() == settings.ParameterName)
                    {
                        comboBoxConfig1.SelectedIndex = i;
                    }
                }
            }
        }

        private void radioButtonParameters_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ComboBox comboBox in comboBoxList)
            {
                comboBox.Items.Clear();
            }

            if (radioButtonParameters.Checked)
            {
                DisplayParameters();
            }
        }

        private void DisplayParameters()
        {
            if (settings.ParameterList.Count > 0)
            {
                foreach (string parameterName in settings.ParameterList)
                {
                    foreach (ComboBox comboBox in comboBoxList)
                    {
                        comboBox.Items.Add(parameterName);
                    }
                }
            }
        }

        private void SaveConfigurations()
        {
            Dictionary<string,string> configurations=new Dictionary<string,string>();

            string name = "";
            string description = "";

            if (null != comboBoxConfig1.SelectedItem)
            {
                name = comboBoxConfig1.SelectedItem.ToString();
                description = textBoxDescription1.Text;
                if (string.Empty == description) { description = " "; }
                if (!configurations.ContainsKey(name)) { configurations.Add(name, description); }
            }
            if (null != comboBoxConfig2.SelectedItem)
            {
                name = comboBoxConfig2.SelectedItem.ToString();
                description = textBoxDescription2.Text;
                if (string.Empty == description) { description = " "; }
                if (!configurations.ContainsKey(name)) { configurations.Add(name, description); }
            }
            if (null != comboBoxConfig3.SelectedItem)
            {
                name = comboBoxConfig3.SelectedItem.ToString();
                description = textBoxDescription3.Text;
                if (string.Empty == description) { description = " "; }
                if (!configurations.ContainsKey(name)) { configurations.Add(name, description); }
            }
            if (null != comboBoxConfig4.SelectedItem)
            {
                name = comboBoxConfig4.SelectedItem.ToString();
                description = textBoxDescription4.Text;
                if (string.Empty == description) { description = " "; }
                if (!configurations.ContainsKey(name)) { configurations.Add(name, description); }
            }
            if (null != comboBoxConfig5.SelectedItem)
            {
                name = comboBoxConfig5.SelectedItem.ToString();
                description = textBoxDescription5.Text;
                if (string.Empty == description) { description = " "; }
                if (!configurations.ContainsKey(name)) { configurations.Add(name, description); }
            }
            
            settings.Configurations = configurations;
        }

        private void bttnApply_Click(object sender, EventArgs e)
        {
            SaveConfigurations();
            if (settings.Configurations.Count > 0)
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
