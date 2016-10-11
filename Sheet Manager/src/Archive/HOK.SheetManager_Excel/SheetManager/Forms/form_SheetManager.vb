Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Data
Imports System.IO
Imports Microsoft.Office.Interop
Imports System.Text


' Update INI functionality
'   Saved job settings for synchronization (by user?)
'   Views to creat... save for later or delete to queue

' New interface to add views to sheets
'   Drafting views as templates, linetypes to describe viewport size and location
'   Parameter rollups (from parent to Child, key configuration (sheet number?))

' New interface for view creation
'   Parameter population for setups?

' Onclick of template, show sheets items in datagrid

Public Class form_SheetManager

    ' Private References
    Private m_Settings As clsSettings
    Private m_UtilitySql As New clsUtilitySQL
    Private m_UtilityInterop As clsUtilityInterop
    Private m_TitleBlock As FamilySymbol = Nothing
    Private m_TitleBlocks As IList(Of Element)
    Private m_Sheets As New List(Of Element)
    Private m_DataTable As System.Data.DataTable
    Private m_ImageList As New System.Windows.Forms.ImageList
    Private m_isExcel As Boolean
    Private m_IsLoaded As Boolean = False
    Private m_ViewSheet As ViewSheet
    Private m_View As View
    Private m_ParameterList As New List(Of String)
    Private m_ParamTblk As Parameter


#Region "Class Constructor and Destructor"

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        ' Build imagelist
        m_ImageList.Images.Add(My.Resources._New)
        m_ImageList.Images.Add(My.Resources.Update)
        TreeViewSheets.ImageList = m_ImageList

        ' Button Visibility Setups
        buttonUpdateCreate.Enabled = False
        buttonExportData.Enabled = False
        buttonRenumberSheets.Enabled = False
        buttonAddViewsToSheets.Enabled = False
        ProgressBar1.Visible = False
        ComboBoxFilter.SelectedIndex = 0


        ' General settings and class constructors
        m_Settings = settings
        m_UtilityInterop = New clsUtilityInterop(m_Settings)

        ' Dialog Title with Version data
        Me.Text = "Sheet Manager - " & m_Settings.ApplicationVersion

        If File.Exists(m_Settings.IniPath()) Then
            iniPathLabel.Text = "INI Path: " & m_Settings.IniPath()
        Else
            iniPathLabel.Text = "INI Path: Not Found"
        End If

        ' Default to Excel if it Exists, otherwise use Access
        If File.Exists(m_Settings.ExcelPath) Then

            Try
                ' Excel
                m_isExcel = True

                ' Load the Excel File Data
                LabelFilePath.Text = m_Settings.ExcelPath

                ' Launch the App and Load the Worksheet List
                If m_UtilityInterop.StartExcel() Then
                    openFileDialog1.FileName = m_Settings.ExcelPath
                    FillTemplateList()

                    ' Grey out the Access button 
                    Me.ButtonConnectAccess.Image = HOK.SheetManager.My.Resources.BW_MsAccess

                Else
                    ListBoxTableSet.Items.Clear()
                    LabelFilePath.Text = "Click Access or Excel File to Continue"
                End If
            Catch ex As Exception
                Dim message As String = ex.Message
            End Try
        Else

            Try
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

                        ' Grey out the Excel button
                        Me.ButtonConnectExcel.Image = HOK.SheetManager.My.Resources.excel2007logo_BW

                    Else
                        ListBoxTableSet.Items.Clear()
                        LabelFilePath.Text = "Click Access or Excel File to Continue"
                    End If

                End If
            Catch ex As Exception
                Dim message As String = ex.Message
            End Try

        End If

        ' Always check on load
        checkBoxUpdateExisting.Checked = True

        ' Get all titleblock families
        Dim colTitleblocks As New FilteredElementCollector(m_Settings.Document)
        colTitleblocks.WhereElementIsElementType()
        m_TitleBlocks = colTitleblocks.OfCategory(BuiltInCategory.OST_TitleBlocks).ToElements

        ' Top node is family name, child is type name
        For Each x As FamilySymbol In m_TitleBlocks

            ' First Add the Parent Node
            Dim m_NodeParent As New System.Windows.Forms.TreeNode
            m_NodeParent.Name = x.Family.Name
            m_NodeParent.Tag = x.Family.Name
            m_NodeParent.Text = x.Family.Name
            Try
                If Me.TreeViewTitleBlocks.Nodes(x.Family.Name) Is Nothing Then
                    Me.TreeViewTitleBlocks.Nodes.Add(m_NodeParent)
                End If
            Catch ex As Exception
                ' do nothing
            End Try

            ' The Child Node - Family Type Name
            Dim m_Node As New System.Windows.Forms.TreeNode
            m_Node.Name = x.Name
            m_Node.Tag = x.Name
            m_Node.Text = x.Name
            Try
                If Me.TreeViewTitleBlocks.Nodes(x.Family.Name).Nodes(x.Name) Is Nothing Then
                    Me.TreeViewTitleBlocks.Nodes(x.Family.Name).Nodes.Add(m_Node)
                End If
            Catch ex As Exception
                ' do nothing
            End Try

        Next

        m_IsLoaded = True

        If Me.ListBoxTableSet.Items.Count > 0 Then Me.ListBoxTableSet.SelectedIndex = 0

        Try
            Me.TreeViewTitleBlocks.SelectedNode = Me.TreeViewTitleBlocks.Nodes(0).Nodes(0)
        Catch ex As Exception
            ' Quiet
        End Try

        Me.TreeViewTitleBlocks.ExpandAll()

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

    ''' <summary>
    ''' Scan the Sheets for Changes
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub ScanSheets()

        ' Fill the datatable
        If m_isExcel = True Then
            m_UtilityInterop.FillDataTableFromExcelWorksheet(Me.ListBoxTableSet.SelectedItem.ToString)
        Else
            m_UtilityInterop.FillDataTableFromAccessTable("TemplateSheet", "TemplateId = " + m_UtilitySql.LiteralOrNull(ListBoxTableSet.SelectedItem.ToString))
        End If
        ' The datatable object
        Dim m_DataTable As Data.DataTable = m_UtilityInterop.DataTable

        ' Verify existence of "Sheet Number" column.
        If Not CheckColumnPresent(m_DataTable, "Sheet Number") Then
            Exit Sub
        End If

        ' Clear the tree nodes
        Me.TreeViewSheets.Nodes.Clear()

        ' Hide the lower buttons
        Me.buttonAddViewsToSheets.Visible = False
        Me.buttonClose.Visible = False
        Me.buttonUpdateCreate.Visible = False
        Me.buttonRenumberSheets.Visible = False
        Me.buttonExportData.Visible = False

        ' Queue the progressbar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 1
            .Value = 0
            .Visible = True
        End With

        Try
            ' Show sheet's in tree view... icons to show updates versus new sheets
            For Each row As DataRow In m_DataTable.Rows

                Dim sheetNumber As String = row("Sheet Number").ToString
                Dim sheetName As String = ""

                If m_DataTable.Columns.Contains("Sheet Name") Then
                    sheetName = row("Sheet Name").ToString
                End If
          
                'Check for empty sheet number, which can arise with Excel case
                If sheetNumber = "" Then
                    Me.ProgressBar1.Increment(1)
                    Continue For
                Else
                    ' Create the parent node
                    Dim tn1 As New System.Windows.Forms.TreeNode
                    tn1.Text = sheetNumber & ": " & sheetName
                    tn1.Tag = sheetNumber
                    tn1.Name = sheetNumber
                    tn1.ForeColor = Drawing.Color.Black
                    tn1.Checked = False

                    Select Case ComboBoxFilter.SelectedIndex
                        Case 0
                            'Select all sheets
                            tn1.Checked = True
                        Case 1
                            'New sheets only
                            If Not m_Settings.Sheets.ContainsKey(sheetNumber) Then
                                tn1.Checked = True
                            End If
                        Case 2
                            'Existing sheets only
                            If m_Settings.Sheets.ContainsKey(sheetNumber) Then
                                tn1.Checked = True
                            End If
                    End Select

                    Me.TreeViewSheets.Nodes.Add(tn1)

                    ' Is it an existing sheet?
                    If m_Settings.Sheets.ContainsKey(sheetNumber) And m_Settings.clsSheetsList.ContainsKey(sheetNumber) Then

                        ' Find discrepancies
                        tn1.ImageIndex = 0
                        tn1.SelectedImageIndex = 0
                        tn1.ForeColor = Drawing.Color.Gray

                        ' The Sheet Element
                        Dim m_SheetItem As ViewSheet = m_Settings.clsSheetsList(sheetNumber).Sheet
                        Dim isPlaceholderSheet As Boolean = False

                        If m_SheetItem.IsPlaceholder Then
                            isPlaceholderSheet = True
                            tn1.Text = sheetNumber & ": " & sheetName & " (Placeholder)"
                        End If

                        ' The Titleblock Element Instance
                        Dim m_TblkItem As Element = Nothing
                        If m_Settings.clsTblksList.ContainsKey(sheetNumber) Then
                            m_TblkItem = m_Settings.clsTblksList(sheetNumber).Element
                        End If

                        ' Iterate the columns in the datarow
                        For Each dc As DataColumn In m_DataTable.Columns
#If RELEASE2013 Or RELEASE2014 Then
                            Dim m_pSht As Parameter = m_SheetItem.Parameter(dc.ColumnName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                            Dim m_pSht As Parameter = m_SheetItem.LookupParameter(dc.ColumnName)
#End If
                            If m_pSht IsNot Nothing Then

                                Dim m_para As New clsPara(m_pSht)

                                Try

                                    ' Is this a Y/N parameter
                                    If m_para.DisplayUnitType.ToUpper = "YESNO" Then

                                        Dim isDiscrepancy As Boolean = False

                                        Select Case row(m_para.Name).ToString.ToUpper
                                            Case "1"
                                                If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                            Case "Y"
                                                If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                            Case "YES"
                                                If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                            Case "X"
                                                If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                            Case "0"
                                                If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                            Case "N"
                                                If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                            Case "NO"
                                                If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                            Case ""
                                                If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                        End Select

                                        If isDiscrepancy = True Then
                                            ' This is a discrepancy
                                            Dim tn2 As New System.Windows.Forms.TreeNode
                                            tn2.Text = m_para.Name & vbTab & ": " & vbTab & m_para.Value & vbTab & " (XL=" & row(m_para.Name).ToString & ")"
                                            tn2.Tag = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                            tn2.Name = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                            Me.TreeViewSheets.Nodes(sheetNumber).Nodes.Add(tn2)
                                            tn2.ImageIndex = 1
                                            tn2.SelectedImageIndex = 1
                                        End If
                                    Else
                                        ' These are not YesNo Parameters
                                        If m_para.Value <> row(m_para.Name).ToString Then
                                            ' This is a discrepancy
                                            Dim tn2 As New System.Windows.Forms.TreeNode
                                            tn2.Text = m_para.Name & vbTab & ": " & vbTab & m_para.Value & vbTab & " (XL=" & row(m_para.Name).ToString & ")"
                                            tn2.Tag = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                            tn2.Name = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                            Me.TreeViewSheets.Nodes(sheetNumber).Nodes.Add(tn2)
                                            tn2.ImageIndex = 1
                                            tn2.SelectedImageIndex = 1

                                        End If
                                    End If

                                Catch ex As Exception

                                End Try

                            End If


                            If isPlaceholderSheet = False And m_TblkItem IsNot Nothing Then
#If RELEASE2013 Or RELEASE2014 Then
                            Dim m_pTblk As Parameter = m_TblkItem.Parameter(dc.ColumnName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                                Dim m_pTblk As Parameter = m_TblkItem.LookupParameter(dc.ColumnName)
#End If

                                If m_pTblk IsNot Nothing Then

                                    Dim m_para As New clsPara(m_pTblk)

                                    Try

                                        ' Is this a Y/N parameter
                                        If m_para.DisplayUnitType.ToUpper = "YESNO" Then

                                            Dim isDiscrepancy As Boolean = False

                                            Select Case row(m_para.Name).ToString.ToUpper
                                                Case "1"
                                                    If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                                Case "Y"
                                                    If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                                Case "YES"
                                                    If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                                Case "X"
                                                    If Double.Parse(m_para.Value) <> 1 Then isDiscrepancy = True
                                                Case "0"
                                                    If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                                Case "N"
                                                    If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                                Case "NO"
                                                    If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                                Case ""
                                                    If Double.Parse(m_para.Value) <> 0 Then isDiscrepancy = True
                                            End Select

                                            If isDiscrepancy = True Then
                                                ' This is a discrepancy
                                                Dim tn2 As New System.Windows.Forms.TreeNode
                                                tn2.Text = m_para.Name & vbTab & ": " & vbTab & m_para.Value & vbTab & " (XL=" & row(m_para.Name).ToString & ")"
                                                tn2.Tag = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                                tn2.Name = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                                Me.TreeViewSheets.Nodes(sheetNumber).Nodes.Add(tn2)
                                                tn2.ImageIndex = 1
                                                tn2.SelectedImageIndex = 1
                                            End If
                                        Else
                                            ' These are not YesNo Parameters
                                            If m_para.Value <> row(m_para.Name).ToString Then
                                                ' This is a discrepancy
                                                Dim tn2 As New System.Windows.Forms.TreeNode
                                                tn2.Text = m_para.Name & vbTab & ": " & vbTab & m_para.Value & vbTab & " (XL=" & row(m_para.Name).ToString & ")"
                                                tn2.Tag = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                                tn2.Name = m_para.Name & vbTab & ": " & vbTab & m_para.Value
                                                Me.TreeViewSheets.Nodes(sheetNumber).Nodes.Add(tn2)
                                                tn2.ImageIndex = 1
                                                tn2.SelectedImageIndex = 1

                                            End If
                                        End If

                                    Catch ex As Exception
                                        Dim message As String = ex.Message
                                    End Try
                                End If
                            End If
                        Next

                        ' If no discrepancies, then remove the parent node
                        'If Me.TreeViewSheets.Nodes(sheetNumber).Nodes.Count = 0 Then
                        'Me.TreeViewSheets.Nodes.Remove(tn1)
                        'End If

                    End If

                End If

                ' Step the progrssbar
                Me.ProgressBar1.Increment(1)

            Next
        Catch ex As Exception
            Dim message As String = ex.Message
        End Try
        

        ' Tell the user if no discrepancies found
        If Me.TreeViewSheets.Nodes.Count < 1 Then
            ShowRevitTaskDialog("Sheet Manager", "No Discrepancies Found!", "")
        End If

        ' Show the lower buttons
        Me.buttonAddViewsToSheets.Visible = True
        Me.buttonClose.Visible = True
        Me.buttonUpdateCreate.Visible = True
        Me.buttonRenumberSheets.Visible = True
        Me.buttonExportData.Visible = True

        ' Hide the progressbar
        Me.ProgressBar1.Visible = False
    End Sub

    ''' <summary>
    ''' Titleblock Selection Update
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub UpdateTitleBlockSelection(ByVal e As System.Windows.Forms.TreeViewEventArgs)
        ' If a parent node is selected, select the child node
        If e.Node.Level = 0 Then
            Me.TreeViewTitleBlocks.SelectedNode = e.Node.Nodes(0)
            Me.TreeViewTitleBlocks.Focus()
            Me.TreeViewTitleBlocks.Select()
        End If

        Dim collector As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
        Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks)
        Dim titleBlocks As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()
        ' Set the titleblock reference rigth here!!!!
        'Get titleblock selection (adjusted for treenode selection)
        For Each titleBlockTest As FamilySymbol In titleBlocks
            If titleBlockTest.Name.ToString = Me.TreeViewTitleBlocks.SelectedNode.Text Then
                m_TitleBlock = titleBlockTest
                Exit For
            End If
        Next

        ' Make sure we leave with a valid titleblock selection
        If m_TitleBlock Is Nothing Then
            For Each titleBlockTest As FamilySymbol In titleBlocks
                m_TitleBlock = titleBlockTest
                Exit For
            Next
        End If
    End Sub

    ''' <summary>
    ''' Show a revit style dialog messagebox
    ''' </summary>
    ''' <param name="p_Title"></param>
    ''' <param name="p_Inst"></param>
    ''' <param name="p_Content"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ShowRevitTaskDialog(ByVal p_Title As String, ByVal p_Inst As String, ByVal p_Content As String) As TaskDialogResult
        Dim m_Dlg As New TaskDialog(p_Title)
        m_Dlg.MainInstruction = p_Inst
        m_Dlg.MainContent = p_Content
        m_Dlg.CommonButtons = TaskDialogCommonButtons.Ok
        m_Dlg.DefaultButton = TaskDialogResult.Ok

        Dim result As TaskDialogResult = m_Dlg.Show
        Select Case result
            Case TaskDialogResult.Ok
                Return TaskDialogResult.Ok
            Case TaskDialogResult.Cancel
                Return TaskDialogResult.Cancel
            Case Else
                Return TaskDialogResult.Close
        End Select
    End Function

#Region "Form Controls and Events"

    ''' <summary>
    ''' Verify that a family instance is selected and not a symbol
    ''' Set the titleblock reference
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub TreeViewTitleBlocks_AfterSelect(ByVal sender As System.Object, _
                                                ByVal e As System.Windows.Forms.TreeViewEventArgs) _
                                            Handles TreeViewTitleBlocks.AfterSelect
        ' Update the Seletion
        UpdateTitleBlockSelection(e)
    End Sub

    ''' <summary>
    ''' Change the external data table
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ListBoxTableSet_SelectedIndexChanged(ByVal sender As System.Object, _
                                                     ByVal e As System.EventArgs) _
                                                 Handles ListBoxTableSet.SelectedIndexChanged
        ' Scan the Sheets
        ScanSheets()
    End Sub

    ''' <summary>
    ''' Add views to sheets
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonAddViewsToSheets_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonAddViewsToSheets.Click
        ' Create a new transaction
        Dim m_Trans As New Transaction(m_Settings.Document, "HOK Add Views to Sheets")
        m_Trans.Start()

        Dim dataTableLocal As System.Data.DataTable
        Dim pointInsert As XYZ = Nothing
        Dim parameterViewSheetInfo As Parameter
        Dim offsetU As Double = 0
        Dim offsetV As Double = 0
        Dim viewsNotPlaced As String = ""
        Dim placed As Boolean = False
        Dim upPlacedViews As New StringBuilder
        Dim placedViews As New StringBuilder

        'Do not allow Access case
        If m_isExcel = False Then
            Dim m_Dlg As New TaskDialog("HOK Sheet Manager")
            m_Dlg.MainInstruction = "HOK Sheet Management Stats"
            m_Dlg.MainContent = "Renumber not supported with Access database.  Processing stopped."
            Dim result As TaskDialogResult = m_Dlg.Show
            Select Case result
                ' Always swap
                Case TaskDialogResult.Ok
                    Return
                Case TaskDialogResult.Cancel
                    Return
                Case Else
                    Return
            End Select
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

        'Start the progress bar and hide the buttons
        Me.buttonAddViewsToSheets.Visible = False
        Me.buttonClose.Visible = False
        Me.buttonUpdateCreate.Visible = False
        Me.buttonRenumberSheets.Visible = False
        Me.buttonExportData.Visible = False
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = dataTableLocal.Rows.Count + 1
            .Value = 0
            .Visible = True
        End With

        'Process each row in rename table
        For Each row As DataRow In dataTableLocal.Rows

            'Increment the progress bar
            Me.ProgressBar1.Increment(1)

            Dim sheetNumber As String = row("Sheet Number").ToString

            'Check for missing values and ignore them
            If String.IsNullOrEmpty(sheetNumber) Then
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
            If Not m_Settings.Sheets.ContainsKey(sheetNumber) Then
                Continue For
            End If
            If Not m_Settings.Views.ContainsKey(row("View Name").ToString) Then
                Continue For
            End If

            If TreeViewSheets.Nodes.ContainsKey(sheetNumber) Then
                If TreeViewSheets.Nodes(sheetNumber).Checked = True Then

                    'Get the sheet and view and place view
                    m_ViewSheet = m_Settings.Sheets(sheetNumber)
                    m_View = m_Settings.Views(row("View Name").ToString)

                    'Confirm that UV values are good numbers and get the values
                    Try
                        offsetU = Convert.ToDouble(row("U").ToString)
                        offsetV = Convert.ToDouble(row("V").ToString)
                    Catch exception As Exception
                        ShowRevitTaskDialog("Sheet Manager", _
                                            m_Settings.ProgramName, _
                                            "Error in 'Start.buttonAddViews_Click' while converting U or V value to number. Processing stopped." & vbLf & vbLf & _
                                            "System message: " & exception.Message)
                        Me.ProgressBar1.Visible = False
                        Return
                    End Try

                    'place the view
                    Try
                        parameterViewSheetInfo = m_View.Parameter(BuiltInParameter.VIEW_SHEET_VIEWPORT_INFO)
                        If parameterViewSheetInfo.AsString.ToUpper = "NOT IN A SHEET" Then
                            Dim boundingBox As BoundingBoxXYZ = m_View.BoundingBox(m_View)

#If RELEASE2013 Then
                    pointInsert = New XYZ(offsetU - m_View.Outline.Min.U, offsetV - m_View.Outline.Min.V, 0)

                    If Viewport.CanAddViewToSheet(m_Settings.Document, m_ViewSheet.Id, m_View.Id) Then
                        Dim createdViewport As Viewport = Viewport.Create(m_Settings.Document, m_ViewSheet.Id, m_View.Id, pointInsert)

                        placed = True
                        placedViews.AppendLine(m_ViewSheet.SheetNumber & " : " & m_View.Name)
                    ElseIf TypeOf m_View Is ViewSchedule Then
                        pointInsert = XYZ.Zero
                        Dim createdScheduleInstance As ScheduleSheetInstance = ScheduleSheetInstance.Create(m_Settings.Document, m_ViewSheet.Id, m_View.Id, pointInsert)
                        Dim centerPoint As XYZ = createdScheduleInstance.Point
                        Dim bbBox As BoundingBoxXYZ = createdScheduleInstance.BoundingBox(m_ViewSheet)
                        Dim diffToMove As XYZ = New XYZ(offsetU - bbBox.Min.X, offsetV - bbBox.Min.Y, 0)
                        ElementTransformUtils.MoveElement(m_Settings.Document, createdScheduleInstance.Id, diffToMove)

                        placed = True
                        placedViews.AppendLine(m_ViewSheet.SheetNumber & " : " & m_View.Name)
                    End If
#ElseIf RELEASE2014 Or RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                            pointInsert = XYZ.Zero

                            If Viewport.CanAddViewToSheet(m_Settings.Document, m_ViewSheet.Id, m_View.Id) Then
                                Dim createdViewport As Viewport = Viewport.Create(m_Settings.Document, m_ViewSheet.Id, m_View.Id, pointInsert)
                                Dim centerPoint As XYZ = createdViewport.GetBoxCenter()
                                Dim outline As Outline = createdViewport.GetBoxOutline()
                                Dim diffToMove As XYZ = New XYZ(offsetU + outline.MaximumPoint.X, offsetV + outline.MaximumPoint.Y, 0)
                                ElementTransformUtils.MoveElement(m_Settings.Document, createdViewport.Id, diffToMove)

                                placed = True
                                placedViews.AppendLine(m_ViewSheet.SheetNumber & " : " & m_View.Name)
                            ElseIf TypeOf m_View Is ViewSchedule Then
                                Dim createdScheduleInstance As ScheduleSheetInstance = ScheduleSheetInstance.Create(m_Settings.Document, m_ViewSheet.Id, m_View.Id, pointInsert)
                                Dim centerPoint As XYZ = createdScheduleInstance.Point
                                Dim bbBox As BoundingBoxXYZ = createdScheduleInstance.BoundingBox(m_ViewSheet)
                                Dim diffToMove As XYZ = New XYZ(offsetU - bbBox.Min.X, offsetV - bbBox.Min.Y, 0)
                                ElementTransformUtils.MoveElement(m_Settings.Document, createdScheduleInstance.Id, diffToMove)

                                placed = True
                                placedViews.AppendLine(m_ViewSheet.SheetNumber & " : " & m_View.Name)

                            End If
#End If
                        Else
                            upPlacedViews.AppendLine(parameterViewSheetInfo.AsString() & " : " & m_View.Name)
                        End If
                        'catch {
                        '    //For now we are ignoring this error since it occurs when the view has already been placed
                        '    continue;

                    Catch exception As Exception
                        If pointInsert IsNot Nothing Then
                            ShowRevitTaskDialog("Sheet Manager", "Processing Stopped", "Error in 'Start.buttonAddViews_Click' while placing view." _
                                           & vbLf & "Sheet Number: """ & row("Sheet Number").ToString & """" & vbLf & "View Name: """ & row("View Name").ToString & """" & vbLf & _
                                           "U: """ & row("U").ToString & """" & vbLf & "V: """ & row("V").ToString & """" & vbLf & _
                                           "pointInsert.X: """ & pointInsert.X.ToString & """" & vbLf & "pointInsert.Y: """ & pointInsert.Y.ToString & """" & vbLf & vbLf & _
                                           "System message: " & exception.Message)

                            Me.ProgressBar1.Visible = False
                        End If
                        Return
                    End Try
                End If
            End If

            
        Next

        ' Commit the transaction
        m_Trans.Commit()

        'Empty the data table since it seems to fill up with repeated use of the command
        dataTableLocal.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False
        Me.buttonAddViewsToSheets.Visible = True
        Me.buttonClose.Visible = True
        Me.buttonUpdateCreate.Visible = True
        Me.buttonRenumberSheets.Visible = True
        Me.buttonExportData.Visible = True

        'Completed message
        If placed = False Then
            If upPlacedViews.Length > 0 Then
                viewsNotPlaced = "Unplaced Views: The follwing views already exist on sheets" & vbCrLf
                viewsNotPlaced += upPlacedViews.ToString
                ShowRevitTaskDialog("Sheet Manager", "Processing Stopped...", viewsNotPlaced)
            Else
                ShowRevitTaskDialog("Sheet Manager", "Processing Stopped...", "No Views Placed")
            End If
        Else
            viewsNotPlaced = "Placed Views" & vbCrLf
            viewsNotPlaced += placedViews.ToString & vbCrLf
            viewsNotPlaced += "Unplaced Views: The follwing views already exist on sheets" & vbCrLf
            viewsNotPlaced += upPlacedViews.ToString
            ShowRevitTaskDialog("Sheet Manager", "Processing Completed", viewsNotPlaced)
        End If
    End Sub

    ''' <summary>
    ''' Renumber sheets
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonRenumberSheets_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonRenumberSheets.Click

        Dim listTestRenumber As New List(Of String)

        'Do not allow Access case
        If m_isExcel = False Then
            ShowRevitTaskDialog("Sheet Manager", "Processing Stopped...", "Renumber not supported with Access database.")
            Return
        End If

        'Get sheet values; assuming Excel
        'm_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        m_UtilityInterop.FillDataTableFromExcelWorksheet("Renumber Sheets")
        m_DataTable = m_UtilityInterop.DataTable
        'Notes on Excel.  The number of rows at this point is the total, including blanks, to the last line.
        'The use of "dataTable.Columns" correctly interprets the first line as columns and then
        'A loop using "row in dataTable.Rows" seems to get each subsequent row, including blanks
        'Another form is: Extended Properties=\"Excel 8.0;HDR=Yes\""); Apparently the HDR indicates that there is a header row.

        ' Make sure the correct columns are present
        If Not CheckColumnPresent(m_DataTable, "OldNumber") Then
            Return
        End If
        If Not CheckColumnPresent(m_DataTable, "NewNumber") Then
            Return
        End If

        'Start the progress bar and hide the buttons
        Me.buttonAddViewsToSheets.Visible = False
        Me.buttonClose.Visible = False
        Me.buttonUpdateCreate.Visible = False
        Me.buttonRenumberSheets.Visible = False
        Me.buttonExportData.Visible = False
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 2
            .Value = 0
            .Visible = True
        End With
        Me.ProgressBar1.Increment(1)

        'Check for logic errors in rename
        For Each sheetNumber As String In m_Settings.Sheets.Keys
            If TreeViewSheets.Nodes.ContainsKey(sheetNumber) Then
                If TreeViewSheets.Nodes(sheetNumber).Checked = True Then
                    listTestRenumber.Add(sheetNumber)
                End If
            End If
        Next

        Using m_Trans As New Transaction(m_Settings.Document, "HOK Sheet Renumber")
            m_Trans.Start()
            Try
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

                            ShowRevitTaskDialog("Sheet Manager", "Logical error at Old Number:", row("OldNumber").ToString & " New Number: " & Convert.ToString(row("NewNumber")))

                            m_DataTable.Clear()
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

                    Dim oldNumber As String = row("OldNumber").ToString
                    Dim newNumber As String = row("NewNumber").ToString

                    'Check for missing values and ignore them
                    If oldNumber = "" Then
                        Continue For
                    End If
                    If newNumber = "" Then
                        Continue For
                    End If

                    'See if sheet exists, otherwise ignore it.  If found renumber and update dictionary
                    If m_Settings.Sheets.ContainsKey(oldNumber) And TreeViewSheets.Nodes.ContainsKey(oldNumber) And Not m_Settings.Sheets.ContainsKey(newNumber) Then
                        If TreeViewSheets.Nodes.ContainsKey(oldNumber) Then
                            If TreeViewSheets.Nodes(oldNumber).Checked = True Then

                                m_ViewSheet = m_Settings.Sheets(oldNumber)
                                m_ViewSheet.SheetNumber = newNumber
                                m_Settings.Sheets.Add(newNumber, m_Settings.Sheets(oldNumber))
                                m_Settings.Sheets.Remove(oldNumber)
                                ' This was missing, pretty important 11-06-13, Don Rudder
                                m_ViewSheet.SheetNumber = newNumber
                            End If
                        End If
                    End If
                Next

                ' Commit the transaction
                m_Trans.Commit()
            Catch ex As Exception
                m_Trans.RollBack()
                Dim message As String = ex.Message
            End Try
        End Using


        'Empty the data table since it seems to fill up with repeated use of the command
        m_DataTable.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False
        Me.buttonAddViewsToSheets.Visible = True
        Me.buttonClose.Visible = True
        Me.buttonUpdateCreate.Visible = True
        Me.buttonRenumberSheets.Visible = True
        Me.buttonExportData.Visible = True
        'Completed message
        ShowRevitTaskDialog("Sheet Manager", "Processing Completed", "Old sheet numbers were replaced with new numbers.")

    End Sub

    Private Sub buttonRenameViews_Click(sender As Object, e As EventArgs) Handles buttonRenameViews.Click
        Dim listTestRename As New List(Of String)

        'Do not allow Access case
        If m_isExcel = False Then
            ShowRevitTaskDialog("Sheet Manager", "Processing Stopped...", "Renumber not supported with Access database.")
            Return
        End If

        'Get sheet values; assuming Excel
        'm_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        m_UtilityInterop.FillDataTableFromExcelWorksheet("Rename Views")
        m_DataTable = m_UtilityInterop.DataTable
        'Notes on Excel.  The number of rows at this point is the total, including blanks, to the last line.
        'The use of "dataTable.Columns" correctly interprets the first line as columns and then
        'A loop using "row in dataTable.Rows" seems to get each subsequent row, including blanks
        'Another form is: Extended Properties=\"Excel 8.0;HDR=Yes\""); Apparently the HDR indicates that there is a header row.

        ' Make sure the correct columns are present
        If Not CheckColumnPresent(m_DataTable, "OldName") Then
            Return
        End If
        If Not CheckColumnPresent(m_DataTable, "NewName") Then
            Return
        End If

        'Start the progress bar and hide the buttons
        Me.buttonAddViewsToSheets.Visible = False
        Me.buttonClose.Visible = False
        Me.buttonUpdateCreate.Visible = False
        Me.buttonRenumberSheets.Visible = False
        Me.buttonRenameViews.Visible = False
        Me.buttonExportData.Visible = False
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 2
            .Value = 0
            .Visible = True
        End With
        Me.ProgressBar1.Increment(1)

        Dim countView As Integer = 0
        Using m_Trans As New Transaction(m_Settings.Document, "HOK View Rename")
            m_Trans.Start()
            Try
                For Each row As DataRow In m_DataTable.Rows

                    'Increment the progress bar
                    Me.ProgressBar1.Increment(1)

                    'Check for missing values and ignore them
                    If row("OldName").ToString = "" Then
                        Continue For
                    End If
                    If row("NewName").ToString = "" Then
                        Continue For
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

                    Dim oldName As String = row("OldName").ToString
                    Dim newName As String = row("NewName").ToString

                    'See if sheet exists, otherwise ignore it.  If found renumber and update dictionary
                    If m_Settings.Views.ContainsKey(oldName) And Not m_Settings.Views.ContainsKey(newName) Then
                        m_View = m_Settings.Views(oldName)
                        m_View.ViewName = newName
                        m_Settings.Views.Add(newName, m_Settings.Views(oldName))
                        m_Settings.Views.Remove(oldName)
                        m_View.ViewName = newName
                        countView = countView + 1
                    End If
                Next

                ' Commit the transaction
                m_Trans.Commit()
            Catch ex As Exception
                m_Trans.RollBack()
                Dim message As String = ex.Message
            End Try
        End Using


        'Empty the data table since it seems to fill up with repeated use of the command
        m_DataTable.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False
        Me.buttonAddViewsToSheets.Visible = True
        Me.buttonClose.Visible = True
        Me.buttonUpdateCreate.Visible = True
        Me.buttonRenumberSheets.Visible = True
        Me.buttonRenameViews.Visible = True
        Me.buttonExportData.Visible = True
        'Completed message
        ShowRevitTaskDialog("Sheet Manager", "Processing Completed", countView.ToString + " old view names were replaced with new numbers.")
    End Sub
    ''' <summary>
    ''' Export Sheet Data
    ''' </summary>
    ''' <param name="sender"></param> 
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonExportData_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonExportData.Click
        ' First test... no sheets then nothing to export.
        If m_Settings.Sheets.Count < 1 Then Exit Sub
        Try
            For Each sheetNumber As String In m_Settings.Sheets.Keys
                ' Get the first sheet
                m_ViewSheet = m_Settings.Sheets(sheetNumber)
            Next
        Catch ex As Exception
            ' This should never happen
        End Try

        ' Data references
        Dim dataRow As DataRow = Nothing
        Dim dataRowArray As DataRow() = New DataRow(0) {}

        'Do not allow Access case (possibly because he couldn't create fields, etc??)
        If m_isExcel = False Then
            ShowRevitTaskDialog("Sheet Manager", "Processing Stopped", "Renumber not supported with Access database")
            Return
        End If



        Dim m_Dlg As New TaskDialog("Sheet Manager")
        m_Dlg.MainInstruction = "Warning"
        m_Dlg.MainContent = "This command will overwrite data." & vbLf & "Be sure that the proper data table is selected before proceeding."
        m_Dlg.AllowCancellation = True
        m_Dlg.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Continue with Export")
        m_Dlg.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Cancel and do Nothing")

        Dim result As TaskDialogResult = m_Dlg.Show
        Select Case result
            Case TaskDialogResult.CommandLink1
                ' Continue

            Case TaskDialogResult.CommandLink2
                ' Cancel
                MsgBox("Canceling Export", MsgBoxStyle.Exclamation, "Canceling")
                Exit Sub
        End Select

        'Get sheet values; assuming Excel
        m_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        m_DataTable = m_UtilityInterop.DataTable

        ' Is there a "Sheet Number" column?
        If Not CheckColumnPresent(m_DataTable, "Sheet Number") Then
            Return
        End If

        If m_Settings.Sheets.Count = 0 Then
            ShowRevitTaskDialog("Sheet Manager", "Nothing to Process", "")
            Return
        End If

        'Start the progress bar
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_Settings.Sheets.Count + 2
            .Value = 0
            .Visible = True
        End With
        Me.ProgressBar1.Increment(1)

        'To avoid transparent look while waiting
        'Determine parameter list from matching parameters and fields
        For Each dataColumn As DataColumn In m_DataTable.Columns

#If RELEASE2013 Or RELEASE2014 Then
            m_ParamTblk = m_ViewSheet.Parameter(dataColumn.ColumnName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
            m_ParamTblk = m_ViewSheet.LookupParameter(dataColumn.ColumnName)
#End If

            If m_ParamTblk IsNot Nothing Then
                m_ParameterList.Add(dataColumn.ColumnName)
            End If

        Next

        For Each sheetNumber As String In m_Settings.Sheets.Keys
            m_ViewSheet = m_Settings.Sheets(sheetNumber)

#If RELEASE2013 Or RELEASE2014 Then
            m_ParamTblk = m_ViewSheet.Parameter("Sheet Number")
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
            m_ParamTblk = m_ViewSheet.LookupParameter("Sheet Number")
#End If

            dataRowArray = m_DataTable.[Select]("[Sheet Number] = '" + m_ParamTblk.AsString() & "'")
            If dataRowArray.Length > 1 Then
                'duplicate record; error.
                ShowRevitTaskDialog("Sheet Manager", "Processing Stopped", "Duplicate Sheet Number in Table: " & sheetNumber)
                Me.ProgressBar1.Visible = False
                Return
            End If
            If dataRowArray.Length = 0 Then
                'no record; make a new one
                dataRow = m_DataTable.NewRow()
                For Each parameterName As String In m_ParameterList

#If RELEASE2013 Or RELEASE2014 Then
                    m_ParamTblk = m_ViewSheet.Parameter(parameterName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                    m_ParamTblk = m_ViewSheet.LookupParameter(parameterName)
#End If

                    Dim m_Para As New clsPara(m_ParamTblk)
                    If m_ParamTblk Is Nothing Then
                        Continue For
                    End If
                    dataRow(parameterName) = m_Para.Value
                Next
                m_DataTable.Rows.Add(dataRow)
            Else
                'existing record
                dataRow = dataRowArray(0)
                For Each parameterName As String In m_ParameterList

#If RELEASE2013 Or RELEASE2014 Then
                    m_ParamTblk = m_ViewSheet.Parameter(parameterName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                    m_ParamTblk = m_ViewSheet.LookupParameter(parameterName)
#End If

                    Dim m_Para As New clsPara(m_ParamTblk)
                    If m_ParamTblk Is Nothing Then
                        Continue For
                    End If
                    If m_ParamTblk.AsString() <> dataRow(parameterName).ToString Then
                        dataRow(parameterName) = m_Para.Value
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

        ' Save the data back to Excel and close the connection
        m_UtilityInterop.ShutDownExcel()

        'Completed message
        ShowRevitTaskDialog("Sheet Manager", "Processing Completed", "All data in Revit is successfully exported to Excel.")

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
            Try
                m_UtilityInterop.FillDataTableFromExcelSheetNames("TemplateId")
                dataTableLocal = m_UtilityInterop.DataTable

                For Each row As DataRow In dataTableLocal.Rows
                    ListBoxTableSet.Items.Add(row("TemplateId").ToString)
                Next
                If ListBoxTableSet.Items.Count = 0 Then
                    ShowRevitTaskDialog("Sheet Manager", "Processing Stopped", "No valid sheets in Excel file." & vbCr & "Nothing to do.")
                    Return
                End If
                'Always set to first, if there is only one then that's it.
                ListBoxTableSet.SelectedIndex = 0
            Catch ex As Exception
                Dim message As String = ex.Message
            End Try
        Else
            Try

                m_UtilityInterop.FillDataTableFromAccessTable("Template")
                dataTableLocal = m_UtilityInterop.DataTable

                For Each row As DataRow In dataTableLocal.Rows
                    ListBoxTableSet.Items.Add(row("TemplateId").ToString)
                Next
                If ListBoxTableSet.Items.Count = 0 Then
                    ShowRevitTaskDialog("Sheet Manager", "Processing Stopped", "No valid sheets in Access database." & vbCr & "Nothing to do.")
                    Return
                End If
                'Always set to first, if there is only one then that's it.
                ListBoxTableSet.SelectedIndex = 0
            Catch ex As Exception

            End Try

        End If

        ' Enable command buttons if we have a titleblock in the model
        Dim collector As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
        Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks)
        Dim titleBlocks As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()
        If titleBlocks.Count = 0 Then
            MsgBox("Please add a Titleblock to your model before using this application", MsgBoxStyle.Exclamation, "No Titleblock in Model")
        Else

            Me.buttonUpdateCreate.Enabled = True
            Me.buttonRenumberSheets.Enabled = True
            Me.buttonAddViewsToSheets.Enabled = True
            Me.buttonExportData.Enabled = True
        End If

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
        ShowRevitTaskDialog("Sheet Manager", "Processing Stopped", "No column named """ & columnName & """ found")
        Return False

    End Function

#End Region

    ''' <summary>
    ''' Public Counter Properties
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NewSheetCount As Integer
    Public Property CreateFailCount As Integer
    Public Property UpdatedSheetCount As Integer
    Public Property UpdatedParameterCount As Integer
    Public Property UpdatedFailCount As Integer
    Private m_ErrorMessage As New StringBuilder

    ''' <summary>
    ''' Add or update the sheet
    ''' </summary>
    ''' <param name="infoRow"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function DoSheet(ByVal infoRow As DataRow, ByVal ParamNames As List(Of String)) As Boolean

        Dim m_SheetExists As Boolean = False
        Dim m_SheetView As ViewSheet = Nothing
        Dim m_SheetTitleblock As Element = Nothing

        Dim sheetNumber As String = infoRow("Sheet Number").ToString()
        Dim sheetName As String = infoRow("Sheet Name").ToString()

        ' Does the Sheet Exist in the model?
        If m_Settings.clsSheetsList.ContainsKey(sheetNumber) Then
            m_SheetExists = True
            m_SheetView = m_Settings.clsSheetsList(sheetNumber).Sheet

            ' Do we want to update values?
            If Not checkBoxUpdateExisting.Checked Then Return True

            ' We want to update it...
            GoTo UpdateExistingElement

        Else
            Using m_SheetTrans As New Transaction(m_Settings.Document, "HOK Sheet Manager, Created: " & sheetNumber)
                m_SheetTrans.Start()
                Try
                    ' Create it
                    Dim m_DocCreate As Autodesk.Revit.Creation.Document = m_Settings.Application.ActiveUIDocument.Document.Create
                    If RadioButtonViewSheet.Checked Then
                        m_SheetView = ViewSheet.Create(m_Settings.Application.ActiveUIDocument.Document, m_TitleBlock.Id)
                    ElseIf RadioButtonPlaceholder.Checked Then
                        m_SheetView = ViewSheet.CreatePlaceholder(m_Settings.Application.ActiveUIDocument.Document)
                    End If

                    ' Update the Sheet Number and Name
                    m_SheetView.SheetNumber = sheetNumber
                    m_SheetView.Name = sheetName

                    m_SheetTrans.Commit()
                    ' Increment the New Sheet Integer
                    NewSheetCount += 1
                Catch ex As Exception
                    m_SheetTrans.RollBack()
                    CreateFailCount += 1
                    m_ErrorMessage.AppendLine("Sheet Number: " & sheetNumber & ", Sheet Name: " & sheetName & vbCr & ex.Message)
                    Return False
                End Try
            End Using

        End If

UpdateExistingElement:
        Using m_SheetUpdateTrans As New Transaction(m_Settings.Document)
            If m_SheetUpdateTrans.Start("HOK Sheet Manager, Updated: " & infoRow("Sheet Number").ToString) = TransactionStatus.Started Then
                Try
                    ' Update all Discrepancies
                    Dim isDiscrepancy As Boolean = False
                    ' Iterate all Parameter names
                    For Each x As String In ParamNames

                        ' Skip Sheet Number and Sheet Name
                        If x.ToUpper = "SHEET NUMBER" Or x.ToUpper = "NAME" Then Continue For

                        ' Avoid Double Searching in Titleblock Family Later
                        Dim m_ParamFoundInSheet As Boolean = False

                        ' If the param is in the sheet, update it and continue for
                        If m_SheetView IsNot Nothing Then

                            ' Does the param exist in here?
#If RELEASE2013 Or RELEASE2014 Then
                Dim m_P As Parameter = m_SheetView.Parameter(x)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                            Dim m_P As Parameter = m_SheetView.LookupParameter(x)
#End If

                            If m_P IsNot Nothing Then

                                ' No Need to search TB element
                                m_ParamFoundInSheet = True

                                Dim m_Para As New clsPara(m_P)
                                If m_Para IsNot Nothing Then

                                    ' Special Handling for YesNo
                                    If m_Para.DisplayUnitType.ToUpper = "YESNO" Then

                                        Select Case infoRow(m_Para.Name).ToString.ToUpper
                                            Case "1"
                                                If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                            Case "Y"
                                                If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                            Case "YES"
                                                If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                            Case "X"
                                                If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                            Case "0"
                                                If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                            Case "N"
                                                If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                            Case "NO"
                                                If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                            Case ""
                                                If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                        End Select

                                        If isDiscrepancy = True Then

                                            m_Para.Value = infoRow(m_Para.Name).ToString

                                            ' Increment the Update Count
                                            UpdatedParameterCount += 1

                                        End If

                                    Else
                                        If m_Para.Value <> infoRow(m_Para.Name).ToString Then
                                            isDiscrepancy = True
                                            m_Para.Value = infoRow(m_Para.Name).ToString

                                            ' Increment the Update Count
                                            UpdatedParameterCount += 1

                                        End If
                                    End If
                                End If
                            End If
                        End If

                        ' Not in sheet, check the Titleblock
                        If m_SheetView.IsPlaceholder = False And m_ParamFoundInSheet = False Then

                            ' Do we have a valid Titleblock Element
                            If m_SheetTitleblock Is Nothing Then

                                ' Is it already collected?
                                If m_Settings.clsTblksList.ContainsKey(infoRow("Sheet Number").ToString) Then
                                    m_SheetTitleblock = m_Settings.clsTblksList(infoRow("Sheet Number").ToString).Element
                                Else

                                    ' Find the Titleblock Family
                                    Dim m_TBeleCol As New FilteredElementCollector(m_Settings.Document)
                                    Dim m_ListTBs As IList(Of Element)
                                    m_ListTBs = m_TBeleCol.OfCategory(BuiltInCategory.OST_TitleBlocks).ToElements

                                    ' Find the Right one...
                                    For Each y As Element In m_ListTBs

                                        ' Itentify the element by Sheet Number
#If RELEASE2013 Or RELEASE2014 Then
                            Dim m_SheetNumberP As Parameter = y.Parameter("Sheet Number")
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                                        Dim m_SheetNumberP As Parameter = y.LookupParameter("Sheet Number")
#End If

                                        If m_SheetNumberP IsNot Nothing Then

                                            ' Try and get the Sheet Number parameter
                                            Dim m_SheetNumberPara As New clsPara(m_SheetNumberP)
                                            If m_SheetNumberPara IsNot Nothing Then

                                                ' Does the Sheet Number Match what We're after?
                                                If m_SheetNumberPara.Value.ToUpper = infoRow("Sheet Number").ToString.ToUpper Then

                                                    ' This is the element we need
                                                    m_SheetTitleblock = y

                                                    Exit For
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If

                            ' Do the Updates to the Titleblock Family
                            If m_SheetTitleblock IsNot Nothing Then

                                ' Does the param exist in here?
#If RELEASE2013 Or RELEASE2014 Then
                    Dim m_P As Parameter = m_SheetTitleblock.Parameter(x)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                                Dim m_P As Parameter = m_SheetTitleblock.LookupParameter(x)
#End If

                                If m_P IsNot Nothing Then

                                    Dim m_Para As New clsPara(m_P)
                                    If m_Para IsNot Nothing Then

                                        ' Special Handling for YesNo
                                        If m_Para.DisplayUnitType.ToUpper = "YESNO" Then

                                            Select Case infoRow(m_Para.Name).ToString.ToUpper
                                                Case "1"
                                                    If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                                Case "Y"
                                                    If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                                Case "YES"
                                                    If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                                Case "X"
                                                    If Double.Parse(m_Para.Value) <> 1 Then isDiscrepancy = True
                                                Case "0"
                                                    If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                                Case "N"
                                                    If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                                Case "NO"
                                                    If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                                Case ""
                                                    If Double.Parse(m_Para.Value) <> 0 Then isDiscrepancy = True
                                            End Select

                                            If isDiscrepancy = True Then

                                                m_Para.Value = infoRow(m_Para.Name).ToString

                                                ' Increment the Update Count
                                                UpdatedParameterCount += 1

                                            End If

                                        Else

                                            If m_Para.Value <> infoRow(m_Para.Name).ToString Then
                                                isDiscrepancy = True
                                                m_Para.Value = infoRow(m_Para.Name).ToString

                                                ' Increment the Update Count
                                                UpdatedParameterCount += 1

                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next

                    m_SheetUpdateTrans.Commit()
                    If m_SheetExists = True Then
                        UpdatedSheetCount += 1
                    End If

                Catch ex As Exception
                    Dim message As String = ex.Message
                    UpdatedFailCount += 1
                    m_SheetUpdateTrans.RollBack()
                End Try
            End If
        End Using

        Return True

    End Function

    ''' <summary>
    ''' Basic Requirements
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function CheckCreationPrerequisites() As Boolean

        ' If no titleblock in model, do not allow this command
        Dim collector As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
        Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks)
        Dim titleBlocks As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()
        If titleBlocks.Count=0 Then
            MsgBox("Add a titleblock prior to running this command", MsgBoxStyle.Exclamation, "Cannot Use this Command Yet")
            Return False
        End If

        ' If no titleblock selection, do not allow this command
        If m_TitleBlock Is Nothing Then
            MsgBox("Select a titleblock prior to running this command", MsgBoxStyle.Exclamation, "No Titleblock Selected")
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Create
    ''' </summary> 
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonUpdateCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonUpdateCreate.Click

        ' Requirements
        If CheckCreationPrerequisites() = False Then Exit Sub

        ' Get the DataTable
        If m_isExcel = False Then
            m_UtilityInterop.FillDataTableFromAccessTable("TemplateSheet", "TemplateId = " & _
                    m_UtilitySql.LiteralOrNull(ListBoxTableSet.SelectedItem.ToString))
        Else
            m_UtilityInterop.FillDataTableFromExcelWorksheet(ListBoxTableSet.SelectedItem.ToString)
        End If

        m_DataTable = m_UtilityInterop.DataTable

        ' Sheet Number Column is Required
        If Not CheckColumnPresent(m_DataTable, "Sheet Number") Then
            Return
        End If
        If Not CheckColumnPresent(m_DataTable, "Sheet Name") Then
            Return
        End If

        ' Form Prep
        Me.buttonAddViewsToSheets.Visible = False
        Me.buttonClose.Visible = False
        Me.buttonUpdateCreate.Visible = False
        Me.buttonRenumberSheets.Visible = False
        Me.buttonExportData.Visible = False
        With Me.ProgressBar1
            .Minimum = 0
            .Maximum = m_DataTable.Rows.Count + 1
            .Value = 1
            .Visible = True
        End With

        ' Reset Prop Counts to 0
        NewSheetCount = 0
        CreateFailCount = 0
        UpdatedSheetCount = 0
        UpdatedParameterCount = 0

        ' The list of params in datatable
        Dim m_ParamNameList As New List(Of String)
        For Each dataColumn As DataColumn In m_DataTable.Columns
            m_ParamNameList.Add(dataColumn.ColumnName)
        Next

       

        'Process each row in template table
        For Each row As DataRow In m_DataTable.Rows

            ' Progress Bar
            Me.ProgressBar1.Increment(1)

            Dim sheetNumber As String = row("Sheet Number").ToString()

            ' Avoid Blank Sheet Numbers
            If String.IsNullOrEmpty(sheetNumber) Then Continue For

            If TreeViewSheets.Nodes.ContainsKey(sheetNumber) Then
                If TreeViewSheets.Nodes(sheetNumber).Checked = True Then
                    ' Process the Sheet
                    DoSheet(row, m_ParamNameList)
                End If
            End If
        Next

        'Empty the data table
        m_DataTable.Clear()
        If m_UtilityInterop.DataTable IsNot Nothing Then
            m_UtilityInterop.DataTable.Clear()
        End If

        'Close the progress bar
        Me.ProgressBar1.Visible = False

        If CreateFailCount > 0 Then
            'Completed message
            ShowRevitTaskDialog("Sheet Manager", "Processing Completed", _
                                NewSheetCount.ToString & " New Sheets Created" & _
                                vbCr & _
                                UpdatedSheetCount.ToString & " Existing Sheets Updated" & _
                                vbCr & _
                                vbCr & _
                                UpdatedParameterCount.ToString & " Parameters Updated!" & _
                                vbCr & _
                                vbCr & _
                                UpdatedFailCount.ToString & " " & " Sheets Skipped due to Elements Locked by Users" & _
                                vbCr & _
                                "------------------------------------------------------" & _
                                CreateFailCount.ToString() & " sheets are failed to create." & vbCr & m_ErrorMessage.ToString())


        Else
            'Completed message
            ShowRevitTaskDialog("Sheet Manager", "Processing Completed", _
                                NewSheetCount.ToString & " New Sheets Created" & _
                                vbCr & _
                                UpdatedSheetCount.ToString & " Existing Sheets Updated" & _
                                vbCr & _
                                vbCr & _
                                UpdatedParameterCount.ToString & " Parameters Updated!" & _
                                vbCr & _
                                vbCr & _
                                UpdatedFailCount.ToString & " " & " Sheets Skipped due to Elements Locked by Users")
        End If

        m_Settings.GetSheetsAndTitleblockInstances()
        ScanSheets()

        'Me.Close()
    End Sub

#Region "Access and Excel Selection Buttons"

    ''' <summary>
    ''' Select and Connect to Excel
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ButtonConnectExcel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonConnectExcel.Click

        openFileDialog1.FileName = ""

        ' Gray out the unused file format button
        m_isExcel = True
        Me.ButtonConnectAccess.Image = HOK.SheetManager.My.Resources.BW_MsAccess
        Me.ButtonConnectExcel.Image = HOK.SheetManager.My.Resources.Excel2007Logo_Med

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
            m_Settings.WriteIniFile()
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
    ''' Connect to Access
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ButtonConnectAccess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonConnectAccess.Click

        openFileDialog1.FileName = ""

        ' Gray out the unused file format button
        m_isExcel = False
        Me.ButtonConnectAccess.Image = HOK.SheetManager.My.Resources.MsAccess_Med
        Me.ButtonConnectExcel.Image = HOK.SheetManager.My.Resources.excel2007logo_BW

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
            m_Settings.WriteIniFile()
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

   

    Private Sub linkHelp_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles linkHelp.LinkClicked
        Dim htmPath As String = "V:\RVT-Data\HOK Program\Documentation\Sheet Manager_Instruction.pdf"
        System.Diagnostics.Process.Start(htmPath)
    End Sub

    Private Sub linkAbout_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles linkAbout.LinkClicked
        Dim aboutBox As AboutBox = New AboutBox
        aboutBox.ShowDialog()

    End Sub

    Private Sub form_SheetManager_FormClosing(sender As Object, e As Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        m_UtilityInterop.ShutDownAccess()
        m_UtilityInterop.ShutDownExcel()
    End Sub

    Private Sub ButtonCheckAll_Click(sender As Object, e As EventArgs) Handles ButtonCheckAll.Click
        Try
            Dim i As Integer
            For i = 0 To TreeViewSheets.Nodes.Count - 1
                TreeViewSheets.Nodes(i).Checked = True
            Next
        Catch ex As Exception
            Dim message As String = ex.Message
        End Try
    End Sub

    Private Sub ButtonUncheck_Click(sender As Object, e As EventArgs) Handles ButtonUncheck.Click
        Try
            Dim i As Integer
            For i = 0 To TreeViewSheets.Nodes.Count - 1
                TreeViewSheets.Nodes(i).Checked = False
            Next
        Catch ex As Exception
            Dim message As String = ex.Message
        End Try
    End Sub

    Private Sub ComboBoxFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxFilter.SelectedIndexChanged
        Try
            If ListBoxTableSet.SelectedItem IsNot Nothing Then
                If ComboBoxFilter.SelectedItem IsNot Nothing Then
                    Dim i As Integer
                    For i = 0 To TreeViewSheets.Nodes.Count - 1
                        TreeViewSheets.Nodes(i).Checked = False
                        Dim sheetNumber As String = TreeViewSheets.Nodes(i).Name
                        Select Case ComboBoxFilter.SelectedIndex
                            Case 0
                                TreeViewSheets.Nodes(i).Checked = True
                            Case 1
                                If Not m_Settings.Sheets.ContainsKey(sheetNumber) Then
                                    TreeViewSheets.Nodes(i).Checked = True
                                End If
                            Case 2
                                If m_Settings.Sheets.ContainsKey(sheetNumber) Then
                                    TreeViewSheets.Nodes(i).Checked = True
                                End If
                        End Select
                    Next
                End If
            End If
        Catch ex As Exception
            Dim message As String = ex.Message
        End Try
    End Sub

   
    
End Class