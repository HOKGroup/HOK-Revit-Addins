using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RevitDBManager.HelperClasses;

namespace RevitDBManager.Helper
{
    class CellValueControl
    {
        enum IncrementType{None, AllInteger, StartInteger, EndInteger};
        private UnitConverter unitConverter;

        public CellValueControl()
        {
            unitConverter = new UnitConverter();
        }

        //incrementally increasing only in columns 
        public void AutoFillSeriese(DataGridView dataGridView, DataGridViewSelectedCellCollection selectedCells)
        {
            List<int> rowIndex = new List<int>(); //selected rows
            List<int> colIndex = new List<int>(); //selected columns
            foreach (DataGridViewCell cell in selectedCells)
            {
                if (!rowIndex.Contains(cell.RowIndex)) { rowIndex.Add(cell.RowIndex); }
                if (!colIndex.Contains(cell.ColumnIndex)) { colIndex.Add(cell.ColumnIndex); }
            }
            rowIndex.Sort();
            colIndex.Sort();

            string firstCell = ""; string secondCell = "";
            //at least two rows of cells are required to define a format of increment
            if (rowIndex.Count > 1)
            {
                
                for (int iCol = 0; iCol < colIndex.Count; iCol++)
                {
                    string suffix = "";
                    bool haveSuffix = false;
                    if (dataGridView.Columns[colIndex[iCol]].ReadOnly) { continue; }

                    if (null != dataGridView.Columns[colIndex[iCol]].Tag) { suffix = dataGridView.Columns[colIndex[iCol]].Tag.ToString(); haveSuffix = true; }

                    DataGridViewCell cell = dataGridView.Rows[rowIndex[0]].Cells[colIndex[iCol]];
                    firstCell = cell.Value.ToString();
                    IncrementType incrType = CheckIncrementType(firstCell);
                    if (incrType == IncrementType.None) { continue; /*Fill Copy*/}
                    cell = dataGridView.Rows[rowIndex[1]].Cells[colIndex[iCol]];
                    secondCell = cell.Value.ToString();
                    

                    int increment = FindIncrementValue(firstCell, secondCell, incrType);
                    for (int iRow = 0; iRow < rowIndex.Count; iRow++)
                    {
                        int value = 0; int digit = 0; string strPart = "";
                        switch (incrType)
                        {
                            case IncrementType.AllInteger:
                                value = int.Parse(firstCell) + increment * iRow;
                                cell = dataGridView.Rows[rowIndex[iRow]].Cells[colIndex[iCol]];
                                if (haveSuffix) { cell.Tag = unitConverter.GetDoubleValue(value.ToString(), suffix); }
                                cell.Value = value.ToString();
                                break;
                            case IncrementType.StartInteger:
                                digit = FindDigitOfInteger(firstCell, IncrementType.StartInteger);
                                strPart = firstCell.Substring(digit);
                                value = int.Parse(firstCell.Substring(0, digit)) + increment * iRow;
                                cell = dataGridView.Rows[rowIndex[iRow]].Cells[colIndex[iCol]];
                                if (haveSuffix) { cell.Tag = unitConverter.GetDoubleValue(value.ToString() + strPart, suffix); }
                                cell.Value = value.ToString()+strPart;
                                break;
                            case IncrementType.EndInteger:
                                digit = FindDigitOfInteger(firstCell, IncrementType.EndInteger);
                                strPart = firstCell.Substring(0,firstCell.Length-digit);
                                value = int.Parse(firstCell.Substring(firstCell.Length - digit)) + increment * iRow;
                                cell = dataGridView.Rows[rowIndex[iRow]].Cells[colIndex[iCol]];
                                if (haveSuffix) { cell.Tag = unitConverter.GetDoubleValue(strPart + value.ToString(), suffix); }
                                cell.Value = strPart+value.ToString();
                                break;
                        }
                    }
                }
            }
        }

        

        //increment will be other values than 1, only if the incrementType of first and second cell is same.
        private int FindIncrementValue(string str1, string str2, IncrementType iType)
        {
            int increment = 1;
            try
            {
                IncrementType incrType = CheckIncrementType(str2);
                int firstVal = 0; int secondVal = 0;
                if (iType == incrType)
                {
                    switch (iType)
                    {
                        case IncrementType.AllInteger:
                            firstVal = int.Parse(str1);
                            secondVal = int.Parse(str2);
                            increment = secondVal - firstVal;
                            break;
                        case IncrementType.StartInteger:
                            firstVal = int.Parse(str1.Substring(0, FindDigitOfInteger(str1, IncrementType.StartInteger)));
                            secondVal = int.Parse(str2.Substring(0, FindDigitOfInteger(str2, IncrementType.StartInteger)));
                            increment = secondVal - firstVal;
                            break;
                        case IncrementType.EndInteger:
                            firstVal = int.Parse(str1.Substring(str1.Length - FindDigitOfInteger(str1, IncrementType.EndInteger)));
                            secondVal = int.Parse(str2.Substring(str2.Length - FindDigitOfInteger(str2, IncrementType.EndInteger)));
                            increment = secondVal - firstVal;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed find increment value with increment type: " + iType.ToString()+"\n"+ex.Message, "CellValueControl Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return increment;
        }

        private int FindDigitOfInteger(string strNum, IncrementType iType)
        {
            int digit = 0;
            char[] charArr = strNum.ToCharArray();
            int integer = 0;
            
            switch (iType)
            {
                case IncrementType.StartInteger:
                    for (int i = 0; i < charArr.Length; i++)
                    {
                        if (int.TryParse(charArr[i].ToString(), out integer)) 
                        {
                            digit++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;

                case IncrementType.EndInteger:
                    for (int i = charArr.Length-1; i > -1; i--)
                    {
                        if (int.TryParse(charArr[i].ToString(), out integer)) 
                        {
                            digit++; 
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
            }
            return digit;
        }

        private bool isStartWitInteger(string str)
        {
            return false;
        }

        /* Case of Increment
         * case1. all integer
         * case2. starting with integer [int][string]
         * case3. ending with integer [string][int]
         * case4. none
         */

        private IncrementType CheckIncrementType(string str)
        {
            if (str == string.Empty) { return IncrementType.None; }

            char[] charArr = str.ToCharArray();
            int number;
            if (int.TryParse(str,out number))
            {
                return IncrementType.AllInteger;
            }
            else if (int.TryParse(charArr[0].ToString(), out number))
            {
                return IncrementType.StartInteger;
            }
            else if (int.TryParse(charArr[charArr.Length - 1].ToString(), out number))
            {
                return IncrementType.EndInteger;
            }
            else
            {
                return IncrementType.None;
            }
        }

        public void FillCopy(DataGridView dataGridView, DataGridViewSelectedCellCollection selectedCells)
        {
            List<int> rowIndex = new List<int>(); //selected rows
            List<int> colIndex = new List<int>(); //selected columns
            foreach (DataGridViewCell cell in selectedCells)
            {
                if (!rowIndex.Contains(cell.RowIndex)) { rowIndex.Add(cell.RowIndex); }
                if (!colIndex.Contains(cell.ColumnIndex)) { colIndex.Add(cell.ColumnIndex); }
            }
            rowIndex.Sort();
            colIndex.Sort();

            string firstCell = "";
            for (int iCol = 0; iCol < colIndex.Count; iCol++)
            {
                string suffix = "";
                bool haveSuffix = false;

                if (dataGridView.Columns[colIndex[iCol]].ReadOnly) { continue; }

                if (null != dataGridView.Columns[colIndex[iCol]].Tag) { suffix = dataGridView.Columns[colIndex[iCol]].Tag.ToString(); haveSuffix = true; }

                DataGridViewCell cell = dataGridView.Rows[rowIndex[0]].Cells[colIndex[iCol]];
                firstCell = cell.Value.ToString();
                for (int iRow = 0; iRow < rowIndex.Count; iRow++)
                {
                    cell = dataGridView.Rows[rowIndex[iRow]].Cells[colIndex[iCol]];
                    if (haveSuffix) { cell.Tag = unitConverter.GetDoubleValue(firstCell, suffix); }
                    cell.Value = firstCell;
                }
            }

        }
    }
}
