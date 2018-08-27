Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

Imports System.IO
Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils
Imports Microsoft.Office.Interop.Excel

Public Class form_ParamScheduleKey

    Private mSettings As clsSettings
    Private mUtilityExcel As clsUtilityExcel
    Private mExcelApplication As Microsoft.Office.Interop.Excel.Application
    Private mScheduleKeyItem As clsScheduleKeyItem
    Private mKeyTablesAll As New Dictionary(Of String, clsScheduleKeySet)

    Public Sub New(ByVal settings As clsSettings)
        Dim combinedValues As String
        Dim positionDelimeter As Integer

        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ParameterTools-ScheduleKeyManager", settings.Document.Application.VersionNumber))

        mSettings = settings
        mUtilityExcel = New clsUtilityExcel
        textBoxPath.Text = mSettings.ExternalKeyPath.Trim()

        textBoxKey1.Text = mSettings.ExternalKeyParamKey1.Trim()
        If mSettings.ExternalKeyParamList1 <> "" Then

            combinedValues = mSettings.ExternalKeyParamList1
            positionDelimeter = combinedValues.IndexOf(";")
            If positionDelimeter = -1 Then
                listBoxParamList1.Items.Add(combinedValues)
            Else
                While positionDelimeter <> -1
                    listBoxParamList1.Items.Add(combinedValues.Substring(0, positionDelimeter))
                    combinedValues = combinedValues.Substring(positionDelimeter + 1)
                    positionDelimeter = combinedValues.IndexOf(";")
                End While
                listBoxParamList1.Items.Add(combinedValues)
            End If
        End If
        textBoxKey2.Text = mSettings.ExternalKeyParamKey2.Trim()
        If mSettings.ExternalKeyParamList2 <> "" Then
            combinedValues = mSettings.ExternalKeyParamList2
            positionDelimeter = combinedValues.IndexOf(";")
            If positionDelimeter = -1 Then
                listBoxParamList2.Items.Add(combinedValues)
            Else
                While positionDelimeter <> -1
                    listBoxParamList2.Items.Add(combinedValues.Substring(0, positionDelimeter))
                    combinedValues = combinedValues.Substring(positionDelimeter + 1)
                    positionDelimeter = combinedValues.IndexOf(";")
                End While
                listBoxParamList2.Items.Add(combinedValues)
            End If
        End If
        textBoxKey3.Text = mSettings.ExternalKeyParamKey3.Trim()
        If mSettings.ExternalKeyParamList3 <> "" Then
            combinedValues = mSettings.ExternalKeyParamList3
            positionDelimeter = combinedValues.IndexOf(";")
            If positionDelimeter = -1 Then
                listBoxParamList3.Items.Add(combinedValues)
            Else
                While positionDelimeter <> -1
                    listBoxParamList3.Items.Add(combinedValues.Substring(0, positionDelimeter))
                    combinedValues = combinedValues.Substring(positionDelimeter + 1)
                    positionDelimeter = combinedValues.IndexOf(";")
                End While
                listBoxParamList3.Items.Add(combinedValues)
            End If
        End If
    End Sub

    Private Sub SaveSettings()
        mSettings.ExternalKeyPath = textBoxPath.Text.Trim()
        'mSettings.ExternalKeyElementType = comboBoxElementType.Text.Trim();
        mSettings.ExternalKeyParamKey1 = textBoxKey1.Text.Trim()
        If listBoxParamList1.Items.Count = 0 Then
            mSettings.ExternalKeyParamList1 = ""
        Else
            mSettings.ExternalKeyParamList1 = listBoxParamList1.Items(0).ToString
            For i As Integer = 1 To listBoxParamList1.Items.Count - 1
                mSettings.ExternalKeyParamList1 = (mSettings.ExternalKeyParamList1 & ";") + listBoxParamList1.Items(i).ToString
            Next
        End If
        mSettings.ExternalKeyParamKey2 = textBoxKey2.Text.Trim()
        If listBoxParamList2.Items.Count = 0 Then
            mSettings.ExternalKeyParamList2 = ""
        Else
            mSettings.ExternalKeyParamList2 = listBoxParamList2.Items(0).ToString
            For i As Integer = 1 To listBoxParamList2.Items.Count - 1
                mSettings.ExternalKeyParamList2 = (mSettings.ExternalKeyParamList2 & ";") + listBoxParamList2.Items(i).ToString
            Next
        End If
        mSettings.ExternalKeyParamKey3 = textBoxKey3.Text.Trim()
        If listBoxParamList3.Items.Count = 0 Then
            mSettings.ExternalKeyParamList3 = ""
        Else
            mSettings.ExternalKeyParamList3 = listBoxParamList3.Items(0).ToString
            For i As Integer = 1 To listBoxParamList3.Items.Count - 1
                mSettings.ExternalKeyParamList3 = (mSettings.ExternalKeyParamList3 & ";") + listBoxParamList3.Items(i).ToString
            Next
        End If
        mSettings.WriteIni()
    End Sub

    Private Function QueryScheduleKeys() As Boolean

        Dim mScheduleKeySet As clsScheduleKeySet
        Dim parametersLocal As DB.ParameterSet

        Try

            If Not (textBoxKey1.Text = "" OrElse listBoxParamList1.Items.Count = 0) Then
                mScheduleKeySet = New clsScheduleKeySet(textBoxKey1.Text)
                For i As Integer = 0 To listBoxParamList1.Items.Count - 1
                    mScheduleKeySet.ParameterNames.Add(listBoxParamList1.Items(i).ToString)
                Next
                mKeyTablesAll.Add(mScheduleKeySet.Name, mScheduleKeySet)
            End If
            If Not (textBoxKey2.Text = "" OrElse listBoxParamList2.Items.Count = 0) Then
                mScheduleKeySet = New clsScheduleKeySet(textBoxKey2.Text)
                For i As Integer = 0 To listBoxParamList2.Items.Count - 1
                    mScheduleKeySet.ParameterNames.Add(listBoxParamList2.Items(i).ToString)
                Next
                mKeyTablesAll.Add(mScheduleKeySet.Name, mScheduleKeySet)
            End If
            If Not (textBoxKey3.Text = "" OrElse listBoxParamList3.Items.Count = 0) Then
                mScheduleKeySet = New clsScheduleKeySet(textBoxKey3.Text)
                For i As Integer = 0 To listBoxParamList3.Items.Count - 1
                    mScheduleKeySet.ParameterNames.Add(listBoxParamList3.Items(i).ToString)
                Next
                mKeyTablesAll.Add(mScheduleKeySet.Name, mScheduleKeySet)
            End If

            Dim elementRooms As New List(Of DB.Element)
            Dim elementScheduleKey As DB.Element

            'ScheduleKeySet scheduleKeySetTest;
            Dim parameterValuesCapture As New List(Of String)()

            Dim numberOfParameters As Integer
            Dim foundParameter As Boolean
            Dim findParamaterFailed As Boolean

            Dim myCol As DB.FilteredElementCollector = New DB.FilteredElementCollector(mSettings.Document)
            Dim elementIterator As IEnumerator = myCol.GetElementIterator

            elementIterator.Reset()
            While elementIterator.MoveNext()
                If Not (elementIterator.Current.[GetType]().FullName = "Autodesk.Revit.DB.Element") Then
                    Continue While
                End If
                elementScheduleKey = TryCast(elementIterator.Current, DB.Element)
                If elementScheduleKey Is Nothing Then
                    Continue While
                End If
                If elementScheduleKey.GetType IsNot Nothing Then
                    Continue While
                End If
                If elementScheduleKey.Category IsNot Nothing Then
                    Continue While
                End If
                If elementScheduleKey.Parameters.IsEmpty Then
                    Continue While
                End If

                'At this point we have a candidate for a key table item although we are not sure. Test is to see if it has the 
                'correct parameter list. Not a sure-fire test and pretty slow but best we have for now.
                parametersLocal = elementScheduleKey.Parameters
                For Each keyTableName As String In mKeyTablesAll.Keys
                    mScheduleKeySet = mKeyTablesAll(keyTableName)
                    numberOfParameters = mScheduleKeySet.ParameterNames.Count
                    findParamaterFailed = False
                    parameterValuesCapture.Clear()
                    For i As Integer = 0 To mScheduleKeySet.ParameterNames.Count - 1
                        foundParameter = False
                        For Each param As DB.Parameter In parametersLocal
                            If param.Definition.Name = mScheduleKeySet.ParameterNames(i) Then
                                parameterValuesCapture.Add(param.AsString())
                                foundParameter = True
                                Exit For
                            End If
                        Next
                        If Not foundParameter Then
                            findParamaterFailed = True
                            Exit For
                        End If
                    Next
                    If findParamaterFailed Then
                        Continue For
                    End If
                    mScheduleKeyItem = New clsScheduleKeyItem(elementScheduleKey.Name)
                    mScheduleKeyItem.ElementId = elementScheduleKey.Id
                    For Each item As String In parameterValuesCapture
                        mScheduleKeyItem.ValueSet.Add(item)
                    Next
                    mScheduleKeySet.ScheduleKeyItems.Add(mScheduleKeyItem)
                Next
            End While

            Return True
        Catch exception As Exception
            MessageBox.Show(exception.Message)
            Return False
        End Try
    End Function

    Private Function ReadOneScheduleKeySet(ByVal worksheet As Worksheet) As Boolean
        Dim elementId As DB.ElementId
        Dim element As DB.Element
        Dim parameterChange As DB.Parameter
        Dim range As Range
        Dim parameterNames As New List(Of String)
        Dim parameterValues As New List(Of String)
        '!!NOTE only handling string vlaues for now.
        Dim cellValue As String
        Dim valueCurrent As String
        Dim valueCompare As String
        Dim listIndex As Integer
        Dim indexRow As Integer = 1

        Try
            'Get the parameter names fror the headings
            For i As Integer = 3 To 99
                range = DirectCast(worksheet.Cells(indexRow, i), Range)
                If range.Value2 Is Nothing Then
                    Exit For
                End If
                If range.Value2.ToString.Trim() = "" Then
                    Exit For
                End If
                cellValue = range.Value2.ToString.Trim()
                If cellValue <> "" Then
                    If Not (parameterNames.Contains(cellValue)) Then
                        parameterNames.Add(cellValue)
                    End If
                End If
            Next

            'Note that synchronization relationship is based on the element ID, not the KeyValue. Hopefully the user has not messed up ID!
            'This does mean that we can rename the KeyValue which is useful for creating a bunch of new records.
            indexRow = 2
            While True

                'Process ID
                range = DirectCast(worksheet.Cells(indexRow, 1), Microsoft.Office.Interop.Excel.Range)
                If range.Value2 Is Nothing Then
                    Exit While
                End If
                cellValue = range.Value2.ToString.Trim()
                If cellValue = "" Then
                    Exit While
                End If

                elementId = New DB.ElementId(CInt(Convert.ToInt64(cellValue)))
                element = mSettings.Document.GetElement(elementId)

                If element IsNot Nothing Then

                    'Process KeyValue. Note breaks out of loop for null/blank values; means something is wrong since we don't want to set to null.
                    range = DirectCast(worksheet.Cells(indexRow, 2), Microsoft.Office.Interop.Excel.Range)
                    If range.Value2 Is Nothing Then
                        Exit While
                    End If
                    cellValue = range.Value2.ToString.Trim()
                    If cellValue = "" Then
                        Exit While
                    End If
#If RELEASE2013 Or RELEASE2014 Then
                    parameterChange = element.Parameter("Key Name")
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                    parameterChange = element.LookupParameter("Key Name")
#End If

                    parameterChange.[Set](cellValue)

                    'Process parameters
                    parameterValues.Clear()
                    For indexColumn As Integer = 3 To parameterNames.Count + 2
                        range = DirectCast(worksheet.Cells(indexRow, indexColumn), Microsoft.Office.Interop.Excel.Range)
                        If range.Value2 Is Nothing Then
                            cellValue = ""
                        Else
                            cellValue = range.Value2.ToString.Trim()
                        End If
                        parameterValues.Add(cellValue)
                    Next
                    listIndex = 0
                    For Each parameterName As String In parameterNames
#If RELEASE2013 Or RELEASE2014 Then
                        parameterChange = element.Parameter(parameterName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                        parameterChange = element.LookupParameter(parameterName)
#End If

                        If parameterChange Is Nothing Then
                            Continue For
                        End If
                        'A column heading doesn't have a corresponding parameter name
                        valueCurrent = parameterChange.AsString()
                        valueCompare = parameterValues(listIndex).ToString
                        If valueCurrent <> valueCompare Then
                            'try not to change values unless necessary
                            parameterChange.[Set](valueCompare)
                        End If
                        listIndex += 1
                    Next
                End If
                indexRow += 1
            End While
            Return True
        Catch exception As Exception
            MessageBox.Show(exception.Message)
            Return False
        End Try
    End Function

    Private Function CheckForValidWorksheetName(ByVal name As String) As Boolean
        If name.Contains(":") Then
            Return False
        End If
        If name.Contains(";") Then
            Return False
        End If
        If name.Contains("/") Then
            Return False
        End If
        If name.Contains("\") Then
            Return False
        End If
        If name.Contains("?") Then
            Return False
        End If
        If name.Contains("*") Then
            Return False
        End If
        If name.Contains("[") Then
            Return False
        End If
        If name.Contains("]") Then
            Return False
        End If
        Return True
    End Function

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        mUtilityExcel = Nothing
        GC.Collect()
        Me.Close()
    End Sub

    Private Sub buttonWrite_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonWrite.Click
        Dim workbook As Workbook
        Dim worksheet As Worksheet
        Dim scheduleKeySet As clsScheduleKeySet
        Dim indexColumn As Integer
        Dim indexRow As Integer
        Dim saveAlertStatus As Boolean
        Try
            If Not QueryScheduleKeys() Then
                Exit Sub
            End If

            'Start Excel (if needed) and bind to the Excel file.
            mExcelApplication = mUtilityExcel.BindToExcel(textBoxPath.Text)
            If mExcelApplication Is Nothing Then
                Exit Sub
            End If
            workbook = mExcelApplication.ActiveWorkbook

            'Create a worksheet for each set
            '!!!!Note that we delete existing with the same name. Works OK but might want to warn the user?
            For Each keyTableName As String In mKeyTablesAll.Keys

                'Check for invalid worksheet names
                If Not CheckForValidWorksheetName(keyTableName) Then
                    MessageBox.Show("Invalid Worksheet name: '" & keyTableName & "'. Ignoring this entry. Process will continue.", mSettings.ProgramName)
                    Continue For
                End If

                scheduleKeySet = mKeyTablesAll(keyTableName)
                saveAlertStatus = mExcelApplication.DisplayAlerts
                mExcelApplication.DisplayAlerts = False
                Try
                    For Each testWorksheet As Worksheet In mExcelApplication.Worksheets
                        If testWorksheet.Name = keyTableName Then
                            testWorksheet.Delete()
                        End If
                    Next
                Catch
                    'case where no worksheets exist
                    MessageBox.Show("Excel was not started properly. Processing halted.", mSettings.ProgramName)
                    Exit Sub
                End Try
                mExcelApplication.DisplayAlerts = saveAlertStatus
                worksheet = TryCast(mExcelApplication.Worksheets.Add(Type.Missing, mExcelApplication.Worksheets(mExcelApplication.Worksheets.Count), Type.Missing, Type.Missing), Microsoft.Office.Interop.Excel.Worksheet)
                If worksheet Is Nothing Then
                    Exit Sub
                End If
                worksheet.Name = keyTableName

                'Write column headings
                indexRow = 1
                indexColumn = 1
                worksheet.Cells.set_Item(indexRow, indexColumn, "ID")
                indexColumn += 1
                worksheet.Cells.set_Item(indexRow, indexColumn, "KeyName")
                indexColumn += 1
                For Each parameterName As String In scheduleKeySet.ParameterNames
                    worksheet.Cells.set_Item(indexRow, indexColumn, parameterName)
                    indexColumn += 1
                Next

                'Write data rows
                indexRow = 2
                'Since headings was 1
                indexColumn = 1
                For Each scheduleKeyItem As clsScheduleKeyItem In scheduleKeySet.ScheduleKeyItems
                    worksheet.Cells.set_Item(indexRow, indexColumn, scheduleKeyItem.ElementId.IntegerValue.ToString)
                    indexColumn += 1
                    worksheet.Cells.set_Item(indexRow, indexColumn, scheduleKeyItem.KeyValue)
                    indexColumn += 1
                    For Each value As String In scheduleKeyItem.ValueSet
                        worksheet.Cells.set_Item(indexRow, indexColumn, value)
                        indexColumn += 1
                    Next
                    indexRow += 1
                    indexColumn = 1
                Next
            Next
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub buttonRead_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonRead.Click
        Dim worksheet As Worksheet

        Try

            'Check that Excel is saved at this point
            If textBoxPath.Text.Trim() = "" Then
                MessageBox.Show("Excel must be saved and the path added to the text box before reading data.", mSettings.ProgramName)
                Exit Sub
            End If

            'Start Excel (if needed) and bind to the Excel file.
            mExcelApplication = mUtilityExcel.BindToExcel(textBoxPath.Text)
            If mExcelApplication Is Nothing Then
                Exit Sub
            End If
            If Not (textBoxKey1.Text = "" OrElse listBoxParamList1.Items.Count = 0) Then
                worksheet = DirectCast(mExcelApplication.Worksheets(textBoxKey1.Text), Worksheet)
                If worksheet IsNot Nothing Then
                    ReadOneScheduleKeySet(worksheet)
                End If
            End If
            If Not (textBoxKey2.Text = "" OrElse listBoxParamList2.Items.Count = 0) Then
                worksheet = DirectCast(mExcelApplication.Worksheets(textBoxKey2.Text), Worksheet)
                If worksheet IsNot Nothing Then
                    ReadOneScheduleKeySet(worksheet)
                End If
            End If
            If Not (textBoxKey3.Text = "" OrElse listBoxParamList3.Items.Count = 0) Then
                worksheet = DirectCast(mExcelApplication.Worksheets(textBoxKey3.Text), Worksheet)
                If worksheet IsNot Nothing Then
                    ReadOneScheduleKeySet(worksheet)
                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub buttonBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonBrowse.Click
        openFileDialog1.Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|All Files (*.*)|*.*"
        If File.Exists(textBoxPath.Text) Then
            openFileDialog1.FileName = textBoxPath.Text
        Else
            openFileDialog1.FileName = ""
            Dim path As String = textBoxPath.Text
            Dim pos As Integer = path.LastIndexOf("\")
            If pos > 1 AndAlso pos < path.Length - 2 Then
                path = textBoxPath.Text.Substring(0, pos)
                If Directory.Exists(path) Then
                    openFileDialog1.InitialDirectory = path
                End If
            End If
        End If
        Dim result As DialogResult = openFileDialog1.ShowDialog()
        If result <> DialogResult.Cancel Then
            textBoxPath.Text = openFileDialog1.FileName
        End If
    End Sub

    Private Sub buttonParamAdd1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonParamAdd1.Click
        listBoxParamList1.Items.Add(textBoxParamNew1.Text.Trim())
        textBoxParamNew1.Text = ""
    End Sub

    Private Sub buttonParamAdd2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonParamAdd2.Click
        listBoxParamList2.Items.Add(textBoxParamNew2.Text.Trim())
        textBoxParamNew2.Text = ""
    End Sub

    Private Sub buttonParamAdd3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonParamAdd3.Click
        listBoxParamList3.Items.Add(textBoxParamNew3.Text.Trim())
        textBoxParamNew3.Text = ""
    End Sub

    Private Sub buttonParamDelete1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonParamDelete1.Click
        If listBoxParamList1.SelectedItems.Count = 0 Then
            MessageBox.Show("Select item to be deleted before running command.", mSettings.ProgramName)
            Exit Sub
        Else
            listBoxParamList1.Items.Remove(listBoxParamList1.SelectedItem)
        End If
    End Sub

    Private Sub buttonParamDelete2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonParamDelete2.Click
        If listBoxParamList2.SelectedItems.Count = 0 Then
            MessageBox.Show("Select item to be deleted before running command.", mSettings.ProgramName)
            Exit Sub
        Else
            listBoxParamList2.Items.Remove(listBoxParamList2.SelectedItem)
        End If
    End Sub

    Private Sub buttonParamDelete3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonParamDelete3.Click
        If listBoxParamList3.SelectedItems.Count = 0 Then
            MessageBox.Show("Select item to be deleted before running command.", mSettings.ProgramName)
            Exit Sub
        Else
            listBoxParamList3.Items.Remove(listBoxParamList3.SelectedItem)
        End If
    End Sub
End Class