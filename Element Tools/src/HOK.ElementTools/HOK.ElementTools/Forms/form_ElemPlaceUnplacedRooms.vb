Imports Autodesk.Revit.DB
Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

' Need better handling for Metric areas as m2

''' <summary>
''' Added support for using RoomSeparation Lines ; Brok Howard 2011-02-22
''' </summary>
''' <remarks></remarks>
Public Class form_ElemPlaceUnplacedRooms

    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private initializing As Boolean = True

    ''' <summary>
    ''' Class Constructor 
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ElementTools-PlaceUnplacedRooms", settings.Document.Application.VersionNumber))

        'Initialize Settings Class
        m_Settings = settings

        ' Add Room Separaration Lines as an Option!
        Me.ComboBoxWallTypes.Items.Add("<Room Separation Lines>")
        ' Add the wall types as options
        Dim collector As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
        Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_Walls)
        Dim wallTypes As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()


        For Each wt As Element In wallTypes
            Me.ComboBoxWallTypes.Items.Add(wt.Name)
        Next

        ' Should we add a way to save and restore the last settings for this?
        Me.ComboBoxWallTypes.SelectedIndex = 0

        ' General Form Configurations... from ini, etc.
        textBoxParameterList1.Text = m_Settings.RoomsPlaceParamList1
        textBoxParameterList2.Text = m_Settings.RoomsPlaceParamList2
        If m_Settings.RoomsPlaceListPadYes1 = "true" Then
            checkBoxPad1.Checked = True
        Else
            checkBoxPad1.Checked = False
        End If
        If m_Settings.RoomsPlaceListPadYes2 = "true" Then
            checkBoxPad2.Checked = True
        Else
            checkBoxPad2.Checked = False
        End If
        textBoxPad1.Text = m_Settings.RoomsPlaceListPad1
        textBoxPad2.Text = m_Settings.RoomsPlaceListPad2
        If m_Settings.RoomsPlaceListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If
        textBoxStartX.Text = m_Settings.RoomsPlaceStartX
        textBoxStartY.Text = m_Settings.RoomsPlaceStartY
        textBoxSpace.Text = m_Settings.RoomsPlaceSpace
        textBoxNoRow.Text = m_Settings.RoomsPlaceNoRow
        textBoxParameterRequiredArea.Text = m_Settings.RoomsPlaceParamReqArea
        textBoxParameterRequiredDefault.Text = m_Settings.RoomsPlaceReqDefault
        ' Avoid rerunning FillRoomsList during setup
        initializing = False
        'Fill the list box with unplaced Rooms
        FillRoomsList()
    End Sub

#Region "Private Functions and Routines"

    ''' <summary>
    ''' Place the selected rooms
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub PlaceTheSelectedRooms()
        ' New Transaction
        Dim m_Trans As New Transaction(m_Settings.Document, "HOK Add Rooms")
        m_Trans.Start()

        If listBoxRooms.SelectedItems.Count < 1 Then
            MessageBox.Show("Select rooms from list before pressing 'Create Rooms' button.", m_Settings.ProgramName)
            Return
        End If

        'Use the level of the current view unless a match is found for data entered into TextBoxLevelNameParam
        Dim myLevel As Level = m_Settings.CurrentLevel

        ' Current Start Z
        Dim m_Zheight As Double = myLevel.Elevation

        'Get values from dialog box
        Dim ptCurrentRowStart As XYZ = New XYZ(CDbl(Convert.ToDouble(textBoxStartX.Text)), CDbl(Convert.ToDouble(textBoxStartY.Text)), m_Zheight)
        Dim space As Double = CDbl(Convert.ToDouble(textBoxSpace.Text))
        Dim noPerRow As Integer = CInt(Convert.ToInt16(textBoxNoRow.Text))
        Dim areaDefault As Double = CDbl(Convert.ToDouble(textBoxParameterRequiredDefault.Text))
        If areaDefault <= 0 Then
            areaDefault = 100
        End If

        'Initial values for geometry
        Dim currentRowPosiiton As Integer = 1
        Dim ptCurrentBox1 As XYZ = New XYZ(ptCurrentRowStart.X, ptCurrentRowStart.Y, m_Zheight)
        Dim maxHeightCurrentRow As Double = 0

        Me.ToolStripStatusLabel1.Text = "Creating " & listBoxRooms.SelectedItems.Count.ToString & " rooms."
        Application.DoEvents()
        System.Threading.Thread.Sleep(1000)
        Me.ToolStripProgressBar1.Value = 0
        Me.ToolStripProgressBar1.Maximum = listBoxRooms.SelectedItems.Count + 1
        Me.ToolStripProgressBar1.Visible = True
        Me.ToolStripProgressBar1.ProgressBar.Refresh()
        Me.ToolStripProgressBar1.PerformStep()

        'Start the progress bar
        'Dim progressBarForm As New form_ElemProgress("Creating " + listBoxRooms.SelectedItems.Count.ToString & " rooms.", listBoxRooms.SelectedItems.Count + 1)
        'progressBarForm.ShowDialog()
        'progressBarForm.Increment()

        'To avoid transparent look while waiting
        For Each listItem As String In listBoxRooms.SelectedItems

            'Get the existing room
            Dim elemId As New ElementId(CInt(Convert.ToInt64(listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.IndexOf("<") - 2))))

            Dim roomToPlace As Architecture.Room = DirectCast(m_Settings.Document.GetElement(elemId), Architecture.Room)
            Dim parameterArea As Parameter = roomToPlace.LookupParameter(textBoxParameterRequiredArea.Text)

            Dim areaCurrent As Double
            If parameterArea Is Nothing Then
                areaCurrent = CDbl(Convert.ToDouble(textBoxParameterRequiredDefault.Text))
            Else
                areaCurrent = parameterArea.AsDouble()
            End If
            If areaCurrent <= 0 Then
                areaCurrent = areaDefault
            End If
            Dim sideCurrent As Double = Math.Sqrt(areaCurrent)

            'Calculate points for geometry;
            Dim ptCurrentBox2 As XYZ = New XYZ(ptCurrentBox1.X + sideCurrent, ptCurrentBox1.Y, m_Zheight)
            Dim ptCurrentBox3 As XYZ = New XYZ(ptCurrentBox1.X + sideCurrent, ptCurrentBox1.Y - sideCurrent, m_Zheight)
            Dim ptCurrentBox4 As XYZ = New XYZ(ptCurrentBox1.X, ptCurrentBox1.Y - sideCurrent, m_Zheight)
            Dim ptCurrentInsert As UV = New UV(ptCurrentBox1.X + sideCurrent / 2, ptCurrentBox1.Y - sideCurrent / 2)

            'Place boundary curves
            Dim line1 As Curve = Line.CreateBound(ptCurrentBox1, ptCurrentBox2)
            Dim line2 As Curve = Line.CreateBound(ptCurrentBox2, ptCurrentBox3)
            Dim line3 As Curve = Line.CreateBound(ptCurrentBox3, ptCurrentBox4)
            Dim line4 As Curve = Line.CreateBound(ptCurrentBox4, ptCurrentBox1)
            

            ' Normal is always up in our case
            Dim normalXYZ As New XYZ(0, 0, m_Zheight + 1)

            If Me.ComboBoxWallTypes.SelectedItem <> "<Room Separation Lines>" Then

                ' Use the selected wall type
                Dim myWallType As WallType = Nothing
                Dim collector As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
                Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_Walls)
                Dim wallTypes As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()

                For Each wt As WallType In wallTypes
                    If wt.Name = Me.ComboBoxWallTypes.SelectedItem Then
                        myWallType = wt
                    End If
                Next

                ' Slam in the walls
                Dim newWall As Wall = Nothing
                newWall = Wall.Create(m_Settings.Document, line1, myWallType.Id, myLevel.Id, 10, 0, False, False)
                newWall = Wall.Create(m_Settings.Document, line2, myWallType.Id, myLevel.Id, 10, 0, False, False)
                newWall = Wall.Create(m_Settings.Document, line3, myWallType.Id, myLevel.Id, 10, 0, False, False)
                newWall = Wall.Create(m_Settings.Document, line4, myWallType.Id, myLevel.Id, 10, 0, False, False)

            Else

                ' Draw the boundary with Room Separation lines
                Dim m_Boundary As New ModelCurveArray
                Dim m_CurveArrary As New CurveArray
                m_CurveArrary.Append(line1)
                m_CurveArrary.Append(line2)
                m_CurveArrary.Append(line3)
                m_CurveArrary.Append(line4)

                Dim m_Plane As SketchPlane
                m_Plane = CreateSketchPlaneByCurve(m_CurveArrary)

                ' Trace the room separation lines...
                m_Boundary = m_Settings.Document.Create.NewRoomBoundaryLines(m_Plane, m_CurveArrary, m_Settings.ActiveView)

            End If

            'Place the new room and copy parameters from the old room to place
            Dim roomNew As Architecture.Room
            Try
                roomNew = m_Settings.Document.Create.NewRoom(m_Settings.CurrentLevel, ptCurrentInsert)
            Catch
                MessageBox.Show("Unable to create new rooms.", m_Settings.ProgramName)
                Me.ToolStripProgressBar1.Visible = False
                Return
            End Try

            ' '' '' Set the room height to a default of 8
            '' ''If Me.ComboBoxWallTypes.SelectedItem = "<Room Separation Lines>" Then
            '' ''    For Each parameterNew As Parameter In roomNew.Parameters
            '' ''        If parameterNew.Definition.Name.ToUpper = "LIMIT OFFSET" Then
            '' ''            Try
            '' ''                Select Case parameterNew.StorageType
            '' ''                    Case StorageType.Double
            '' ''                        parameterNew.Set(m_Zheight + 8)
            '' ''                        Exit Select
            '' ''                End Select
            '' ''            Catch
            '' ''            End Try
            '' ''            'Not sure why some that can't be set are not read only (like perimeter) but ignore them
            '' ''            Exit For
            '' ''        End If
            '' ''    Next
            '' ''End If

            ' Parameter Data
            For Each parameterToPlace As Parameter In roomToPlace.Parameters
                If parameterToPlace.IsReadOnly = False Then

                    ' The ParamName
                    Dim m_ParamTest As String = parameterToPlace.Definition.Name.ToUpper

                    If m_ParamTest <> "AREA" Or _
                         m_ParamTest <> "LEVEL" Or _
                         m_ParamTest <> "UNBOUNDED HEIGHT" Or _
                         m_ParamTest <> "UPPER LIMIT" Or _
                         m_ParamTest <> "BASE OFFSET" Or _
                         m_ParamTest <> "VOLUME" Or _
                         m_ParamTest <> "LIMIT OFFSET" Then
                        For Each parameterNew As Parameter In roomNew.Parameters
                            If parameterNew.Definition.Name = parameterToPlace.Definition.Name Then
                                Try
                                    Select Case parameterNew.StorageType
                                        Case StorageType.ElementId
                                            ' Do nothing... problematic in this use
                                            Exit Select
                                        Case StorageType.String
                                            parameterNew.[Set](parameterToPlace.AsString)
                                            Exit Select
                                        Case StorageType.Double
                                            parameterNew.[Set](parameterToPlace.AsDouble)
                                            Exit Select
                                        Case StorageType.Integer
                                            parameterNew.[Set](parameterToPlace.AsInteger)
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
            Next

            'Tag the new room
            Dim roomId As LinkElementId = New LinkElementId(roomNew.Id)
            Dim roomTag As Architecture.RoomTag = m_Settings.Document.Create.NewRoomTag(roomId, ptCurrentInsert, m_Settings.ActiveView.Id)
            roomTag.HasLeader = False

            'Delete the old area
            m_Settings.Document.Delete(roomToPlace.Id)

            'Increment geometry values
            If sideCurrent > maxHeightCurrentRow Then
                maxHeightCurrentRow = sideCurrent
            End If
            If currentRowPosiiton < noPerRow Then
                ptCurrentBox1 = New XYZ(ptCurrentBox1.X + sideCurrent + space, ptCurrentRowStart.Y, m_Zheight)
                currentRowPosiiton += 1
            Else
                ptCurrentRowStart = New XYZ(CDbl(Convert.ToDouble(textBoxStartX.Text)), ptCurrentRowStart.Y - (maxHeightCurrentRow + space), m_Zheight)
                ptCurrentBox1 = New XYZ(ptCurrentRowStart.X, ptCurrentRowStart.Y, m_Zheight)

                maxHeightCurrentRow = 0
                currentRowPosiiton = 1
            End If

            'Increment the Progress Bar
            Me.ToolStripProgressBar1.PerformStep()
        Next

        Try
            ' Commit the transaction
            m_Trans.Commit()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, ex.Source)
            m_Trans.RollBack()
        End Try

        'Remove all the values from the list (can't do as we go along since messes up increment)
        While listBoxRooms.SelectedItems.Count > 0
            listBoxRooms.Items.Remove(listBoxRooms.SelectedItem)
        End While

        'Close the progress bar.
        Me.ToolStripStatusLabel1.Text = "Ready"
        Me.ToolStripProgressBar1.Visible = False
    End Sub

    ''' <summary>
    ''' Pad zeros for Room Names
    ''' </summary>
    ''' <param name="input"></param>
    ''' <param name="length"></param>
    ''' <remarks></remarks>
    Private Sub PadZeros(ByRef input As String, ByVal length As Integer)
        While input.Length < length
            input = "0" & input
        End While
    End Sub

    ''' <summary>
    ''' Save the environment settings to the Ini file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SaveSettings()
        ' Save the parameter names
        m_Settings.RoomsPlaceParamList1 = textBoxParameterList1.Text
        m_Settings.RoomsPlaceParamList2 = textBoxParameterList2.Text
        If checkBoxPad1.Checked Then
            m_Settings.RoomsPlaceListPadYes1 = "true"
        Else
            m_Settings.RoomsPlaceListPadYes1 = "false"
        End If
        If checkBoxPad2.Checked Then
            m_Settings.RoomsPlaceListPadYes2 = "true"
        Else
            m_Settings.RoomsPlaceListPadYes2 = "false"
        End If
        m_Settings.RoomsPlaceListPad1 = textBoxPad1.Text
        m_Settings.RoomsPlaceListPad2 = textBoxPad2.Text
        If checkBoxListReverse.Checked Then
            m_Settings.RoomsPlaceListReverse = "true"
        Else
            m_Settings.RoomsPlaceListReverse = "false"
        End If

        m_Settings.RoomsPlaceStartX = textBoxStartX.Text
        m_Settings.RoomsPlaceStartY = textBoxStartY.Text
        m_Settings.RoomsPlaceSpace = textBoxSpace.Text
        m_Settings.RoomsPlaceNoRow = textBoxNoRow.Text
        m_Settings.RoomsPlaceParamReqArea = textBoxParameterRequiredArea.Text
        m_Settings.RoomsPlaceReqDefault = textBoxParameterRequiredDefault.Text

        m_Settings.WriteIni()
    End Sub

    ''' <summary>
    ''' Create a new sketch plane
    ''' </summary>
    ''' <param name="normal"></param>
    ''' <param name="origin"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateSketchPlane(ByVal normal As XYZ, ByVal origin As XYZ) As SketchPlane
        Try

            Dim geometryPlane As Plane = Plane.CreateByNormalAndOrigin(normal, origin)
            If geometryPlane Is Nothing Then
                Throw New Exception("Create the geometry plane failed.")
            End If
            Dim sPlane As SketchPlane = SketchPlane.Create(m_Settings.Document, geometryPlane)

            If sPlane Is Nothing Then
                ' assert the creation is successful
                Throw New Exception("Create the sketch plane failed.")
            End If

            ' Return the sketch plane array
            Return sPlane

        Catch ex As Exception
            Throw New Exception("Can not create the sketch plane, message: " + ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Create a new sketch plane from a curvearray
    ''' </summary>
    ''' <param name="curve"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateSketchPlaneByCurve(ByVal curve As CurveArray) As SketchPlane
        Try
            ' First create a Geometry.Plane which need in NewSketchPlane() method
            Dim curveList As List(Of Curve) = New List(Of Curve)
            Dim curveIterator As CurveArrayIterator = curve.ForwardIterator
            While (curveIterator.MoveNext())
                Dim aCurve As Curve = curveIterator.Current
                curveList.Add(aCurve)
            End While

            Dim cloop As CurveLoop = CurveLoop.Create(curveList)
            Dim geometryPlane As Plane = cloop.GetPlane()
            If geometryPlane Is Nothing Then
                ' assert the creation is successful
                Throw New Exception("Create the geometry plane failed.")
            End If
            ' Then create a sketch plane using the Geometry.Plane

            Dim plane As SketchPlane = SketchPlane.Create(m_Settings.Document, geometryPlane)

            If plane Is Nothing Then
                ' assert the creation is successful
                Throw New Exception("Create the sketch plane failed.")
            End If

            ' Return the sketch plane array
            Return plane

        Catch ex As Exception
            Throw New Exception("Can not create the sketch plane, message: " + ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Get the List of Rooms in the Model
    ''' </summary>
    ''' <remarks></remarks> 
    Private Sub FillRoomsList()
        ' Start with setting the zero padding for the room numbering strings
        Dim padZeros1 As Integer = 0
        Dim padZeros2 As Integer = 0
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

        ' Get the Rooms Collection
        Dim filCollector As New FilteredElementCollector(m_Settings.Document)
        filCollector.OfCategory(BuiltInCategory.OST_Rooms)
        Dim m_Rooms As New List(Of Element)
        m_Rooms = filCollector.ToElements

        ' Clear and reset the form list controls
        mListItems.Clear()
        listBoxRooms.Items.Clear()

        ' Iterate the Rooms
        For Each x_Room As Architecture.Room In m_Rooms

            Dim listBy1 As String = ""
            Dim listBy2 As String = ""
            Dim notPlaced As Boolean = False

            For Each parameter As Parameter In x_Room.Parameters
                If parameter.Definition.Name = "Area" Then
                    If parameter.AsDouble() = 0 Then
                        notPlaced = True
                    Else
                        Exit For
                    End If
                End If
                If parameter.Definition.Name = textBoxParameterList1.Text Then
                    Select Case parameter.StorageType
                        Case StorageType.String
                            listBy1 = parameter.AsString()
                            Exit Select
                        Case StorageType.Double
                            listBy1 = parameter.AsDouble().ToString
                            Exit Select
                        Case StorageType.Integer
                            listBy1 = parameter.AsInteger().ToString
                            Exit Select
                        Case Else
                            'ignore none and ElementID case
                            Exit Select
                    End Select
                    If checkBoxPad1.Checked Then
                        PadZeros(listBy1, padZeros1)
                    End If
                End If
                If parameter.Definition.Name = textBoxParameterList2.Text Then
                    Select Case parameter.StorageType
                        Case StorageType.String
                            listBy2 = parameter.AsString()
                            Exit Select
                        Case StorageType.Double
                            listBy2 = parameter.AsDouble().ToString
                            Exit Select
                        Case StorageType.Integer
                            listBy2 = parameter.AsInteger().ToString
                            Exit Select
                        Case Else
                            'ignore none and ElementID case
                            Exit Select
                    End Select
                    If checkBoxPad2.Checked Then
                        PadZeros(listBy2, padZeros2)
                    End If

                End If
            Next
            If notPlaced Then
                mListItems.Add((listBy1 & " + " & listBy2 & Convert.ToString(m_Settings.Spacer) & "<") + x_Room.Id.IntegerValue.ToString & ">")
            End If
        Next

        ' Sort the List Alpha
        mListItems.Sort()

        If checkBoxListReverse.Checked Then
            mListItems.Reverse()
        End If

        ' Add the items to the listbox
        For Each item As String In mListItems
            listBoxRooms.Items.Add(item)
        Next

    End Sub

#End Region

#Region "Form Controls and Events"

    ''' <summary>
    ''' Pick starting point for rooms placement
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ButtonPickPoint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPickPoint.Click
        Me.DialogResult = DialogResult.Retry

        'Me.Hide()
        'Try
        '    Dim point As XYZ = m_Settings.UIdoc.Selection.PickPoint("Please pick a point to place component.")
        '    If point IsNot Nothing Then
        '        Me.textBoxStartX.Text = point.X.ToString
        '        Me.textBoxStartY.Text = point.Y.ToString
        '    Else
        '        Dim test1 As String = ""
        '    End If
        'Catch ex As Exception
        '    Dim message As String = ex.Message
        'End Try
        'Me.ShowDialog()
    End Sub

    ''' <summary>
    ''' Close the program
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = DialogResult.OK
    End Sub

    ''' <summary>
    ''' Place the Rooms
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click
        ' Place the rooms
        PlaceTheSelectedRooms()
    End Sub

    Private Sub textBoxParameterList1_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList1.Leave
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

    Private Sub textBoxParameterList2_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList2.Leave
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

    Private Sub checkBoxPad1_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad1.CheckedChanged
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

    Private Sub checkBoxPad2_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad2.CheckedChanged
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

    Private Sub textBoxPad1_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad1.Leave
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

    Private Sub textBoxPad2_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad2.Leave
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

    Private Sub checkBoxListReverse_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            FillRoomsList()
        End If
    End Sub

#End Region

End Class