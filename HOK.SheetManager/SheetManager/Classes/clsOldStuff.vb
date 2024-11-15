'' ''Imports Autodesk.Revit.DB

'' ''Public Class clsOldStuff

'' ''    Private Sub oldstuff()
'' ''        Dim dataTableLocal As System.Data.DataTable
'' ''        Dim listSheetsString As New List(Of String)
'' ''        Dim listSheetsId As New List(Of ElementId)
'' ''        Dim listTestRenumber As New List(Of String)
'' ''        Dim elementId As ElementId
'' ''        Dim viewSheet As ViewSheet = Nothing
'' ''        Dim columnFound As Boolean

'' ''        'Do not allow Access case
'' ''        If tabControl1.SelectedTab.Name = tabPageAccess.Name Then
'' ''            MessageBox.Show("Renumber not supported with Access database.  Processing stopped.")
'' ''            Return
'' ''        End If

'' ''        'Switch OleDb and Interop cases; get sheet values; assuming Excel
'' ''        If mRunAsInterop Then
'' ''            'Interop case (Assume proper interop application is running at this point.)
'' ''            mUtilityInterop.FillDataTableFromExcelWorksheet(listBoxTemplateSetExcel.SelectedItem.ToString())
'' ''            dataTableLocal = mUtilityInterop.DataTable
'' ''        Else
'' ''            mUtilityOleDb.FillDataTableFromExcelWorksheet(listBoxTemplateSetExcel.SelectedItem.ToString())
'' ''            dataTableLocal = mUtilityOleDb.DataTable
'' ''        End If

'' ''        'Notes on Excel.  The number of rows at this point is the total, including blanks, to the last line.
'' ''        'The use of "dataTable.Columns" correctly interprets the first line as columns and then
'' ''        'A loop using "row in dataTable.Rows" seems to get each subsequent row, including blanks
'' ''        'Another form is: Extended Properties=\"Excel 8.0;HDR=Yes\""); Apparently the HDR indicates that there is a header row.

'' ''        'Insure that the correct columns are present.
'' ''        columnFound = False
'' ''        For Each dataColumn As DataColumn In dataTableLocal.Columns
'' ''            If dataColumn.ColumnName = "OldNumber" Then
'' ''                columnFound = True
'' ''                Continue For
'' ''            End If
'' ''        Next
'' ''        If Not columnFound Then
'' ''            MessageBox.Show("No column named ""OldNumber"" found.  Processing stopped.")
'' ''            Return
'' ''        End If
'' ''        columnFound = False
'' ''        For Each dataColumn As DataColumn In dataTableLocal.Columns
'' ''            If dataColumn.ColumnName = "NewNumber" Then
'' ''                columnFound = True
'' ''                Continue For
'' ''            End If
'' ''        Next
'' ''        If Not columnFound Then
'' ''            MessageBox.Show("No column named ""NewNumber"" found.  Processing stopped.")
'' ''            Return
'' ''        End If

'' ''        'Start the progress bar
'' ''        Dim progressBarForm As New ProgressBar(dataTableLocal.Rows.Count + 2)
'' ''        progressBarForm.Text = "Checking Renumber Logic"
'' ''        progressBarForm.Show()
'' ''        progressBarForm.Increment()
'' ''        'To avoid transparent look while waiting
'' ''        'Get a list of existing sheets.  Note that "listSheetsInt" and "listSheetsString"
'' ''        'are matching lists with the element ID as an int and the sheet number as a string.
'' ''        Dim elementIterator As ElementIterator = mCommandData.Application.ActiveDocument.Elements
'' ''        elementIterator.Reset()
'' ''        While elementIterator.MoveNext()
'' ''            viewSheet = TryCast(elementIterator.Current, ViewSheet)
'' ''            If viewSheet Is Nothing Then
'' ''                Continue While
'' ''            End If
'' ''            listSheetsId.Add(viewSheet.Id)
'' ''            For Each parameterItem As Parameter In viewSheet.Parameters
'' ''                If parameterItem.Definition.Name = "Sheet Number" Then
'' ''                    listSheetsString.Add(parameterItem.AsString())
'' ''                    Continue For
'' ''                End If
'' ''            Next
'' ''        End While

'' ''        'Check for logic errors in rename
'' ''        For i As Integer = 0 To listSheetsString.Count - 1
'' ''            listTestRenumber.Add(listSheetsString(i))
'' ''        Next
'' ''        'listSheetsString.CopyTo(listTestRenumber);
'' ''        For Each row As DataRow In dataTableLocal.Rows

'' ''            'Increment the progress bar
'' ''            progressBarForm.Increment()

'' ''            'Check for missing values and ignore them
'' ''            If row("OldNumber").ToString() = "" Then
'' ''                Continue For
'' ''            End If
'' ''            If row("NewNumber").ToString() = "" Then
'' ''                Continue For
'' ''            End If

'' ''            'See if sheet exists, otherwise ignore it.  If found, check that new number doesn't exist.
'' ''            For i As Integer = 0 To listTestRenumber.Count - 1
'' ''                If listTestRenumber(i) = row("OldNumber").ToString() Then
'' ''                    For j As Integer = 0 To listTestRenumber.Count - 1
'' ''                        If listTestRenumber(j) = row("NewNumber").ToString() Then
'' ''                            MessageBox.Show(("Logical error at Old Number: " & row("OldNumber").ToString() & " New Number: ") + row("NewNumber"))
'' ''                            dataTableLocal.Clear()
'' ''                            If mUtilityOleDb.DataTable IsNot Nothing Then
'' ''                                mUtilityOleDb.DataTable.Clear()
'' ''                            End If
'' ''                            If mUtilityInterop.DataTable IsNot Nothing Then
'' ''                                mUtilityInterop.DataTable.Clear()
'' ''                            End If
'' ''                            progressBarForm.Close()

'' ''                            Return
'' ''                        End If
'' ''                    Next
'' ''                    listTestRenumber(i) = row("NewNumber").ToString()
'' ''                End If
'' ''            Next
'' ''        Next

'' ''        'Restart the progress bar
'' ''        progressBarForm.Text = "Renumbering Sheets"
'' ''        progressBarForm.Reset()
'' ''        progressBarForm.Increment()
'' ''        'To avoid transparent look while waiting
'' ''        'Process each row in rename table
'' ''        For Each row As DataRow In dataTableLocal.Rows

'' ''            'Increment the progress bar
'' ''            progressBarForm.Increment()

'' ''            'Check for missing values and ignore them
'' ''            If row("OldNumber").ToString() = "" Then
'' ''                Continue For
'' ''            End If
'' ''            If row("NewNumber").ToString() = "" Then
'' ''                Continue For
'' ''            End If

'' ''            'See if sheet exists, otherwise ignore it
'' ''            'existingSheet = false;
'' ''            For i As Integer = 0 To listSheetsString.Count - 1
'' ''                If listSheetsString(i) = row("OldNumber").ToString() Then
'' ''                    'existingSheet = true;
'' ''                    elementId = listSheetsId(i)
'' ''                    viewSheet = DirectCast(mCommandData.Application.ActiveDocument.get_Element(elementId), ViewSheet)
'' ''                    viewSheet.SheetNumber = row("NewNumber").ToString()
'' ''                    Continue For
'' ''                End If
'' ''            Next
'' ''        Next

'' ''        'Empty the data table since it seems to fill up with repeated use of the command
'' ''        dataTableLocal.Clear()
'' ''        If mUtilityOleDb.DataTable IsNot Nothing Then
'' ''            mUtilityOleDb.DataTable.Clear()
'' ''        End If
'' ''        If mUtilityInterop.DataTable IsNot Nothing Then
'' ''            mUtilityInterop.DataTable.Clear()
'' ''        End If

'' ''        'Close the progress bar and restore cursor.
'' ''        progressBarForm.Close()


'' ''        'Completed message
'' ''        MessageBox.Show("Processing completed.")
'' ''    End Sub


'' ''End Class
