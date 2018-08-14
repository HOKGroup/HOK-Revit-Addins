Imports Autodesk.Revit.DB
Imports Microsoft.Office.Interop
Imports System.Windows.Forms

Public Class form_ParamWriteToExcel

    Private m_Settings As clsSettings
    Private m_Excel As clsUtilityExcel
    Private m_Properties As New clsUtilityProperties
    Private m_ExcelApp As Excel.Application
    Private m_ExcelWasStarted As Boolean = True

    ''' <summary>
    ''' Form Class Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()
        m_Settings = settings
        m_Excel = New clsUtilityExcel
        Dim categoryList As New List(Of String)
        ' Category Listings
        For Each category As Category In settings.Document.Settings.Categories
            If category.Name.Contains("Tag") Then Continue For
            categoryList.Add(category.Name)
        Next
        ' Sort the List and bind to the control
        categoryList.Sort()
        Me.ComboBoxCategory.DataSource = categoryList
        Me.ComboBoxCategory.SelectedIndex = 0
        ' Apply saved settings to the form
        Try
            Select Case m_Settings.ExternalWriteParametersRadio.ToUpper
                Case "BOTH"
                    Me.CheckBoxType.Checked = True
                    Me.CheckBoxInstance.Checked = True
                Case "INSTANCE"
                    Me.CheckBoxInstance.Checked = True
                    Me.CheckBoxType.Checked = False
                Case "TYPE"
                    Me.CheckBoxType.Checked = True
                    Me.CheckBoxInstance.Checked = False
            End Select
            If m_Settings.ExternalWriteParametersRadio = "Both" Then

            End If
            Me.ComboBoxCategory.SelectedItem = m_Settings.ExternalWriteElementType
        Catch
        End Try

        ' Hide the progressbar
        Me.ProgressBar1.Visible = False
    End Sub

    ''' <summary>
    ''' Write selected data to Excel
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WriteToExcel()

        ' Verify the category
        Dim m_category As Category = Nothing
        For Each categoryTest As Category In m_Settings.Document.Settings.Categories
            If categoryTest.Name = Me.ComboBoxCategory.SelectedItem.ToString Then
                m_category = categoryTest
                Exit For
            End If
        Next
        If m_category Is Nothing Then
            MessageBox.Show("Missing or invalid Element Type. Stoppping process.", m_Settings.ProgramName)
            Exit Sub
        End If

        ' Select all elements by category
        Dim filCollector As New FilteredElementCollector(m_Settings.Document)
        filCollector.OfCategory(m_category.Id.IntegerValue)
        Dim m_CatElements As New List(Of Element)
        m_CatElements = filCollector.ToElements

        With Me.ProgressBar1
            .Visible = True
            .Maximum = m_CatElements.Count + 1
            .Minimum = 0
            .Value = 0
        End With

        Dim m_SelElements As New List(Of Element)

        ' Restrict to pre-selection if checked
        If radioButtonProcessSelection.Checked Then
            If m_Settings.ElementsPreSelected.IsEmpty Then
                MessageBox.Show("No elements were selected in Revit prior to running command." & _
                                vbLf & "Pre-select elements and rerun this command, or use the 'All Elements in Model' option.", _
                                m_Settings.ProgramName)
                Exit Sub
            End If
            For Each m_element As Element In m_CatElements
                If m_Settings.ElementsPreSelected.Contains(m_element) Then
                    m_SelElements.Add(m_element)
                End If
            Next
            If m_SelElements.Count = 0 Then
                MessageBox.Show("None of the pre-selected elements were of type: " & _
                                Me.ComboBoxCategory.SelectedItem.ToString & "." & vbLf & _
                                "Re-select elements and rerun this command, or use the 'All Elements in Model' option.", _
                                m_Settings.ProgramName)
                Exit Sub
            End If
        Else
            m_SelElements = m_CatElements
        End If

        Try

            ' Prop Names
            Dim m_InstProps As New List(Of String)
            Dim m_TypeProps As New List(Of String)

            ' Get Type and Inst prop names
            For Each Elem As Element In m_SelElements
                If m_InstProps.Count > 0 And m_TypeProps.Count > 0 Then Exit For

                If Elem.GetTypeId.IntegerValue.ToString = "-1" Then
                    If m_TypeProps.Count = 0 Then
                        ' Get the Type Properties
                        m_TypeProps = m_Properties.GetPropNames(Elem)
                    End If
                Else
                    If m_InstProps.Count = 0 Then
                        ' Get the Instance Properties
                        m_InstProps = m_Properties.GetPropNames(Elem)
                    End If
                End If

            Next

            Dim m_ExcelWorksheet As New Excel.Worksheet
            Dim row As Integer = 0

            ' Process each Element
            For Each Elem As Element In m_SelElements

                Me.ProgressBar1.Increment(1)

                ' Skip proper elements
                If Elem.GetTypeId.IntegerValue.ToString = "-1" Then
                    ' Type - skip if inst is checked
                    If Me.CheckBoxInstance.Checked = True Then Continue For
                Else
                    ' Inst - skip only if inst not checked
                    If Me.CheckBoxInstance.Checked = False Then Continue For
                End If

                ' Get property names and set up Excel with first element found
                Dim column As Integer
                If row = 0 Then
                    row = 1

                    ' Make sure Excel is started
                    If m_ExcelWasStarted Then
                        m_ExcelApp = m_Excel.LaunchExcel
                        m_ExcelApp.Visible = True
                    End If
                    m_ExcelWorksheet = m_Excel.AddWorksheet(m_ExcelWasStarted, Me.ComboBoxCategory.SelectedItem.ToString)
                    m_ExcelWasStarted = False

                    'Write column headings
                    m_ExcelWorksheet.Cells(row, 1) = "ID"
                    m_ExcelWorksheet.Cells(row, 2) = "Name"
                    column = 3
                    If Me.CheckBoxInstance.Checked = True Then
                        For Each stringPropertyName As String In m_InstProps
                            m_ExcelWorksheet.Cells(row, column) = "I:" & stringPropertyName
                            column += 1
                        Next
                    End If
                    If Me.CheckBoxType.Checked = True Then
                        For Each stringPropertyName As String In m_TypeProps
                            m_ExcelWorksheet.Cells(row, column) = "T:" & stringPropertyName
                            column += 1
                        Next
                    End If

                    ' First data row is 2, headings was 1
                    row = 2

                End If

                ' Get property values and write them to a new line in Excel
                column = 1
                m_ExcelWorksheet.Cells(row, 1) = Elem.Id.IntegerValue.ToString
                m_ExcelWorksheet.Cells(row, 2) = Elem.Name
                column = 3
                If Me.CheckBoxInstance.Checked = True Then
                    For Each stringPropertyName As String In m_InstProps
#If RELEASE2013 Or RELEASE2014 Then
                        Dim p As Parameter = Elem.Parameter(stringPropertyName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                        Dim p As Parameter = Elem.LookupParameter(stringPropertyName)
#End If

                        If p IsNot Nothing Then
                            Dim m_para As New clsPara(p)
                            m_ExcelWorksheet.Cells(row, column) = m_para.Value
                        End If
                        column += 1
                    Next
                End If

                ' Write out the type parameters
                If Me.CheckBoxType.Checked = True Then

                    For Each stringPropertyName As String In m_TypeProps

                        If Me.CheckBoxInstance.Checked = True Then
                            ' This is not a type element, get it from this instance
                            Try
                                Dim m_TypeElem As Element = m_Settings.Document.GetElement(Elem.GetTypeId)
#If RELEASE2013 Or RELEASE2014 Then
                                Dim p As Parameter = m_TypeElem.Parameter(stringPropertyName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                                Dim p As Parameter = m_TypeElem.LookupParameter(stringPropertyName)
#End If
                                If p IsNot Nothing Then
                                    Dim m_para As New clsPara(p)
                                    m_ExcelWorksheet.Cells(row, column) = m_para.Value
                                End If
                            Catch ex As Exception
                                ' Nothing
                            End Try

                        Else
                            ' This is a type element
#If RELEASE2013 Or RELEASE2014 Then
                            Dim p As Parameter = Elem.Parameter(stringPropertyName)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                            Dim p As Parameter = Elem.LookupParameter(stringPropertyName)
#End If
                            If p IsNot Nothing Then
                                Dim m_para As New clsPara(p)
                                m_ExcelWorksheet.Cells(row, column) = m_para.Value
                            End If
                        End If

                        column += 1

                    Next

                End If

                row += 1

            Next

            'Check if any elements found
            If row = 0 Then
                MessageBox.Show("No elemets of selected type found.")
                Exit Sub
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Get the checked radio button name in a group
    ''' </summary>
    ''' <param name="groupBox"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetRadioButton(ByVal groupBox As GroupBox) As String
        Dim radioButton As RadioButton
        For Each controlTest As System.Windows.Forms.Control In groupBox.Controls
            If controlTest.GetType Is GetType(RadioButton) Then
                radioButton = DirectCast(controlTest, RadioButton)
                If radioButton.Checked Then
                    Return radioButton.Name
                End If
            End If
        Next
        Return ""
    End Function

    ''' <summary>
    ''' Set the checked radio button name in a group
    ''' </summary>
    ''' <param name="groupBox"></param>
    ''' <param name="nameRadioButton"></param>
    ''' <remarks></remarks>
    Private Sub SetRadioButton(ByVal groupBox As GroupBox, ByVal nameRadioButton As String)
        Dim radioButton As RadioButton
        For Each controlTest As System.Windows.Forms.Control In groupBox.Controls
            If controlTest.GetType Is GetType(RadioButton) Then
                radioButton = DirectCast(controlTest, RadioButton)
                If radioButton.Name = nameRadioButton Then
                    radioButton.Checked = True
                    Exit Sub
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Save the settings to the INI file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SaveSettings()
        m_Settings.ExternalWriteElementType = Me.ComboBoxCategory.SelectedItem.ToString
        If Me.CheckBoxInstance.Checked = True And Me.CheckBoxType.Checked = True Then
            m_Settings.ExternalWriteParametersRadio = "Both"
        End If
        If Me.CheckBoxInstance.Checked = True And Me.CheckBoxType.Checked = False Then
            m_Settings.ExternalWriteParametersRadio = "Instance"
        End If
        If Me.CheckBoxInstance.Checked = False And Me.CheckBoxType.Checked = True Then
            m_Settings.ExternalWriteParametersRadio = "Type"
        End If
        m_Settings.ExternalWriteProcessRadio = GetRadioButton(groupBoxElementsToProcess)
        m_Settings.WriteIni()
    End Sub

#Region "Form Controls and Events"

    ''' <summary>
    ''' Close the app
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonClose_Click(ByVal sender As System.Object, _
                                  ByVal e As System.EventArgs) _
                              Handles buttonClose.Click
        SaveSettings()
        Me.Close()
    End Sub

    ''' <summary>
    ''' Start writing the data out to Excel
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonWrite_Click(ByVal sender As System.Object, _
                                  ByVal e As System.EventArgs) _
                              Handles buttonWrite.Click
        SaveSettings()
        Me.buttonWrite.Visible = False
        Me.buttonClose.Visible = False
        WriteToExcel()
        Me.ProgressBar1.Visible = False
        Me.buttonWrite.Visible = True
        Me.buttonClose.Visible = True
    End Sub

    ''' <summary>
    ''' Nothing checked, disable the start button
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub CheckBoxInstance_CheckedChanged(ByVal sender As System.Object, _
                                                ByVal e As System.EventArgs) _
                                            Handles CheckBoxInstance.CheckedChanged
        If Me.CheckBoxInstance.Checked = False And Me.CheckBoxType.Checked = False Then
            Me.buttonWrite.Enabled = False
        Else
            Me.buttonWrite.Enabled = True
        End If
    End Sub

    ''' <summary>
    ''' Nothing checked, disable the start button
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub CheckBoxType_CheckedChanged(ByVal sender As System.Object, _
                                            ByVal e As System.EventArgs) _
                                        Handles CheckBoxType.CheckedChanged
        If Me.CheckBoxInstance.Checked = False And Me.CheckBoxType.Checked = False Then
            Me.buttonWrite.Enabled = False
        Else
            Me.buttonWrite.Enabled = True
        End If
    End Sub

#End Region

End Class