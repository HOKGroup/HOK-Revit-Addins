using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HOK.Core.Utilities;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public partial class Form_Parameters : Form
    {
        private List<string> parameters = new List<string>();
        private Dictionary<string/*massParam*/, string/*elemParam*/> selectedParam = new Dictionary<string, string>();

        public Dictionary<string, string> SelectedParam { get { return selectedParam; } set { selectedParam = value; } }

        public Form_Parameters(List<string> param, Dictionary<string, string> selParam)
        {
            parameters = param;
            selectedParam = selParam;
            InitializeComponent();
        }

        private void Form_Parameters_Load(object sender, EventArgs e)
        {
            try
            {
                int index = 0;
                foreach (string paramName in parameters)
                {
                    index = dataGridViewParam.Rows.Add();
                    dataGridViewParam.Rows[index].Cells[0].Value = false;
                    dataGridViewParam.Rows[index].Cells[1].Value = paramName;
                    dataGridViewParam.Rows[index].Cells[2].Value = paramName;

                    if (selectedParam.ContainsKey(paramName))
                    {
                        dataGridViewParam.Rows[index].Cells[0].Value = true;
                        dataGridViewParam.Rows[index].Cells[2].Value = selectedParam[paramName];
                    }
                }
                dataGridViewParam.Sort(dataGridViewParam.Columns[1], ListSortDirection.Ascending);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bttnApply_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                string message = "Please enter a name of the following element parameters:";
                selectedParam = new Dictionary<string, string>();
                foreach (DataGridViewRow row in dataGridViewParam.Rows)
                {
                    if (Convert.ToBoolean(row.Cells[0].Value.ToString()))
                    {
                        if (row.Cells[2].Value.ToString().Length > 0)
                        {
                            selectedParam.Add(row.Cells[1].Value.ToString(), row.Cells[2].Value.ToString());
                        }
                        else
                        {
                            strBuilder.AppendLine(row.Cells[1].Value.ToString());
                        }
                    }
                }
                if (strBuilder.Length > 0)
                {
                    MessageBox.Show(message + "\n" + strBuilder.ToString(), "Missing Element Parameters", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void bttnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewParam.Rows)
            {
                row.Cells[0].Value = false;
            }
        }

        private void bttnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewParam.Rows)
            {
                row.Cells[0].Value = true;
            }
        }

       
    }
}
