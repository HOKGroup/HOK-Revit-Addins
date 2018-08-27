Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

Public Class form_ElemPlaceUnplacedAreas

    Private m_Settings As clsSettings
    Private mListItems As New List(Of String)
    Private initializing As Boolean = True

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ElementTools-PlaceUnplacedAreas", settings.Document.Application.VersionNumber))

        'Initialize the settings text boxes
        m_Settings = settings
        'assuming already initialized
        textBoxParameterList1.Text = m_Settings.AreasPlaceParamList1
        textBoxParameterList2.Text = m_Settings.AreasPlaceParamList2
        If m_Settings.AreasPlaceListPadYes1 = "true" Then
            checkBoxPad1.Checked = True
        Else
            checkBoxPad1.Checked = False
        End If
        If m_Settings.AreasPlaceListPadYes2 = "true" Then
            checkBoxPad2.Checked = True
        Else
            checkBoxPad2.Checked = False
        End If
        textBoxPad1.Text = m_Settings.AreasPlaceListPad1
        textBoxPad2.Text = m_Settings.AreasPlaceListPad2
        If m_Settings.AreasPlaceListReverse = "true" Then
            checkBoxListReverse.Checked = True
        Else
            checkBoxListReverse.Checked = False
        End If

        textBoxStartX.Text = m_Settings.AreasPlaceStartX
        textBoxStartY.Text = m_Settings.AreasPlaceStartY
        textBoxSpace.Text = m_Settings.AreasPlaceSpace
        textBoxNoRow.Text = m_Settings.AreasPlaceNoRow
        textBoxParameterRequiredArea.Text = m_Settings.AreasPlaceParamReqArea
        textBoxParameterRequiredDefault.Text = m_Settings.AreasPlaceReqDefault

        initializing = False
        'to avoid rerunning FillRoomsList during setup
        'Fill the list box with unplaced areas
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

        Dim notPlaced As Boolean

        '' ''For Each categoryTest As DB.Category In mSettings.Document.Settings.Categories
        '' ''    If categoryTest.Name = "Areas" Then
        '' ''        category = categoryTest
        '' ''        Exit For
        '' ''    End If
        '' ''Next

        Dim filCollector As New DB.FilteredElementCollector(m_Settings.Document)
        ' Add the category to the filter
        filCollector.OfCategory(DB.BuiltInCategory.OST_Areas)
        ' Return the list of elements from the filter collector
        elements = filCollector.ToElements

        '' ''Dim filter As Autodesk.Revit.Filter = Nothing
        '' ''Filter = mSettings.Application.Create.Filter.NewCategoryFilter(category)
        '' ''mSettings.Application.ActiveDocument.get_Elements(Filter, elements)

        mListItems.Clear()
        listBoxAreas.Items.Clear()
        For Each element As DB.Element In elements

            'The selection filter seems to return both the symbols and the instances; assume we only want the instances.
            If TypeOf element Is DB.ElementType Then
                Continue For
            End If

            listBy1 = ""
            listBy2 = ""
            notPlaced = False

            For Each parameter As DB.Parameter In element.Parameters
                If parameter.Definition Is Nothing Then
                    Continue For
                End If
                'Old known legacy bug of null parameter definitions; shoudn't hurt to ignore these
                Try
                    Dim dummy As String = parameter.Definition.Name
                Catch exception As Exception
                    MessageBox.Show(exception.Message)
                    Return
                End Try
                If parameter.Definition.Name = "Area" Then
                    If parameter.AsDouble() = 0 Then
                        notPlaced = True
                    Else
                        Exit For
                    End If
                End If
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
                    If listBy1 Is Nothing Then
                        listBy1 = ""
                    End If
                    If checkBoxPad1.Checked Then
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
                    If listBy2 Is Nothing Then
                        listBy2 = ""
                    End If
                    If checkBoxPad2.Checked Then
                        PadZeros(listBy2, padZeros2)
                    End If

                End If
            Next
            If notPlaced Then
                mListItems.Add((listBy1 & " + " & listBy2 & Convert.ToString(m_Settings.Spacer) & "<") + element.Id.IntegerValue.ToString & ">")
            End If
        Next

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

    Private Sub SaveSettings()

        m_Settings.AreasPlaceParamList1 = textBoxParameterList1.Text
        m_Settings.AreasPlaceParamList2 = textBoxParameterList2.Text
        If checkBoxPad1.Checked Then
            m_Settings.AreasPlaceListPadYes1 = "true"
        Else
            m_Settings.AreasPlaceListPadYes1 = "false"
        End If
        If checkBoxPad2.Checked Then
            m_Settings.AreasPlaceListPadYes2 = "true"
        Else
            m_Settings.AreasPlaceListPadYes2 = "false"
        End If
        m_Settings.AreasPlaceListPad1 = textBoxPad1.Text
        m_Settings.AreasPlaceListPad2 = textBoxPad2.Text
        If checkBoxListReverse.Checked Then
            m_Settings.AreasPlaceListReverse = "true"
        Else
            m_Settings.AreasPlaceListReverse = "false"
        End If

        m_Settings.AreasPlaceStartX = textBoxStartX.Text
        m_Settings.AreasPlaceStartY = textBoxStartY.Text
        m_Settings.AreasPlaceSpace = textBoxSpace.Text
        m_Settings.AreasPlaceNoRow = textBoxNoRow.Text
        m_Settings.AreasPlaceParamReqArea = textBoxParameterRequiredArea.Text
        m_Settings.AreasPlaceReqDefault = textBoxParameterRequiredDefault.Text

        m_Settings.WriteIni()
    End Sub

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub buttonCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCreate.Click

        Dim line As DB.Line
        Dim areaToPlace As DB.Area
        Dim areaNew As DB.Area
        Dim areaTag As DB.AreaTag
        Dim parameterArea As DB.Parameter
        Dim elemId As DB.ElementId
        Dim areaBoundaryLine As DB.ModelCurve

        'List of Curves is used to store Area's boundary lines
        Dim curves As List(Of DB.Curve) = New List(Of DB.Curve)

        'List of boundary lines in case we want to delete them.
        Dim modelCurves As List(Of DB.ModelCurve) = New List(Of DB.ModelCurve)

        Dim origin As New DB.XYZ(0, 0, 0)
        Dim norm As New DB.XYZ(0, 0, 1)

        'Autodesk.Revit.Elements.ViewPlan activeView = (Autodesk.Revit.Elements.ViewPlan) document.ActiveView;

        Dim space As Double
        Dim maxHeightCurrentRow As Double
        Dim areaDefault As Double
        Dim areaCurrent As Double
        Dim sideCurrent As Double
        Dim noPerRow As Integer
        Dim currentRowPosiiton As Integer

        If listBoxAreas.SelectedItems.Count < 1 Then
            MessageBox.Show("Select areas from list before pressing 'Create Areas' button.", m_Settings.ProgramName)
            Return
        End If

        'Get values from dialog box

        Dim ptCurrentRowStart As DB.XYZ = New DB.XYZ(CDbl(Convert.ToDouble(textBoxStartX.Text)), CDbl(Convert.ToDouble(textBoxStartY.Text)), 0)
        space = CDbl(Convert.ToDouble(textBoxSpace.Text))
        noPerRow = CInt(Convert.ToInt16(textBoxNoRow.Text))
        areaDefault = CDbl(Convert.ToDouble(textBoxParameterRequiredDefault.Text))
        If areaDefault <= 0 Then
            areaDefault = 100
        End If

        'Initial values for geometry
        currentRowPosiiton = 1
        Dim ptCurrentBox1 As DB.XYZ = New DB.XYZ(ptCurrentRowStart.X, ptCurrentRowStart.Y, 0)
        maxHeightCurrentRow = 0

        'Start the progress bar
        Me.ToolStripStatusLabel1.Text = "Creating " & listBoxAreas.SelectedItems.Count.ToString & " areas."
        Application.DoEvents()
        System.Threading.Thread.Sleep(1000)
        Me.ToolStripProgressBar1.Value = 0
        Me.ToolStripProgressBar1.Maximum = listBoxAreas.SelectedItems.Count + 1
        Me.ToolStripProgressBar1.Visible = True
        Me.ToolStripProgressBar1.ProgressBar.Refresh()
        Me.ToolStripProgressBar1.PerformStep()

        'Dim progressBarForm As New form_ElemProgress("Creating " + listBoxAreas.SelectedItems.Count.ToString & " areas.", listBoxAreas.SelectedItems.Count + 1)
        'progressBarForm.ShowDialog()
        'progressBarForm.Increment()
        'To avoid transparent look while waiting

        For Each listItem As String In listBoxAreas.SelectedItems
            Dim transaction As New Transaction(m_Settings.Document)
            If transaction.Start("Add New Areas") = TransactionStatus.Started Then
                'Get the existing area
                elemId = New DB.ElementId(CInt(Convert.ToInt64(listItem.Substring(listItem.IndexOf("<") + 1, listItem.Length - listItem.IndexOf("<") - 2))))
                areaToPlace = DirectCast(m_Settings.Document.GetElement(elemId), DB.Area)
#If RELEASE2013 Or RELEASE2014 Then
                parameterArea = areaToPlace.Parameter(textBoxParameterRequiredArea.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Then
                parameterArea = areaToPlace.LookupParameter(textBoxParameterRequiredArea.Text)
#End If

                If parameterArea Is Nothing Then
                    areaCurrent = CDbl(Convert.ToDouble(textBoxParameterRequiredDefault.Text))
                Else
                    areaCurrent = parameterArea.AsDouble()
                End If
                If areaCurrent <= 0 Then
                    areaCurrent = areaDefault
                End If
                sideCurrent = Math.Sqrt(areaCurrent)

                'Calculate points for geometry;

                Dim ptCurrentBox2 As DB.XYZ = New DB.XYZ(ptCurrentBox1.X + sideCurrent, ptCurrentBox1.Y, 0)
                Dim ptCurrentBox3 As DB.XYZ = New DB.XYZ(ptCurrentBox1.X + sideCurrent, ptCurrentBox1.Y - sideCurrent, 0)
                Dim ptCurrentBox4 As DB.XYZ = New DB.XYZ(ptCurrentBox1.X, ptCurrentBox1.Y - sideCurrent, 0)
                Dim ptCurrentInsert As DB.UV = New DB.UV(ptCurrentBox1.X + sideCurrent / 2, ptCurrentBox1.Y - sideCurrent / 2)

                'Place the boundary line

#If RELEASE2013 Then
                curves.Clear()
                line = m_Settings.Application.Application.Create.NewLine(ptCurrentBox1, ptCurrentBox2, True)
                curves.Add(line)
                line = m_Settings.Application.Application.Create.NewLine(ptCurrentBox2, ptCurrentBox3, True)
                curves.Add(line)
                line = m_Settings.Application.Application.Create.NewLine(ptCurrentBox3, ptCurrentBox4, True)
                curves.Add(line)
                line = m_Settings.Application.Application.Create.NewLine(ptCurrentBox4, ptCurrentBox1, True)
                curves.Add(line)

                Dim plane As DB.Plane = m_Settings.Document.Application.Create.NewPlane(norm, origin)
                Dim sketchPlane As DB.SketchPlane = m_Settings.Document.Create.NewSketchPlane(plane)
#ElseIf RELEASE2014 Or RELEASE2015 Or RELEASE2016 Then
                curves.Clear()
                line = DB.Line.CreateBound(ptCurrentBox1, ptCurrentBox2)
                curves.Add(line)
                line = DB.Line.CreateBound(ptCurrentBox2, ptCurrentBox3)
                curves.Add(line)
                line = DB.Line.CreateBound(ptCurrentBox3, ptCurrentBox4)
                curves.Add(line)
                line = DB.Line.CreateBound(ptCurrentBox4, ptCurrentBox1)
                curves.Add(line)

                Dim plane As DB.Plane = m_Settings.Document.Application.Create.NewPlane(norm, origin)
                Dim sketchPlane As DB.SketchPlane = SketchPlane.Create(m_Settings.Document, plane)
#ElseIf RELEASE2017 Then
                curves.Clear()
                line = DB.Line.CreateBound(ptCurrentBox1, ptCurrentBox2)
                curves.Add(line)
                line = DB.Line.CreateBound(ptCurrentBox2, ptCurrentBox3)
                curves.Add(line)
                line = DB.Line.CreateBound(ptCurrentBox3, ptCurrentBox4)
                curves.Add(line)
                line = DB.Line.CreateBound(ptCurrentBox4, ptCurrentBox1)
                curves.Add(line)

                Dim plane As DB.Plane = Plane.CreateByNormalAndOrigin(norm, origin)
                Dim sketchPlane As DB.SketchPlane = sketchPlane.Create(m_Settings.Document, plane)
#End If

                For Each curve As DB.Curve In curves
                    areaBoundaryLine = m_Settings.Document.Create.NewAreaBoundaryLine(sketchPlane, curve, m_Settings.ActiveViewPlan)
                    modelCurves.Add(areaBoundaryLine)
                Next

                'Place the new area and copy parameters from the old area to place
                Try
                    areaNew = m_Settings.Document.Create.NewArea(m_Settings.ActiveViewPlan, ptCurrentInsert)
                Catch
                    MessageBox.Show("Unable to create new areas.  This may be becasue the current view is not an area plan." & vbLf & vbLf & "Note that a room will have been placed at location wher the area was to have been placed" & "and should be deleted.", m_Settings.ProgramName)
                    Me.ToolStripProgressBar1.PerformStep()
                    For Each modelCurve As DB.ModelCurve In modelCurves
                        m_Settings.Document.Delete(modelCurve.Id)
                    Next

                    Return
                End Try

                For Each parameterToPlace As DB.Parameter In areaToPlace.Parameters
                    If parameterToPlace.IsReadOnly = False Then
                        For Each parameterNew As DB.Parameter In areaNew.Parameters
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
                Next

                'Tag the new area
                areaTag = m_Settings.Document.Create.NewAreaTag(m_Settings.ActiveViewPlan, areaNew, ptCurrentInsert)
                areaTag.HasLeader = False
                'areaTag.Location.Move(relocateTag);

                'Delete the old area
                m_Settings.Document.Delete(areaToPlace.Id)


                'Increment geometry values
                If sideCurrent > maxHeightCurrentRow Then
                    maxHeightCurrentRow = sideCurrent
                End If
                If currentRowPosiiton < noPerRow Then

                    ptCurrentBox1 = New DB.XYZ(ptCurrentBox1.X + sideCurrent + space, ptCurrentRowStart.Y, 0)

                    '' ''ptCurrentBox1.X = ptCurrentBox1.X + sideCurrent + space

                    currentRowPosiiton += 1
                Else

                    ptCurrentRowStart = New DB.XYZ(CDbl(Convert.ToDouble(textBoxStartX.Text)), ptCurrentRowStart.Y - (maxHeightCurrentRow + space), 0)
                    ptCurrentBox1 = New DB.XYZ(ptCurrentRowStart.X, ptCurrentRowStart.Y, 0)

                    '' ''ptCurrentRowStart.Y = ptCurrentRowStart.Y - (maxHeightCurrentRow + space)
                    '' ''ptCurrentBox1.X = ptCurrentRowStart.X
                    '' ''ptCurrentBox1.Y = ptCurrentRowStart.Y

                    maxHeightCurrentRow = 0
                    currentRowPosiiton = 1
                End If

                'Increment the Progress Bar
                Me.ToolStripProgressBar1.PerformStep()
                transaction.Commit()
            End If
        Next

        'Remove all the values from the list (can't do as we go along since messes up increment)
        While listBoxAreas.SelectedItems.Count > 0
            listBoxAreas.Items.Remove(listBoxAreas.SelectedItem)
        End While

        'Close the progress bar.
        Me.ToolStripStatusLabel1.Text = "Ready"
        Me.ToolStripProgressBar1.Visible = False
    End Sub

    Private Sub textBoxParameterList1_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList1.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxParameterList2_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxParameterList2.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxPad1_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad1.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxPad2_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxPad2.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad1_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad1.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub textBoxPad2_Leave(ByVal sender As Object, ByVal e As EventArgs) Handles textBoxPad2.Leave
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

    Private Sub checkBoxListReverse_CheckedChanged(ByVal sender As Object, ByVal e As EventArgs) Handles checkBoxListReverse.CheckedChanged
        If Not initializing Then
            FillAreasList()
        End If
    End Sub

End Class