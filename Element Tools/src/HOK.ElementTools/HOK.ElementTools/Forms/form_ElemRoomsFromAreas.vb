Imports Autodesk.Revit

Imports System.Windows.Forms
Imports Autodesk.Revit.DB

Public Class form_ElemRoomsFromAreas

    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private excludePlaced As Boolean
    Private excludeNotPlaced As Boolean
    Private initializing As Boolean = True

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        'Initialize the controls from settings
        m_Settings = settings
        'assuming already initialized
        If m_Settings.RoomsFromAreasIncludePlaced = "true" Then
            radioButtonPlaced.Checked = True
        Else
            radioButtonPlaced.Checked = False
        End If
        If m_Settings.RoomsFromAreasIncludeNotPlaced = "true" Then
            radioButtonNotPlaced.Checked = True
        Else
            radioButtonNotPlaced.Checked = False
        End If
        If m_Settings.RoomsFromAreasIncludeBoth = "true" Then
            radioButtonBoth.Checked = True
        Else
            radioButtonBoth.Checked = False
        End If
        textBoxParameterList1.Text = m_Settings.RoomsFromAreasParamList1
        textBoxParameterList2.Text = m_Settings.RoomsFromAreasParamList2
        If m_Settings.RoomsFromAreasListPadYes1 = "true" Then
            checkBoxPad1.Checked = True
        Else
            checkBoxPad1.Checked = False
        End If
        If m_Settings.RoomsFromAreasListPadYes2 = "true" Then
            checkBoxPad2.Checked = True
        Else
            checkBoxPad2.Checked = False
        End If
        textBoxPad1.Text = m_Settings.RoomsFromAreasListPad1
        textBoxPad2.Text = m_Settings.RoomsFromAreasListPad2
        If m_Settings.RoomsFromAreasListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        initializing = False
        'to avoid rerunning FillRoomsList during setup
        'Fill the list box with areas
        FillAreasList()
    End Sub

    Private Sub FillAreasList()
        Dim category As DB.Category = Nothing
        Dim elements As New List(Of DB.Element)

        Dim listBy1 As String
        Dim listBy2 As String
        Dim padZeros1 As Integer
        Dim padZeros2 As Integer
        Try
            padZeros1 = CInt(Convert.ToInt16(textBoxPad1.Text))
        Catch
            padZeros1 = 0
        End Try
        Try
            padZeros2 = CInt(Convert.ToInt16(textBoxPad2.Text))
        Catch
            padZeros2 = 0
        End Try
        If padZeros1 < 0 Then
            padZeros1 = 0
        End If
        If padZeros2 < 0 Then
            padZeros2 = 0
        End If

        Dim placed As Boolean

        Try
            For Each categoryTest As DB.Category In m_Settings.Document.Settings.Categories
                If categoryTest.Name = "Areas" Then
                    category = categoryTest
                    Continue For
                End If
            Next

            Dim filCollector As New DB.FilteredElementCollector(m_Settings.Document)
            ' Add the category to the filter
            filCollector.OfCategory(category.Id.IntegerValue)
            ' Return the list of elements from the filter collector
            elements = filCollector.ToElements

            '' ''Dim filter As DB.Filter = Nothing
            '' ''Filter = mSettings.Application.Create.Filter.NewCategoryFilter(category)
            '' ''mSettings.Application.ActiveDocument.get_Elements(Filter, elements)

            'Set up for placed/not placed selection based on radio button group
            excludePlaced = False
            excludeNotPlaced = False
            If radioButtonPlaced.Checked Then
                excludeNotPlaced = True
            ElseIf radioButtonNotPlaced.Checked Then
                excludePlaced = True
            End If

            mListItems.Clear()
            listBoxAreas.Items.Clear()
            For Each element As DB.Element In elements

                'The selection filter seems to return both the symbols and the instances; assume we only want the instances.
                If TypeOf element Is DB.FamilySymbol Then
                    Continue For
                End If

                '***** Note that placed but not enclosed areas will get included as if not placed; not really a problem but may be confusing
                'Control placed or not placed and level
                placed = True
                For Each parameter As DB.Parameter In element.Parameters
                    If parameter.Definition.Name = "Area" Then
                        If parameter.AsDouble() = 0 Then
                            placed = False
                        End If
                        'Assuming that this means it is not placed (also gets not enclosed)
                        Exit For
                    End If
                Next
                If excludePlaced AndAlso placed Then
                    Continue For
                End If
                If excludeNotPlaced AndAlso Not placed Then
                    Continue For
                End If
                'Restrict placed areas to current level
                '***** Note this may be too restrictive but avoids problem with determining bounding box.
                'Note that unplaced areas seem to have a level value

                '***** Not imposing restiction at this time
                'if ((element.Level.Name == mSettings.CurrentLevel.Name) && placed) continue;

                listBy1 = ""
                listBy2 = ""


                For Each parameter As DB.Parameter In element.Parameters
                    If parameter.Definition.Name = textBoxParameterList1.Text Then
                        Select Case parameter.StorageType
                            Case DB.StorageType.String
                                listBy1 = parameter.AsString()
                                Exit Select
                            Case DB.StorageType.Double
                                listBy1 = parameter.AsDouble().ToString
                                Exit Select
                            Case DB.StorageType.Integer
                                listBy1 = parameter.AsInteger().ToString
                                Exit Select
                            Case Else
                                'ignore none and ElementID case
                                Exit Select
                        End Select
                        If checkBoxPad1.Checked Then
                            If listBy1 Is Nothing Then
                                listBy1 = "0"
                            End If
                            PadZeros(listBy1, padZeros1)
                        End If
                    End If
                    If parameter.Definition.Name = textBoxParameterList2.Text Then
                        Select Case parameter.StorageType
                            Case DB.StorageType.String
                                listBy2 = parameter.AsString()
                                Exit Select
                            Case DB.StorageType.Double
                                listBy2 = parameter.AsDouble().ToString
                                Exit Select
                            Case DB.StorageType.Integer
                                listBy2 = parameter.AsInteger().ToString
                                Exit Select
                            Case Else
                                'ignore none and ElementID case
                                Exit Select
                        End Select
                        If checkBoxPad2.Checked Then
                            If listBy1 Is Nothing Then
                                listBy2 = "0"
                            End If
                            PadZeros(listBy2, padZeros2)
                        End If

                    End If
                Next

                'Add to the list
                mListItems.Add((listBy1 & " + " & listBy2 & Convert.ToString(m_Settings.Spacer) & "<") + element.Id.IntegerValue.ToString & ">")
            Next

            mListItems.Sort()
            If checkBoxListReverse.Checked Then
                mListItems.Reverse()
            End If
            For Each item As String In mListItems
                listBoxAreas.Items.Add(item)
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message, m_Settings.ProgramName)
        End Try


    End Sub

    Private Sub PadZeros(ByRef input As String, ByVal length As Integer)
        While input.Length < length
            input = "0" & input
        End While
    End Sub

    Private Sub SaveSettings()

        If radioButtonPlaced.Checked Then
            m_Settings.RoomsFromAreasIncludePlaced = "true"
        Else
            m_Settings.RoomsFromAreasIncludePlaced = "false"
        End If
        If radioButtonNotPlaced.Checked Then
            m_Settings.RoomsFromAreasIncludeNotPlaced = "true"
        Else
            m_Settings.RoomsFromAreasIncludeNotPlaced = "false"
        End If
        If radioButtonBoth.Checked Then
            m_Settings.RoomsFromAreasIncludeBoth = "true"
        Else
            m_Settings.RoomsFromAreasIncludeBoth = "false"
        End If
        m_Settings.RoomsFromAreasParamList1 = textBoxParameterList1.Text
        m_Settings.RoomsFromAreasParamList2 = textBoxParameterList2.Text
        If checkBoxPad1.Checked Then
            m_Settings.RoomsFromAreasListPadYes1 = "true"
        Else
            m_Settings.RoomsFromAreasListPadYes1 = "false"
        End If
        If checkBoxPad2.Checked Then
            m_Settings.RoomsFromAreasListPadYes2 = "true"
        Else
            m_Settings.RoomsFromAreasListPadYes2 = "false"
        End If
        m_Settings.RoomsFromAreasListPad1 = textBoxPad1.Text
        m_Settings.RoomsFromAreasListPad2 = textBoxPad2.Text
        If checkBoxListReverse.Checked Then
            m_Settings.RoomsFromAreasListReverse = "true"
        Else
            m_Settings.RoomsFromAreasListReverse = "false"
        End If

        m_Settings.WriteIni()
    End Sub

    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click
        '***Note that we are not checking for duplicate room numbers.  Revit allows creation but shows a warning.
        'Better would be to have option to update parameter values of existing matches between rooms and areas???
        ' New Transaction
        Dim m_Trans As New DB.Transaction(m_Settings.Document, "HOK Rooms from Areas")
        m_Trans.Start()

        Dim roomNew As DB.Architecture.Room
        Dim areaToConvert As DB.Area
        Dim roomTag As DB.Architecture.RoomTag
        Dim elemId As DB.ElementId
        Dim boundingBox As DB.BoundingBoxXYZ
        Dim placed As Boolean
        Dim errorNotAreaPlan As Boolean = False
        Dim ptCurrentInsert As DB.UV = New DB.UV(0, 0)

        If listBoxAreas.SelectedItems.Count < 1 Then
            MessageBox.Show("Select rooms from list before pressing 'Create Rooms' button.", m_Settings.ProgramName)
            Return
        End If

        Me.ToolStripStatusLabel1.Text = "Creating " & listBoxAreas.SelectedItems.Count.ToString & " rooms."
        Application.DoEvents()
        System.Threading.Thread.Sleep(1000)
        Me.ToolStripProgressBar1.Value = 0
        Me.ToolStripProgressBar1.Maximum = listBoxAreas.SelectedItems.Count + 1
        Me.ToolStripProgressBar1.Visible = True
        Me.ToolStripProgressBar1.ProgressBar.Refresh()
        Me.ToolStripProgressBar1.PerformStep()

        'Start the progress bar
        'Dim progressBarForm As New form_ElemProgress("Creating " + listBoxAreas.SelectedItems.Count.ToString & " rooms.", listBoxAreas.SelectedItems.Count + 1)
        'progressBarForm.ShowDialog()
        'progressBarForm.Increment()
        'To avoid transparent look while waiting
        For Each listItem As String In listBoxAreas.SelectedItems

            'Get the area
            elemId = New DB.ElementId(CInt(Convert.ToInt64(listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.LastIndexOf("<") - 2))))
            areaToConvert = DirectCast(m_Settings.Document.GetElement(elemId), DB.Area)


            'Place the new room and copy parameters from the old room to place
            'ElementId levelId = mSettings.CurrentLevel.Id;
            placed = True
            For Each parameter As DB.Parameter In areaToConvert.Parameters
                If parameter.Definition.Name = "Area" Then
                    If parameter.AsDouble() = 0 Then
                        placed = False
                    End If
                    'Assuming that this means it is not placed 
                    Exit For
                End If
            Next
            Try
                If placed Then
                    'Note that bounding box will be null if the active view doesn't contain the area.
                    boundingBox = areaToConvert.BoundingBox(m_Settings.ActiveView)
                    If boundingBox Is Nothing Then
                        errorNotAreaPlan = True
                        'listBoxAreas.SelectedItems.Remove(listItem);   //can't do this since it messes up the increment
                        Me.ToolStripProgressBar1.PerformStep()
                        Continue For
                    End If
                    ptCurrentInsert = New DB.UV((boundingBox.Max.X + boundingBox.Min.X) / 2, (boundingBox.Max.Y + boundingBox.Min.Y) / 2)

                    roomNew = m_Settings.Document.Create.NewRoom(m_Settings.CurrentLevel, ptCurrentInsert) 'Creates a new room on a level at a specified point. 
                Else
                    roomNew = m_Settings.Document.Create.NewRoom(m_Settings.CurrentPhase) 'Creates a new unplaced room and with an assigned phase. 
                End If
            Catch
                MessageBox.Show("Unable to create new room. Halting process.", m_Settings.ProgramName)
                Me.ToolStripProgressBar1.Visible = False
                Return
            End Try

            For Each parameterToPlace As DB.Parameter In areaToConvert.Parameters
                If parameterToPlace.IsReadOnly = False Then
                    If parameterToPlace.Definition.Name.ToUpper() <> "AREA" Then
                        If parameterToPlace.Definition.Name.ToUpper() <> "LEVEL" Then
                            If parameterToPlace.Definition.Name.ToUpper() <> "UNBOUNDED HEIGHT" Then
                                If parameterToPlace.Definition.Name.ToUpper() <> "UPPER LIMIT" Then
                                    If parameterToPlace.Definition.Name.ToUpper() <> "BASE OFFSET" Then
                                        If parameterToPlace.Definition.Name.ToUpper() <> "VOLUME" Then
                                            If parameterToPlace.Definition.Name.ToUpper() <> "LIMIT OFFSET" Then
                                                For Each parameterNew As DB.Parameter In roomNew.Parameters
                                                    If parameterNew.Definition.Name = parameterToPlace.Definition.Name Then
                                                        Try
                                                            Select Case parameterNew.StorageType
                                                                Case DB.StorageType.ElementId
                                                                    'This doesn't work; ignore for now but could be an issue with key tables
                                                                    'parameterNew.Set(ref parameterToPlace.AsElementId());
                                                                    Exit Select
                                                                Case DB.StorageType.String
                                                                    parameterNew.[Set](parameterToPlace.AsString())
                                                                    Exit Select
                                                                Case DB.StorageType.Double
                                                                    parameterNew.[Set](parameterToPlace.AsDouble())
                                                                    Exit Select
                                                                Case DB.StorageType.Integer
                                                                    parameterNew.[Set](parameterToPlace.AsInteger())
                                                                    Exit Select
                                                                Case Else
                                                                    'ignore none case
                                                                    Exit Select
                                                            End Select
                                                        Catch
                                                        End Try
                                                        'Not sure why some that can't be set are not read only (like perimeter) but ignore them
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Next
            'Tag the new room
#If RELEASE2013 Then
            roomTag = m_Settings.Document.Create.NewRoomTag(roomNew, ptCurrentInsert, m_Settings.ActiveView)
#ElseIf RELEASE2014 Or RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
            Dim linkRoomId As LinkElementId = New LinkElementId(roomNew.Id)
            roomTag = m_Settings.Document.Create.NewRoomTag(linkRoomId, ptCurrentInsert, m_Settings.ActiveView.Id)
#End If

            roomTag.HasLeader = False

            'Increment the Progress Bar
            Me.ToolStripProgressBar1.PerformStep()
        Next

        ' Commit the transaction
        m_Trans.Commit()

        'Remove all the values from the list (can't do as we go along since messes up increment)
        While listBoxAreas.SelectedItems.Count > 0
            listBoxAreas.Items.Remove(listBoxAreas.SelectedItem)
        End While

        'Close the progress bar.
        Me.ToolStripStatusLabel1.Text = "Ready"
        Me.ToolStripProgressBar1.Visible = False

        If errorNotAreaPlan Then
            MessageBox.Show("One or more new rooms were not created because the" & vbLf & "current view is not an area plan showing the area.", m_Settings.ProgramName)
        End If
    End Sub

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = Windows.Forms.DialogResult.OK
    End Sub

    Private Sub radioButtonPlaced_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonPlaced.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub radioButtonNotPlaced_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonNotPlaced.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub radioButtonBoth_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonBoth.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterList1_Leave_1(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList1.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad1_Leave_1(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad1.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterList2_Leave_1(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList2.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad2_Leave_1(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad2.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxPad1_CheckedChanged_1(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad1.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxPad2_CheckedChanged_1(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad2.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxListReverse_CheckedChanged_1(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

End Class