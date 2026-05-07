using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Form = System.Windows.Forms.Form;

namespace HOK.RoomsToMass.ParameterAssigner
{
    public partial class MessageBoxForm : Form
    {
        private string formTitle = "";
        private string messageContents = "";
        private string logFileName = "";
        private bool logEnabled = false;
        private bool logVisible = true;

        [DefaultValue("")]
        public string FormTitle { get { return formTitle; } set { formTitle = value; } }
        [DefaultValue("")]
        public string MessageContents { get { return messageContents; } set { messageContents = value; } }
        [DefaultValue("")]
        public string LogFileName { get { return logFileName; } set { logFileName = value; } }
        [DefaultValue(false)]
        public bool LogEnabled { get { return logEnabled; } set { logEnabled = value; } }
        [DefaultValue(true)]
        public bool LogVisible { get { return logVisible; } set { logVisible = value; } }

        public MessageBoxForm(string title, string message, string logFile, bool log, bool visibleLog)
        {
            formTitle = title;
            messageContents = message;
            logFileName = logFile;
            logEnabled = log;
            logVisible = visibleLog;

            InitializeComponent();

            this.Text = formTitle;
            richTextBoxMessage.Text = message;
            buttonLog.Enabled = logEnabled;
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
