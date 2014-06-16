using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic;
using HOK.BCFReader.GenericForms;

namespace HOK.BCFReader.GenericClasses
{
    public enum RunTypes { Trial = 0, Expired = 1, Unknown = 3 }

    public class TrialMaker
    {
        private string productName;
        private string hideFilePath;
        private string companyName;
        private string identifier;
        private string licenseKey;
        private string userPassword;
        private string productID;
        private string productGuid;

        private bool isPassed;
        private int trialDays;
        private RunTypes runType;

        public string ProductName { get { return productName; } set { productName = value; } }
        public string HideFilePath { get { return hideFilePath; } set { hideFilePath = value; } }
        public string CompanyName { get { return companyName; } set { companyName = value; } }
        public string Identifier { get { return identifier; } set { identifier = value; } }
        public string UserPassword { get { return userPassword; } set { userPassword = value; } }
        public string LicenseKey { get { return licenseKey; } set { licenseKey = value; } }
        public string ProductID { get { return productID; } set { productID = value; } }
        public bool IsPassed { get { return isPassed; } set { isPassed = value; } }
        public RunTypes RunType { get { return runType; } set { runType = value; } }
        public string ProductGuid { get { return productGuid; } set { productGuid = value; } }
        public int TrialDays { get { return trialDays; } set { trialDays = value; } }

        public TrialMaker(string toolName, string hideFile)
        {
            productName = toolName;
            hideFilePath = hideFile;
            Guid guid = new Guid("{A81E6B45-A8FF-49B7-8AF3-1EF5FC8F3027}");
            productGuid = guid.ToString();

            runType = CheckRegistration();
            if (runType == RunTypes.Trial && trialDays > 10)
            {
                isPassed = true;
            }
            else
            {
                isPassed = false;
            }
        }

        private RunTypes CheckRegistration()
        {
            try
            {
                if (File.Exists(hideFilePath))
                {
                    trialDays = CheckHideFile();
                    if (trialDays == 0)
                    {
                        runType = RunTypes.Expired;
                    }
                    else
                    {
                        runType = RunTypes.Trial;
                    }
                }
                else
                {
                    runType = RunTypes.Unknown;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to check the registration condition.\n"+ex.Message, "TrialMaker:CheckRegistration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return runType;
        }

        private int CheckHideFile()
        {
            string[] HideInfo;
            HideInfo = FileReadWrite.ReadFile(hideFilePath).Split(';');
            long DiffDays;
            int DaysToEnd;
            int defDays=0;

            DaysToEnd = Convert.ToInt32(HideInfo[1]);
            if (DaysToEnd <= 0)
            {
                defDays = 0;
                return 0;
            }
            DateTime dt = new DateTime(Convert.ToInt64(HideInfo[0]));
            DiffDays = DateAndTime.DateDiff(DateInterval.Day,
                dt.Date, DateTime.Now.Date,
                FirstDayOfWeek.Saturday,
                FirstWeekOfYear.FirstFullWeek);

            DaysToEnd = Convert.ToInt32(HideInfo[1]);
            DiffDays = Math.Abs(DiffDays);

            defDays = DaysToEnd - Convert.ToInt32(DiffDays);

            if (defDays <= 0)
            {
                defDays = 0;
            }
            return defDays;
        }

        public bool FirstActivate()
        {
            bool result = false;
            try
            {
                productID = "";
                productID = GetProductId(); //combination of a company name and guid
                productID = Encryption.Boring(Encryption.InverseByBase(productID, 10));

                licenseKey = Encryption.MakePassword(productID, identifier);

                if (userPassword == licenseKey)
                {
                    MakeHideFile();
                    result = true;
                }
                else
                {
                    MessageBox.Show("The password is incorrect.\n Please enter a valid activation code.", "Incorrect Activation Code", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    result = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to activate the BCF Reader.\n" + ex.Message, "TrialMaker:FirstActivate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                result = false;
            }
            return result;
        }

        public bool Reactivate()
        {
            bool result=false;
            try
            {
                productID = "";
                productID = GetProductId(); //combination of a company name and guid
                productID = Encryption.Boring(Encryption.InverseByBase(productID, 10));

                licenseKey = Encryption.MakePassword(productID, identifier);

                if (userPassword == licenseKey)
                {
                    string[] hideInfo = FileReadWrite.ReadFile(hideFilePath).Split(';');
                    for (int i = 4; i < hideInfo.Length; i++)
                    {
                        if (hideInfo[i] == identifier)
                        {
                            MessageBox.Show("You've already tried with the activation code.\nTry to get another identifier with a valid activation code.", "Failed to activate.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                    }
                    string hideData;
                    hideData = DateTime.Now.Ticks.ToString();
                    for (int i = 1; i < hideInfo.Length; i++)
                    {
                        hideData += ";" + hideInfo[i];
                    }
                    hideData += ";" + identifier;

                    if (File.Exists(hideFilePath))
                    {
                        File.Delete(hideFilePath);
                    }

                    FileReadWrite.WriteFile(hideFilePath, hideData);
                    trialDays = 90;
                    result = true;
                }
                else
                {
                    MessageBox.Show("The password is incorrect.\n Please enter a valid activation code.", "Incorrect Password", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    result = false;
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to reactivate the BCF Reader.\n"+ex.Message, "TrialMaker:Reactivate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private string GetProductId()
        {
            try
            {
                productID += companyName;
                productID += productGuid;
                productID = RemoveUseLess(productID);

                return productID.Substring(0,25).ToUpper();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get a product ID.\n"+ex.Message, "TrialMaker:GetProductId", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private string RemoveUseLess(string baseString)
        {
            char ch;
            for (int i = baseString.Length - 1; i >= 0; i--)
            {
                ch = char.ToUpper(baseString[i]);
                if ((ch < 'A' || ch > 'Z') && (ch < '0' || ch > '9')) 
                {
                    baseString = baseString.Remove(i, 1);
                }
            }
            return baseString;
        }

        private void MakeHideFile()
        {
            try
            {
                string HideInfo;
                trialDays = 90;
                int tempRunTimes = 10000;
                HideInfo = DateTime.Now.Ticks + ";";
                HideInfo += trialDays + ";" + tempRunTimes + ";" + productID + ";" + identifier;
                FileReadWrite.WriteFile(hideFilePath, HideInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to make a hide file.\n"+ex.Message, "TrialMaker:MakeHideFile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
