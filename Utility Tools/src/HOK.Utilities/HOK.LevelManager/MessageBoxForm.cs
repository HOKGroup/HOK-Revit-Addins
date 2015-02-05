using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HOK.LevelManager
{
    public partial class MessageBoxForm : Form
    {
        private string formTitle = "";
        private string messageContents = "";
        private string logFileName = "";
        private bool logEnabled = false;

        public string FormTitle { get { return formTitle; } set { formTitle = value; } }
        public string MessageContents { get { return messageContents; } set { messageContents = value; } }
        public string LogFileName { get { return logFileName; } set { logFileName = value; } }
        public bool LogEnabled { get { return logEnabled; } set { logEnabled = value; } }

        public MessageBoxForm(string title, string message, string logFile, bool log)
        {
            formTitle = title;
            messageContents = message;
            logFileName = logFile;
            logEnabled = log;

            InitializeComponent();

            this.Text = formTitle;
            richTextBoxMessage.Text = message;
            buttonLog.Enabled = logEnabled;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(logFileName))
            {
                System.Diagnostics.Process.Start(logFileName);
            }
        }

        private void bttnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
