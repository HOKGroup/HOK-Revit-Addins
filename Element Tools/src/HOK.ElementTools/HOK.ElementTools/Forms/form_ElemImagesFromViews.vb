Imports Autodesk.Revit

Imports System.Windows.Forms
Imports System.IO
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

Public Class form_ElemImagesFromViews

    Const cMaxNumberOfImageFiles As Integer = 1000
    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private initializing As Boolean = True

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ElementTools-ImagesFromViews", settings.Document.Application.VersionNumber))

        'Initialize the settings text boxes
        m_Settings = settings
        'assuming already initialized
        If m_Settings.ImagesFromViewsIncludeExisting = "true" Then
            checkBoxIncludeExisting.Checked = True
        Else
            checkBoxIncludeExisting.Checked = False
        End If
        If m_Settings.ImagesFromViewsRestrictPrefix = "true" Then
            checkBoxRestrictPrefix.Checked = True
        Else
            checkBoxRestrictPrefix.Checked = False
        End If
        textBoxRestrictPrefixValue.Text = m_Settings.ImagesFromViewsRestrictPrefixValue
        If m_Settings.ImagesFromViewsListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        textBoxFolderPath.Text = m_Settings.ImagesFromViewsFolderPath
        If textBoxFolderPath.Text = "" Then
            textBoxFolderPath.Text = settings.ProjectFolderPath
        End If

        initializing = False
        'to avoid rerunning FillRoomsList during setup

        'Fill the list box with unplaced areas
        FillViewsList()
    End Sub

    Private Sub textBoxFolderPath_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxFolderPath.Leave
        If Not initializing Then
            FillViewsList()
        End If
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

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click
        ' New Transaction
        Dim m_Trans As New DB.Transaction(m_Settings.Document, "HOK Images from Views")
        m_Trans.Start()

        Dim dialogResult__1 As DialogResult
        Dim path As String = textBoxFolderPath.Text

        If path.EndsWith("\") Then
            path = path.Substring(0, path.Length - 1)
        End If
        'In case use added a "\"
        folderBrowserDialogImages.SelectedPath = path
        dialogResult__1 = folderBrowserDialogImages.ShowDialog()
        If dialogResult__1 = DialogResult.OK Then
            path = folderBrowserDialogImages.SelectedPath.ToString
            If Directory.Exists(path) Then
                textBoxFolderPath.Text = path
            Else
                textBoxFolderPath.Text = ""
            End If
            FillViewsList()
        End If

        ' Commit the transaction
        m_Trans.Commit()

    End Sub

    Private Sub buttonBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonBrowse.Click
        Dim viewToUse As DB.View
        Dim viewSet As New DB.ViewSet
        Dim exportOptions As New DB.DWFExportOptions
        Dim elementId As DB.ElementId
        Dim elementData As String
        Dim stringElementId As String
        Dim imageName As String
        Dim pathDwfFile As String

        If listBoxViews.SelectedItems.Count < 1 Then
            MessageBox.Show("Select view(s) from list before pressing 'Create Images' button.", m_Settings.ProgramName)
            Return
        End If

        If Not Directory.Exists(textBoxFolderPath.Text) Then
            MessageBox.Show("Invalid folder specified for placing images.", m_Settings.ProgramName)
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
            stringElementId = elementData.Substring(0, elementData.IndexOf("|"))
            elementData = elementData.Substring(elementData.IndexOf("|") + 1)
            imageName = elementData

            'Get the view
            elementId = New DB.ElementId(CInt(Convert.ToInt64(stringElementId)))
            '' ''elementId.Value = CInt(Convert.ToInt64(stringElementId))
            viewToUse = DirectCast(m_Settings.Document.GetElement(elementId), DB.View)

            'Create the dwf path and delete existing file if any
            pathDwfFile = textBoxFolderPath.Text + "\" & imageName & ".dwf"
            If File.Exists(pathDwfFile) Then
                File.Delete(pathDwfFile)
            End If

            'Create dwf output
            viewSet.Clear()
            viewSet.Insert(viewToUse)
            m_Settings.Document.Export(textBoxFolderPath.Text, imageName, viewSet, exportOptions)

            Me.ToolStripProgressBar1.PerformStep()
        Next

        'Remove all the values from the list (can't do as we go along since messes up increment)
        While listBoxViews.SelectedItems.Count > 0
            listBoxViews.Items.Remove(listBoxViews.SelectedItem)
        End While

        Me.ToolStripProgressBar1.Visible = False
    End Sub

    Private Sub FillViewsList()

        Dim elementsRaw As New List(Of DB.Element)
        Dim viewPlans As New List(Of DB.ViewPlan)
        Dim viewPlanTest As DB.ViewPlan

        Dim imageFileNames As String() = New String(cMaxNumberOfImageFiles - 1) {}
        'note we can't use array list since Directory.GetFiles returns an array
        Dim imageNames As New List(Of String)

        Dim imageFileName As String
        Dim elementData As String

        'Check for valid images folder
        listBoxViews.Items.Clear()
        If textBoxFolderPath.Text = "" Then
            Return
        End If
        If Not Directory.Exists(textBoxFolderPath.Text) Then
            Return
        End If

        Dim imageExists As Boolean

        ' Should this just be views??
        Dim CollectorSheets As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorSheets.OfCategory(DB.BuiltInCategory.OST_Sheets)
        elementsRaw = CollectorSheets.ToElements
        '' ''Dim iterSheets As IEnumerator = CollectorSheets.GetElementIterator

        ' '' ''Create list of views, filtering for floor plan only.
        '' ''Dim filter As Autodesk.Revit.Filter = Nothing
        '' ''filter = mSettings.Application.Create.Filter.NewCategoryFilter(BuiltInCategory.OST_Views)
        '' ''mSettings.Application.ActiveDocument.Elements(filter, elementsRaw)

        For Each element As DB.Element In elementsRaw
            viewPlanTest = TryCast(element, DB.ViewPlan)
            If viewPlanTest Is Nothing Then
                Continue For
            End If
            If viewPlanTest.ViewType <> DB.ViewType.FloorPlan Then
                Continue For
            End If
            viewPlans.Add(viewPlanTest)
        Next

        'Create list of image names
        imageFileNames = Directory.GetFiles(textBoxFolderPath.Text, "*.dwf")
        For Each fileName As String In imageFileNames
            imageFileName = fileName.Substring(0, fileName.Length - 4)
            'strip the .dwf off the end
            imageFileName = imageFileName.Substring(imageFileName.LastIndexOf("\") + 1)
            'strip the path off the start
            imageNames.Add(imageFileName)
        Next

        mListItems.Clear()
        listBoxViews.Items.Clear()

        For Each viewPlan As DB.ViewPlan In viewPlans

            'Restrict to prefix name if indicated
            If checkBoxRestrictPrefix.Checked Then
                If textBoxRestrictPrefixValue.Text <> "" Then
                    If viewPlan.Name.Substring(0, textBoxRestrictPrefixValue.Text.Length) <> textBoxRestrictPrefixValue.Text Then
                        Continue For
                    End If
                End If
            End If

            'Determine if image already exists and skip if include existing checkbox not checked
            imageExists = False
            For Each testName As String In imageNames
                If viewPlan.Name = testName Then
                    imageExists = True
                    Exit For
                End If
            Next
            If imageExists AndAlso Not checkBoxIncludeExisting.Checked Then
                Continue For
            End If

            elementData = ("<" + viewPlan.Id.IntegerValue.ToString & "|") + viewPlan.Name & ">"
            If imageExists Then
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

        If checkBoxIncludeExisting.Checked Then
            m_Settings.ImagesFromViewsIncludeExisting = "true"
        Else
            m_Settings.ImagesFromViewsIncludeExisting = "false"
        End If
        If checkBoxRestrictPrefix.Checked Then
            m_Settings.ImagesFromViewsRestrictPrefix = "true"
        Else
            m_Settings.ImagesFromViewsRestrictPrefix = "false"
        End If
        m_Settings.ImagesFromViewsRestrictPrefixValue = textBoxRestrictPrefixValue.Text
        If checkBoxListReverse.Checked Then
            m_Settings.ImagesFromViewsListReverse = "true"
        Else
            m_Settings.ImagesFromViewsListReverse = "false"
        End If

        m_Settings.ImagesFromViewsFolderPath = textBoxFolderPath.Text

        m_Settings.WriteIni()
    End Sub

End Class