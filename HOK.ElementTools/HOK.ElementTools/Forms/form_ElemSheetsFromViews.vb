Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

Public Class form_ElemSheetsFromViews

    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private initializing As Boolean = True

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ElementTools-SheetsFromViews", settings.Document.Application.VersionNumber))

        'Initialize the settings text boxes
        m_Settings = settings
        'assuming already initialized
        If m_Settings.SheetsFromViewsIncludeExisting = "true" Then
            checkBoxIncludeExisting.Checked = True
        Else
            checkBoxIncludeExisting.Checked = False
        End If
        If m_Settings.SheetsFromViewsRestrictPrefix = "true" Then
            checkBoxRestrictPrefix.Checked = True
        Else
            checkBoxRestrictPrefix.Checked = False
        End If
        textBoxRestrictPrefixValue.Text = m_Settings.SheetsFromViewsRestrictPrefixValue
        If m_Settings.SheetsFromViewsListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        textBoxTitleblock.Text = m_Settings.SheetsFromViewsTitleblock

        initializing = False
        'to avoid rerunning FillRoomsList during setup
        'Fill the list box with unplaced areas
        FillViewsList()
    End Sub

    Private Sub FillViewsList()

        Dim elementsViewsRaw As New List(Of DB.Element)
        Dim elementsViewSheetsRaw As New List(Of DB.Element)
        Dim viewPlans As New List(Of DB.ViewPlan)
        Dim viewSheets As New Dictionary(Of String, String)()
        'sheet number, element ID
        Dim viewPlanTest As DB.ViewPlan

        Dim elementData As String = ""
        Dim viewElementId As String = ""
        Dim sheetElementId As String = ""
        Dim sheetName As String = ""

        'Using Room name as pased to  "Title on Sheet" parameter.
        Dim sheetExists As Boolean

        'Create lists of views, selecting for floor plans only.

        Dim CollectorViews As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorViews.OfCategory(DB.BuiltInCategory.OST_Views)
        elementsViewsRaw = CollectorViews.ToElements

        '' ''Dim filterViews As Autodesk.Revit.Filter = Nothing
        '' ''filterViews = mSettings.Application.Create.Filter.NewCategoryFilter(BuiltInCategory.OST_Views)
        '' ''mSettings.Application.ActiveDocument.get_Elements(filterViews, elementsViewsRaw)
        For Each element As DB.Element In elementsViewsRaw
            viewPlanTest = TryCast(element, DB.ViewPlan)
            If viewPlanTest IsNot Nothing Then
                If viewPlanTest.ViewType = DB.ViewType.FloorPlan Then
                    viewPlans.Add(viewPlanTest)
                End If
                Continue For
            End If
        Next

        'Create the list of ViewSheets (a dictionary so we can store both ID and Sheet Number which is a parameter.)
        'Note that we are assuming the names are unique and that the parameter does exist.

        Dim CollectorSheets As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorSheets.OfCategory(DB.BuiltInCategory.OST_Sheets)
        elementsViewSheetsRaw = CollectorSheets.ToElements

        '' ''Dim filterViewSheets As Autodesk.Revit.Filter = Nothing
        '' ''filterViewSheets = mSettings.Application.Create.Filter.NewCategoryFilter(BuiltInCategory.OST_Sheets)
        '' ''mSettings.Application.ActiveDocument.get_Elements(filterViewSheets, elementsViewSheetsRaw)
        For Each viewSheet As DB.ViewSheet In elementsViewSheetsRaw
            viewSheets.Add(viewSheet.Name, viewSheet.Id.Value.ToString())
        Next

        'Prepare the list box
        mListItems.Clear()
        listBoxViews.Items.Clear()

        'Process for each view
        For Each viewPlan As DB.ViewPlan In viewPlans

            'Restrict to prefix name if indicated
            If checkBoxRestrictPrefix.Checked Then
                If textBoxRestrictPrefixValue.Text <> "" Then
                    If viewPlan.Name.Length < textBoxRestrictPrefixValue.Text.Length Then
                        Continue For
                    End If
                    If viewPlan.Name.Substring(0, textBoxRestrictPrefixValue.Text.Length) <> textBoxRestrictPrefixValue.Text Then
                        Continue For
                    End If
                End If
            End If

            'Determine if sheet already exists and skip if include existing checkbox not checked
            If viewSheets.TryGetValue(viewPlan.Name, sheetElementId) Then
                sheetExists = True
            Else
                sheetExists = False
                sheetElementId = "*"
            End If
            If sheetExists AndAlso Not checkBoxIncludeExisting.Checked Then
                Continue For
            End If

            'Get the value for the sheetName
            sheetName = "Room Data Sheet"
            'Default value
            For Each parameterTest As DB.Parameter In viewPlan.Parameters
                If parameterTest.Definition.Name = "Title on Sheet" Then
                    sheetName = parameterTest.AsString()
                    Exit For
                End If
            Next

            'Add the entry to the list box   
            viewElementId = viewPlan.Id.Value.ToString
            elementData = "<" & viewElementId & "|" & sheetElementId & "|" & sheetName & ">"
            If sheetExists Then
                mListItems.Add(viewPlan.Name + " (E)" & Convert.ToString(m_Settings.Spacer) & elementData)
            Else
                mListItems.Add(Convert.ToString(viewPlan.Name + m_Settings.Spacer) & elementData)

            End If
        Next
        mListItems.Sort()
        If checkBoxListReverse.Checked Then
            mListItems.Reverse()
        End If
        For Each item As String In mListItems
            listBoxViews.Items.Add(item)
        Next

    End Sub

    Private Sub SaveSettings()

        m_Settings.SheetsFromViewsTitleblock = textBoxTitleblock.Text
        If checkBoxRestrictPrefix.Checked Then
            m_Settings.SheetsFromViewsRestrictPrefix = "true"
        Else
            m_Settings.SheetsFromViewsRestrictPrefix = "false"
        End If
        m_Settings.SheetsFromViewsRestrictPrefixValue = textBoxRestrictPrefixValue.Text
        If checkBoxIncludeExisting.Checked Then
            m_Settings.SheetsFromViewsIncludeExisting = "true"
        Else
            m_Settings.SheetsFromViewsIncludeExisting = "false"
        End If
        If checkBoxListReverse.Checked Then
            m_Settings.SheetsFromViewsListReverse = "true"
        Else
            m_Settings.SheetsFromViewsListReverse = "false"
        End If

        m_Settings.WriteIni()
    End Sub

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click
        ' New Transaction
        Dim m_Trans As New DB.Transaction(m_Settings.Document, "HOK Sheets from Views")
        m_Trans.Start()

        Dim viewToUse As DB.View
        Dim sheetToCreate As DB.ViewSheet
        Dim titleBlock As DB.FamilySymbol = Nothing
        Dim elementId As DB.ElementId
        Dim elementData As String
        Dim viewElementId As String
        Dim sheetElementId As String
        Dim sheetName As String
        Dim validTitleblock As Boolean = False

        'Check that user has selected at least one view.
        If listBoxViews.SelectedItems.Count < 1 Then
            MessageBox.Show("Select view(s) from list before pressing 'Create Sheets' button.", m_Settings.ProgramName)
            Return
        End If

        Dim collector As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
        Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_TitleBlocks)
        Dim titleBlocks As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()

        'Check that the titleblock selction is valid in case user typed in or it came from settings
        For Each testTitleblock As DB.FamilySymbol In titleBlocks
            If testTitleblock.Name = textBoxTitleblock.Text Then
                titleBlock = testTitleblock
                validTitleblock = True
                Exit For
            End If
        Next
        If Not validTitleblock Then
            MessageBox.Show("The titleblock setting is not valid.  Command cannot continue.", m_Settings.ProgramName)
            Return
        End If

        Me.ToolStripStatusLabel1.Text = "Creating " & listBoxViews.SelectedItems.Count.ToString & " areas."
        Application.DoEvents()
        System.Threading.Thread.Sleep(1000)
        Me.ToolStripProgressBar1.Value = 0
        Me.ToolStripProgressBar1.Maximum = listBoxViews.SelectedItems.Count + 1
        Me.ToolStripProgressBar1.Visible = True
        Me.ToolStripProgressBar1.ProgressBar.Refresh()
        Me.ToolStripProgressBar1.PerformStep()

        'Start the progress bar
        'Dim progressBarForm As New form_ElemProgress("Creating " + listBoxViews.SelectedItems.Count.ToString & " areas.", listBoxViews.SelectedItems.Count + 1)
        'progressBarForm.ShowDialog()
        'progressBarForm.Increment()
        'To avoid transparent look while waiting
        For Each listItem As String In listBoxViews.SelectedItems

            'Get the element data
            elementData = listItem.Substring(listItem.LastIndexOf("<") + 1, listItem.Length - listItem.LastIndexOf("<") - 2)
            viewElementId = elementData.Substring(0, elementData.IndexOf("|"))
            elementData = elementData.Substring(elementData.IndexOf("|") + 1)
            sheetElementId = elementData.Substring(0, elementData.IndexOf("|"))
            elementData = elementData.Substring(elementData.IndexOf("|") + 1)
            sheetName = elementData

            'Get the view
            elementId = NewElementId(Convert.ToInt64(viewElementId))
            viewToUse = DirectCast(m_Settings.Document.GetElement(elementId), DB.View)

            'If it is an existing sheet, delete it
            If sheetElementId <> "*" Then
                '"*" indicates no existing sheet with the same name.
                elementId = NewElementId(Convert.ToInt64(sheetElementId))
                m_Settings.Document.Delete(elementId)
            End If

            'Make the new sheet if needed and skip existing sheets if option to update not selected.
            sheetToCreate = ViewSheet.Create(m_Settings.Document, titleBlock.Id)
            sheetToCreate.SheetNumber = viewToUse.Name.ToString()
            sheetToCreate.Name = sheetName

            'Insert the view
            Dim viewPort As DB.Viewport
            viewPort = DB.Viewport.Create(m_Settings.Document, sheetToCreate.Id, viewToUse.Id, New DB.XYZ(0, 0, 0))

            '*** haven't figured out placement yet
            '*** also doesn't seem to be any way to access the viewport

            'Increment the Progress Bar
            Me.ToolStripProgressBar1.PerformStep()
        Next

        ' Commit the transaction
        m_Trans.Commit()

        'Remove all the values from the list (can't do as we go along since messes up increment)
        While listBoxViews.SelectedItems.Count > 0
            listBoxViews.Items.Remove(listBoxViews.SelectedItem)
        End While

        'Close the progress bar.
        Me.ToolStripProgressBar1.Visible = False
    End Sub

    Private Sub buttonSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonSelect.Click
        Dim dialog As New form_ElemSelectTitleblock(m_Settings, textBoxTitleblock)
        dialog.ShowDialog()
    End Sub

    Private Sub checkBoxRestrictPrefix_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxRestrictPrefix.CheckedChanged
        If Not initializing Then
            FillViewsList()
        End If
    End Sub

    Private Sub textBoxRestrictPrefixValue_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxRestrictPrefixValue.Leave
        If Not initializing Then
            FillViewsList()
        End If
    End Sub

    Private Sub checkBoxIncludeExisting_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxIncludeExisting.CheckedChanged
        If Not initializing Then
            FillViewsList()
        End If
    End Sub

    Private Sub checkBoxListReverse_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            FillViewsList()
        End If
    End Sub
End Class