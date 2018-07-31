Imports Autodesk.Revit

Imports System.Windows.Forms

Public Class form_ParamStringCalculate

    Private m_Settings As clsSettings
    Private mStringAccumulate As String

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()
        m_Settings = settings

        comboBoxElementType.Text = m_Settings.StringCalcElementType
        SetRadioButton(groupBoxElementsToProcess, m_Settings.StringCalcProcessRadio)

        SetRadioButton(groupBoxNode1, m_Settings.StringCalcNode1Radio)
        textBoxNode1Constant.Text = m_Settings.StringCalcNode1Constant
        textBoxNode1Source.Text = m_Settings.StringCalcNode1Source
        textBoxNode1Target.Text = m_Settings.StringCalcNode1Target
        SetCheckBoxValue(checkBoxNode1Include, m_Settings.StringCalcNode1Include)

        SetRadioButton(groupBoxNode2, m_Settings.StringCalcNode2Radio)
        textBoxNode2Constant.Text = m_Settings.StringCalcNode2Constant
        textBoxNode2Source.Text = m_Settings.StringCalcNode2Source
        textBoxNode2Target.Text = m_Settings.StringCalcNode2Target
        SetCheckBoxValue(checkBoxNode2Include, m_Settings.StringCalcNode2Include)

        SetRadioButton(groupBoxNode3, m_Settings.StringCalcNode3Radio)
        textBoxNode3Constant.Text = m_Settings.StringCalcNode3Constant
        textBoxNode3Source.Text = m_Settings.StringCalcNode3Source
        textBoxNode3Target.Text = m_Settings.StringCalcNode3Target
        SetCheckBoxValue(checkBoxNode3Include, m_Settings.StringCalcNode3Include)

        SetRadioButton(groupBoxNode4, m_Settings.StringCalcNode4Radio)
        textBoxNode4Constant.Text = m_Settings.StringCalcNode4Constant
        textBoxNode4Source.Text = m_Settings.StringCalcNode4Source
        textBoxNode4Target.Text = m_Settings.StringCalcNode4Target
        SetCheckBoxValue(checkBoxNode4Include, m_Settings.StringCalcNode4Include)

        SetRadioButton(groupBoxNode5, m_Settings.StringCalcNode5Radio)
        textBoxNode5Constant.Text = m_Settings.StringCalcNode5Constant
        textBoxNode5Source.Text = m_Settings.StringCalcNode5Source
        textBoxNode5Target.Text = m_Settings.StringCalcNode5Target
        SetCheckBoxValue(checkBoxNode5Include, m_Settings.StringCalcNode5Include)

        textBoxConcatenateTo.Text = m_Settings.StringCalcConcatTarget

        FillElementTypeList()
    End Sub

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.Close()
    End Sub

#Region "********************************************** Private Functions ****************************************************"
    Private Function ProcessOneNode(ByVal element As DB.Element, ByVal node As Integer) As Boolean
        Dim radioButton As RadioButton
        Dim stringNode As String = node.ToString
        Dim mode As String = ""
        Dim valueToAssign As String = ""

        Dim groupBox As GroupBox = DirectCast(Me.Controls("groupBoxNode" & stringNode), GroupBox)

        Dim textBoxConstant As TextBox = DirectCast(groupBox.Controls("textBoxNode" & node.ToString & "Constant"), TextBox)
        Dim textBoxSource As TextBox = DirectCast(groupBox.Controls("textBoxNode" & node.ToString & "Source"), TextBox)
        Dim textBoxTarget As TextBox = DirectCast(groupBox.Controls("textBoxNode" & node.ToString & "Target"), TextBox)
        Dim checkBoxInclude As CheckBox = DirectCast(groupBox.Controls("checkBoxNode" & node.ToString & "Include"), CheckBox)

        radioButton = DirectCast(groupBox.Controls("radioButtonNode" & stringNode & "NotUsed"), RadioButton)
        If radioButton.Checked Then
            mode = "NotUsed"
        Else
            radioButton = DirectCast(groupBox.Controls("radioButtonNode" & stringNode & "Constant"), RadioButton)
            If radioButton.Checked Then
                mode = "Constant"
            Else
                radioButton = DirectCast(groupBox.Controls("radioButtonNode" & stringNode & "Parameter"), RadioButton)
                If radioButton.Checked Then
                    mode = "Parameter"
                Else
                    radioButton = DirectCast(groupBox.Controls("radioButtonNode" & stringNode & "LevelNo"), RadioButton)
                    If radioButton.Checked Then
                        mode = "LevelNo"
                    Else
                        radioButton = DirectCast(groupBox.Controls("radioButtonNode" & stringNode & "ElementId"), RadioButton)
                        If radioButton.Checked Then
                            mode = "ElementId"
                        End If
                    End If
                End If
            End If
        End If

        'If node not used just return
        If mode = "NotUsed" Then
            Return True
        End If

        'Get values as needed
        Select Case mode
            Case "Constant"
                valueToAssign = textBoxConstant.Text
                Exit Select
            Case "Parameter"
                For Each parameter As DB.Parameter In element.Parameters
                    If parameter.Definition.Name = textBoxSource.Text Then
                        If parameter.AsString() IsNot Nothing Then
                            valueToAssign = parameter.AsString()
                        End If
                        'Not sure why value can be null but occurs
                        Exit For
                    End If
                Next
                Exit Select
            Case "LevelNo"
#If RELEASE2013 Then
                If element.Level Is Nothing Then
                    valueToAssign = ""
                Else
                    valueToAssign = m_Settings.Document.GetElement(element.Level.Id).Name
                    valueToAssign = valueToAssign.Substring(valueToAssign.LastIndexOf(" ") + 1)
                End If
#ElseIf RELEASE2014 Or RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                If element.LevelId Is Nothing Then
                valueToAssign = ""
                Else
                    valueToAssign = m_Settings.Document.GetElement(element.LevelId).Name
                    valueToAssign = valueToAssign.Substring(valueToAssign.LastIndexOf(" ") + 1)
                End If
#End If

                Exit Select
            Case "ElementId"
                valueToAssign = element.Id.IntegerValue.ToString
                Exit Select
            Case Else
                Exit Select
        End Select

        'Assign values
        If valueToAssign Is Nothing Then
            valueToAssign = ""
        End If
        'Believe we removed the cause of this but leave an error catch just in case.
        valueToAssign = valueToAssign.Trim()
        'Revit seems to do this to all parameters which cause checkk of existing value to fail.
        If textBoxTarget.Text <> "" Then
            For Each parameter As DB.Parameter In element.Parameters
                If parameter.Definition.Name = textBoxTarget.Text Then
                    If parameter.AsString() <> valueToAssign Then
                        parameter.[Set](valueToAssign)
                    End If
                End If
            Next
        End If
        If checkBoxInclude.Checked Then
            mStringAccumulate = mStringAccumulate + valueToAssign
        End If
        Return True
    End Function

    Private Function ProcessOneElement(ByVal element As DB.Element) As Boolean
        mStringAccumulate = ""
        For i As Integer = 1 To 5
            ProcessOneNode(element, i)
        Next
        mStringAccumulate = mStringAccumulate.Trim()
        'Revit seems to do this to all parameters which cause checkk of existing value to fail.
        If textBoxConcatenateTo.Text <> "" Then
            For Each parameter As DB.Parameter In element.Parameters
                If parameter.Definition.Name = textBoxConcatenateTo.Text Then
                    If parameter.AsString() <> mStringAccumulate Then
                        If parameter.IsReadOnly Then
                            MessageBox.Show("Unable to set parameter '" & parameter.Definition.Name & "' because it is read-only.", m_Settings.ProgramName)
                            Return False
                        End If
                        parameter.[Set](mStringAccumulate)
                    End If
                End If
            Next
        End If
        Return True
    End Function

    Private Function ValidateOneParameter(ByVal element As DB.Element, ByVal control As Control) As Boolean
        If control.Text = "" Then
            Return True
        End If
        For Each parameter As DB.Parameter In element.Parameters
            If parameter.Definition.Name = control.Text Then
                If parameter.StorageType = DB.StorageType.String Then
                    Return True
                End If
            End If
        Next
        MessageBox.Show("Parameter validation failed at parameter: " & control.Text & ". Processing stopped.", m_Settings.ProgramName)
        Return False
    End Function
    Private Function ValidateParameters(ByVal element As DB.Element) As Boolean
        If Not ValidateOneParameter(element, textBoxNode1Source) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode1Target) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode2Source) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode2Target) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode3Source) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode3Target) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode4Source) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode4Target) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode5Source) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxNode5Target) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxConcatenateTo) Then
            Return False
        End If
        Return True
    End Function

    Private Sub FillElementTypeList()
        'This isn't finding all the categories; for example "Property Lines" is a subset of Site
        'Convert to style like ElementParent where we use a button to search for all valid values in current model
        'Also for Write to excel?
        Dim categoryList As New List(Of String)()
        For Each category As DB.Category In m_Settings.Document.Settings.Categories
            categoryList.Add(category.Name)
        Next
        categoryList.Sort()
        For Each stringCategory As String In categoryList
            comboBoxElementType.Items.Add(stringCategory)
            'comboBoxElementType.SelectedIndex = 0;

        Next
    End Sub

    Private Function GetRadioButton(ByVal groupBox As GroupBox) As String
        Dim radioButton As RadioButton
        For Each controlTest As Control In groupBox.Controls
            If controlTest.[GetType]() Is GetType(RadioButton) Then
                radioButton = DirectCast(controlTest, RadioButton)
                If radioButton.Checked Then
                    Return radioButton.Name
                End If
            End If
        Next
        Return ""
    End Function
    Private Sub SetRadioButton(ByVal groupBox As GroupBox, ByVal nameRadioButton As String)
        Dim radioButton As RadioButton
        For Each controlTest As Control In groupBox.Controls
            If controlTest.[GetType]() Is GetType(RadioButton) Then
                radioButton = DirectCast(controlTest, RadioButton)
                If radioButton.Name = nameRadioButton Then
                    radioButton.Checked = True
                    Exit Sub
                End If
            End If
        Next
    End Sub
    Private Function GetCheckBoxValue(ByVal checkBox As CheckBox) As String
        If checkBox.Checked Then
            Return "true"
        Else
            Return "false"
        End If
    End Function
    Private Sub SetCheckBoxValue(ByVal checkBox As CheckBox, ByVal value As String)
        If value = "true" Then
            checkBox.Checked = True
        Else
            checkBox.Checked = False
        End If
    End Sub
#End Region

    Private Sub SaveSettings()

        m_Settings.StringCalcElementType = comboBoxElementType.Text
        m_Settings.StringCalcProcessRadio = GetRadioButton(groupBoxElementsToProcess)

        m_Settings.StringCalcNode1Radio = GetRadioButton(groupBoxNode1)
        m_Settings.StringCalcNode1Constant = textBoxNode1Constant.Text
        m_Settings.StringCalcNode1Source = textBoxNode1Source.Text
        m_Settings.StringCalcNode1Target = textBoxNode1Target.Text
        m_Settings.StringCalcNode1Include = GetCheckBoxValue(checkBoxNode1Include)

        m_Settings.StringCalcNode2Radio = GetRadioButton(groupBoxNode2)
        m_Settings.StringCalcNode2Constant = textBoxNode2Constant.Text
        m_Settings.StringCalcNode2Source = textBoxNode2Source.Text
        m_Settings.StringCalcNode2Target = textBoxNode2Target.Text
        m_Settings.StringCalcNode2Include = GetCheckBoxValue(checkBoxNode2Include)

        m_Settings.StringCalcNode3Radio = GetRadioButton(groupBoxNode3)
        m_Settings.StringCalcNode3Constant = textBoxNode3Constant.Text
        m_Settings.StringCalcNode3Source = textBoxNode3Source.Text
        m_Settings.StringCalcNode3Target = textBoxNode3Target.Text
        m_Settings.StringCalcNode3Include = GetCheckBoxValue(checkBoxNode3Include)

        m_Settings.StringCalcNode4Radio = GetRadioButton(groupBoxNode4)
        m_Settings.StringCalcNode4Constant = textBoxNode4Constant.Text
        m_Settings.StringCalcNode4Source = textBoxNode4Source.Text
        m_Settings.StringCalcNode4Target = textBoxNode4Target.Text
        m_Settings.StringCalcNode4Include = GetCheckBoxValue(checkBoxNode4Include)

        m_Settings.StringCalcNode5Radio = GetRadioButton(groupBoxNode5)
        m_Settings.StringCalcNode5Constant = textBoxNode5Constant.Text
        m_Settings.StringCalcNode5Source = textBoxNode5Source.Text
        m_Settings.StringCalcNode5Target = textBoxNode5Target.Text
        m_Settings.StringCalcNode5Include = GetCheckBoxValue(checkBoxNode5Include)

        m_Settings.StringCalcConcatTarget = textBoxConcatenateTo.Text

        m_Settings.WriteIni()
    End Sub

    Private Sub buttonProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonProcess.Click
        ' New Transaction
        Dim m_Trans As New DB.Transaction(m_Settings.Document, "HOK Parameter Strings")
        m_Trans.Start()

        Dim category As DB.Category = Nothing
        Dim elementsSelected As New List(Of DB.Element)
        Dim elementsModel As New List(Of DB.Element)
        Dim categoryFound As Boolean = False
        Dim firstElement As Boolean = True

        For Each categoryTest As DB.Category In m_Settings.Document.Settings.Categories
            If categoryTest.Name = comboBoxElementType.Text Then
                category = categoryTest
                categoryFound = True
                Exit For
            End If
        Next
        If Not categoryFound Then
            MessageBox.Show("Missing or invalid Element Type. Stoppping process.", m_Settings.ProgramName)
            Exit Sub
        End If

        ' New filtered element collector by category, category must be an actual db.Category
        Dim filCollector As New DB.FilteredElementCollector(m_Settings.Document)
        filCollector.OfCategory(category.Id.IntegerValue)
        elementsModel = filCollector.ToElements

        ' '' '' Old Way Removed...
        '' ''Dim filter As Filter
        '' ''filter = mSettings.Application.Create.Filter.NewCategoryFilter(category)
        '' ''mSettings.Application.ActiveDocument.get_Elements(filter, elementsModel)

        If radioButtonProcessSelection.Checked Then
            If m_Settings.ElementsPreSelected.IsEmpty Then
                MessageBox.Show("No elements were selected in Revit prior to running command." & vbLf & "Pre-select elements and rerun this command, or use the 'All Elements in Model' option.", m_Settings.ProgramName)
                Exit Sub
            End If
            For Each element As DB.Element In elementsModel
                If m_Settings.ElementsPreSelected.Contains(element) Then
                    elementsSelected.Add(element)
                End If
            Next
            If elementsSelected.Count = 0 Then
                MessageBox.Show("None of the pre-selected elements were of type: " & comboBoxElementType.Text & "." & vbLf & "Re-select elements and rerun this command, or use the 'All Elements in Model' option.", m_Settings.ProgramName)
                Exit Sub

            End If
        Else
            elementsSelected = elementsModel
        End If

        'Start the progress bar
        Dim progressBarForm As New form_ParamProgressBar(("Processing " & elementsSelected.Count.ToString & " ") + comboBoxElementType.Text & ".", elementsSelected.Count + 1)
        progressBarForm.Show()
        progressBarForm.Increment()
        'To avoid transparent look while waiting
        For Each element As DB.Element In elementsSelected

            'Check the first element for valid parameters.
            If firstElement Then
                If Not ValidateParameters(element) Then
                    progressBarForm.Close()
                    Exit Sub
                End If
                firstElement = False
            End If

            'Process the element
            If Not ProcessOneElement(element) Then
                MessageBox.Show("Error processing element with element ID: " & element.Id.IntegerValue.ToString & ". Processing stopped.", m_Settings.ProgramName)
                progressBarForm.Close()
                Exit Sub
            End If

            'Increment the Progress Bar
            progressBarForm.Increment()
        Next

        ' Commit the transaction
        m_Trans.Commit()

        'Close the progress bar.
        progressBarForm.Close()
    End Sub
End Class