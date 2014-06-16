using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RevitDBManager.ViewerForms;
using System.Data;
using System.Windows.Forms;

namespace RevitDBManager.HelperClasses
{
    public class UnitConverter
    {
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, string/*DUT*/>> dutDictionary = new Dictionary<string, Dictionary<string, string>>();
        private Dictionary<string/*tableName*/, Dictionary<string/*paramName*/, string/*suffix*/>> suffixDictionary = new Dictionary<string, Dictionary<string, string>>();
        private DataSet revitDataSet;

        private char[] generalSuffix = new char[] { '%', '"', '\'', '°' };

        public Dictionary<string, Dictionary<string, string>> DUTDictionary { get { return dutDictionary; } set { dutDictionary = value; } }
        public Dictionary<string, Dictionary<string, string>> SuffixDictionary { get { return suffixDictionary; } set { suffixDictionary = value; } }
        public DataSet RevitDataSet { get { return revitDataSet; } set { revitDataSet = value; } }

        public UnitConverter()
        {
        }

        public void CollectParamSuffix()
        {
            string tableName = "";
            string paramName = "";
            try
            {
                foreach (DataTable dTable in revitDataSet.Tables)
                {
                    tableName = dTable.TableName;
                    if (!suffixDictionary.ContainsKey(tableName)) { suffixDictionary.Add(tableName, new Dictionary<string, string>()); }
                    if (!dutDictionary.ContainsKey(tableName)) { continue; }

                    foreach (DataColumn column in dTable.Columns)
                    {
                        paramName = column.ColumnName;
                        if (dutDictionary[tableName].ContainsKey(paramName)) //columnName=>paramName
                        {
                            string dut = dutDictionary[tableName][paramName];
                            if (dTable.Rows.Count > 0)
                            {
                                string sampleVal = dTable.Rows[0][column].ToString();
                                if (dut.Contains("DUT_FEET_FRACTIONAL_INCHES"))
                                {
                                    suffixDictionary[tableName].Add(paramName, "FFI");
                                }
                                else if (dut.Contains("DUT_FRACTIONAL_INCHES"))
                                {
                                    suffixDictionary[tableName].Add(paramName, "FI");
                                }
                                else if(sampleVal!=string.Empty)
                                {
                                    string suffix = GetSuffix(sampleVal);
                                    if (suffix != string.Empty)
                                    {
                                        suffixDictionary[tableName].Add(paramName, suffix);
                                    }
                                }
                                else if (sampleVal == string.Empty)
                                {
                                    foreach (DataRow row in dTable.Rows)
                                    {
                                        if (row[column].ToString() == string.Empty) { continue; }
                                        else
                                        {
                                            sampleVal = row[column].ToString();
                                            string suffix = GetSuffix(sampleVal); //until the sample value is not empty
                                            if (suffix != string.Empty)
                                            {
                                                suffixDictionary[tableName].Add(paramName, suffix);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect parameters suffixes.Table Name: "+tableName+" ["+paramName+"]\n"+ex.Message, "UnitConverter Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public string GetSuffix(string strValue)
        {
            string suffix = "";
            double lastDigit=0;
            
            if(generalSuffix.Contains(strValue[strValue.Length - 1]))
            {
                suffix = strValue[strValue.Length - 1].ToString();
            }
            else if (strValue.Contains(" ")) //eg. 10.00 mm
            {
                string[] strSplit = strValue.Split(' ');
                suffix = strSplit[1];
            }
            else if(double.TryParse(strValue, out lastDigit))
            {
                suffix = "NONE";
            }
            
            return suffix;
        }

        public void AddDoubleValueTag(string tableName, DataGridView mainGridView)
        {
            if (suffixDictionary.ContainsKey(tableName))
            {
                foreach (DataGridViewColumn column in mainGridView.Columns)
                {
                    string paramName = column.Name;
                    if (suffixDictionary[tableName].ContainsKey(paramName))
                    {
                        string suffix = suffixDictionary[tableName][paramName];
                        if (generalSuffix.Contains(suffix[0]) || suffix.Contains("FFI") || suffix.Contains("FI") ||suffix.Contains("NONE")) { column.Tag = suffix; }
                        else { column.Tag = " " + suffix; }

                        foreach (DataGridViewRow row in mainGridView.Rows)
                        {
                            string value = row.Cells[paramName].Value.ToString();
                            row.Cells[paramName].Tag = GetDoubleValue(value, suffix);
                        }
                    }
                }
            }
        }

        public double GetDoubleValue(string strValue, string suffix)
        {
            double dblVal = 0;
            try
            {
                if (suffix == "FFI" && ValidateFormat(strValue, suffix)) //DUT_FEET_FRACTIONAL_INCHES [FFI]  eg.81' - 3 23/256"
                {
                    string[] feetInches = strValue.Split('-');
                    if (feetInches.Length == 2)
                    {
                        double dblFeet = 0;
                        string feet = feetInches[0].Replace("'", "");
                        double.TryParse(feet, out dblFeet); //81

                        double dblInches = 0;
                        string inches = feetInches[1].Replace("\"", ""); // 3 23/256
                        string[] division = inches.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                        if (division.Length == 2)//eg.81' - 3 23/256"
                        {
                            string quotient = division[0]; //3
                            string[] fraction = division[1].Split('/');
                            if (fraction.Length == 2)
                            {
                                string nominator = fraction[0];//23
                                string denominator = fraction[1];//256
                                dblInches = double.Parse(nominator) / double.Parse(denominator);
                            }
                            dblInches = double.Parse(quotient) + dblInches; //3 + 23/256
                            dblVal = dblFeet + dblInches / 12;
                        }
                        else if (division.Length == 1) //eg. 81' - 3"
                        {
                            dblInches = double.Parse(division[0]); //3 
                            dblVal = dblFeet + dblInches / 12;
                        }
                    }
                }
                else if (suffix == "FI" && ValidateFormat(strValue, suffix)) //DUT_FRACTIONAL_INCHES  eg. 3 23/256"
                {
                    strValue = strValue.Replace("\"", "");
                    string[] division = strValue.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (division.Length == 2)
                    {
                        double dblInches = 0;
                        string quotient = division[0]; //3
                        string[] fraction = division[1].Split('/');
                        if (fraction.Length == 2)
                        {
                            string nominator = fraction[0];//23
                            string denominator = fraction[1];//256
                            dblInches = double.Parse(nominator) / double.Parse(denominator);
                        }
                        dblVal = double.Parse(quotient) + dblInches; //3 + 23/256
                    }
                }
                else if (strValue.Contains(suffix))
                {
                    string dblStr = strValue.Replace(suffix, "");
                    dblStr = dblStr.Replace(" ", string.Empty);
                    double.TryParse(dblStr, out dblVal);
                }
                else if (suffix == "NONE" && ValidateFormat(strValue, suffix))
                {
                    double.TryParse(strValue, out dblVal);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get double value.\n"+ex.Message, "UnitConverter Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }            
            return dblVal;
        }

        public bool ValidateFormat(string strValue, string suffix)
        {
            bool valid = false;

            if (suffix == "FFI")
            {
                if (strValue.Contains("-") && strValue.Contains("'") && strValue.Contains("\""))
                {
                    if (strValue.Split('-').Length == 2)
                    {
                        valid = true;
                    }
                }
            }
            else if (suffix == "FI")
            {
                if (strValue[strValue.Length - 1] == '"')
                {
                    valid = true;
                }
            }
            else if (suffix == "NONE")
            {
                double dblVal = 0;
                if(double.TryParse(strValue, out dblVal))
                {
                    valid = true;
                }
            }

            return valid;
        }

        //to fix common mistake of error in formating
        public string FixToValidFormat(string strValue, string suffix)
        {
            string fixedValue = "";
            char[] separator = new char[] { '\'', '"' };
            if (suffix == "FFI")
            {
                if (!strValue.Contains("-") && strValue.Contains("\"") && strValue.Contains("'")) //10' 3 34/256"
                {
                    string[] splitVal = strValue.Split(separator);
                    if (splitVal.Length == 3)
                    {
                        if (splitVal[1][0] != ' ') { splitVal[1] = " " + splitVal[1]; } //10'3 34/256"
                        fixedValue = string.Format("{0}' -{1}\"", splitVal[0], splitVal[1]); 
                    }
                    else
                    {
                        fixedValue = "0' - 0\"";
                    }
                }
            }
            else if (suffix == "FI")
            {

            }
            return fixedValue;
        }

        public string ConvertDecimalToFractional(string strValue, string suffix)
        {
            string fractionalVal = "";
            double dblValue = 0;
            int feetVal = 0;
            int inchVal = 0;
            int nomiVal = 0;
            int denomiVal = 0;

         
            if (!double.TryParse(strValue, out dblValue)) //invalid format
            {
                if (suffix == "FFI")
                {
                    return "0' - 0\"";
                }
                if (suffix == "FI")
                {
                    return "0\"";
                }
                if (suffix == "NONE")
                {
                    return "0";
                }
            }

            /*get values*/


            if (suffix == "FFI")
            {
                feetVal = (int)dblValue;
                double dblInch=(dblValue - feetVal)*12;
                inchVal = (int)dblInch;
                double frInch = dblInch - inchVal;
                denomiVal = 256;
                nomiVal = (int)(frInch * 256);

                if (frInch!=0)
                {
                    fractionalVal = string.Format("{0}' - {1} {2}/{3}\"",feetVal, inchVal, nomiVal, denomiVal);
                }
                else
                {
                    fractionalVal = string.Format("{0}' - {1}\"",feetVal, inchVal);
                }
            }
            else if (suffix == "FI")
            {
                inchVal = (int)dblValue;
                double dblInch = dblValue - inchVal;
                denomiVal = 256;
                nomiVal = (int)(dblInch * 256);

                if (dblInch != 0)
                {
                    fractionalVal = string.Format("{0} {1}/{2}\"", inchVal, nomiVal, denomiVal);
                }
                else
                {
                    fractionalVal = string.Format("{0}\"", inchVal);
                }
            }
            else if (suffix == "NONE")
            {
                fractionalVal = strValue;
            }

            return fractionalVal;
        }
    }
}
