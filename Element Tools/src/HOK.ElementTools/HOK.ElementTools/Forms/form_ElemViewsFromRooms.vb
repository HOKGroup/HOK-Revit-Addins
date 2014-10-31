Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Architecture

Imports System.Windows.Forms

Public Class form_ElemViewsFromRooms

    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private mDictGroupedByRooms As New Dictionary(Of String, List(Of Room))
    Private mRoomsToProcess As New List(Of Room)
    Private mViewTemplates As New Dictionary(Of ViewType, Dictionary(Of String, ElementId))
    Private mElevationMaps As New Dictionary(Of Integer, Integer) 'viewid, markerid
    Private initializing As Boolean = True

#Region "Constructor"
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()
        'Initialize the settings text boxes
        m_Settings = settings

        CollectViewTemplates()
        CollecElevationMarks()

        'assuming already initialized
        If m_Settings.ViewsFromRoomsGrouping = "single" Then
            radioButtonGroupSingle.Checked = True
        Else
            radioButtonGroupMultiple.Checked = True
        End If
        Me.ProgressBar1.Visible = False
        textBoxParameterList1.Text = m_Settings.ViewsFromRoomsParamList1
        textBoxParameterList2.Text = m_Settings.ViewsFromRoomsParamList2
        If m_Settings.ViewsFromRoomsListPadYes1 = "true" Then
            checkBoxPad1.Checked = True
        Else
            checkBoxPad1.Checked = False
        End If
        If m_Settings.ViewsFromRoomsListPadYes2 = "true" Then
            checkBoxPad2.Checked = True
        Else
            checkBoxPad2.Checked = False
        End If
        textBoxPad1.Text = m_Settings.ViewsFromRoomsListPad1
        textBoxPad2.Text = m_Settings.ViewsFromRoomsListPad2
        textBoxParameterGroupBy.Text = m_Settings.ViewsFromRoomsParamGroupBy
        If m_Settings.ViewsFromRoomsListExisting = "true" Then
            checkBoxListExisting.Checked = True
        Else
            checkBoxListExisting.Checked = False
        End If
        If m_Settings.ViewsFromRoomsListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        textBoxParameterViewName.Text = m_Settings.ViewsFromRoomsParmViewName
        textBoxParameterRoomName.Text = m_Settings.ViewsFromRoomsParmRoomName
        textBoxScale.Text = m_Settings.ViewsFromRoomsScale
        textBoxPrefixViewTarget.Text = m_Settings.ViewsFromRoomsPrefixViewTarget
        If m_Settings.ViewsFromRoomsViewType = "2d" Then
            radioButtonType2d.Checked = True
        ElseIf m_Settings.ViewsFromRoomsViewType = "Elevation" Then
            RadioButtonTypeElevation.Checked = True
        ElseIf m_Settings.ViewsFromRoomsViewType = "3dBox" Then
            radioButtonType3dBox.Checked = True
        ElseIf m_Settings.ViewsFromRoomsViewType = "3dCrop" Then
            radioButtonType3dCrop.Checked = True
        Else
            radioButtonType3dBoxCrop.Checked = True
        End If
        '"3dBoxCrop" case
        textBoxVectorX.Text = m_Settings.ViewsFromRoomsVectorX
        textBoxVectorY.Text = m_Settings.ViewsFromRoomsVectorY
        textBoxVectorZ.Text = m_Settings.ViewsFromRoomsVectorZ
        If m_Settings.ViewsFromRoomsReplaceExisting = "true" Then
            checkBoxReplaceExisting.Checked = True
        Else
            checkBoxReplaceExisting.Checked = False
        End If
        If m_Settings.ViewsFromRoomsSizeBoxType = "dynamic" Then
            radioButtonSizeBoxDynamic.Checked = True
        Else
            radioButtonSizeBoxFixed.Checked = True
        End If
        textBoxBoxSpace.Text = m_Settings.ViewsFromRoomsBoxSpace
        textBoxBoxFixedX.Text = m_Settings.ViewsFromRoomsBoxFixedX
        textBoxBoxFixedY.Text = m_Settings.ViewsFromRoomsBoxFixedY
        textBoxBoxFixedZ.Text = m_Settings.ViewsFromRoomsBoxFixedZ
        If m_Settings.ViewsFromRoomsBoxShow = "true" Then
            checkBoxBoxShow.Checked = True
        Else
            checkBoxBoxShow.Checked = False
        End If
        If m_Settings.ViewsFromRoomsSizeCropType = "dynamic" Then
            radioButtonSizeCropDynamic.Checked = True
        Else
            radioButtonSizeCropFixed.Checked = True
        End If
        textBoxCropSpace.Text = m_Settings.ViewsFromRoomsCropSpace
        textBoxCropFixedX.Text = m_Settings.ViewsFromRoomsCropFixedX
        textBoxCropFixedY.Text = m_Settings.ViewsFromRoomsCropFixedY
        'textBoxCropFixedZ.Text = mSettings.ViewsFromRoomsCropFixedZ;
        If m_Settings.ViewsFromRoomsCropShow = "true" Then
            checkBoxCropShow.Checked = True
        Else
            checkBoxCropShow.Checked = False
        End If

        initializing = False
        'to avoid rerunning FillRoomsList during setup
        'Fill the list box with unplaced rooms
        If RadioButtonTypeElevation.Checked Then
            FillRoomsList(True)
        Else
            FillRoomsList(False)
        End If

    End Sub

    Private Sub CollectViewTemplates()
        Try
            Dim CollectorViews As New DB.FilteredElementCollector(m_Settings.Document)
            CollectorViews.OfCategory(DB.BuiltInCategory.OST_Views)
            Dim viewList As List(Of DB.View) = CollectorViews.ToElements.Cast(Of DB.View).ToList()

            For Each aView As DB.View In viewList
                If aView.IsTemplate Then
                    Dim viewType As ViewType = aView.ViewType
                    If viewType = DB.ViewType.Undefined Then
                        Continue For
                    End If

                    If mViewTemplates.ContainsKey(viewType) Then
                        mViewTemplates(viewType).Add(aView.Name, aView.Id)
                    Else
                        Dim templateDictionary As New Dictionary(Of String, ElementId)
                        templateDictionary.Add("<None>", ElementId.InvalidElementId)
                        templateDictionary.Add(aView.Name, aView.Id)
                        mViewTemplates.Add(viewType, templateDictionary)
                    End If
                End If
            Next

        Catch ex As Exception
            MessageBox.Show("Cannot gather information of View Templates.", m_Settings.ProgramName)
        End Try
    End Sub

    Private Sub CollecElevationMarks()
        Try
            Dim collecterMarks As New DB.FilteredElementCollector(m_Settings.Document)
            collecterMarks.OfClass(GetType(ElevationMarker))

            Dim viewMarkers As List(Of ElevationMarker) = collecterMarks.ToElements.Cast(Of ElevationMarker).ToList()

            For Each marker As ElevationMarker In viewMarkers
                If marker.CurrentViewCount > 0 Then
                    For index As Integer = 0 To marker.CurrentViewCount - 1
                        Dim elevationId As ElementId = marker.GetViewId(index)
                        If elevationId <> ElementId.InvalidElementId And Not mElevationMaps.ContainsKey(elevationId.IntegerValue) Then
                            mElevationMaps.Add(elevationId.IntegerValue, marker.Id.IntegerValue)
                        End If
                    Next
                End If
            Next

        Catch ex As Exception
            MessageBox.Show("Cannot gather information of Elevation Marks." & vbCrLf & ex.Message, m_Settings.ProgramName)
        End Try
    End Sub

#End Region

    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click

        Dim parameter As DB.Parameter
        Dim view2d As DB.View 'The 2D view to be created
        Dim view3d As DB.View3D 'The 3D view to be created
        Dim viewElevation_A As DB.ViewSection = Nothing 'The elevation view to be created
        Dim viewElevation_B As DB.ViewSection = Nothing 'The elevation view to be created
        Dim viewElevation_C As DB.ViewSection = Nothing 'The elevation view to be created
        Dim viewElevation_D As DB.ViewSection = Nothing 'The elevation view to be created
        Dim viewDirection As New DB.XYZ
        Dim elementIdOfRoom As DB.ElementId
        Dim elementIdOfView As DB.ElementId
        Dim boundingBoxRoom As BoundingBoxXYZ
        Dim boundingBoxCrop As New DB.BoundingBoxXYZ
        Dim boundingBoxBox As New DB.BoundingBoxXYZ
        Dim xyzWorldMax As New DB.XYZ(0, 0, 0) 'Max derived from room in real world
        Dim xyzWorldMin As New DB.XYZ(0, 0, 0) 'Min derived from room in real world
        Dim xyzWorldCen As New DB.XYZ(0, 0, 0) 'Center derived from room in real world
        Dim xyzBoxMax As New DB.XYZ(0, 0, 0) 'Max translated to view
        Dim xyzBoxMin As New DB.XYZ(0, 0, 0) 'Min translated to view
        Dim xyzViewMax As New DB.XYZ(0, 0, 0) 'Max translated to view
        Dim xyzViewMin As New DB.XYZ(0, 0, 0) 'Min translated to view
        Dim xyzViewCen As New DB.XYZ(0, 0, 0) 'Center translated to view
        Dim xyzVertex As New List(Of DB.XYZ)  '= mSettings.Application.Application.Create.NewXYZ '.Create.NewXYZArray()
        'Array of points from room geometry
        Dim geometryElementShell As DB.GeometryElement
        Dim solidShellFace As Solid
        Dim xyzArrayEdges As New List(Of DB.XYZ) ' XYZArray
        Dim transformView As Transform
        Dim transformInverse As Transform
        Dim xyzVertex3dView As New List(Of DB.XYZ) ' XYZArray = mSettings.Application.Create.NewXYZArray()
        Dim xyzVertexElevationView As New List(Of DB.XYZ)
        'Holds all transformed points

        Dim roomToUse As Room = Nothing
        Dim viewName As String = ""
        Dim viewNameComposite As String = ""
        Dim roomName As String = ""
        Dim groupName As String = ""
        Dim scale As Integer
        Dim spaceBox As Double = 0
        Dim spaceCrop As Double = 0
        Dim fixedHalfBoxX As Double = 0
        Dim fixedHalfBoxY As Double = 0
        Dim fixedFullBoxZ As Double = 0
        Dim fixedHalfCropX As Double = 0
        Dim fixedHalfCropY As Double = 0
        Dim vectorX As Double = 0
        Dim vectorY As Double = 0
        Dim vectorZ As Double = 0
        Dim elementData As String
        Dim roomElementId As String = ""
        Dim viewElementId2d As String
        Dim viewElementIdElevation_A As String
        Dim viewElementIdElevation_B As String
        Dim viewElementIdElevation_C As String
        Dim viewElementIdElevation_D As String
        Dim viewElementId3dB As String
        Dim viewElementId3dC As String
        Dim viewElementId3dBC As String

        Using transGroup As New TransactionGroup(m_Settings.Document, "HOK Views from Rooms")
            Try
                transGroup.Start()
                If listBoxRooms.SelectedItems.Count < 1 Then
                    MessageBox.Show("Select one or more items from list before pressing 'Create Views' button.", m_Settings.ProgramName)
                    Return
                Else
                    With Me.ProgressBar1
                        .Minimum = 0
                        .Maximum = listBoxRooms.SelectedItems.Count + 1
                        .Value = 0
                        .Visible = True
                    End With
                End If

                Dim categorySectionBox As DB.Category
                categorySectionBox = m_Settings.Application.ActiveUIDocument.Document.Settings.Categories.Item(BuiltInCategory.OST_SectionBox)
                ' .Application.ActiveDocument.Settings.Categories.Item(BuiltInCategory.OST_SectionBox)

                'Get scale
                Try
                    scale = CInt(Convert.ToInt16(textBoxScale.Text))
                Catch
                    MessageBox.Show("Error interpreting scale value as a number.", m_Settings.ProgramName)
                    Return
                End Try

                'Check vector if needed
                If radioButtonType3dBox.Checked OrElse radioButtonType3dCrop.Checked OrElse radioButtonType3dBoxCrop.Checked Then
                    If textBoxVectorX.Text.Trim() = "" OrElse textBoxVectorY.Text.Trim() = "" OrElse textBoxVectorZ.Text.Trim() = "" Then
                        MessageBox.Show("X, Y, and Z values for direction vector must be provided if a 3D view is used.", m_Settings.ProgramName)
                        Return
                    End If
                    Try
                        vectorX = Convert.ToDouble(textBoxVectorX.Text)
                        vectorY = Convert.ToDouble(textBoxVectorY.Text)
                        vectorZ = Convert.ToDouble(textBoxVectorZ.Text)
                    Catch
                        MessageBox.Show("Error interpreting X, Y, or Z value as a number at View Direction Vector.", m_Settings.ProgramName)
                        Return
                    End Try
                End If

                'Check section box size values if needed
                If radioButtonType3dBox.Checked OrElse radioButtonType3dBoxCrop.Checked Then
                    If radioButtonSizeBoxDynamic.Checked Then
                        If textBoxBoxSpace.Text.Trim() = "" Then
                            MessageBox.Show("Space value must be provided if Dynamic Section Box Size is used.", m_Settings.ProgramName)
                            Return
                        End If
                        Try
                            spaceBox = CDbl(Convert.ToDouble(textBoxBoxSpace.Text))
                        Catch
                            MessageBox.Show("Error interpreting Space value at Dynamic Section Box Size.", m_Settings.ProgramName)
                            Return
                        End Try
                    Else
                        'radioButtonSizeBoxFixed.Checked  
                        If textBoxBoxFixedX.Text.Trim() = "" OrElse textBoxBoxFixedY.Text.Trim() = "" OrElse textBoxBoxFixedZ.Text.Trim() = "" Then
                            MessageBox.Show("X, Y, and Z values must be provided if Fixed Dimension Section Box Size is used.", m_Settings.ProgramName)
                            Return
                        End If
                        Try
                            fixedHalfBoxX = Convert.ToDouble(textBoxBoxFixedX.Text) / 2
                            fixedHalfBoxY = Convert.ToDouble(textBoxBoxFixedY.Text) / 2
                            'Note that Z is based on bottom not center
                            fixedFullBoxZ = Convert.ToDouble(textBoxBoxFixedZ.Text)
                        Catch
                            MessageBox.Show("Error interpreting X, Y, or Z value as a number at Fixed Dimension Section Box Size.", m_Settings.ProgramName)
                            Return
                        End Try
                    End If
                End If

                'Check crop size values
                If radioButtonSizeCropDynamic.Checked Then
                    If textBoxCropSpace.Text.Trim() = "" Then
                        MessageBox.Show("Space value must be provided if Dynamic Crop Size is used.", m_Settings.ProgramName)
                        Return
                    End If
                    Try
                        spaceCrop = CDbl(Convert.ToDouble(textBoxCropSpace.Text))
                    Catch
                        MessageBox.Show("Error interpreting Space value at Fixed Dimension Crop Size.", m_Settings.ProgramName)
                        Return
                    End Try
                Else
                    '(radioButtonSizeCropFixed.Checked) {
                    If textBoxCropFixedX.Text.Trim() = "" OrElse textBoxCropFixedY.Text.Trim() = "" Then
                        MessageBox.Show("X and Y values must be provided if Fixed Dimension Crop Size is used.", m_Settings.ProgramName)
                        Return
                    End If
                    Try
                        fixedHalfCropX = Convert.ToDouble(textBoxCropFixedX.Text) / 2
                        fixedHalfCropY = Convert.ToDouble(textBoxCropFixedY.Text) / 2
                    Catch
                        MessageBox.Show("Error interpreting X or Y value as a number at Fixed Dimension Crop Size.", m_Settings.ProgramName)
                        Return
                    End Try
                End If

                'To avoid transparent look while waiting
                'Process for each selection.
                For Each listItem As String In listBoxRooms.SelectedItems
                    'Get the values from the list
                    If radioButtonGroupSingle.Checked Then
                        'single room case
                        'Get the room values from the list and add the prefix to the view name
                        elementData = listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.IndexOf("<") - 2)
                        roomElementId = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId2d = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_A = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_B = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_C = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_D = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId3dB = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId3dC = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId3dBC = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewName = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        roomName = elementData
                        viewName = textBoxPrefixViewTarget.Text + viewName
                    Else
                        'multiple rooms (grouped) case                                                   
                        elementData = listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.IndexOf("<") - 2)
                        viewElementId2d = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_A = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_B = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_C = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementIdElevation_D = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId3dB = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId3dC = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        viewElementId3dBC = elementData.Substring(0, elementData.IndexOf("|"))
                        elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                        groupName = elementData
                        viewName = textBoxPrefixViewTarget.Text + groupName
                    End If

                    'Get room in single room case or a representative room in group case
                    'This has to be done here in order to get the extremities and to use the level when creating the view
                    If radioButtonGroupSingle.Checked Then
                        'single room case
                        elementIdOfRoom = New DB.ElementId(CInt(Convert.ToInt64(roomElementId)))
                        '(Note get_Element() is supposed to take a string but doesn't seem to work)
                        roomToUse = DirectCast(m_Settings.Document.GetElement(elementIdOfRoom), Room)
                    Else
                        'In group case they are already in the dictionary
                        roomToUse = mDictGroupedByRooms(groupName)(0)
                    End If

                    'Get the room extremeties and center to use for the section box and simple crop box
                    'In the 2D and 3DB cases the bounding box is sufficient for the crop
                    'In the 3DC and 3DBC cases we need to project all of the corners since we don't really know which one will govern
                    'In 3DB and 3DBC cases we need for the section box.
                    If Not radioButtonType3dCrop.Checked Then
                        'In 3DC case we don't need a section box and crop box needs to be based on extremities 
                        boundingBoxRoom = roomToUse.BoundingBox(Nothing)

                        Dim xyzWorldMax_X As Double = boundingBoxRoom.Max.X
                        Dim xyzWorldMax_Y As Double = boundingBoxRoom.Max.Y
                        Dim xyzWorldMax_Z As Double = boundingBoxRoom.Max.Z
                        Dim xyzWorldMin_X As Double = boundingBoxRoom.Min.X
                        Dim xyzWorldMin_Y As Double = boundingBoxRoom.Min.Y
                        Dim xyzWorldMin_Z As Double = boundingBoxRoom.Min.Z

                        If radioButtonGroupMultiple.Checked Then
                            'multiple room case 
                            For Each roomProcess As Room In mDictGroupedByRooms(groupName)
                                boundingBoxRoom = roomProcess.BoundingBox(Nothing)
                                If boundingBoxRoom.Max.X > xyzWorldMax.X Then
                                    xyzWorldMax_X = boundingBoxRoom.Max.X
                                End If
                                If boundingBoxRoom.Max.Y > xyzWorldMax.Y Then
                                    xyzWorldMax_Y = boundingBoxRoom.Max.Y
                                End If
                                If boundingBoxRoom.Max.Z > xyzWorldMax.Z Then
                                    xyzWorldMax_Z = boundingBoxRoom.Max.Z
                                End If
                                If boundingBoxRoom.Min.X < xyzWorldMin.X Then
                                    xyzWorldMin_X = boundingBoxRoom.Min.X
                                End If
                                If boundingBoxRoom.Min.Y < xyzWorldMin.Y Then
                                    xyzWorldMin_Y = boundingBoxRoom.Min.Y
                                End If
                                If boundingBoxRoom.Min.Z < xyzWorldMin.Z Then
                                    xyzWorldMin_Z = boundingBoxRoom.Min.Z
                                End If
                            Next
                        End If

                        xyzWorldMax = New DB.XYZ(xyzWorldMax_X, xyzWorldMax_Y, xyzWorldMax_Z)
                        xyzWorldMin = New DB.XYZ(xyzWorldMin_X, xyzWorldMin_Y, xyzWorldMin_Z)

                        xyzWorldCen = New DB.XYZ((xyzWorldMin.X + xyzWorldMax.X) / 2, (xyzWorldMin.Y + xyzWorldMax.Y) / 2, (xyzWorldMin.Z + xyzWorldMax.Z) / 2)


                        'Calculate bounding box for Section Box
                        If radioButtonSizeBoxDynamic.Checked Then
                            'Adjust section box to fit room(s) case
                            '****NOTE USING FIXED HEIGHT AS PROPORTION OF ROOM HEIGHT

                            xyzBoxMax = New DB.XYZ(xyzWorldMax.X + spaceBox, xyzWorldMax.Y + spaceBox, 0.9 * xyzWorldMax.Z)
                            xyzBoxMin = New DB.XYZ(xyzWorldMin.X - spaceBox, xyzWorldMin.Y - spaceBox, xyzWorldMin.Z - spaceBox)

                        Else
                            'radioButtonSizeBoxFixed.Checked - Fixed section box size case
                            '****NOTE USING FULL Z HEIGHT AND PROPORTION FOR LOWER Z VALUE

                            xyzBoxMax = New DB.XYZ(xyzWorldCen.X + fixedHalfBoxX, xyzWorldCen.Y + fixedHalfBoxY, fixedFullBoxZ)
                            xyzBoxMin = New DB.XYZ(xyzWorldCen.X - fixedHalfBoxX, xyzWorldCen.Y - fixedHalfBoxY, xyzWorldMin.Z - 0.1 * fixedFullBoxZ)

                        End If
                        boundingBoxBox.Max = xyzBoxMax
                        boundingBoxBox.Min = xyzBoxMin

                        'Calculate the bounding box for the 2D crop; don't need Z.
                        If radioButtonSizeCropDynamic.Checked Then
                            'Adjust crop box to fit room(s) case
                            xyzWorldMax = New DB.XYZ(xyzWorldMax.X + spaceCrop, xyzWorldMax.Y + spaceCrop, 0)
                            xyzWorldMin = New DB.XYZ(xyzWorldMin.X - spaceCrop, xyzWorldMin.Y - spaceCrop, 0)
                        Else
                            'radioButtonSizeFixed.Checked - Fixed crop box size case
                            xyzWorldMax = New DB.XYZ(xyzWorldCen.X + fixedHalfCropX, xyzWorldCen.Y + fixedHalfCropY, 0)
                            xyzWorldMin = New DB.XYZ(xyzWorldCen.X - fixedHalfCropX, xyzWorldCen.Y - fixedHalfCropY, 0)

                        End If
                        boundingBoxCrop.Max = xyzWorldMax
                        boundingBoxCrop.Min = xyzWorldMin
                    End If
                    If radioButtonType3dCrop.Checked OrElse radioButtonType3dBoxCrop.Checked Then
                        '3DC and 3DBC cases
                        'Use the shell to get the extremities.
                        If radioButtonGroupSingle.Checked Then
                            'single room case
                            mRoomsToProcess.Clear()
                            mRoomsToProcess.Add(roomToUse)
                        Else
                            '(radioButtonGroupMultiple.Checked) - multiple room case
                            mRoomsToProcess = mDictGroupedByRooms(groupName)
                        End If
                        xyzVertex.Clear()
                        For Each roomProcess As DB.Architecture.Room In mRoomsToProcess
                            geometryElementShell = roomProcess.ClosedShell

                            For Each geometryObject As GeometryObject In geometryElementShell

                                If TypeOf geometryObject Is Solid Then
                                    'get all the edges in it
                                    solidShellFace = TryCast(geometryObject, Solid)
                                    For Each edge As DB.Edge In solidShellFace.Edges
                                        xyzArrayEdges = edge.Tessellate()
                                        For Each xyzEdgePt As DB.XYZ In xyzArrayEdges
                                            'Collect all the verticies.
                                            '***Note there are many duplicate points in it.ut it is proably more efficient not to eliminate them here.
                                            xyzVertex.Add(xyzEdgePt)
                                        Next
                                    Next
                                End If
                            Next
                        Next
                    End If

                    'Create the view
                    Try
                        Dim collector As New FilteredElementCollector(m_Settings.Document)
                        collector.OfClass(GetType(ViewFamilyType))

                        Dim viewTypeList As List(Of Element)
                        viewTypeList = collector.ToElements().ToList()

                        Dim view2dFamilyType As ViewFamilyType = Nothing
                        Dim view3dFamilyType As ViewFamilyType = Nothing
                        Dim viewElevationFamilyType As ViewFamilyType = Nothing

                        For Each viewType As Element In viewTypeList
                            Dim viewfamilytype As ViewFamilyType
                            viewfamilytype = CType(viewType, ViewFamilyType)
                            If viewfamilytype.ViewFamily = ViewFamily.FloorPlan Then
                                view2dFamilyType = viewfamilytype
                            End If
                            If viewfamilytype.ViewFamily = ViewFamily.ThreeDimensional Then
                                view3dFamilyType = viewfamilytype
                            End If
                            If viewfamilytype.ViewFamily = ViewFamily.Elevation Then
                                viewElevationFamilyType = viewfamilytype
                            End If
                        Next

                        If radioButtonType2d.Checked Then

                            Using trans As New Transaction(m_Settings.Document, "Create 2D View Plan")
                                Try
                                    trans.Start()
                                    '2D Case
                                    viewNameComposite = viewName & "-2D"
                                    If viewElementId2d = "*" Then
                                        '"*" indicates that no existing view exists.
                                        view2d = ViewPlan.Create(m_Settings.Document, view2dFamilyType.Id, roomToUse.Level.Id)

                                    Else
                                        elementIdOfView = New DB.ElementId(CInt(Convert.ToInt64(viewElementId2d)))
                                        view2d = TryCast(m_Settings.Document.GetElement(elementIdOfView), DB.View)
                                        If checkBoxReplaceExisting.Checked Then
                                            m_Settings.Document.Delete(view2d.Id)
                                            view2d = ViewPlan.Create(m_Settings.Document, view2dFamilyType.Id, roomToUse.Level.Id)
                                        End If
                                    End If

                                    'set view template
                                    If ComboBoxViewTemplate.SelectedItem IsNot Nothing Then
                                        Dim selectedTemplateName As String = ComboBoxViewTemplate.SelectedItem.ToString
                                        Dim selectedTemplateId As ElementId = ElementId.InvalidElementId
                                        If selectedTemplateName <> "<None>" Then
                                            Dim selectedViewType As ViewType = ComboBoxViewTemplate.Tag
                                            If mViewTemplates.ContainsKey(selectedViewType) Then
                                                If mViewTemplates(selectedViewType).ContainsKey(selectedTemplateName) Then
                                                    selectedTemplateId = mViewTemplates(selectedViewType)(selectedTemplateName)

                                                    If view2d.IsValidViewTemplate(selectedTemplateId) Then
                                                        parameter = view2d.Parameter(BuiltInParameter.VIEW_TEMPLATE)
                                                        If parameter IsNot Nothing Then
                                                            parameter.[Set](selectedTemplateId)
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If

                                    'Define and activate the crop box
                                    view2d.CropBoxActive = True
                                    view2d.CropBox = boundingBoxCrop
                                    If checkBoxCropShow.Checked Then
                                        view2d.CropBoxVisible = True
                                    Else
                                        view2d.CropBoxVisible = False
                                    End If

                                    'Fill in name and parameter values
                                    view2d.Name = viewNameComposite
                                    'view3d.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(1);
                                    view2d.Scale = scale

#If RELEASE2013 Or RELEASE2014 Then
                    parameter = view2d.Parameter("Title on Sheet")
#ElseIf RELEASE2015 Then
                                    parameter = view2d.LookupParameter("Title on Sheet")
#End If

                                    If parameter IsNot Nothing Then
                                        parameter.[Set](roomName)
                                    End If
                                    trans.Commit()
                                Catch ex As Exception
                                    trans.RollBack()
                                End Try
                            End Using
                           

                        ElseIf RadioButtonTypeElevation.Checked Then
                            Using trans As New Transaction(m_Settings.Document, "Create Elevation View")
                                Dim rbb As BoundingBoxXYZ = roomToUse.BoundingBox(Nothing)
                                Dim midPt As XYZ = 0.5 * (rbb.Max + rbb.Min)
                                Dim markerLocation As XYZ = New XYZ(midPt.X, midPt.Y, rbb.Min.Z)

                                Try
                                    trans.Start()

                                    'create elevation view
                                    viewNameComposite = viewName & "-Elevation"
                                    Dim surfix() As String = {"-D", "-A", "-B", "-C"}
                                    Dim createdElevations As List(Of ViewSection) = New List(Of ViewSection)

                                    If viewElementIdElevation_A = "*" And viewElementIdElevation_B = "*" And viewElementIdElevation_C = "*" And viewElementIdElevation_D = "*" Then
                                        Dim marker As ElevationMarker = ElevationMarker.CreateElevationMarker(m_Settings.Document, viewElevationFamilyType.Id, markerLocation, scale)
                                        For index As Integer = 0 To 3
                                            Dim viewElevation As ViewSection = marker.CreateElevation(m_Settings.Document, m_Settings.ActiveViewPlan.Id, index)
                                            viewElevation.Name = viewNameComposite & surfix(index)
#If RELEASE2013 Or RELEASE2014 Then
                                            parameter = viewElevation.Parameter("Title on Sheet")
#ElseIf RELEASE2015 Then
                                            parameter = viewElevation.LookupParameter("Title on Sheet")
#End If
                                            If parameter IsNot Nothing Then
                                                parameter.[Set](roomName)
                                            End If

                                            Select Case index
                                                Case 0
                                                    viewElevation_A = viewElevation
                                                Case 1
                                                    viewElevation_B = viewElevation
                                                Case 2
                                                    viewElevation_C = viewElevation
                                                Case 3
                                                    viewElevation_D = viewElevation
                                            End Select

                                            createdElevations.Add(viewElevation)
                                        Next
                                    Else
                                        'replace or skip
                                        Dim elevationId_A As Integer = 0
                                        Dim elevationId_B As Integer = 0
                                        Dim elevationId_C As Integer = 0
                                        Dim elevationId_D As Integer = 0

                                        Integer.TryParse(viewElementIdElevation_A, elevationId_A)
                                        Integer.TryParse(viewElementIdElevation_B, elevationId_B)
                                        Integer.TryParse(viewElementIdElevation_C, elevationId_C)
                                        Integer.TryParse(viewElementIdElevation_D, elevationId_D)

                                        'find elevation marker
                                        Dim marker As ElevationMarker = Nothing
                                        If mElevationMaps.ContainsKey(elevationId_A) Then
                                            Dim markerId As ElementId = New ElementId(mElevationMaps(elevationId_A))
                                            marker = m_Settings.Document.GetElement(markerId)
                                        ElseIf mElevationMaps.ContainsKey(elevationId_B) Then
                                            Dim markerId As ElementId = New ElementId(mElevationMaps(elevationId_B))
                                            marker = m_Settings.Document.GetElement(markerId)
                                        ElseIf mElevationMaps.ContainsKey(elevationId_C) Then
                                            Dim markerId As ElementId = New ElementId(mElevationMaps(elevationId_C))
                                            marker = m_Settings.Document.GetElement(markerId)
                                        ElseIf mElevationMaps.ContainsKey(elevationId_D) Then
                                            Dim markerId As ElementId = New ElementId(mElevationMaps(elevationId_D))
                                            marker = m_Settings.Document.GetElement(markerId)
                                        End If

                                        If checkBoxReplaceExisting.Checked Then
                                            'delete existing views and elevation marker
                                            If elevationId_A <> 0 Then
                                                elementIdOfView = New DB.ElementId(elevationId_A)
                                                m_Settings.Document.Delete(elementIdOfView)
                                            End If
                                            If elevationId_B <> 0 Then
                                                elementIdOfView = New DB.ElementId(elevationId_B)
                                                m_Settings.Document.Delete(elementIdOfView)
                                            End If
                                            If elevationId_C <> 0 Then
                                                elementIdOfView = New DB.ElementId(elevationId_C)
                                                m_Settings.Document.Delete(elementIdOfView)
                                            End If
                                            If elevationId_D <> 0 Then
                                                elementIdOfView = New DB.ElementId(elevationId_D)
                                                m_Settings.Document.Delete(elementIdOfView)
                                            End If
                                            If marker IsNot Nothing Then
                                                m_Settings.Document.Delete(marker.Id)
                                            End If

                                            'create new elevation marker and views
                                            marker = ElevationMarker.CreateElevationMarker(m_Settings.Document, viewElevationFamilyType.Id, markerLocation, scale)
                                            For index As Integer = 0 To 3
                                                Dim viewElevation As ViewSection = marker.CreateElevation(m_Settings.Document, m_Settings.ActiveViewPlan.Id, index)
                                                viewElevation.Name = viewNameComposite & surfix(index)
#If RELEASE2013 Or RELEASE2014 Then
                                                parameter = viewElevation.Parameter("Title on Sheet")
#ElseIf RELEASE2015 Then
                                                parameter = viewElevation.LookupParameter("Title on Sheet")
#End If
                                                If parameter IsNot Nothing Then
                                                    parameter.[Set](roomName)
                                                End If

                                                Select Case index
                                                    Case 0
                                                        viewElevation_A = viewElevation
                                                    Case 1
                                                        viewElevation_B = viewElevation
                                                    Case 2
                                                        viewElevation_C = viewElevation
                                                    Case 3
                                                        viewElevation_D = viewElevation
                                                End Select

                                                createdElevations.Add(viewElevation)
                                            Next
                                        ElseIf marker IsNot Nothing Then
                                            For index As Integer = 0 To 3
                                                Dim viewElevation As ViewSection = Nothing
                                                If marker.IsAvailableIndex(index) Then
                                                    viewElevation = marker.CreateElevation(m_Settings.Document, m_Settings.ActiveViewPlan.Id, index)
                                                ElseIf marker.GetViewId(index) <> ElementId.InvalidElementId Then
                                                    viewElevation = m_Settings.Document.GetElement(marker.GetViewId(index))
                                                End If

                                                viewElevation.Name = viewNameComposite & surfix(index)
#If RELEASE2013 Or RELEASE2014 Then
                                                parameter = viewElevation.Parameter("Title on Sheet")
#ElseIf RELEASE2015 Then
                                                parameter = viewElevation.LookupParameter("Title on Sheet")
#End If
                                                If parameter IsNot Nothing Then
                                                    parameter.[Set](roomName)
                                                End If

                                                Select Case index
                                                    Case 0
                                                        viewElevation_A = viewElevation
                                                    Case 1
                                                        viewElevation_B = viewElevation
                                                    Case 2
                                                        viewElevation_C = viewElevation
                                                    Case 3
                                                        viewElevation_D = viewElevation
                                                End Select

                                                createdElevations.Add(viewElevation)
                                            Next
                                        End If
                                    End If

                                    'set view template
                                    If ComboBoxViewTemplate.SelectedItem IsNot Nothing Then
                                        Dim selectedTemplateName As String = ComboBoxViewTemplate.SelectedItem.ToString
                                        Dim selectedTemplateId As ElementId = ElementId.InvalidElementId
                                        If selectedTemplateName <> "<None>" Then
                                            Dim selectedViewType As ViewType = ComboBoxViewTemplate.Tag
                                            If mViewTemplates.ContainsKey(selectedViewType) Then
                                                If mViewTemplates(selectedViewType).ContainsKey(selectedTemplateName) Then
                                                    selectedTemplateId = mViewTemplates(selectedViewType)(selectedTemplateName)

                                                    For Each elevation As ViewSection In createdElevations
                                                        If elevation.IsValidViewTemplate(selectedTemplateId) Then
                                                            parameter = elevation.Parameter(BuiltInParameter.VIEW_TEMPLATE)
                                                            If parameter IsNot Nothing Then
                                                                parameter.[Set](selectedTemplateId)
                                                            End If
                                                        End If
                                                    Next
                                                End If
                                            End If
                                        End If
                                    End If

                                    m_Settings.Document.Regenerate()
                                    trans.Commit()
                                Catch ex As Exception
                                    trans.RollBack()
                                End Try
                            End Using

                        Else
                            Using trans As New Transaction(m_Settings.Document, "Create 3D Views")
                                Try
                                    trans.Start()
                                    '3D cases
                                    viewDirection = New DB.XYZ(vectorX, vectorY, vectorZ)
                                    'Dim eyeposition As XYZ = New XYZ(-4.42, -9.97, 79.61) 'origin
                                    'Dim updirection As XYZ = New XYZ(0.3, 0.9, 0.3) 'updirection
                                    'Dim viewDirectionVector As XYZ = New XYZ(-0.0953, -0.286, 0.953) 'viewdirection
                                    'Dim forwarddirection As XYZ = updirection.CrossProduct(viewDirectionVector) 'rightdirection

                                    If radioButtonType3dBox.Checked Then
                                        '3DB case
                                        viewNameComposite = viewName & "-3DB"
                                        If viewElementId3dB = "*" Then
                                            '"*" indicates that no existing view exists.
                                            view3d = view3d.CreateIsometric(m_Settings.Document, view3dFamilyType.Id)
                                            'view3d.SetOrientation(New ViewOrientation3D(eyeposition, updirection, forwarddirection))
                                        Else
                                            elementIdOfView = New DB.ElementId(CInt(Convert.ToInt64(viewElementId3dB)))
                                            view3d = TryCast(m_Settings.Document.GetElement(elementIdOfView), DB.View3D)
                                            If checkBoxReplaceExisting.Checked Then
                                                m_Settings.Document.Delete(view3d.Id)
                                                view3d = view3d.CreateIsometric(m_Settings.Document, view3dFamilyType.Id)
                                                'view3d.SetOrientation(New ViewOrientation3D(eyeposition, updirection, forwarddirection))
                                            End If
                                        End If

                                        'Note the there is no crop box being set here.

                                        'Setup and activate the section box
#If RELEASE2013 Then
                        view3d.SectionBox.Enabled = True
                        view3d.SectionBox = boundingBoxBox
#ElseIf RELEASE2014 Or RELEASE2015 Then
                                        view3d.IsSectionBoxActive = True
                                        view3d.SetSectionBox(boundingBoxBox)
#End If

                                        Dim param(1) As Object
                                        param(0) = categorySectionBox
                                        param(1) = True

                                        If checkBoxBoxShow.Checked Then
                                            view3d.GetType().InvokeMember("SetVisibility", Reflection.BindingFlags.InvokeMethod, Nothing, view3d, param)
                                            'view3d.SetVisibility(categorySectionBox, True)
                                        Else
                                            param(1) = False
                                            view3d.GetType().InvokeMember("SetVisibility", Reflection.BindingFlags.InvokeMethod, Nothing, view3d, param)
                                            'view3d.SetVisibility(categorySectionBox, False)
                                        End If

                                        view3d.Name = viewNameComposite
                                        'view3d.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(1);
                                        view3d.Scale = scale
                                    Else

                                        ' (radioButtonType3dCrop.Checked || radioButtonType3dBoxCrop.Checked) - 3DC and 3DBC cases
                                        If radioButtonType3dCrop.Checked Then
                                            viewNameComposite = viewName & "-3DC"
                                        Else
                                            viewNameComposite = viewName & "-3DBC"
                                        End If
                                        'radioButtonType3dBoxCrop.Checked)                            
                                        If (radioButtonType3dCrop.Checked AndAlso viewElementId3dC = "*") OrElse (radioButtonType3dBoxCrop.Checked AndAlso viewElementId3dBC = "*") Then
                                            '"*" indicates that no existing view exists.
                                            view3d = view3d.CreateIsometric(m_Settings.Document, view3dFamilyType.Id)
                                            'view3d.SetOrientation(New ViewOrientation3D(eyeposition, updirection, forwarddirection))
                                        Else
                                            If radioButtonType3dCrop.Checked Then
                                                elementIdOfView = New DB.ElementId(CInt(Convert.ToInt64(viewElementId3dC)))
                                            Else
                                                elementIdOfView = New DB.ElementId(CInt(Convert.ToInt64(viewElementId3dBC)))
                                            End If
                                            '(radioButtonType3dBoxCrop.Checked)
                                            view3d = TryCast(m_Settings.Document.GetElement(elementIdOfView), DB.View3D)
                                            If checkBoxReplaceExisting.Checked Then
                                                m_Settings.Document.Delete(view3d.Id)
                                                view3d = view3d.CreateIsometric(m_Settings.Document, view3dFamilyType.Id)
                                                'view3d.SetOrientation(New ViewOrientation3D(eyeposition, updirection, forwarddirection))
                                            End If
                                        End If

                                        'Setup and activate the section box in 3D Box case
                                        If radioButtonType3dBoxCrop.Checked Then
                                            '3DBC case
#If RELEASE2013 Then
                            view3d.SectionBox.Enabled = True
                            view3d.SectionBox = boundingBoxBox
#ElseIf RELEASE2014 Or RELEASE2015 Then
                                            view3d.IsSectionBoxActive = True
                                            view3d.SetSectionBox(boundingBoxBox)
#End If

                                            Dim param(1) As Object
                                            param(0) = categorySectionBox
                                            param(1) = True

                                            If checkBoxBoxShow.Checked Then
                                                view3d.GetType().InvokeMember("SetVisibility", Reflection.BindingFlags.InvokeMethod, Nothing, view3d, param)
                                                'view3d.SetVisibility(categorySectionBox, True)
                                            Else
                                                param(1) = False
                                                view3d.GetType().InvokeMember("SetVisibility", Reflection.BindingFlags.InvokeMethod, Nothing, view3d, param)
                                                'view3d.SetVisibility(categorySectionBox, False)
                                            End If
                                        End If

                                        'Get the crop box and then use it to find the transform of the view
                                        '(the inverse of the view's transform back to the model space.)
                                        boundingBoxCrop = view3d.CropBox
                                        transformView = boundingBoxCrop.Transform
                                        transformInverse = transformView.Inverse

                                        'Project the points; then find max and min of projected points (ingore Z)
                                        xyzVertex3dView.Clear()
                                        For Each xyzPtWork As DB.XYZ In xyzVertex
                                            xyzVertex3dView.Add(transformInverse.OfPoint(xyzPtWork))
                                        Next

                                        Dim xyzViewMax_X As Double = xyzVertex3dView.Item(0).X
                                        Dim xyzViewMax_Y As Double = xyzVertex3dView.Item(0).Y
                                        Dim xyzViewMin_X As Double = xyzViewMax.X
                                        Dim xyzViewMin_Y As Double = xyzViewMax.Y

                                        For Each xyzPtWork As DB.XYZ In xyzVertex3dView
                                            If xyzViewMax.X < xyzPtWork.X Then
                                                xyzViewMax_X = xyzPtWork.X
                                            End If
                                            If xyzViewMax.Y < xyzPtWork.Y Then
                                                xyzViewMax_Y = xyzPtWork.Y
                                            End If
                                            If xyzViewMin.X > xyzPtWork.X Then
                                                xyzViewMin_X = xyzPtWork.X
                                            End If
                                            If xyzViewMin.Y > xyzPtWork.Y Then
                                                xyzViewMin_Y = xyzPtWork.Y
                                            End If
                                        Next
                                        xyzViewMax = New DB.XYZ(xyzViewMax_X, xyzViewMax_Y, 0)
                                        xyzViewMin = New DB.XYZ(xyzViewMax.X, xyzViewMax.Y, 0)


                                        'Adjust the points for space or fixed size and create bounding box
                                        If radioButtonSizeCropDynamic.Checked Then
                                            'Adjust crop box to fit room(s) case  
                                            xyzViewMax = New DB.XYZ(xyzViewMax.X + spaceCrop, xyzViewMax.Y + spaceCrop, 0)
                                            xyzViewMin = New DB.XYZ(xyzViewMin.X - spaceCrop, xyzViewMin.Y - spaceCrop, 0)

                                        Else
                                            '(radioButtonSizeCropFixed.Checked) - Fixed crop box size case
                                            xyzViewCen = New DB.XYZ((xyzViewMax.X + xyzViewMin.X) / 2, (xyzViewMax.Y + xyzViewMin.Y) / 2, 0)
                                            xyzViewMax = New DB.XYZ(xyzViewCen.X + fixedHalfCropX, xyzViewCen.Y + fixedHalfCropY, 0)
                                            xyzViewMin = New DB.XYZ(xyzViewCen.X - fixedHalfCropX, xyzViewCen.Y - fixedHalfCropY, 0)

                                        End If
                                        boundingBoxCrop.Max = xyzViewMax
                                        boundingBoxCrop.Min = xyzViewMin

                                        'Set up and create the crop box
                                        view3d.CropBox = boundingBoxCrop
                                        view3d.CropBoxActive = True
                                        If checkBoxCropShow.Checked Then
                                            view3d.CropBoxVisible = True
                                        Else
                                            view3d.CropBoxVisible = False
                                        End If

                                        If ComboBoxViewTemplate.SelectedItem IsNot Nothing Then
                                            Dim selectedTemplateName As String = ComboBoxViewTemplate.SelectedItem.ToString
                                            Dim selectedTemplateId As ElementId = ElementId.InvalidElementId
                                            If selectedTemplateName <> "<None>" Then
                                                Dim selectedViewType As ViewType = ComboBoxViewTemplate.Tag
                                                If mViewTemplates.ContainsKey(selectedViewType) Then
                                                    If mViewTemplates(selectedViewType).ContainsKey(selectedTemplateName) Then
                                                        selectedTemplateId = mViewTemplates(selectedViewType)(selectedTemplateName)

                                                        If view3d.IsValidViewTemplate(selectedTemplateId) Then
                                                            parameter = view3d.Parameter(BuiltInParameter.VIEW_TEMPLATE)
                                                            If parameter IsNot Nothing Then
                                                                parameter.[Set](selectedTemplateId)
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If

                                        'Complete the view settings
                                        view3d.Name = viewNameComposite
                                        'view3d.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(1);
                                        view3d.Scale = scale
                                    End If
                                    trans.Commit()
                                Catch ex As Exception
                                    trans.RollBack()
                                End Try

                            End Using

                        End If
                    Catch exception As Exception
                        MessageBox.Show("Unable to create view: '" & viewNameComposite & "'." & vbLf & vbLf & "This may be becasue it is an existing view and is the only view open" & vbLf & "so Revit cannot delete it in order to recreate it." & vbLf & vbLf & "System message: " & exception.Message, m_Settings.ProgramName)
                        Continue For
                    End Try

                    Me.ProgressBar1.Increment(1)
                Next

                '***We don't want to remove the items from the list since the user might want to create the 2D or 3D version after they did the other one.
                '****Try just refilling the list (may be too slow?)
                If RadioButtonTypeElevation.Checked Then
                    FillRoomsList(True)
                Else
                    FillRoomsList(False)
                End If

                'Close the progress bar.
                Me.ProgressBar1.Visible = False
                transGroup.Assimilate()
            Catch ex As Exception
                transGroup.RollBack()
            End Try
            
        End Using
        
    End Sub


#Region "Controls and Events"
    Private Sub SaveSettings()

        If radioButtonGroupSingle.Checked Then
            m_Settings.ViewsFromRoomsGrouping = "single"
        Else
            m_Settings.ViewsFromRoomsGrouping = "multiple"
        End If
        m_Settings.ViewsFromRoomsParamList1 = textBoxParameterList1.Text
        m_Settings.ViewsFromRoomsParamList2 = textBoxParameterList2.Text
        If checkBoxPad1.Checked Then
            m_Settings.ViewsFromRoomsListPadYes1 = "true"
        Else
            m_Settings.ViewsFromRoomsListPadYes1 = "false"
        End If
        If checkBoxPad2.Checked Then
            m_Settings.ViewsFromRoomsListPadYes2 = "true"
        Else
            m_Settings.ViewsFromRoomsListPadYes2 = "false"
        End If
        m_Settings.ViewsFromRoomsListPad1 = textBoxPad1.Text
        m_Settings.ViewsFromRoomsListPad2 = textBoxPad2.Text
        m_Settings.ViewsFromRoomsParamGroupBy = textBoxParameterGroupBy.Text
        If checkBoxListExisting.Checked Then
            m_Settings.ViewsFromRoomsListExisting = "true"
        Else
            m_Settings.ViewsFromRoomsListExisting = "false"
        End If
        If checkBoxListReverse.Checked Then
            m_Settings.ViewsFromRoomsListReverse = "true"
        Else
            m_Settings.ViewsFromRoomsListReverse = "false"
        End If

        m_Settings.ViewsFromRoomsParmViewName = textBoxParameterViewName.Text
        m_Settings.ViewsFromRoomsParmRoomName = textBoxParameterRoomName.Text
        m_Settings.ViewsFromRoomsScale = textBoxScale.Text
        m_Settings.ViewsFromRoomsPrefixViewTarget = textBoxPrefixViewTarget.Text
        If radioButtonType2d.Checked Then
            m_Settings.ViewsFromRoomsViewType = "2d"
        ElseIf RadioButtonTypeElevation.Checked Then
            m_Settings.ViewsFromRoomsViewType = "Elevation"
        ElseIf radioButtonType3dBox.Checked Then
            m_Settings.ViewsFromRoomsViewType = "3dBox"
        ElseIf radioButtonType3dCrop.Checked Then
            m_Settings.ViewsFromRoomsViewType = "3dCrop"
        Else
            m_Settings.ViewsFromRoomsViewType = "3dBoxCrop"
        End If
        'radioButtonType3dBoxCrop.Checked
        m_Settings.ViewsFromRoomsVectorX = textBoxVectorX.Text
        m_Settings.ViewsFromRoomsVectorY = textBoxVectorY.Text
        m_Settings.ViewsFromRoomsVectorZ = textBoxVectorZ.Text
        If checkBoxReplaceExisting.Checked Then
            m_Settings.ViewsFromRoomsReplaceExisting = "true"
        Else
            m_Settings.ViewsFromRoomsReplaceExisting = "false"
        End If

        If radioButtonSizeBoxDynamic.Checked Then
            m_Settings.ViewsFromRoomsSizeBoxType = "dynamic"
        Else
            m_Settings.ViewsFromRoomsSizeBoxType = "fixed"
        End If
        m_Settings.ViewsFromRoomsBoxSpace = textBoxBoxSpace.Text
        m_Settings.ViewsFromRoomsBoxFixedX = textBoxBoxFixedX.Text
        m_Settings.ViewsFromRoomsBoxFixedY = textBoxBoxFixedY.Text
        m_Settings.ViewsFromRoomsBoxFixedZ = textBoxBoxFixedZ.Text
        If checkBoxBoxShow.Checked Then
            m_Settings.ViewsFromRoomsBoxShow = "true"
        Else
            m_Settings.ViewsFromRoomsBoxShow = "false"
        End If

        If radioButtonSizeCropDynamic.Checked Then
            m_Settings.ViewsFromRoomsSizeCropType = "dynamic"
        Else
            m_Settings.ViewsFromRoomsSizeCropType = "fixed"
        End If
        m_Settings.ViewsFromRoomsCropSpace = textBoxCropSpace.Text
        m_Settings.ViewsFromRoomsCropFixedX = textBoxCropFixedX.Text
        m_Settings.ViewsFromRoomsCropFixedY = textBoxCropFixedY.Text
        'mSettings.ViewsFromRoomsCropFixedZ = textBoxCropFixedZ.Text;
        If checkBoxCropShow.Checked Then
            m_Settings.ViewsFromRoomsCropShow = "true"
        Else
            m_Settings.ViewsFromRoomsCropShow = "false"
        End If

        m_Settings.WriteIni()
    End Sub

    Private Sub FillRoomsList(activeViewOnly As Boolean)
        Dim categoryViews As BuiltInCategory = BuiltInCategory.OST_Views
        Dim categoryRooms As BuiltInCategory = BuiltInCategory.OST_Rooms
        Dim elementsRooms As New List(Of DB.Element)
        Dim viewsSelection As New List(Of DB.Element)
        Dim viewsSelection2d As New List(Of DB.Element)
        Dim viewsSelctionElevation_A As New List(Of DB.Element)
        Dim viewsSelctionElevation_B As New List(Of DB.Element)
        Dim viewsSelctionElevation_C As New List(Of DB.Element)
        Dim viewsSelctionElevation_D As New List(Of DB.Element)
        Dim viewsSelection3dBox As New List(Of DB.Element)
        Dim viewsSelection3dCrop As New List(Of DB.Element)
        Dim viewsSelection3dBoxCrop As New List(Of DB.Element)
        Dim testView As DB.View
        Dim parameter As DB.Parameter

        Dim listBy1 As String
        Dim listBy2 As String
        Dim padZeros1 As Integer
        Dim padZeros2 As Integer

        Dim parameterNameGroupBy As String
        Dim parameterValueGroupBy As String
        Dim parameterFound As Boolean

        Dim viewNameShort As String
        Dim viewNameLong2d As String
        Dim viewNameLongElevation_A As String
        Dim viewNameLongElevation_B As String
        Dim viewNameLongElevation_C As String
        Dim viewNameLongElevation_D As String
        Dim viewNameLong3dBox As String
        Dim viewNameLong3dCrop As String
        Dim viewNameLong3dBoxCrop As String
        Dim roomName As String
        Dim roomElementId As String
        Dim viewElementId2d As String
        Dim viewElementIdElevation_A As String
        Dim viewElementIdElevation_B As String
        Dim viewElementIdElevation_C As String
        Dim viewElementIdElevation_D As String
        Dim viewElementId3dBox As String
        Dim viewElementId3dCrop As String
        Dim viewElementId3dBoxCrop As String
        Dim elementData As String
        Dim buildCode As String

        'bool notPlaced;
        Dim existingView2d As Boolean
        Dim existingViewElevation_A As Boolean
        Dim existingViewElevation_B As Boolean
        Dim existingViewElevation_C As Boolean
        Dim existingViewElevation_D As Boolean
        Dim existingView3dBox As Boolean
        Dim existingView3dCrop As Boolean
        Dim existingView3dBoxCrop As Boolean
        'bool blankViewName;

        'Fill the list
        mListItems.Clear()
        listBoxRooms.Items.Clear()

        'Create selection filters and make selections
        If activeViewOnly Then
            If m_Settings.ActiveViewPlan Is Nothing Then
                MessageBox.Show("Please open a plan view from which new elevation views will derive ints extents and inherit settings.", m_Settings.ProgramName)
                Return
            End If
            Dim CollectorRooms As New DB.FilteredElementCollector(m_Settings.Document, m_Settings.ActiveViewPlan.Id)
            CollectorRooms.OfCategory(DB.BuiltInCategory.OST_Rooms)
            elementsRooms = CollectorRooms.ToElements
        Else
            Dim CollectorRooms As New DB.FilteredElementCollector(m_Settings.Document)
            CollectorRooms.OfCategory(DB.BuiltInCategory.OST_Rooms)
            elementsRooms = CollectorRooms.ToElements
        End If
        

        Dim CollectorViews As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorViews.OfCategory(DB.BuiltInCategory.OST_Views)
        viewsSelection = CollectorViews.ToElements


        For Each elementTestView As DB.Element In viewsSelection
            If elementTestView.Name.EndsWith("-2D") Then
                viewsSelection2d.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-Elevation-A") Then
                viewsSelctionElevation_A.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-Elevation-B") Then
                viewsSelctionElevation_B.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-Elevation-C") Then
                viewsSelctionElevation_C.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-Elevation-D") Then
                viewsSelctionElevation_D.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-3DB") Then
                viewsSelection3dBox.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-3DC") Then
                viewsSelection3dCrop.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-3DBC") Then
                viewsSelection3dBoxCrop.Add(elementTestView)
            End If
        Next

        If elementsRooms.Count = 0 Then
            MessageBox.Show("No rooms found.", m_Settings.ProgramName)
            Return
        End If

        'Single Room Case:
        If radioButtonGroupSingle.Checked Then
            'single room case
            labelListTitle.Text = "Select Rooms For Which to Create a View:"
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

            For Each RmElement As DB.Architecture.Room In elementsRooms
                listBy1 = ""
                listBy2 = ""
                'notPlaced = false;
                viewNameShort = ""
                roomName = ""
                'blankViewName = false;
                existingView2d = False
                existingViewElevation_A = False
                existingViewElevation_B = False
                existingViewElevation_C = False
                existingViewElevation_D = False
                existingView3dBox = False
                existingView3dCrop = False
                existingView3dBoxCrop = False
                viewElementId2d = "*"
                viewElementIdElevation_A = "*"
                viewElementIdElevation_B = "*"
                viewElementIdElevation_C = "*"
                viewElementIdElevation_D = "*"
                viewElementId3dBox = "*"
                viewElementId3dCrop = "*"
                viewElementId3dBoxCrop = "*"

                'Only include properly placed and bounded rooms
#If RELEASE2013 Or RELEASE2014 Then
                parameter = RmElement.Parameter("Area")
#ElseIf RELEASE2015 Then
                parameter = RmElement.LookupParameter("Area")
#End If

                If parameter.AsDouble() = 0 Then
                    'Assuming that this means it is not placed or not bounded
                    'notPlaced = true;
                    'break;
                    Continue For
                End If

                'Get view name and determine if a view already exists
                'In case of blank view name skip the room
#If RELEASE2013 Or RELEASE2014 Then
                parameter = RmElement.Parameter(textBoxParameterViewName.Text)
#ElseIf RELEASE2015 Then
                parameter = RmElement.LookupParameter(textBoxParameterViewName.Text)
#End If

                If parameter Is Nothing Then
                    viewNameShort = ""
                Else
                    viewNameShort = parameter.AsString()
                End If


                If viewNameShort = "" Then
                    'ignore this room
                    'blankViewName = true;
                    'break;
                    Continue For
                End If
                viewNameLong2d = textBoxPrefixViewTarget.Text + viewNameShort & "-2D"
                viewNameLongElevation_A = textBoxPrefixViewTarget.Text + viewNameShort & "-Elevation-A"
                viewNameLongElevation_B = textBoxPrefixViewTarget.Text + viewNameShort & "-Elevation-B"
                viewNameLongElevation_C = textBoxPrefixViewTarget.Text + viewNameShort & "-Elevation-C"
                viewNameLongElevation_D = textBoxPrefixViewTarget.Text + viewNameShort & "-Elevation-D"
                viewNameLong3dBox = textBoxPrefixViewTarget.Text + viewNameShort & "-3DB"
                viewNameLong3dCrop = textBoxPrefixViewTarget.Text + viewNameShort & "-3DC"
                viewNameLong3dBoxCrop = textBoxPrefixViewTarget.Text + viewNameShort & "-3DBC"
                buildCode = ""
                For Each testElement As DB.Element In viewsSelection2d
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If viewNameLong2d = testView.Name.ToString Then
                        existingView2d = True
                        viewElementId2d = testView.Id.IntegerValue.ToString
                        buildCode = buildCode & " (E-2D"
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelctionElevation_A
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If testView.Name.Contains(viewNameLongElevation_A) Then
                        existingViewElevation_A = True
                        viewElementIdElevation_A = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-Elev-A"
                        Else
                            buildCode = buildCode & "&Elev-A"
                        End If
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelctionElevation_B
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If testView.Name.Contains(viewNameLongElevation_B) Then
                        existingViewElevation_B = True
                        viewElementIdElevation_B = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-Elev-B"
                        Else
                            buildCode = buildCode & "&Elev-B"
                        End If
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelctionElevation_C
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If testView.Name.Contains(viewNameLongElevation_C) Then
                        existingViewElevation_C = True
                        viewElementIdElevation_C = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-Elev-C"
                        Else
                            buildCode = buildCode & "&Elev-C"
                        End If
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelctionElevation_D
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If testView.Name.Contains(viewNameLongElevation_D) Then
                        existingViewElevation_D = True
                        viewElementIdElevation_D = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-Elev-D"
                        Else
                            buildCode = buildCode & "&Elev-D"
                        End If
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelection3dBox
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If viewNameLong3dBox = testView.Name.ToString Then
                        existingView3dBox = True
                        viewElementId3dBox = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-3DB"
                        Else
                            buildCode = buildCode & "&3DB"
                        End If
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelection3dCrop
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If viewNameLong3dCrop = testView.Name.ToString Then
                        existingView3dCrop = True
                        viewElementId3dCrop = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-3DC"
                        Else
                            buildCode = buildCode & "&3DC"
                        End If
                        Exit For
                    End If
                Next
                For Each testElement As DB.Element In viewsSelection3dBoxCrop
                    testView = TryCast(testElement, DB.View)
                    If testView Is Nothing Then
                        Continue For
                    End If
                    'Seem to pick up some elements that are not views                               
                    If viewNameLong3dBoxCrop = testView.Name.ToString Then
                        existingView3dBoxCrop = True
                        viewElementId3dBoxCrop = testView.Id.IntegerValue.ToString
                        If buildCode = "" Then
                            buildCode = buildCode & " (E-3DBC"
                        Else
                            buildCode = buildCode & "&3DBC"
                        End If
                        Exit For
                    End If
                Next
                If buildCode <> "" Then
                    buildCode = buildCode & ")"
                End If

                'If existing and check box not checked then don't need to continue with this room
                If (existingView2d OrElse existingViewElevation_A OrElse existingViewElevation_B OrElse existingViewElevation_C OrElse existingViewElevation_D OrElse existingView3dBox OrElse existingView3dCrop OrElse existingView3dBoxCrop) AndAlso Not checkBoxListExisting.Checked Then
                    Continue For
                End If

                'Get the room name value (we are assuming it is a string
#If RELEASE2013 Or RELEASE2014 Then
                parameter = RmElement.Parameter(textBoxParameterRoomName.Text)
#ElseIf RELEASE2015 Then
                parameter = RmElement.LookupParameter(textBoxParameterRoomName.Text)
#End If

                If parameter Is Nothing Then
                    roomName = ""
                Else
                    roomName = parameter.AsString()
                End If

                'Get list values
#If RELEASE2013 Or RELEASE2014 Then
                parameter = RmElement.Parameter(textBoxParameterList1.Text)
#ElseIf RELEASE2015 Then
                parameter = RmElement.LookupParameter(textBoxParameterList1.Text)
#End If

                If parameter Is Nothing Then
                    listBy1 = ""
                Else
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
#If RELEASE2013 Or RELEASE2014 Then
                parameter = RmElement.Parameter(textBoxParameterList2.Text)
#ElseIf RELEASE2015 Then
                parameter = RmElement.LookupParameter(textBoxParameterList2.Text)
#End If

                If parameter Is Nothing Then
                    listBy2 = ""
                Else
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

                'Create the list entry for placed rooms
                'if (viewNameShort == "") continue; //means we never found a name
                'if (notPlaced) continue;
                'if (blankViewName) continue;
                'if ((existingView2d || existingView3dBox || existingView3dCrop || existingView3dBoxCrop) && !checkBoxListExisting.Checked) continue;
                roomElementId = RmElement.Id.IntegerValue.ToString
                elementData = "<" & roomElementId & "|" & viewElementId2d & "|" & viewElementIdElevation_A & "|" & viewElementIdElevation_B & "|" & viewElementIdElevation_C & "|" & viewElementIdElevation_D & "|" & viewElementId3dBox & "|" & viewElementId3dCrop & "|" & viewElementId3dBoxCrop & "|" & viewNameShort & "|" & roomName & ">"
                mListItems.Add(listBy1 & " + " & listBy2 & buildCode & Convert.ToString(m_Settings.Spacer) & elementData)
            Next
        Else
            'end of single rooms case
            'Multiple Rooms Case:
            'multiple rooms (grouped) case                
            labelListTitle.Text = "Select Room Groups For Which to Create a View:"

            'Insure good group-by parameter
            parameterNameGroupBy = textBoxParameterGroupBy.Text.Trim()
            If parameterNameGroupBy = "" Then
                'No message here since user proably just clicked the radio button
                'MessageBox.Show("A 'Group By' parameter value must be provided in 'Grouped Rooms' case.", mSettings.ProgramName);
                Return
            End If
            parameterFound = False
            For Each parameterTest As Parameter In elementsRooms(0).Parameters
                If parameterTest.Definition.Name = parameterNameGroupBy Then
                    parameterFound = True
                    Exit For
                End If
            Next
            If Not parameterFound Then
                MessageBox.Show("Rooms do not include a parameter named: '" & parameterNameGroupBy & "'.", m_Settings.ProgramName)
                Return
            End If

            'Loop through all rooms
            mDictGroupedByRooms.Clear()
            For Each element As DB.Element In elementsRooms
                existingView2d = False
                existingViewElevation_A = False
                existingViewElevation_B = False
                existingViewElevation_C = False
                existingViewElevation_D = False
                existingView3dBox = False
                existingView3dCrop = False
                existingView3dBoxCrop = False
                viewElementId2d = "*"
                viewElementIdElevation_A = "*"
                viewElementIdElevation_B = "*"
                viewElementIdElevation_C = "*"
                viewElementIdElevation_D = "*"
                viewElementId3dBox = "*"
                viewElementId3dCrop = "*"
                viewElementId3dBoxCrop = "*"

#If RELEASE2013 Or RELEASE2014 Then
                parameter = element.Parameter(parameterNameGroupBy)
#ElseIf RELEASE2015 Then
                parameter = element.LookupParameter(parameterNameGroupBy)
#End If
                If parameter Is Nothing Then
                    Continue For
                End If
                'not sure why but it occurs
                If parameter.AsString() Is Nothing Then
                    Continue For
                End If
                'not sure why but it occurs
                parameterValueGroupBy = parameter.AsString().Trim()
                If parameterValueGroupBy = "" Then
                    Continue For
                End If
                'allowing blank is trouble; user should have a value like "<none>".
#If RELEASE2013 Or RELEASE2014 Then
                parameter = element.Parameter("Area")
#ElseIf RELEASE2015 Then
                parameter = element.LookupParameter("Area")
#End If

                'Only include properly placed and bounded rooms
                If parameter.AsDouble() = 0 Then
                    Continue For
                End If

                'Assuming that this means it is not placed or not bounded
                If mDictGroupedByRooms.ContainsKey(parameterValueGroupBy) Then
                    mDictGroupedByRooms(parameterValueGroupBy).Add(TryCast(element, DB.Architecture.Room))
                Else
                    Dim listRooms As New List(Of DB.Architecture.Room)
                    listRooms.Add(TryCast(element, DB.Architecture.Room))

                    '' ''              listRooms = New List(Of DB.Architecture.Room)

                    '' ''              ' = New List(Of db.Architecture.Room)() With { _
                    '' ''	TryCast(element, db.Architecture.Room) _
                    '' ''}

                    mDictGroupedByRooms.Add(parameterValueGroupBy, listRooms)

                    'Get view name and determine if a view already exists
                    viewNameLong2d = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-2D"
                    viewNameLongElevation_A = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-Elevation-A"
                    viewNameLongElevation_B = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-Elevation-B"
                    viewNameLongElevation_C = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-Elevation-C"
                    viewNameLongElevation_D = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-Elevation-D"
                    viewNameLong3dBox = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-3DB"
                    viewNameLong3dCrop = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-3DC"
                    viewNameLong3dBoxCrop = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-3DBC"
                    buildCode = ""
                    For Each testElement As DB.Element In viewsSelection2d
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLong2d = testView.Name.ToString Then
                            existingView2d = True
                            viewElementId2d = testView.Id.IntegerValue.ToString
                            buildCode = buildCode & " (E-2D"
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelctionElevation_A
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLongElevation_A = testView.Name.ToString Then
                            existingViewElevation_A = True
                            viewElementIdElevation_A = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-Elev-A"
                            Else
                                buildCode = buildCode & "&Elev-A"
                            End If
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelctionElevation_B
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLongElevation_B = testView.Name.ToString Then
                            existingViewElevation_B = True
                            viewElementIdElevation_B = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-Elev-B"
                            Else
                                buildCode = buildCode & "&Elev-B"
                            End If
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelctionElevation_C
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLongElevation_C = testView.Name.ToString Then
                            existingViewElevation_C = True
                            viewElementIdElevation_C = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-Elev-C"
                            Else
                                buildCode = buildCode & "&Elev-C"
                            End If
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelctionElevation_D
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLongElevation_D = testView.Name.ToString Then
                            existingViewElevation_D = True
                            viewElementIdElevation_D = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-Elev-D"
                            Else
                                buildCode = buildCode & "&Elev-D"
                            End If
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelection3dBox
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLong3dBox = testView.Name.ToString Then
                            existingView3dBox = True
                            viewElementId3dBox = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-3DB"
                            Else
                                buildCode = buildCode & "&3DB"
                            End If
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelection3dCrop
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLong3dCrop = testView.Name.ToString Then
                            existingView3dCrop = True
                            viewElementId3dCrop = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-3DC"
                            Else
                                buildCode = buildCode & "&3DC"
                            End If
                            Exit For
                        End If
                    Next
                    For Each testElement As DB.Element In viewsSelection3dBoxCrop
                        testView = TryCast(testElement, DB.View)
                        If testView Is Nothing Then
                            Continue For
                        End If
                        'Seem to pick up some elements that are not views                               
                        If viewNameLong3dBoxCrop = testView.Name.ToString Then
                            existingView3dBoxCrop = True
                            viewElementId3dBoxCrop = testView.Id.IntegerValue.ToString
                            If buildCode = "" Then
                                buildCode = buildCode & " (E-3DBC"
                            Else
                                buildCode = buildCode & "&3DBC"
                            End If
                            Exit For
                        End If
                    Next
                    If buildCode <> "" Then
                        buildCode = buildCode & ")"
                    End If

                    'If existing and check box not checked then don't need to continue with this room
                    If (existingView2d OrElse existingViewElevation_A OrElse existingViewElevation_B OrElse existingViewElevation_C OrElse existingViewElevation_D OrElse existingView3dBox OrElse existingView3dCrop OrElse existingView3dBoxCrop) AndAlso Not checkBoxListExisting.Checked Then
                        Continue For
                    End If

                    'Create the list entry
                    elementData = "<" & viewElementId2d & "|" & viewElementIdElevation_A & "|" & viewElementIdElevation_B & "|" & viewElementIdElevation_C & "|" & viewElementIdElevation_D & "|" & viewElementId3dBox & "|" & viewElementId3dCrop & "|" & viewElementId3dBoxCrop & "|" & parameterValueGroupBy & ">"
                    mListItems.Add(parameterValueGroupBy & buildCode & Convert.ToString(m_Settings.Spacer) & elementData)
                End If
            Next
        End If

        mListItems.Sort()
        If checkBoxListReverse.Checked Then
            mListItems.Reverse()
        End If
        For Each item As String In mListItems
            listBoxRooms.Items.Add(item)
        Next
    End Sub

    Private Sub PadZeros(ByRef input As String, ByVal length As Integer)
        While input.Length < length
            input = "0" & input
        End While
    End Sub
    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.Close()
    End Sub
    Private Sub checkBoxIncludeExisting_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxListExisting.CheckedChanged
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub radioButtonGroupSingle_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles radioButtonGroupSingle.CheckedChanged
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxParameterList1_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList1.Leave
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxParameterList2_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList2.Leave
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub checkBoxPad1_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad1.CheckedChanged
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub checkBoxPad2_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad2.CheckedChanged
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxPad1_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad1.Leave
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxPad2_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad2.Leave
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub checkBoxListReverse_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxPad2_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad2.TextChanged
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxPrefixViewTarget_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPrefixViewTarget.Leave
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxParameterGroupBy_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterGroupBy.Leave
        If Not initializing Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub
    Private Sub textBoxParameterGroupBy_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles textBoxParameterGroupBy.KeyPress
        If (e.KeyChar.ToString = vbCr) OrElse (e.KeyChar.ToString = vbTab) Then
            If RadioButtonTypeElevation.Checked Then
                FillRoomsList(True)
            Else
                FillRoomsList(False)
            End If
        End If
    End Sub

    Private Sub radioButtonType2d_CheckedChanged(sender As Object, e As EventArgs) Handles radioButtonType2d.CheckedChanged
        If radioButtonType2d.Checked Then
            If Not initializing Then
                FillRoomsList(False)
            End If

            ComboBoxViewTemplate.Tag = ViewType.FloorPlan
            ComboBoxViewTemplate.DataSource = Nothing
            If mViewTemplates.ContainsKey(ViewType.FloorPlan) Then
                Dim templateNames As List(Of String) = mViewTemplates(ViewType.FloorPlan).Keys.ToList
                ComboBoxViewTemplate.DataSource = templateNames
            End If
        End If
        
    End Sub

    Private Sub RadioButtonTypeElevation_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButtonTypeElevation.CheckedChanged
        If RadioButtonTypeElevation.Checked Then
            If Not initializing Then
                FillRoomsList(True)
            End If

            ComboBoxViewTemplate.Tag = ViewType.Elevation
            ComboBoxViewTemplate.DataSource = Nothing
            If mViewTemplates.ContainsKey(ViewType.Elevation) Then
                Dim templateNames As List(Of String) = mViewTemplates(ViewType.Elevation).Keys.ToList
                ComboBoxViewTemplate.DataSource = templateNames
            End If
        End If
    End Sub

    Private Sub radioButtonType3dBox_CheckedChanged(sender As Object, e As EventArgs) Handles radioButtonType3dBox.CheckedChanged
        If radioButtonType3dBox.Checked Then
            If Not initializing Then
                FillRoomsList(False)
            End If
            ComboBoxViewTemplate.Tag = ViewType.ThreeD
            ComboBoxViewTemplate.DataSource = Nothing
            If mViewTemplates.ContainsKey(ViewType.ThreeD) Then
                Dim templateNames As List(Of String) = mViewTemplates(ViewType.ThreeD).Keys.ToList
                ComboBoxViewTemplate.DataSource = templateNames
            End If
        End If
    End Sub

    Private Sub radioButtonType3dCrop_CheckedChanged(sender As Object, e As EventArgs) Handles radioButtonType3dCrop.CheckedChanged
        If radioButtonType3dCrop.Checked Then
            If Not initializing Then
                FillRoomsList(False)
            End If

            ComboBoxViewTemplate.Tag = ViewType.ThreeD
            ComboBoxViewTemplate.DataSource = Nothing
            If mViewTemplates.ContainsKey(ViewType.ThreeD) Then
                Dim templateNames As List(Of String) = mViewTemplates(ViewType.ThreeD).Keys.ToList
                ComboBoxViewTemplate.DataSource = templateNames
            End If
        End If
    End Sub

    Private Sub radioButtonType3dBoxCrop_CheckedChanged(sender As Object, e As EventArgs) Handles radioButtonType3dBoxCrop.CheckedChanged
        If radioButtonType3dBoxCrop.Checked Then
            If Not initializing Then
                FillRoomsList(False)
            End If

            ComboBoxViewTemplate.Tag = ViewType.ThreeD
            ComboBoxViewTemplate.DataSource = Nothing
            If mViewTemplates.ContainsKey(ViewType.ThreeD) Then
                Dim templateNames As List(Of String) = mViewTemplates(ViewType.ThreeD).Keys.ToList
                ComboBoxViewTemplate.DataSource = templateNames
            End If
        End If
    End Sub

#End Region

    
End Class