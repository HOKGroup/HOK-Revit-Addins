using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HOK.BCFReader_SerialMaker
{
    public partial class RecordForm : Form
    {
        private string iniPath = @"\\group\hok\FWR\depts\buildingSMART\Z-Applicaiton Development\BCF KEys\SerialRecords.ini";
        private Dictionary<string/*LicenseKey*/, LicenseKey> keyDictionary = new Dictionary<string, LicenseKey>();
        private string[] splitter = new string[] { "##" };

        public Dictionary<string, LicenseKey> KeyDictionary { get { return keyDictionary; } set { keyDictionary = value; } }

        public RecordForm()
        {
            InitializeComponent();
            if (File.Exists(iniPath))
            {
                ReadRecords();
            }
        }

        private void ReadRecords()
        {
            try
            {
                FileStream fs = new FileStream(iniPath, FileMode.OpenOrCreate, FileAccess.Read);
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] licenseInfo = line.Split(splitter, StringSplitOptions.None);
                        if (licenseInfo.Length == 5)
                        {
                            LicenseKey lk = new LicenseKey();
                            lk.CompanyName = licenseInfo[0];
                            lk.Identifier = licenseInfo[1];
                            lk.KeyString = licenseInfo[2];
                            lk.DateGenerated = licenseInfo[3];
                            lk.GeneratedBy = licenseInfo[4];

                            if (!keyDictionary.ContainsKey(lk.KeyString))
                            {
                                keyDictionary.Add(lk.KeyString, lk);
                            }
                        }
                    }
                    sr.Close();
                }
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read the record of the generated license keys.\n" + ex.Message, "recordForm:ReadRecords", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void DisplayRecords()
        {
            try
            {
                dataGridViewRecords.Rows.Clear();
                foreach (string key in keyDictionary.Keys)
                {
                    LicenseKey licenseKey=keyDictionary[key];
                    string[] rowArray = new string[] { licenseKey.CompanyName, licenseKey.Identifier, licenseKey.KeyString, licenseKey.DateGenerated, licenseKey.GeneratedBy };
                    dataGridViewRecords.Rows.Add(rowArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to display the record.\n" + ex.Message, "recordForm:DisplayRecords", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void WriteRecords()
        {
            try
            {
                if (!File.Exists(iniPath))
                {
                    FileStream fs = File.Create(iniPath);
                    fs.Close();
                }

                string tempFile = Path.GetTempFileName();
                using (StreamReader sr = new StreamReader(iniPath))
                {
                    using (StreamWriter sw = new StreamWriter(tempFile))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            sw.WriteLine("");
                        }
                    }
                }
                File.Delete(iniPath);
                File.Move(tempFile, iniPath);

                FileStream fileStream = File.Open(iniPath, FileMode.Create);
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    foreach (string key in keyDictionary.Keys)
                    {
                        LicenseKey licenseKey = keyDictionary[key];
                        string strLine = licenseKey.CompanyName + splitter[0] + licenseKey.Identifier + splitter[0] + licenseKey.KeyString + splitter[0] + licenseKey.DateGenerated + splitter[0] + licenseKey.GeneratedBy;
                        sw.WriteLine(strLine);
                    }
                    sw.Close();
                }
                fileStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to write the record of the generated license keys.\n" + ex.Message, "recordForm:ReadRecords", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            if (null != textBoxKeyword.Text)
            {
                if (textBoxKeyword.Text.Length > 0)
                {
                    string keywords = textBoxKeyword.Text;
                    foreach (DataGridViewRow row in dataGridViewRecords.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.Value.ToString().Contains(keywords))
                            {
                                cell.Selected = true;
                            }
                            else
                            {
                                cell.Selected = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public class LicenseKey
    {
        public string CompanyName { get; set; }
        public string Identifier { get; set; }
        public string KeyString { get; set; }
        public string DateGenerated { get; set; }
        public string GeneratedBy { get; set; }
    }
}
