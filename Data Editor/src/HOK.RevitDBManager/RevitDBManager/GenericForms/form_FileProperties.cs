using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RevitDBManager.Classes;
using System.IO;

namespace RevitDBManager.GenericForms
{
    public partial class form_FileProperties : Form
    {
        private DBFileInfo dbFileInfo;

        public DBFileInfo DataBaseFileInfo { get { return dbFileInfo; } set { dbFileInfo = value; } }

        public form_FileProperties(DBFileInfo fileInfo )
        {
            dbFileInfo = fileInfo;
            InitializeComponent();
        }

        private void form_FileProperties_Load(object sender, EventArgs e)
        {
            textBoxName.Text = dbFileInfo.FileName;
            textBoxPath.Text = dbFileInfo.FilePath;

            if (File.Exists(dbFileInfo.FilePath))
            {
                FileInfo fileInfo = new FileInfo(dbFileInfo.FilePath);
                labelSize.Text = fileInfo.Length.ToString() + " bytes";
                labelModified.Text = dbFileInfo.DateModified;
                labelCreated.Text = fileInfo.CreationTime.ToString();
                labelBy.Text = dbFileInfo.ModifiedBy;
                richTextBoxComments.Text = dbFileInfo.Comments;
                checkBoxActive.Checked = dbFileInfo.isDefault;
            }
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            dbFileInfo.Comments = richTextBoxComments.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        

      
    }
}
