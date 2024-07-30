Imports System.Data
Imports System.IO
Imports Microsoft.Office.Interop.Excel
Imports Autodesk.Revit
Imports System.Windows.Forms

' Update INI functionality
'   Saved job settings for synchronization (by user?)
'   Views to creat... save for later or delete to queue

' Tree view of all titleblocks with types as child nodes

' New interface to add views to sheets
'   Drafting views as templates, linetypes to describe viewport size and location
'   Parameter rollups (from parent to Child, key configuration (sheet number?))

' New interface for view creation
'   Parameter population for setups?

' Onclick of template, show sheets items in datagrid

Public Class form_Main

    Private m_Settings As clsSettings
    Private m_UtilitySql As New clsUtilitySql
    Private m_UtilityInterop As clsUtilityInterop
    Private m_TitleBlock As DB.FamilySymbol
    Private m_TitleBlocks As New List(Of DB.Element)
    Private m_DataTable As System.Data.DataTable

    ' Current Application is Excel
    Private m_isExcel As Boolean

    Private mDictionarySheets As New Dictionary(Of String, DB.ViewSheet)
    Private mDictionaryViews As New Dictionary(Of String, DB.View)
    Private mElements As New List(Of DB.Element)
    Private mViewSheet As DB.ViewSheet
    Private mView As DB.View
    Private mParameterList As New List(Of String)
    Private mParameter As DB.Parameter

#Region "Class Constructor and Destructor"

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        'Always set to first, if there is only one then that's it.
        buttonCreate.Enabled = False
        buttonWriteData.Enabled = False
        buttonRenumber.Enabled = False
        buttonAddViews.Enabled = False

        ' General settings and class constructors
        m_Settings = settings
        m_UtilityInterop = New clsUtilityInterop(m_Settings)

        ' Dialog Title with Version data
        Me.Text = "Sheet Manager - " & m_Settings.ApplicationVersion
        Me.ProgressBar1.Visible = False

        ' Default to Excel if it Exists, otherwise use Access
        If File.Exists(m_Settings.ExcelPath) Then

            ' Excel
            m_isExcel = True

            ' Load the Excel File Data
            LabelFilePath.Text = m_Settings.ExcelPath

            ' Launch the App and Load the Worksheet List
            If m_UtilityInterop.StartExcel() Then
                openFileDialog1.FileName = m_Settings.ExcelPath
                FillTemplateList()
            Else
                ListBoxTableSet.Items.Clear()
                LabelFilePath.Text = "Click Access or Excel File to Continue"
            End If

        Else
            ' Try Access
            If File.Exists(m_Settings.AccessPath) Then

                ' Access
                m_isExcel = False

                ' Load the Access File Data
                LabelFilePath.Text = m_Settings.AccessPath

                ' Launch the App and Load the Worksheet List
                If m_UtilityInterop.StartAccess Then
                    openFileDialog1.FileName = m_Settings.AccessPath
                    FillTemplateList()
                Else
                    ListBoxTableSet.Items.Clear()
                    LabelFilePath.Text = "Click Access or Excel File to Continue"
                End If

            End If
        End If

        ' Always check on load
        checkBoxUpdateExisting.Checked = True

        ' Filter all titleblock families
        Dim colTitleblocks As New DB.FilteredElementCollector(m_Settings.Application.ActiveUIDocument.Document)
        colTitleblocks.WhereElementIsNotElementType()
        m_TitleBlocks = colTitleblocks.OfCategory(DB.BuiltInCategory.OST_TitleBlocks).ToElements

        ' Top node is family name, child is type name
        For Each x In m_TitleBlocks
            ' The Child Node
            Dim m_Node As New TreeNode
            m_Node.Name = x.Name
            m_Node.Tag = x.Name
            m_Node.Text = x.Name
            ' The Parent Node
            Dim m_NodeParent As New TreeNode
            m_NodeParent.Name = x.GetType.Name
            m_NodeParent.Tag = x.GetType.Name
            m_NodeParent.Text = x.GetType.Name
            ' Add the parent first if it doesn't already
            Try
                Me.TreeViewTitleBlocks.Nodes.Add(m_NodeParent)
            Catch ex As Exception
                ' do nothing
            End Try
            Try
                Me.TreeViewTitleBlocks.Nodes(x.GetType.Name).Nodes.Add(m_Node)
            Catch ex As Exception
                ' do nothing
            End Try

        Next

        'Fill the m_TitleBlock list box
        For Each fs As DB.FamilySymbol In m_Settings.Application.ActiveUIDocument.Document.TitleBlocks
            listBoxTitleblocks.Items.Add(fs.Name.ToString)
        Next
        If listBoxTitleblocks.Items.Count = 0 Then
            MessageBox.Show("There are no titleblocks in project, add one and try again.")
            Close()
        End If
        listBoxTitleblocks.SelectedIndex = 0

    End Sub

    ''' <summary>
    ''' Class Destructor
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        m_UtilityInterop.ShutDownAccess()
        m_UtilityInterop.ShutDownExcel()
        Me.Close()
    End Sub

#End Region

#Region "Private Functions"

    ''' <summary>
    ''' Fill the template listbox
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub FillTemplateList()

        ' The table object
        Dim dataTableLocal As System.Data.DataTable

        ' Clear both the lists in case of any problems with getting data
        ListBoxTableSet.Items.Clear()

        If m_isExcel = True Then
            m_UtilityInterop.FillDataTableFromExcelSheetNames("TemplateId")
            dataTableLocal = m_UtilityInterop.DataTable

            For Each row As DataRow In dataTableLocal.Rows
                ListBoxTableSet.Items.Add(row("TemplateId").ToString)
            Next
            If ListBoxTableSet.Items.Count = 0 Then
                MessageBox.Show("There are no valid sheets in Excel file.  Command cannot be used.")
                Return
            End If
            'Always set to first, if there is only one then that's it.
            ListBoxTableSet.SelectedIndex = 0

        Else
            Try

                m_UtilityInterop.FillDataTableFromAccessTable("Template")
                dataTableLocal = m_UtilityInterop.DataTable

                For Each row As DataRow In dataTableLocal.Rows
                    ListBoxTableSet.Items.Add(row("TemplateId").ToString)
                Next
                If ListBoxTableSet.Items.Count = 0 Then
                    MessageBox.Show("There are no valid sheets in Access file.  Command cannot be used.")
                    Return
                End If
                'Always set to first, if there is only one then that's it.
                ListBoxTableSet.SelectedIndex = 0
            Catch ex As Exception

            End Try

        End If

        ' Enable command buttons
        Me.buttonCreate.Enabled = True
        Me.buttonRenumber.Enabled = True
        Me.buttonAddViews.Enabled = True
        Me.buttonWriteData.Enabled = True

    End Sub

    ''' <summary>
    ''' Verifty existence of column
    ''' </summary>
    ''' <param name="dataTable">Data Table Name</param>
    ''' <param name="columnName">Column Name</param>
    ''' <returns>True on success</returns>
    ''' <remarks></remarks>
    Private Function CheckColumnPresent(ByVal dataTable As System.Data.DataTable, ByVal columnName As String) As Boolean

        ' Iterate all columns until match found
        For Each dataColumn As DataColumn In dataTable.Columns
            If dataColumn.ColumnName = columnName Then
                ' Column Exists
                Return True
            End If
        Next

        ' Column does not exist
        MessageBox.Show("No column named """ & columnName & """ found.  Processing stopped.", m_Settings.ProgramName)
        Return False

    End Function

    ''' <summary>
    ''' List of all Views
    ''' </summary>
    ''' <param name="mode"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetListOfViews(ByVal mode As String) As Boolean
        Select Case mode.ToUpper
            Case "SHEET"
                Dim CollectorSheets As New DB.FilteredElementCollector(m_Settings.Application.ActiveUIDocument.Document)
                CollectorSheets.OfCategory(DB.BuiltInCategory.OST_Sheets)
                mElements = CollectorSheets.ToElements
                For Each elementTest As DB.Element In mElements
                    mViewSheet = TryCast(elementTest, DB.ViewSheet)
                    If Not mDictionarySheets.ContainsKey(mViewSheet.SheetNumber) Then 'Not sure if this could ever happen
                        mDictionarySheets.Add(mViewSheet.SheetNumber, mViewSheet)
                    End If
                Next
                Exit Select
            Case "VIEW"
                Dim CollectorViews As New DB.FilteredElementCollector(m_Settings.Application.ActiveUIDocument.Document)
                CollectorViews.OfCategory(DB.BuiltInCategory.OST_Views)
                mElements = CollectorViews.ToElements

                For Each elementTest As DB.Element In mElements
                    mView = TryCast(elementTest, DB.View)
                    mViewSheet = TryCast(elementTest, DB.ViewSheet)
                    If mViewSheet IsNot Nothing Then
                        Continue For
                    End If
                    'We don't want to get any of the sheets in case they had the same name
                    If Not mDictionaryViews.ContainsKey(mView.ViewName) Then 'Not sure if this could ever happen
                        mDictionaryViews.Add(mView.ViewName, mView)
                    End If
                Next

                Exit Select
            Case Else

                MessageBox.Show("Unknown case.", m_Settings.ProgramName)
                Return False
        End Select
        Return True
    End Function

#End Region

    Private Sub buttonAddViews_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonAddViews.Click
        Dim dataTableLocal As System.Data.DataTable
        Dim pointInsert As New DB.UV
        Dim parameterViewSheetInfo As DB.Parameter
        Dim offsetU As Double = 0
        Dim offsetV As Double = 0
        Dim viewsNotPlaced As String = ""

        'Do not allow Access case
        If m_isExcel = False Then
            MessageBox.Show("Renumber not supported with Access database.  Processing stopped.", m_Settings.ProgramName)
            Return
        End If

        'Read the data from Excel to local table
        m_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        dataTableLocal = m_UtilityInterop.DataTable

        'Notes on Excel.  The number of rows at this point is the total, including blanks, to the last line.
        'The use of "dataTable.Columns" correctly interprets the first line as columns and then
        'A loop using "row in dataTable.Rows" seems to get each subsequent row, including blanks
        'Another form is: Extended Properties=\"Excel 8.0;HDR=Yes\""); Apparently the HDR indicates that there is a header row.

        'Insure that the correct columns are present.
        If Not CheckColumnPresent(dataTableLocal, "Sheet Number") Then
            Return
        End If
        If Not CheckColumnPresent(dataTableLocal, "View Name") Then
            Return
        End If
        If Not CheckColumnPresent(dataTableLocal, "U") Then
            Return
        End If
        If Not CheckColumnPresent(dataTableLocal, "V") Then
            Return
        End If

        'Start the progress bar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = dataTableLocal.Rows.Count + 1
            .Value = 0
            .Visible = True
        End With

        'To avoid transparent look while waiting
        'Get lists of existing sheets and views
        'if (!GetListOfViews("viewport")) return;
        If Not GetListOfViews("sheet") Then
            Return
        End If
        If Not GetListOfViews("view") Then
            Return
        End If

        'Process each row in rename table
        For Each row As DataRow In dataTableLocal.Rows

            'Increment the progress bar
            Me.ProgressBar1.Increment(1)

            'Check for missing values and ignore them
            If row("Sheet Number").ToString = "" Then
                Continue For
            End If
            If row("View Name").ToString = "" Then
                Continue For
            End If
            If row("U").ToString = "" Then
                Continue For
            End If
            If row("V").ToString = "" Then
                Continue For
            End If

            'See if sheet and view exist; if not ignore step
            If Not mDictionarySheets.ContainsKey(row("Sheet Number").ToString) Then
                Continue For
            End If
            If Not mDictionaryViews.ContainsKey(row("View Name").ToString) Then
                Continue For
            End If

            'Get the sheet and view and place view
            mViewSheet = mDictionarySheets(row("Sheet Number").ToString)
            mView = mDictionaryViews(row("View Name").ToString)

            'Confirm that UV values are good numbers and get the values
            Try
                offsetU = Convert.ToDouble(row("U").ToString)
                offsetV = Convert.ToDouble(row("V").ToString)
            Catch exception As Exception
                MessageBox.Show("Error in 'Start.buttonAddViews_Click' while converting U or V value to number. Processing stopped." & vbLf & vbLf & "System message: " & exception.Message, m_Settings.ProgramName)
                Me.ProgressBar1.Visible = False
                Return
            End Try

            'place the view
            Try
                parameterViewSheetInfo = mView.Parameter(DB.BuiltInParameter.VIEW_SHEET_VIEWPORT_INFO)
                If parameterViewSheetInfo.AsString.ToUpper = "NOT IN A SHEET" Then
                    pointInsert = New DB.UV(offsetU - mView.Outline.Min.U, offsetV - mView.Outline.Min.V)
                    mViewSheet.AddView(mView, pointInsert)
                Else
                    If viewsNotPlaced = "" Then
                        viewsNotPlaced = "The following views were not placed because they have already been placed on a sheet:" & vbLf & Convert.ToString(mView.Name)
                    Else
                        viewsNotPlaced = viewsNotPlaced & "; " & Convert.ToString(mView.Name)
                    End If
                End If
                'catch {
                '    //For now we are ignoring this error since it occurs when the view has already been placed
                '    continue;

            Catch exception As Exception
                MessageBox.Show((("Error in 'Start.buttonAddViews_Click' while placing view.  Processing stopped." & vbLf & "Sheet Number: """ & row("Sheet Number").ToString & """" & vbLf & "View Name: """ & row("View Name").ToString & """" & vbLf & "U: """ & row("U").ToString & """" & vbLf & "V: """ & row("V").ToString & """" & vbLf & "pointInsert.U: """) + pointInsert.U.ToString & """" & vbLf & "pointInsert.V: """) + pointInsert.V.ToString & """" & vbLf & vbLf & "System message: " & exception.Message, m_Settings.ProgramName)
                Me.ProgressBar1.Visible = False
                Return
            End Try
        Next

        'Empty the data table since it seems to fill up with repeated use of the command
        dataTableLocal.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False

        'Completed message
        If viewsNotPlaced = "" Then
            MessageBox.Show("Processing completed.")
        Else
            MessageBox.Show("Processing completed." & vbLf & vbLf & viewsNotPlaced)
        End If
    End Sub

    Private Sub buttonRenumber_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonRenumber.Click
        Dim listTestRenumber As New List(Of String)

        'Do not allow Access case
        If m_isExcel = False Then
            MessageBox.Show("Renumber not supported with Access database.  Processing stopped.")
            Return
        End If

        'Get sheet values; assuming Excel
        m_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        m_DataTable = m_UtilityInterop.DataTable
        'Notes on Excel.  The number of rows at this point is the total, including blanks, to the last line.
        'The use of "dataTable.Columns" correctly interprets the first line as columns and then
        'A loop using "row in dataTable.Rows" seems to get each subsequent row, including blanks
        'Another form is: Extended Properties=\"Excel 8.0;HDR=Yes\""); Apparently the HDR indicates that there is a header row.

        'Insure that the correct columns are present.
        If Not CheckColumnPresent(m_DataTable, "OldNumber") Then
            Return
        End If
        If Not CheckColumnPresent(m_DataTable, "NewNumber") Then
            Return
        End If

        'Start the progress bar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 2
            .Value = 0
            .Visible = True
        End With
        Me.ProgressBar1.Increment(1)

        'To avoid transparent look while waiting, get list of existing sheets.
        If Not GetListOfViews("sheet") Then
            Return
        End If

        'Check for logic errors in rename
        For Each sheetNumber As String In mDictionarySheets.Keys
            listTestRenumber.Add(sheetNumber)
        Next


        For Each row As DataRow In m_DataTable.Rows

            'Increment the progress bar
            Me.ProgressBar1.Increment(1)

            'Check for missing values and ignore them
            If row("OldNumber").ToString = "" Then
                Continue For
            End If
            If row("NewNumber").ToString = "" Then
                Continue For
            End If

            'See if sheet exists, otherwise ignore it.  If found, check that new number doesn't exist.
            If listTestRenumber.Contains(row("OldNumber").ToString) Then
                If listTestRenumber.Contains(row("NewNumber").ToString) Then
                    MessageBox.Show("Logical error at Old Number: " & row("OldNumber").ToString & " New Number: " & Convert.ToString(row("NewNumber")))
                    m_DataTable.Clear()
                    'if (mUtilityOleDb.DataTable != null) mUtilityOleDb.DataTable.Clear();
                    If m_UtilityInterop.DataTable IsNot Nothing Then
                        m_UtilityInterop.DataTable.Clear()
                    End If
                    Me.ProgressBar1.Visible = False
                    Return
                End If
                listTestRenumber.Remove(row("OldNumber").ToString)
                listTestRenumber.Add(row("NewNumber").ToString)
            End If
        Next

        'Restart the progress bar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 2
            .Value = 0
            .Visible = True
        End With
        Me.ProgressBar1.Increment(1)



        'To avoid transparent look while waiting
        'Process each row in rename table
        For Each row As DataRow In m_DataTable.Rows

            'Increment the progress bar
            Me.ProgressBar1.Increment(1)

            'Check for missing values and ignore them
            If row("OldNumber").ToString = "" Then
                Continue For
            End If
            If row("NewNumber").ToString = "" Then
                Continue For
            End If

            'See if sheet exists, otherwise ignore it.  If found renumber and update dictionary
            If mDictionarySheets.ContainsKey(row("OldNumber").ToString) Then
                mViewSheet = mDictionarySheets(row("OldNumber").ToString)
                mViewSheet.SheetNumber = row("NewNumber").ToString
                mDictionarySheets.Add(row("NewNumber").ToString, mDictionarySheets(row("OldNumber").ToString))
                mDictionarySheets.Remove(row("OldNumber").ToString)
            End If
        Next

        'Empty the data table since it seems to fill up with repeated use of the command
        m_DataTable.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False

        'Completed message
        MessageBox.Show("Processing completed.")
    End Sub

    Private Sub buttonWriteData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonWriteData.Click
        Dim dataRow As DataRow = Nothing
        Dim dataRowArray As DataRow() = New DataRow(0) {}
        Dim dlgResult As DialogResult

        'Do not allow Access case
        If m_isExcel = False Then
            MessageBox.Show("Renumber not supported with Access database.  Processing stopped.")
            Return
        End If

        'Warn user about overwriting data
        Dim msg As String = "Warning!  This command may overwrite data.  Be sure that the proper data dable is selected" & vbLf & "before proceeding." & vbLf & vbLf & "Select 'Cancel' to stop, or 'OK' to continue."
        dlgResult = MessageBox.Show(msg, Convert.ToString(m_Settings.ProgramName) & " Warning", MessageBoxButtons.OKCancel)
        If dlgResult <> DialogResult.OK Then
            Return
        End If

        'Get sheet values; assuming Excel
        m_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        m_DataTable = m_UtilityInterop.DataTable

        ' Is there a "Sheet Number" column?
        If Not CheckColumnPresent(m_DataTable, "Sheet Number") Then
            Return
        End If

        'Get list of existing sheets since we'd like to count them at this point
        If Not GetListOfViews("sheet") Then
            Return
        End If
        If mDictionarySheets.Count = 0 Then
            MessageBox.Show("No sheets to process.", m_Settings.ProgramName)
            Return
        End If

        'Start the progress bar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = mDictionarySheets.Count + 2
            .Value = 0
            .Visible = True
        End With
        Me.ProgressBar1.Increment(1)

        'To avoid transparent look while waiting
        'Determine parameter list from matching parameters and fields
        For Each dataColumn As DataColumn In m_DataTable.Columns
            mParameter = mViewSheet.Parameter(dataColumn.ColumnName)
            If mParameter IsNot Nothing Then
                mParameterList.Add(dataColumn.ColumnName)
            End If
        Next

        'Loop through all sheets, find record and update it or make a new record
        'Note on logic errors:
        '  We are assuming that Revit will not allow a duplicate Sheet Number
        '  If we find a duplicate in the table we report error and quit           
        For Each sheetNumber As String In mDictionarySheets.Keys
            mViewSheet = mDictionarySheets(sheetNumber)
            mParameter = mViewSheet.Parameter("Sheet Number")
            dataRowArray = m_DataTable.[Select]("[Sheet Number] = '" + mParameter.AsString() & "'")
            If dataRowArray.Length > 1 Then
                'duplicate record; error.
                MessageBox.Show("Duplicate Sheet Number in table: " & sheetNumber & ". Processing stopped.")
                Me.ProgressBar1.Visible = False
                Return
            End If
            If dataRowArray.Length = 0 Then
                'no record; make a new one
                dataRow = m_DataTable.NewRow()
                For Each parameterName As String In mParameterList
                    mParameter = mViewSheet.Parameter(parameterName)
                    If mParameter Is Nothing Then
                        Continue For
                    End If
                    dataRow(parameterName) = mParameter.AsString()
                Next
                m_DataTable.Rows.Add(dataRow)
            Else
                'existing record
                dataRow = dataRowArray(0)
                For Each parameterName As String In mParameterList
                    mParameter = mViewSheet.Parameter(parameterName)
                    If mParameter Is Nothing Then
                        Continue For
                    End If
                    If mParameter.AsString() <> dataRow(parameterName).ToString Then
                        dataRow(parameterName) = mParameter.AsString()
                    End If
                Next
            End If

            Me.ProgressBar1.Increment(1)

        Next

        'Write the changes back to the Access/Excel file
        m_UtilityInterop.FillExcelWorksheetFromDataTable(ListBoxTableSet.SelectedItem.ToString)
        m_UtilityInterop.DataTable.Clear()
        'Seems to fill up with repeated use of the command?
        'Close the progress bar
        Me.ProgressBar1.Visible = False

        'Completed message
        MessageBox.Show("Processing completed.", m_Settings.ProgramName)

        Me.Close()

    End Sub

    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click
        Dim listSheetsString As New List(Of String)
        Dim listSheetsId As New List(Of DB.ElementId)
        Dim firstSheet As Boolean = True
        Dim existingSheet As Boolean

        'Switch between Access and Excel cases and get sheet values
        If m_isExcel = False Then
            m_UtilityInterop.FillDataTableFromAccessTable("TemplateSheet", "TemplateId = " + m_UtilitySql.LiteralOrNull(ListBoxTableSet.SelectedItem.ToString))
        Else
            'assuming "tabPageExcel"
            m_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        End If

        m_DataTable = m_UtilityInterop.DataTable

        'Insure that there is a "Sheet Number" column.
        If Not CheckColumnPresent(m_DataTable, "Sheet Number") Then
            Return
        End If

        'Start the progress bar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 1
            .Value = 1
            .Visible = True
        End With

        'Get titleblock selection
        For Each titleBlockTest As DB.FamilySymbol In m_Settings.Application.ActiveUIDocument.Document.TitleBlocks
            If titleBlockTest.Name.ToString = listBoxTitleblocks.SelectedItem.ToString Then
                m_TitleBlock = titleBlockTest
                Exit For
            End If
        Next

        'Get list of existing sheets
        If Not GetListOfViews("sheet") Then
            Return
        End If

        'Process each row in template table
        For Each row As DataRow In m_DataTable.Rows

            'Increment the progress bar
            Me.ProgressBar1.Increment(1)

            'Check for empty sheet number, which can arise with Excel case
            If row("Sheet Number").ToString = "" Then
                Continue For
            End If

            'See if it is an existing sheet, if it is, get the sheet.
            existingSheet = False
            If mDictionarySheets.ContainsKey(row("Sheet Number").ToString) Then
                existingSheet = True
                mViewSheet = mDictionarySheets(row("Sheet Number").ToString)
            End If

            'If here, then sheet did not exist.  Make the new sheet and skip existing sheets if option to update not selected.
            If existingSheet Then
                If Not checkBoxUpdateExisting.Checked Then
                    Continue For
                End If
            Else

                Dim docCreation As Autodesk.Revit.Creation.Document = m_Settings.Application.ActiveUIDocument.Document.Create
                mViewSheet = docCreation.NewViewSheet(m_TitleBlock)

            End If

            'Determine parameter list from matching parameters and fields
            If firstSheet Then
                For Each dataColumn As DataColumn In m_DataTable.Columns
                    mParameter = mViewSheet.Parameter(dataColumn.ColumnName)
                    If mParameter IsNot Nothing Then
                        mParameterList.Add(dataColumn.ColumnName)
                    End If
                Next
                firstSheet = False
            End If

            'Set the sheet parameters only if they are different to avoid unnecessary edits
            For Each parameterName As String In mParameterList
                mParameter = mViewSheet.Parameter(parameterName)
                If mParameter Is Nothing Then
                    Continue For
                End If
                If mParameter.AsString() <> row(parameterName).ToString Then
                    mParameter.[Set](row(parameterName).ToString)
                End If
            Next
        Next

        'Empty the data table since it seems to fill up with repeated use of the command
        m_DataTable.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False

        'Completed message
        MessageBox.Show("Processing completed.")

        Me.Close()

    End Sub

#Region "Access and Excel Selection Buttons"

    ''' <summary>
    ''' Select and Connect to Excel
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ButtonUseExcel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonUseExcel.Click

        openFileDialog1.FileName = ""

        ' Gray out the unused file format button
        m_isExcel = True
        Me.ButtonUseAccess.Image = HOK.SheetManager.My.Resources.BW_MsAccess
        Me.ButtonUseExcel.Image = HOK.SheetManager.My.Resources.Excel2007Logo_Med

        ' Set the file dialog open filter
        openFileDialog1.Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"

        ' Startup Path for selection
        If File.Exists(m_Settings.ExcelPath) Then
            ' Start the file selection path near existing file
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(m_Settings.ExcelPath)
        Else
            ' Start the file selection path near ini
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(m_Settings.IniPath)
        End If

        ' Select a valid document for connection
        Me.openFileDialog1.ShowDialog()

        ' Test for valid file name
        If openFileDialog1.FileName = "" Or File.Exists(openFileDialog1.FileName) = False Then
            ' Error, no valid file
        Else
            m_Settings.ExcelPath = openFileDialog1.FileName
        End If

        ' Launch the App and Load the Worksheet List
        If m_UtilityInterop.StartExcel() Then
            FillTemplateList()
        Else
            ListBoxTableSet.Items.Clear()
            LabelFilePath.Text = "Click Access or Excel File to Continue"
        End If

        ' Set the form label to the file path
        LabelFilePath.Text = m_Settings.ExcelPath

    End Sub

    ''' <summary>  
    ''' Select and Connect to Access
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ButtonUseAccess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonUseAccess.Click

        openFileDialog1.FileName = ""

        ' Gray out the unused file format button
        m_isExcel = False
        Me.ButtonUseAccess.Image = HOK.SheetManager.My.Resources.MsAccess_Med
        Me.ButtonUseExcel.Image = HOK.SheetManager.My.Resources.excel2007logo_BW

        ' Set the file dialog open filter
        openFileDialog1.Filter = "Access Files (*.mdb;*.accdb)|*.mdb;*.accdb|All Files (*.*)|*.*"

        ' Startup Path for selection
        If File.Exists(m_Settings.AccessPath) Then
            ' Start the file selection path near existing file
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(m_Settings.AccessPath)
        Else
            ' Start the file selection path near ini
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(m_Settings.IniPath)
        End If

        ' Select a valid document for connection
        Me.openFileDialog1.ShowDialog()

        ' Test for valid file name
        If openFileDialog1.FileName = "" Or File.Exists(openFileDialog1.FileName) = False Then
            ' Error, no valid file
        Else
            m_Settings.AccessPath = openFileDialog1.FileName
        End If

        ' Launch the App and Load the Worksheet List
        If m_UtilityInterop.StartAccess() Then
            FillTemplateList()
        Else
            ListBoxTableSet.Items.Clear()
            LabelFilePath.Text = "Click Access or Excel File to Continue"
        End If

        ' Set the form label to the file path
        LabelFilePath.Text = m_Settings.AccessPath

    End Sub

#End Region

End Class