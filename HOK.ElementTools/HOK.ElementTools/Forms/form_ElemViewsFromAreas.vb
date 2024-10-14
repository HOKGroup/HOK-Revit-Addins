Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Architecture

Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils


Public Class form_ElemViewsFromAreas
    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private mDictGroupedByAreas As New Dictionary(Of String, List(Of Area))
    Private mAreasToProcess As New List(Of Area)
    Private initializing As Boolean = True

#Region "Constructor"
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ElementTools-ViewsFromAreas", settings.Document.Application.VersionNumber))

        'Initialize the settings text boxes
        m_Settings = settings
        'assuming already initialized
        If m_Settings.ViewsFromAreasGrouping = "single" Then
            radioButtonGroupSingle.Checked = True
        Else
            radioButtonGroupMultiple.Checked = True
        End If
        Me.ProgressBar1.Visible = False
        textBoxParameterList1.Text = m_Settings.ViewsFromAreasParamList1
        textBoxParameterList2.Text = m_Settings.ViewsFromAreasParamList2
        If m_Settings.ViewsFromAreasListPadYes1 = "true" Then
            checkBoxPad1.Checked = True
        Else
            checkBoxPad1.Checked = False
        End If
        If m_Settings.ViewsFromAreasListPadYes2 = "true" Then
            checkBoxPad2.Checked = True
        Else
            checkBoxPad2.Checked = False
        End If
        textBoxPad1.Text = m_Settings.ViewsFromAreasListPad1
        textBoxPad2.Text = m_Settings.ViewsFromAreasListPad2
        textBoxParameterGroupBy.Text = m_Settings.ViewsFromAreasParamGroupBy
        If m_Settings.ViewsFromAreasListExisting = "true" Then
            checkBoxListExisting.Checked = True
        Else
            checkBoxListExisting.Checked = False
        End If
        If m_Settings.ViewsFromAreasListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        textBoxParameterViewName.Text = m_Settings.ViewsFromAreasParmViewName
        textBoxParameterAreaName.Text = m_Settings.ViewsFromAreasParmAreaName
        textBoxScale.Text = m_Settings.ViewsFromAreasScale
        textBoxPrefixViewTarget.Text = m_Settings.ViewsFromAreasPrefixViewTarget
        If m_Settings.ViewsFromAreasViewType = "2d" Then
            radioButtonType2d.Checked = True
        ElseIf m_Settings.ViewsFromAreasViewType = "3dBox" Then
            radioButtonType3dBox.Checked = True
        ElseIf m_Settings.ViewsFromAreasViewType = "3dCrop" Then
            radioButtonType3dCrop.Checked = True
        Else
            radioButtonType3dBoxCrop.Checked = True
        End If
        '"3dBoxCrop" case
        textBoxVectorX.Text = m_Settings.ViewsFromAreasVectorX
        textBoxVectorY.Text = m_Settings.ViewsFromAreasVectorY
        textBoxVectorZ.Text = m_Settings.ViewsFromAreasVectorZ
        If m_Settings.ViewsFromAreasReplaceExisting = "true" Then
            checkBoxReplaceExisting.Checked = True
        Else
            checkBoxReplaceExisting.Checked = False
        End If
        If m_Settings.ViewsFromAreasSizeBoxType = "dynamic" Then
            radioButtonSizeBoxDynamic.Checked = True
        Else
            radioButtonSizeBoxFixed.Checked = True
        End If
        textBoxBoxSpace.Text = m_Settings.ViewsFromAreasBoxSpace
        textBoxBoxFixedX.Text = m_Settings.ViewsFromAreasBoxFixedX
        textBoxBoxFixedY.Text = m_Settings.ViewsFromAreasBoxFixedY
        textBoxBoxFixedZ.Text = m_Settings.ViewsFromAreasBoxFixedZ
        If m_Settings.ViewsFromAreasBoxShow = "true" Then
            checkBoxBoxShow.Checked = True
        Else
            checkBoxBoxShow.Checked = False
        End If
        If m_Settings.ViewsFromAreasSizeCropType = "dynamic" Then
            radioButtonSizeCropDynamic.Checked = True
        Else
            radioButtonSizeCropFixed.Checked = True
        End If
        textBoxCropSpace.Text = m_Settings.ViewsFromAreasCropSpace
        textBoxCropFixedX.Text = m_Settings.ViewsFromAreasCropFixedX
        textBoxCropFixedY.Text = m_Settings.ViewsFromAreasCropFixedY
        'textBoxCropFixedZ.Text = mSettings.ViewsFromAreasCropFixedZ;
        If m_Settings.ViewsFromAreasCropShow = "true" Then
            checkBoxCropShow.Checked = True
        Else
            checkBoxCropShow.Checked = False
        End If

        initializing = False
        'to avoid rerunning FillAreasList during setup
        'Fill the list box with unplaced Areas
        FillAreasList()
    End Sub
#End Region


#Region "Controls and Events"

    Private Sub SaveSettings()

        If radioButtonGroupSingle.Checked Then
            m_Settings.ViewsFromAreasGrouping = "single"
        Else
            m_Settings.ViewsFromAreasGrouping = "multiple"
        End If
        m_Settings.ViewsFromAreasParamList1 = textBoxParameterList1.Text
        m_Settings.ViewsFromAreasParamList2 = textBoxParameterList2.Text
        If checkBoxPad1.Checked Then
            m_Settings.ViewsFromAreasListPadYes1 = "true"
        Else
            m_Settings.ViewsFromAreasListPadYes1 = "false"
        End If
        If checkBoxPad2.Checked Then
            m_Settings.ViewsFromAreasListPadYes2 = "true"
        Else
            m_Settings.ViewsFromAreasListPadYes2 = "false"
        End If
        m_Settings.ViewsFromAreasListPad1 = textBoxPad1.Text
        m_Settings.ViewsFromAreasListPad2 = textBoxPad2.Text
        m_Settings.ViewsFromAreasParamGroupBy = textBoxParameterGroupBy.Text
        If checkBoxListExisting.Checked Then
            m_Settings.ViewsFromAreasListExisting = "true"
        Else
            m_Settings.ViewsFromAreasListExisting = "false"
        End If
        If checkBoxListReverse.Checked Then
            m_Settings.ViewsFromAreasListReverse = "true"
        Else
            m_Settings.ViewsFromAreasListReverse = "false"
        End If

        m_Settings.ViewsFromAreasParmViewName = textBoxParameterViewName.Text
        m_Settings.ViewsFromAreasParmAreaName = textBoxParameterAreaName.Text
        m_Settings.ViewsFromAreasScale = textBoxScale.Text
        m_Settings.ViewsFromAreasPrefixViewTarget = textBoxPrefixViewTarget.Text
        If radioButtonType2d.Checked Then
            m_Settings.ViewsFromAreasViewType = "2d"
        ElseIf radioButtonType3dBox.Checked Then
            m_Settings.ViewsFromAreasViewType = "3dBox"
        ElseIf radioButtonType3dCrop.Checked Then
            m_Settings.ViewsFromAreasViewType = "3dCrop"
        Else
            m_Settings.ViewsFromAreasViewType = "3dBoxCrop"
        End If
        'radioButtonType3dBoxCrop.Checked
        m_Settings.ViewsFromAreasVectorX = textBoxVectorX.Text
        m_Settings.ViewsFromAreasVectorY = textBoxVectorY.Text
        m_Settings.ViewsFromAreasVectorZ = textBoxVectorZ.Text
        If checkBoxReplaceExisting.Checked Then
            m_Settings.ViewsFromAreasReplaceExisting = "true"
        Else
            m_Settings.ViewsFromAreasReplaceExisting = "false"
        End If

        If radioButtonSizeBoxDynamic.Checked Then
            m_Settings.ViewsFromAreasSizeBoxType = "dynamic"
        Else
            m_Settings.ViewsFromAreasSizeBoxType = "fixed"
        End If
        m_Settings.ViewsFromAreasBoxSpace = textBoxBoxSpace.Text
        m_Settings.ViewsFromAreasBoxFixedX = textBoxBoxFixedX.Text
        m_Settings.ViewsFromAreasBoxFixedY = textBoxBoxFixedY.Text
        m_Settings.ViewsFromAreasBoxFixedZ = textBoxBoxFixedZ.Text
        If checkBoxBoxShow.Checked Then
            m_Settings.ViewsFromAreasBoxShow = "true"
        Else
            m_Settings.ViewsFromAreasBoxShow = "false"
        End If

        If radioButtonSizeCropDynamic.Checked Then
            m_Settings.ViewsFromAreasSizeCropType = "dynamic"
        Else
            m_Settings.ViewsFromAreasSizeCropType = "fixed"
        End If
        m_Settings.ViewsFromAreasCropSpace = textBoxCropSpace.Text
        m_Settings.ViewsFromAreasCropFixedX = textBoxCropFixedX.Text
        m_Settings.ViewsFromAreasCropFixedY = textBoxCropFixedY.Text
        'mSettings.ViewsFromAreasCropFixedZ = textBoxCropFixedZ.Text;
        If checkBoxCropShow.Checked Then
            m_Settings.ViewsFromAreasCropShow = "true"
        Else
            m_Settings.ViewsFromAreasCropShow = "false"
        End If

        m_Settings.WriteIni()
    End Sub

    Private Sub FillAreasList()
        Dim categoryViews As BuiltInCategory = BuiltInCategory.OST_Views
        Dim categoryAreas As BuiltInCategory = BuiltInCategory.OST_Areas
        Dim elementsAreas As New List(Of DB.Element)
        Dim viewsSelection As New List(Of DB.Element)
        Dim viewsSelection2d As New List(Of DB.Element)
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
        Dim viewNameLong3dBox As String
        Dim viewNameLong3dCrop As String
        Dim viewNameLong3dBoxCrop As String
        Dim areaName As String
        Dim areaElementId As String
        Dim viewElementId2d As String
        Dim viewElementId3dBox As String
        Dim viewElementId3dCrop As String
        Dim viewElementId3dBoxCrop As String
        Dim elementData As String
        Dim buildCode As String

        'bool notPlaced;
        Dim existingView2d As Boolean
        Dim existingView3dBox As Boolean
        Dim existingView3dCrop As Boolean
        Dim existingView3dBoxCrop As Boolean
        'bool blankViewName;

        'Create selection filters and make selections

        Dim CollectorAreas As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorAreas.OfCategory(DB.BuiltInCategory.OST_Areas)
        elementsAreas = CollectorAreas.ToElements

        Dim CollectorViews As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorViews.OfCategory(DB.BuiltInCategory.OST_Views)
        viewsSelection = CollectorViews.ToElements


        For Each elementTestView As DB.Element In viewsSelection
            If elementTestView.Name.EndsWith("-2D") Then
                viewsSelection2d.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-3DB") Then
                viewsSelection3dBox.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-3DC") Then
                viewsSelection3dCrop.Add(elementTestView)
            ElseIf elementTestView.Name.EndsWith("-3DBC") Then
                viewsSelection3dBoxCrop.Add(elementTestView)
            End If
        Next

        If elementsAreas.Count = 0 Then
            MessageBox.Show("No Areas found.", m_Settings.ProgramName)
            Return
        End If

        'Fill the list
        mListItems.Clear()
        listBoxAreas.Items.Clear()

        'Single Area Case:
        If radioButtonGroupSingle.Checked Then
            'single Area case
            labelListTitle.Text = "Select Areas For Which to Create a View:"
            Try
                padZeros1 = Convert.ToInt16(textBoxPad1.Text)
            Catch
                padZeros1 = 0
            End Try
            Try
                padZeros2 = Convert.ToInt16(textBoxPad2.Text)
            Catch
                padZeros2 = 0
            End Try
            If padZeros1 < 0 Then
                padZeros1 = 0
            End If
            If padZeros2 < 0 Then
                padZeros2 = 0
            End If

            For Each RmElement As Area In elementsAreas
                listBy1 = ""
                listBy2 = ""
                'notPlaced = false;
                viewNameShort = ""
                areaName = ""
                'blankViewName = false;
                existingView2d = False
                existingView3dBox = False
                existingView3dCrop = False
                existingView3dBoxCrop = False
                viewElementId2d = "*"
                viewElementId3dBox = "*"
                viewElementId3dCrop = "*"
                viewElementId3dBoxCrop = "*"

                'Only include properly placed and bounded Areas
                parameter = RmElement.LookupParameter("Area")

                If parameter.AsDouble() = 0 Then
                    'Assuming that this means it is not placed or not bounded
                    'notPlaced = true;
                    'break;
                    Continue For
                End If

                'Get view name and determine if a view already exists
                'In case of blank view name skip the Area
                parameter = RmElement.LookupParameter(textBoxParameterViewName.Text)

                If parameter Is Nothing Then
                    viewNameShort = ""
                Else
                    viewNameShort = parameter.AsString()
                End If


                If viewNameShort = "" Then
                    'ignore this Area
                    'blankViewName = true;
                    'break;
                    Continue For
                End If
                viewNameLong2d = textBoxPrefixViewTarget.Text + viewNameShort & "-2D"
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
                        viewElementId2d = testView.Id.Value.ToString
                        buildCode = buildCode & " (E-2D"
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
                        viewElementId3dBox = testView.Id.Value.ToString
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
                        viewElementId3dCrop = testView.Id.Value.ToString
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
                        viewElementId3dBoxCrop = testView.Id.Value.ToString
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

                'If existing and check box not checked then don't need to continue with this Area
                If (existingView2d OrElse existingView3dBox OrElse existingView3dCrop OrElse existingView3dBoxCrop) AndAlso Not checkBoxListExisting.Checked Then
                    Continue For
                End If

                'Get the Area name value (we are assuming it is a string
                parameter = RmElement.LookupParameter(textBoxParameterAreaName.Text)

                If parameter Is Nothing Then
                    areaName = ""
                Else
                    areaName = parameter.AsString()
                End If

                'Get list values
                parameter = RmElement.LookupParameter(textBoxParameterList1.Text)

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
                parameter = RmElement.LookupParameter(textBoxParameterList2.Text)

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

                'Create the list entry for placed Areas
                'if (viewNameShort == "") continue; //means we never found a name
                'if (notPlaced) continue;
                'if (blankViewName) continue;
                'if ((existingView2d || existingView3dBox || existingView3dCrop || existingView3dBoxCrop) && !checkBoxListExisting.Checked) continue;
                areaElementId = RmElement.Id.Value.ToString
                elementData = "<" & areaElementId & "|" & viewElementId2d & "|" & viewElementId3dBox & "|" & viewElementId3dCrop & "|" & viewElementId3dBoxCrop & "|" & viewNameShort & "|" & areaName & ">"
                mListItems.Add(listBy1 & " + " & listBy2 & buildCode & Convert.ToString(m_Settings.Spacer) & elementData)
            Next
        Else
            'end of single Areas case
            'Multiple Areas Case:
            'multiple Areas (grouped) case                
            labelListTitle.Text = "Select Area Groups For Which to Create a View:"

            'Insure good group-by parameter
            parameterNameGroupBy = textBoxParameterGroupBy.Text.Trim()
            If parameterNameGroupBy = "" Then
                'No message here since user proably just clicked the radio button
                'MessageBox.Show("A 'Group By' parameter value must be provided in 'Grouped Areas' case.", mSettings.ProgramName);
                Return
            End If
            parameterFound = False
            For Each parameterTest As Parameter In elementsAreas(0).Parameters
                If parameterTest.Definition.Name = parameterNameGroupBy Then
                    parameterFound = True
                    Exit For
                End If
            Next
            If Not parameterFound Then
                MessageBox.Show("Areas do not include a parameter named: '" & parameterNameGroupBy & "'.", m_Settings.ProgramName)
                Return
            End If

            'Loop through all Areas
            mDictGroupedByAreas.Clear()
            For Each element As DB.Element In elementsAreas
                existingView2d = False
                existingView3dBox = False
                existingView3dCrop = False
                existingView3dBoxCrop = False
                viewElementId2d = "*"
                viewElementId3dBox = "*"
                viewElementId3dCrop = "*"
                viewElementId3dBoxCrop = "*"

                parameter = element.LookupParameter(parameterNameGroupBy)

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
                parameter = element.LookupParameter("Area")

                'Only include properly placed and bounded Areas
                If parameter.AsDouble() = 0 Then
                    Continue For
                End If

                'Assuming that this means it is not placed or not bounded
                If mDictGroupedByAreas.ContainsKey(parameterValueGroupBy) Then
                    mDictGroupedByAreas(parameterValueGroupBy).Add(TryCast(element, Area))
                Else
                    Dim listAreas As New List(Of Area)
                    listAreas.Add(TryCast(element, Area))

                    '' ''              listAreas = New List(Of DB.Architecture.Area)

                    '' ''              ' = New List(Of db.Architecture.Area)() With { _
                    '' ''	TryCast(element, db.Architecture.Area) _
                    '' ''}

                    mDictGroupedByAreas.Add(parameterValueGroupBy, listAreas)

                    'Get view name and determine if a view already exists
                    viewNameLong2d = textBoxPrefixViewTarget.Text + parameterValueGroupBy & "-2D"
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
                            viewElementId2d = testView.Id.Value.ToString
                            buildCode = buildCode & " (E-2D"
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
                            viewElementId3dBox = testView.Id.Value.ToString
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
                            viewElementId3dCrop = testView.Id.Value.ToString
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
                            viewElementId3dBoxCrop = testView.Id.Value.ToString
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

                    'If existing and check box not checked then don't need to continue with this Area
                    If (existingView2d OrElse existingView3dBox OrElse existingView3dCrop OrElse existingView3dBoxCrop) AndAlso Not checkBoxListExisting.Checked Then
                        Continue For
                    End If

                    'Create the list entry
                    elementData = "<" & viewElementId2d & "|" & viewElementId3dBox & "|" & viewElementId3dCrop & "|" & viewElementId3dBoxCrop & "|" & parameterValueGroupBy & ">"
                    mListItems.Add(parameterValueGroupBy & buildCode & Convert.ToString(m_Settings.Spacer) & elementData)
                End If
            Next
        End If

        mListItems.Sort()
        If checkBoxListReverse.Checked Then
            mListItems.Reverse()
        End If
        For Each item As String In mListItems
            listBoxAreas.Items.Add(item)
        Next
    End Sub

    Private Sub PadZeros(ByRef input As String, ByVal length As Integer)
        While input.Length < length
            input = "0" & input
        End While
    End Sub

    Private Function FindLevelHeight(ByRef inputLevel As Level) As Integer
        Dim height As Double = 0
        Try
            Dim collector As New Autodesk.Revit.DB.FilteredElementCollector(m_Settings.Document)
            Dim collection As ICollection(Of Autodesk.Revit.DB.Element) = collector.OfClass(GetType(Autodesk.Revit.DB.Level)).ToElements()
            For Each e As Autodesk.Revit.DB.Element In collection
                Dim level As Autodesk.Revit.DB.Level = TryCast(e, Autodesk.Revit.DB.Level)

                If level IsNot Nothing Then
                    Dim temp As Double = 0
                    temp = level.Elevation - inputLevel.Elevation
                    If height = 0 Then
                        height = temp
                    End If

                    If temp > 0 And temp < height Then
                        height = temp
                    End If
                End If
            Next
            Return height
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return height
        End Try

    End Function


    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click
        ' New Transaction
        Dim m_Trans As New DB.Transaction(m_Settings.Document, "HOK Views from Areas")
        m_Trans.Start()

        Dim parameter As DB.Parameter
        Dim view2d As DB.View 'The 2D view to be created
        Dim view3d As View3D 'The 3D view to be created
        Dim viewDirection As New DB.XYZ
        Dim elementIdOfArea As DB.ElementId
        Dim elementIdOfView As DB.ElementId
        Dim boundingBoxArea As BoundingBoxXYZ
        Dim boundingBoxCrop As New DB.BoundingBoxXYZ
        Dim boundingBoxBox As New DB.BoundingBoxXYZ
        Dim xyzWorldMax As New DB.XYZ(0, 0, 0) 'Max derived from Area in real world
        Dim xyzWorldMin As New DB.XYZ(0, 0, 0) 'Min derived from Area in real world
        Dim xyzWorldCen As New DB.XYZ(0, 0, 0) 'Center derived from Area in real world
        Dim xyzBoxMax As New DB.XYZ(0, 0, 0) 'Max translated to view
        Dim xyzBoxMin As New DB.XYZ(0, 0, 0) 'Min translated to view
        Dim xyzViewMax As New DB.XYZ(0, 0, 0) 'Max translated to view
        Dim xyzViewMin As New DB.XYZ(0, 0, 0) 'Min translated to view
        Dim xyzViewCen As New DB.XYZ(0, 0, 0) 'Center translated to view
        Dim xyzVertex As New List(Of DB.XYZ)  '= mSettings.Application.Application.Create.NewXYZ '.Create.NewXYZArray()
        'Array of points from Area geometry
        'Dim geometryElementShell As DB.GeometryElement
        'Dim solidShellFace As Solid
        Dim xyzArrayEdges As New List(Of DB.XYZ) ' XYZArray
        Dim transformView As Transform
        Dim transformInverse As Transform
        Dim xyzVertex3dView As New List(Of DB.XYZ) ' XYZArray = mSettings.Application.Create.NewXYZArray()
        'Holds all transformed points
        Dim areaPlans As New List(Of DB.View)

        Dim areaToUse As Area = Nothing
        Dim viewName As String = ""
        Dim viewNameComposite As String = ""
        Dim areaName As String = ""
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
        Dim areaElementId As String = ""
        Dim viewElementId2d As String
        Dim viewElementId3dB As String
        Dim viewElementId3dC As String
        Dim viewElementId3dBC As String


        'Check that user has made a selection.
        If listBoxAreas.SelectedItems.Count < 1 Then
            MessageBox.Show("Select one or more items from list before pressing 'Create Views' button.", m_Settings.ProgramName)
            Return
        Else
            With Me.ProgressBar1
                .Minimum = 0
                .Maximum = listBoxAreas.SelectedItems.Count + 1
                .Value = 0
                .Visible = True
            End With
        End If

        Dim viewElements As New List(Of Element)
        Dim CollectorAreas As New DB.FilteredElementCollector(m_Settings.Document)
        CollectorAreas.OfCategory(DB.BuiltInCategory.OST_Views)
        viewElements = CollectorAreas.ToElements

        For Each aview As Element In viewElements
            Dim view As DB.View = DirectCast(aview, DB.View)
            If view.ViewType = ViewType.AreaPlan Then
                areaPlans.Add(view)
            End If
        Next

        Dim categorySectionBox As DB.Category
        categorySectionBox = m_Settings.Application.ActiveUIDocument.Document.Settings.Categories.Item(BuiltInCategory.OST_SectionBox)
        ' .Application.ActiveDocument.Settings.Categories.Item(BuiltInCategory.OST_SectionBox)

        'Get scale
        Try
            scale = Convert.ToInt16(textBoxScale.Text)
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

        For Each listItem As String In listBoxAreas.SelectedItems
            Try
                'Get the values from the list
                If radioButtonGroupSingle.Checked Then
                    'single Area case
                    'Get the Area values from the list and add the prefix to the view name
                    elementData = listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.IndexOf("<") - 2)
                    areaElementId = elementData.Substring(0, elementData.IndexOf("|"))
                    elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                    viewElementId2d = elementData.Substring(0, elementData.IndexOf("|"))
                    elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                    viewElementId3dB = elementData.Substring(0, elementData.IndexOf("|"))
                    elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                    viewElementId3dC = elementData.Substring(0, elementData.IndexOf("|"))
                    elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                    viewElementId3dBC = elementData.Substring(0, elementData.IndexOf("|"))
                    elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                    viewName = elementData.Substring(0, elementData.IndexOf("|"))
                    elementData = elementData.Substring(elementData.IndexOf("|") + 1)
                    areaName = elementData
                    viewName = textBoxPrefixViewTarget.Text + viewName
                Else
                    'multiple Areas (grouped) case                                                   
                    elementData = listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.IndexOf("<") - 2)
                    viewElementId2d = elementData.Substring(0, elementData.IndexOf("|"))
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



                'Get Area in single Area case or a representative Area in group case
                'This has to be done here in order to get the extremities and to use the level when creating the view
                If radioButtonGroupSingle.Checked Then
                    'single Area case
                    elementIdOfArea = NewElementId(Convert.ToInt64(areaElementId))
                    '(Note get_Element() is supposed to take a string but doesn't seem to work)
                    areaToUse = DirectCast(m_Settings.Document.GetElement(elementIdOfArea), Area)
                Else
                    'In group case they are already in the dictionary
                    areaToUse = mDictGroupedByAreas(groupName)(0)
                End If

                'Get the Area extremeties and center to use for the section box and simple crop box
                'In the 2D and 3DB cases the bounding box is sufficient for the crop
                'In the 3DC and 3DBC cases we need to project all of the corners since we don't really know which one will govern
                'In 3DB and 3DBC cases we need for the section box.
                If Not radioButtonType3dCrop.Checked Then
                    'In 3DC case we don't need a section box and crop box needs to be based on extremities 
                    Dim activeView As DB.View = Nothing
                    For Each aview As DB.View In areaPlans
                        If areaToUse.Level.Id = aview.GenLevel.Id Then
                            activeView = aview
                        End If
                    Next

                    If activeView Is Nothing Then
                        MessageBox.Show("Cannot find areaplan for the area: " & areaToUse.Name & " " & areaToUse.Number)
                        Continue For
                    End If

                    boundingBoxArea = areaToUse.BoundingBox(activeView)

                    If boundingBoxArea IsNot Nothing Then

                        Dim xyzWorldMax_X As Double = boundingBoxArea.Max.X
                        Dim xyzWorldMax_Y As Double = boundingBoxArea.Max.Y
                        Dim xyzWorldMax_Z As Double = boundingBoxArea.Max.Z
                        Dim xyzWorldMin_X As Double = boundingBoxArea.Min.X
                        Dim xyzWorldMin_Y As Double = boundingBoxArea.Min.Y
                        Dim xyzWorldMin_Z As Double = boundingBoxArea.Min.Z

                        If radioButtonGroupMultiple.Checked Then
                            'multiple Area case 
                            For Each AreaProcess As Area In mDictGroupedByAreas(groupName)
                                boundingBoxArea = AreaProcess.BoundingBox(Nothing)
                                If boundingBoxArea.Max.X > xyzWorldMax.X Then
                                    xyzWorldMax_X = boundingBoxArea.Max.X
                                End If
                                If boundingBoxArea.Max.Y > xyzWorldMax.Y Then
                                    xyzWorldMax_Y = boundingBoxArea.Max.Y
                                End If
                                If boundingBoxArea.Max.Z > xyzWorldMax.Z Then
                                    xyzWorldMax_Z = boundingBoxArea.Max.Z
                                End If
                                If boundingBoxArea.Min.X < xyzWorldMin.X Then
                                    xyzWorldMin_X = boundingBoxArea.Min.X
                                End If
                                If boundingBoxArea.Min.Y < xyzWorldMin.Y Then
                                    xyzWorldMin_Y = boundingBoxArea.Min.Y
                                End If
                                If boundingBoxArea.Min.Z < xyzWorldMin.Z Then
                                    xyzWorldMin_Z = boundingBoxArea.Min.Z
                                End If
                            Next
                        End If

                        xyzWorldMax = New DB.XYZ(xyzWorldMax_X, xyzWorldMax_Y, xyzWorldMax_Z)
                        xyzWorldMin = New DB.XYZ(xyzWorldMin_X, xyzWorldMin_Y, xyzWorldMin_Z)

                        xyzWorldCen = New DB.XYZ((xyzWorldMin.X + xyzWorldMax.X) / 2, (xyzWorldMin.Y + xyzWorldMax.Y) / 2, (xyzWorldMin.Z + xyzWorldMax.Z) / 2)


                        'Calculate bounding box for Section Box
                        If radioButtonSizeBoxDynamic.Checked Then
                            'Adjust section box to fit Area(s) case
                            '****NOTE USING FIXED HEIGHT AS PROPORTION OF Area HEIGHT

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
                            'Adjust crop box to fit Area(s) case
                            xyzWorldMax = New DB.XYZ(xyzWorldMax.X + spaceCrop, xyzWorldMax.Y + spaceCrop, 0)
                            xyzWorldMin = New DB.XYZ(xyzWorldMin.X - spaceCrop, xyzWorldMin.Y - spaceCrop, 0)
                        Else
                            'radioButtonSizeFixed.Checked - Fixed crop box size case
                            xyzWorldMax = New DB.XYZ(xyzWorldCen.X + fixedHalfCropX, xyzWorldCen.Y + fixedHalfCropY, 0)
                            xyzWorldMin = New DB.XYZ(xyzWorldCen.X - fixedHalfCropX, xyzWorldCen.Y - fixedHalfCropY, 0)

                        End If
                        boundingBoxCrop.Max = xyzWorldMax
                        boundingBoxCrop.Min = xyzWorldMin
                    Else
                        MessageBox.Show(areaToUse.Name & ": Unable to find BoundingBox of Area in this view. " & m_Settings.Document.ActiveView.Name & vbCrLf & "Please choose an appropriate view for the Area object.", "ActiveView Error:", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Continue For
                    End If
                End If
                If radioButtonType3dCrop.Checked OrElse radioButtonType3dBoxCrop.Checked Then
                    '3DC and 3DBC cases
                    'Use the shell to get the extremities.
                    If radioButtonGroupSingle.Checked Then
                        'single Area case
                        mAreasToProcess.Clear()
                        mAreasToProcess.Add(areaToUse)
                    Else
                        '(radioButtonGroupMultiple.Checked) - multiple Area case
                        mAreasToProcess = mDictGroupedByAreas(groupName)
                    End If
                    xyzVertex.Clear()

                    'For Each AreaProcess As Area In mAreasToProcess
                    '    geometryElementShell = AreaProcess.ClosedShell

                    '    For Each geometryObject As GeometryObject In geometryElementShell

                    '        If TypeOf geometryObject Is Solid Then
                    '            'get all the edges in it
                    '            solidShellFace = TryCast(geometryObject, Solid)
                    '            For Each edge As DB.Edge In solidShellFace.Edges
                    '                xyzArrayEdges = edge.Tessellate()
                    '                For Each xyzEdgePt As DB.XYZ In xyzArrayEdges
                    '                    'Collect all the verticies.
                    '                    '***Note there are many duplicate points in it.ut it is proably more efficient not to eliminate them here.
                    '                    xyzVertex.Add(xyzEdgePt)
                    '                Next
                    '            Next
                    '        End If
                    '    Next
                    'Next

                    'Collect xyzVertex: Find unbounded height by adjacent level
                    For Each AreaProcess As Area In mAreasToProcess
                        Dim height = FindLevelHeight(AreaProcess.Level)

                        For Each boundarySegmentList As List(Of Autodesk.Revit.DB.BoundarySegment) In AreaProcess.GetBoundarySegments(New SpatialElementBoundaryOptions)
                            For Each boundarySegment As Autodesk.Revit.DB.BoundarySegment In boundarySegmentList

                                Dim curve As Curve = boundarySegment.GetCurve()
                                Dim endPoint As XYZ = curve.GetEndPoint(0)

                                xyzVertex.Add(endPoint)
                                Dim newVertex As XYZ = New XYZ(endPoint.X, endPoint.Y, endPoint.Z + height)
                                xyzVertex.Add(newVertex)
                            Next
                        Next
                    Next

                End If

                'Create the view

                Dim collector As New FilteredElementCollector(m_Settings.Document)
                collector.OfClass(GetType(ViewFamilyType))

                Dim viewTypeList As List(Of Element)
                viewTypeList = collector.ToElements().ToList()

                Dim view2dFamilyType As ViewFamilyType = Nothing
                Dim view3dFamilyType As ViewFamilyType = Nothing

                For Each viewType As Element In viewTypeList
                    Dim viewfamilytype As ViewFamilyType
                    viewfamilytype = CType(viewType, ViewFamilyType)
                    If viewfamilytype.ViewFamily = ViewFamily.AreaPlan Then
                        view2dFamilyType = viewfamilytype
                    End If
                    If viewfamilytype.ViewFamily = ViewFamily.ThreeDimensional Then
                        view3dFamilyType = viewfamilytype
                    End If
                Next

                If radioButtonType2d.Checked Then
                    '2D Case
                    viewNameComposite = viewName & "-2D"
                    If viewElementId2d = "*" Then
                        '"*" indicates that no existing view exists.
                        view2d = ViewPlan.Create(m_Settings.Document, view2dFamilyType.Id, areaToUse.Level.Id)

                    Else
                        elementIdOfView = NewElementId(Convert.ToInt64(viewElementId2d))
                        view2d = TryCast(m_Settings.Document.GetElement(elementIdOfView), DB.View)
                        If checkBoxReplaceExisting.Checked Then
                            m_Settings.Document.Delete(view2d.Id)
                            view2d = ViewPlan.Create(m_Settings.Document, view2dFamilyType.Id, areaToUse.Level.Id)
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
                    parameter = view2d.LookupParameter("Title on Sheet")

                    If parameter IsNot Nothing Then
                        parameter.[Set](areaName)
                    End If
                Else

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
                            elementIdOfView = NewElementId(Convert.ToInt64(viewElementId3dB))
                            view3d = TryCast(m_Settings.Document.GetElement(elementIdOfView), DB.View3D)
                            If checkBoxReplaceExisting.Checked Then
                                m_Settings.Document.Delete(view3d.Id)
                                view3d = view3d.CreateIsometric(m_Settings.Document, view3dFamilyType.Id)
                                'view3d.SetOrientation(New ViewOrientation3D(eyeposition, updirection, forwarddirection))
                            End If
                        End If

                        'Note the there is no crop box being set here.

                        'Setup and activate the section box
                        view3d.IsSectionBoxActive = True
                        view3d.SetSectionBox(boundingBoxBox)
                        
                        Dim param(1) As Object
                        param(0) = categorySectionBox
                        param(1) = True

                        If checkBoxBoxShow.Checked Then
                            view3d.SetCategoryHidden(categorySectionBox.Id, False)
                        Else
                            param(1) = False
                            view3d.SetCategoryHidden(categorySectionBox.Id, True)
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
                                elementIdOfView = NewElementId(Convert.ToInt64(viewElementId3dC))
                            Else
                                elementIdOfView = NewElementId(Convert.ToInt64(viewElementId3dBC))
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
                            view3d.IsSectionBoxActive = True
                            view3d.SetSectionBox(boundingBoxBox)
                            

                            Dim param(1) As Object
                            param(0) = categorySectionBox
                            param(1) = True

                            If checkBoxBoxShow.Checked Then
                            view3d.SetCategoryHidden(categorySectionBox.Id, False)
                            Else
                                param(1) = False
                            view3d.SetCategoryHidden(categorySectionBox.Id, True)
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
                            'Adjust crop box to fit Area(s) case  
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

                        'Complete the view settings
                        view3d.Name = viewNameComposite
                        'view3d.get_Parameter(BuiltInParameter.VIEW_DETAIL_LEVEL).Set(1);
                        view3d.Scale = scale
                    End If
                End If
            Catch exception As Exception
                MessageBox.Show("Unable to create view: '" & viewNameComposite & "'." & vbLf & vbLf & "This may be becasue it is an existing view and is the only view open" & vbLf & "so Revit cannot delete it in order to recreate it." & vbLf & vbLf & "System message: " & exception.Message, m_Settings.ProgramName)
                Continue For
            End Try

            Me.ProgressBar1.Increment(1)
        Next


        ' Commit the transaction
        m_Trans.Commit()

        '***We don't want to remove the items from the list since the user might want to create the 2D or 3D version after they did the other one.
        '****Try just refilling the list (may be too slow?)
        FillAreasList()

        'Close the progress bar.
        Me.ProgressBar1.Visible = False
    End Sub

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = DialogResult.OK
    End Sub


    Private Sub checkBoxListExisting_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxListExisting.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub radioButtonGroupSingle_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles radioButtonGroupSingle.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterList1_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxParameterList1.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterList2_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxParameterList2.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxPad1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxPad1.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxPad2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxPad2.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad1_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxPad1.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad2_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxPad2.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxListReverse_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxPad2.TextChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPrefixViewTarget_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxPrefixViewTarget.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterGroupBy_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles textBoxParameterGroupBy.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterGroupBy_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles textBoxParameterGroupBy.KeyPress
        If (e.KeyChar.ToString = vbCr) OrElse (e.KeyChar.ToString = vbTab) Then
            FillAreasList()
        End If
    End Sub

#End Region

End Class