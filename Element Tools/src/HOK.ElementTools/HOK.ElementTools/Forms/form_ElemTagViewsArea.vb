Imports Autodesk.Revit

Imports System.Windows.Forms
Imports Autodesk.Revit.DB
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

Public Class form_ElemTagViewsArea
    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private initializing As Boolean = True

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ElementTools-TagViewsArea", settings.Document.Application.VersionNumber))

        'Initialize the settings text boxes
        m_Settings = settings
        'assuming already initialized
        If m_Settings.TagViewsIncludeExisting = "true" Then
            checkBoxIncludeExisting.Checked = True
        Else
            checkBoxIncludeExisting.Checked = False
        End If
        If m_Settings.TagViewsRestrictPrefix = "true" Then
            checkBoxRestrictPrefix.Checked = True
        Else
            checkBoxRestrictPrefix.Checked = False
        End If
        textBoxRestrictPrefixValue.Text = m_Settings.TagViewsRestrictPrefixValueArea
        If m_Settings.TagViewsListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        textBoxAreaTag.Text = m_Settings.TagViewsAreaTag
        textBoxParameterViewName.Text = m_Settings.TagViewsParmViewName
        textBoxPrefixViewSource.Text = m_Settings.TagViewsPrefixViewSourceArea
        If m_Settings.TagViewsStripSuffix = "true" Then
            checkBoxStripSuffix.Checked = True
        Else
            checkBoxStripSuffix.Checked = False
        End If

        initializing = False
        'to avoid rerunning FillAreasList during setup
        'Fill the list box with unplaced areas
        FillViewsList()
    End Sub

    Private Sub FillViewsList()
        'Note that Revit does not allow Areas to be visible or to add tags in 3D view.  This code would allow it
        'so most of the code has been left as is in case this ever gets fixed.  For now we just filter out the 3D cases.

        Dim elementsViewsRaw As New List(Of DB.Element)
        'Dim views As New List(Of DB.View)
        '' ''Dim viewTest As DB.View
        Dim areaTagTest As AreaTag
        Dim areaTagTypeTest As AreaTagType = Nothing

        Dim nameRoot As String
        Dim suffix As String = ""
        Dim elementData As String
        Dim viewToUseElementId As String
        'View to be duplicated
        Dim prefixFound As Boolean
        Dim areaIsAlreadyTagged As Boolean

        'Strip blanks from text fields
        CleanTextInput()

        'Create lists of views, selecting for floor plans only.
        Dim CollectorViews As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorViews.OfCategory(DB.BuiltInCategory.OST_Views)
        elementsViewsRaw = CollectorViews.ToElements

        '' ''For Each element As DB.Element In elementsViewsRaw
        '' ''    viewTest = TryCast(element, DB.View)
        '' ''    If viewTest IsNot Nothing Then
        '' ''        Dim planViewTest As DB.ViewPlan
        '' ''        planViewTest = TryCast(viewTest, DB.ViewPlan)
        '' ''        'This is where we filter out non-plan views for now until Revit fixes it.
        '' ''        If planViewTest IsNot Nothing Then
        '' ''            views.Add(viewTest)
        '' ''        End If
        '' ''    End If
        '' ''Next

        'Prepare the list box
        mListItems.Clear()
        listBoxViews.Items.Clear()

        'Process for each view
        'foreach (Autodesk.Revit.Elements.ViewPlan viewPlan in viewPlans) {

        For Each view As DB.View In elementsViewsRaw

            If view.ViewType <> DB.ViewType.AreaPlan Then
                Continue For
            End If

            'Restrict to prefix name if indicated
            prefixFound = False
            nameRoot = view.Name
            'Fallback case if no filter in use will just append to whatever name is used
            If textBoxRestrictPrefixValue.Text <> "" Then
                If view.Name.Length >= textBoxRestrictPrefixValue.Text.Length Then
                    If view.Name.Substring(0, textBoxRestrictPrefixValue.Text.Length) = textBoxRestrictPrefixValue.Text Then
                        prefixFound = True
                    End If
                End If
            End If
            If checkBoxRestrictPrefix.Checked AndAlso Not prefixFound Then
                Continue For
            End If

            'Strip the prefix value according to the value set 
            If textBoxPrefixViewSource.Text <> "" Then
                If view.Name.Length > textBoxPrefixViewSource.Text.Length Then
                    If view.Name.Substring(0, textBoxPrefixViewSource.Text.Length) = textBoxPrefixViewSource.Text Then
                        nameRoot = view.Name.Substring(textBoxPrefixViewSource.Text.Length)
                        suffix = ""
                        If checkBoxStripSuffix.Checked Then
                            If nameRoot.EndsWith("-2D") Then
                                nameRoot = nameRoot.Remove(nameRoot.Length - 3)
                                suffix = "-2D"
                            ElseIf nameRoot.EndsWith("-3DB") Then
                                nameRoot = nameRoot.Remove(nameRoot.Length - 4)
                                suffix = "-3DB"
                            ElseIf nameRoot.EndsWith("-3DC") Then
                                nameRoot = nameRoot.Remove(nameRoot.Length - 4)
                                suffix = "-3DC"
                            ElseIf nameRoot.EndsWith("-3DBC") Then
                                nameRoot = nameRoot.Remove(nameRoot.Length - 5)
                                suffix = "-3DBC"
                            End If
                        End If
                    End If
                End If
            End If

            'Check if the view alread has a tag of the current type to show in list and element data so we don't add a second one
            areaIsAlreadyTagged = False
            'Need to fix this to use filter

            Dim ElementInViewCol As New DB.FilteredElementCollector(m_Settings.Document, view.Id)
            Dim ElementInView As New List(Of DB.Element)
            ElementInView = ElementInViewCol.ToElements

            For Each myElement As DB.Element In ElementInView
                If myElement.GetType IsNot Nothing Then

                    ' The OLD way
                    '' ''If TypeOf myElement.ObjectType Is DB.Architecture.AreaTagType Then

                    ' The NEW way
                    If TypeOf m_Settings.Document.GetElement(myElement.GetTypeId) Is AreaTagType Then
                        areaTagTest = DirectCast(myElement, AreaTag)
                        areaTagTypeTest = areaTagTest.AreaTagType
                        If areaTagTypeTest.Name = textBoxAreaTag.Text Then
                            areaIsAlreadyTagged = True
                            Exit For
                        End If
                    End If
                End If
            Next

            'Add the entry to the list box          
            viewToUseElementId = view.Id.IntegerValue.ToString
            elementData = "<" & viewToUseElementId & "|" & nameRoot & "|" & suffix & ">"

            If checkBoxIncludeExisting.Checked = True Then
                If areaIsAlreadyTagged Then
                    mListItems.Add(view.Name + " (E)" & Convert.ToString(m_Settings.Spacer) & elementData)
                Else
                    mListItems.Add(Convert.ToString(view.Name + m_Settings.Spacer) & elementData)
                End If
            Else
                If Not areaIsAlreadyTagged Then
                    mListItems.Add(Convert.ToString(view.Name + m_Settings.Spacer) & elementData)
                End If
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

    Private Sub CleanTextInput()
        textBoxRestrictPrefixValue.Text = textBoxRestrictPrefixValue.Text.Trim()
        textBoxParameterViewName.Text = textBoxParameterViewName.Text.Trim()
        textBoxPrefixViewSource.Text = textBoxPrefixViewSource.Text.Trim()
    End Sub

    Private Sub SaveSettings()

        'Strip blanks from text fields
        CleanTextInput()

        If checkBoxIncludeExisting.Checked Then
            m_Settings.TagViewsIncludeExisting = "true"
        Else
            m_Settings.TagViewsIncludeExisting = "false"
        End If
        If checkBoxRestrictPrefix.Checked Then
            m_Settings.TagViewsRestrictPrefix = "true"
        Else
            m_Settings.TagViewsRestrictPrefix = "false"
        End If
        m_Settings.TagViewsRestrictPrefixValue = textBoxRestrictPrefixValue.Text
        If checkBoxListReverse.Checked Then
            m_Settings.TagViewsListReverse = "true"
        Else
            m_Settings.TagViewsListReverse = "false"
        End If

        m_Settings.TagViewsAreaTag = textBoxAreaTag.Text
        m_Settings.TagViewsParmViewName = textBoxParameterViewName.Text
        m_Settings.TagViewsPrefixViewSource = textBoxPrefixViewSource.Text
        If checkBoxStripSuffix.Checked Then
            m_Settings.TagViewsStripSuffix = "true"
        Else
            m_Settings.TagViewsStripSuffix = "false"
        End If

        m_Settings.WriteIni()
    End Sub

    Private Sub buttonAddTags_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonAddTags.Click
        ' New Transaction
        Dim m_Trans As New DB.Transaction(m_Settings.Document, "HOK Add Tags")
        m_Trans.Start()

        Dim elementsViews As New List(Of DB.Element)
        Dim areasIdLookup As New Dictionary(Of String, String)
        Dim areaTest As Area
        Dim viewToUse As DB.View
        Dim areaToTag As Area
        Dim areaTag As AreaTag
        Dim areaTagType As AreaTagType = Nothing
        Dim areaTagTypes As New List(Of Element)
        Dim areaBoundingBox As New DB.BoundingBoxXYZ
        Dim elementId As DB.ElementId

        Dim elementData As String = ""
        Dim viewToUseElementId As String = ""
        Dim areaElementId As String = ""
        Dim nameRoot As String = ""
        Dim suffix As String = ""
        Dim validAreaTagType As Boolean = False

        Try
            'Strip blanks from text fields
            CleanTextInput()

            'Check that user has selected at least one view.
            If listBoxViews.SelectedItems.Count < 1 Then
                MessageBox.Show("Select view(s) from list before pressing 'Add Tags' button.", m_Settings.ProgramName)
                Return
            End If

            Dim CollectorAreaTagTypes As New DB.FilteredElementCollector(m_Settings.Document)
            CollectorAreaTagTypes.OfCategory(DB.BuiltInCategory.OST_AreaTags).WhereElementIsElementType()
            areaTagTypes = CollectorAreaTagTypes.ToElements


            'Check that the Area tag selction is valid in case user typed in or it came from settings
            For Each testTagType As Element In areaTagTypes
                If testTagType.Name = textBoxAreaTag.Text Then
                    areaTagType = testTagType
                    validAreaTagType = True
                    Exit For
                End If
            Next
            If Not validAreaTagType Then
                MessageBox.Show("The Area tag selection is not valid.  Command cannot continue.", m_Settings.ProgramName)
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
            'Dim progressBarForm As New form_ElemProgress("Add tags to " + listBoxViews.SelectedItems.Count.ToString & " Areas.", listBoxViews.SelectedItems.Count + 1)
            'progressBarForm.ShowDialog()
            'progressBarForm.Increment()
            'To avoid transparent look while waiting
            'Create a list to be able to look up a view name root and return the associated Area ID

            Dim CollectorAreas As New DB.FilteredElementCollector(m_Settings.Document)
            CollectorAreas.OfCategory(DB.BuiltInCategory.OST_Areas)
            elementsViews = CollectorAreas.ToElements

            For Each elementTest As DB.Element In elementsViews
                areaTest = DirectCast(elementTest, Area)
                If areaTest IsNot Nothing Then
                    For Each parameterTest As DB.Parameter In areaTest.Parameters
                        If parameterTest.Definition.Name = textBoxParameterViewName.Text Then
                            areasIdLookup.Add(parameterTest.AsString(), areaTest.Id.IntegerValue.ToString)
                            Exit For
                        End If
                    Next
                End If
            Next

            For Each listItem As String In listBoxViews.SelectedItems

                'Get the element data
                elementData = listItem.Substring(listItem.LastIndexOf("<") + 1, listItem.Length - listItem.LastIndexOf("<") - 2)
                viewToUseElementId = elementData.Substring(0, elementData.IndexOf("|"))
                'The view that is being used
                elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                nameRoot = elementData.Substring(0, elementData.IndexOf("|"))
                elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                suffix = elementData

                '***Note that we don't actually use the suffix value.  Leaving in place in case a reason arises, such as adding to tag?

                'Get the view to use
                elementId = New DB.ElementId(CInt(Convert.ToInt64(viewToUseElementId)))
                viewToUse = DirectCast(m_Settings.Document.GetElement(elementId), DB.View)

                'Add the Area data tag
                'Note, we are assuming that the original view was created in the form "Area" + <ViewRoot> so that the nameRoot value at this point
                'is the Area number (or other parameter used.)  Failing this, we just omit the tag.  Ideally we would look for the closest Area in the cropbox but that would require 
                'getting all of the Area insertion points and finding the closest one.  Alternatively we could allow the user to specify the prefixes.
                If areaTagType IsNot Nothing Then
                    'test shouldn't be necessary
                    If areasIdLookup.TryGetValue(nameRoot, areaElementId) Then
                        elementId = New DB.ElementId(CInt(Convert.ToInt64(areaElementId)))
                        areaToTag = DirectCast(m_Settings.Document.GetElement(elementId), Area)
                        areaBoundingBox = areaToTag.BoundingBox(viewToUse)
                        Dim uvPointInsertion As New DB.UV((areaBoundingBox.Max.X + areaBoundingBox.Min.X) / 2, (areaBoundingBox.Max.Y + areaBoundingBox.Min.Y) / 2)
                        areaTag = m_Settings.Document.Create.NewAreaTag(viewToUse, areaToTag, uvPointInsertion)
                        areaTag.AreaTagType = areaTagType
                    End If
                Else
                    areaToTag = Nothing
                End If

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
            Me.ToolStripStatusLabel1.Text = "Ready"
        Catch exception As Exception
            MessageBox.Show("Failed to add Area Tag: " & vbLf & exception.Message)
        End Try
    End Sub


    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub buttonSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonSelect.Click
        Dim dialog As New form_ElemSelectAreaTag(m_Settings, textBoxAreaTag)
        dialog.ShowDialog()
        'Note that textBoxAreaTag.text is set by SelectAreaTag dialog
        If Not initializing Then
            FillViewsList()
        End If
    End Sub


    Private Sub textBoxAreaTag_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxAreaTag.Leave
        If Not initializing Then
            FillViewsList()
        End If
    End Sub

    Private Sub textBoxAreaTag_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles textBoxAreaTag.KeyPress
        If e.KeyChar.ToString = vbCr Then
            textBoxAreaTag_Leave(sender, e)
        End If
    End Sub

    Private Sub checkBoxRestrictPrefix_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxRestrictPrefix.CheckedChanged
        If Not initializing Then
            FillViewsList()
        End If
    End Sub


    Private Sub textBoxRestrictPrefixValue_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxRestrictPrefixValue.Leave
        If Not initializing Then
            FillViewsList()
        End If
    End Sub

    Private Sub checkBoxIncludeExisting_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxIncludeExisting.CheckedChanged
        If Not initializing Then
            FillViewsList()
        End If
    End Sub


    Private Sub checkBoxListReverse_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            FillViewsList()
        End If
    End Sub


    Private Sub textBoxParameterViewName_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxParameterViewName.Leave
        If Not initializing Then
            FillViewsList()
        End If
    End Sub

    Private Sub textBoxParameterViewName_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles textBoxParameterViewName.KeyPress
        If e.KeyChar.ToString = vbCr Then
            textBoxParameterViewName_Leave(sender, e)
        End If
    End Sub


    Private Sub textBoxPrefixViewSource_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxPrefixViewSource.Leave
        If Not initializing Then
            FillViewsList()
        End If
    End Sub


    Private Sub textBoxPrefixViewSource_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles textBoxPrefixViewSource.KeyPress
        If e.KeyChar.ToString = vbCr Then
            textBoxPrefixViewSource_Leave(sender, e)
        End If
    End Sub

End Class