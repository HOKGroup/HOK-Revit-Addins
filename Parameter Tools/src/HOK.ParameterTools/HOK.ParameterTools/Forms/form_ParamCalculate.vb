Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

Public Class form_ParamCalculate

    Private m_Settings As clsSettings
    Private mCategory As Category = Nothing

    Private mElementsSelected As New List(Of Element)
    Private mElementsModel As New List(Of Element)

    Private mFormula As String 'The user-friendly version of the formula
    Private mFormulaCode As String 'The short verion of the formula used to interact with the dialog boxes
    Private mParameterNameA As String
    Private mParameterNameB As String
    Private mParameterNameC As String
    Private mConstantValueA As String
    Private mConstantValueB As String
    Private mConstantValueC As String
    Private mParameterNameGroupBy As String
    Private mParameterNameTarget As String

    Private mValueNeededA As Boolean 'This is kind of a dummy since we always need an A
    Private mValueNeededB As Boolean
    Private mValueNeededC As Boolean
    Private mValueNeededGroupBy As Boolean
    Private mParameterExistsA As Boolean
    Private mParameterExistsB As Boolean
    Private mParameterExistsC As Boolean
    Private mConstantExistsA As Boolean
    Private mConstantExistsB As Boolean
    Private mConstantExistsC As Boolean
    Private mParameterExistsGroupBy As Boolean
    Private mAggregation As Boolean 'true in case of summations, for example, whe a value is accumulated and then written at the end.

    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ParameterTools-MathCalculation", settings.Document.Application.VersionNumber))

        m_Settings = settings

        comboBoxElementType.Text = m_Settings.MathCalcElementType
        SetRadioButton(groupBoxElementsToProcess, m_Settings.MathCalcProcessRadio)

        textBoxParameterA.Text = m_Settings.MathCalcParameterA
        textBoxParameterB.Text = m_Settings.MathCalcParameterB
        textBoxParameterC.Text = m_Settings.MathCalcParameterC
        textBoxConstantA.Text = m_Settings.MathCalcConstantA
        textBoxConstantB.Text = m_Settings.MathCalcConstantB
        textBoxConstantC.Text = m_Settings.MathCalcConstantC
        textBoxGroupBy.Text = m_Settings.MathCalcGroupBy
        textBoxTarget.Text = m_Settings.MathCalcTarget
        SetRadioButton(groupBoxFunction, m_Settings.MathCalcFunctionRadio)

        SetCheckBoxValue(checkBoxFormulaInclude1, m_Settings.MathCalcFormulaInclude1)
        SetCheckBoxValue(checkBoxFormulaInclude2, m_Settings.MathCalcFormulaInclude2)
        SetCheckBoxValue(checkBoxFormulaInclude3, m_Settings.MathCalcFormulaInclude3)
        textBoxFormula1.Text = m_Settings.MathCalcFormula1
        textBoxFormula2.Text = m_Settings.MathCalcFormula2
        textBoxFormula3.Text = m_Settings.MathCalcFormula3

        FillElementTypeList()
    End Sub

#Region "********************************************** Private Functions ****************************************************"
    Private Function GetParameterValue(ByVal element As Element, ByVal nameParameter As String, ByRef returnValue As Double) As Boolean
#If RELEASE2013 Or RELEASE2014 Then
        Dim parameter As Parameter = element.Parameter(nameParameter)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
        Dim parameter As Parameter = element.LookupParameter(nameParameter)
#End If
        If parameter Is Nothing Then
            Return False
        End If
        'couldn't find the parameter
        If parameter.StorageType = StorageType.Double Then
            'type double is never null
            returnValue = parameter.AsDouble()
            Return True
        ElseIf parameter.StorageType = StorageType.Integer Then
            'type integer is never null
            returnValue = Convert.ToDouble(parameter.AsInteger())
            Return True
        End If
        'Something wrong with the storage type
        Return False
    End Function
    Private Function GetParameterValue(ByVal element As Element, ByVal nameParameter As String, ByRef returnValue As String) As Boolean
#If RELEASE2013 Or RELEASE2014 Then
        Dim parameter As Parameter = element.Parameter(nameParameter)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
        Dim parameter As Parameter = element.LookupParameter(nameParameter)
#End If

        If parameter Is Nothing Then
            Return False
        End If
        'couldn't find the parameter
        If parameter.StorageType = StorageType.String Then
            If parameter.AsString() Is Nothing Then
                returnValue = ""
            Else
                'Revit seems to be inconsistent as to when it uses a null?
                returnValue = parameter.AsString()
            End If
            Return True
        End If
        'Something wrong with the storage type
        Return False
    End Function

    Private Function SetParameterValue(ByVal element As Element, ByVal nameParameter As String, ByVal value As Double) As Boolean
#If RELEASE2013 Or RELEASE2014 Then
        Dim parameter As Parameter = element.Parameter(nameParameter)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
        Dim parameter As Parameter = element.LookupParameter(nameParameter)
#End If
        If parameter Is Nothing Then
            Return False
        End If
        'couldn't find the parameter
        Try
            parameter.[Set](value)
        Catch
            Return False
        End Try
        Return True
    End Function

    Private Function ValidateOneParameter(ByVal element As Element, ByVal control As System.Windows.Forms.Control) As Boolean
        'Checks that parameter exists and is a numerical type.
        If control.Text = "" Then
            Return True
        End If
#If RELEASE2013 Or RELEASE2014 Then
        Dim parameter As Parameter = element.Parameter(control.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
        Dim parameter As Parameter = element.LookupParameter(control.Text)
#End If
        If parameter Is Nothing Then
            Return False
        End If
        'couldn't find the parameter
        If parameter.StorageType = StorageType.Double Then
            Return True
        End If
        If parameter.StorageType = StorageType.Integer Then
            Return True
        End If
        MessageBox.Show("Parameter validation failed at parameter: " & control.Text & ". Processing stopped.", m_Settings.ProgramName)
        Return False
    End Function
    Private Function ValidateParameters(ByVal element As Element) As Boolean
        If Not ValidateOneParameter(element, textBoxParameterA) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxParameterB) Then
            Return False
        End If
        If Not ValidateOneParameter(element, textBoxTarget) Then
            Return False
        End If
        Return True
    End Function

    Private Sub FillElementTypeList()
        'This isn't finding all the categories; for example "Property Lines" is a subset of Site
        'Convert to style like ElementParent where we use a button to search for all valid values in current model
        'Also for Write to excel?
        Dim categoryList As New List(Of String)
        For Each category As Category In m_Settings.Document.Settings.Categories
            categoryList.Add(category.Name)
        Next
        categoryList.Sort()
        For Each stringCategory As String In categoryList
            comboBoxElementType.Items.Add(stringCategory)
        Next
    End Sub


    Private Sub SaveSettings()
        m_Settings.MathCalcElementType = comboBoxElementType.Text
        m_Settings.MathCalcProcessRadio = GetRadioButton(groupBoxElementsToProcess)
        m_Settings.MathCalcParameterA = textBoxParameterA.Text
        m_Settings.MathCalcParameterB = textBoxParameterB.Text
        m_Settings.MathCalcParameterC = textBoxParameterC.Text
        m_Settings.MathCalcConstantA = textBoxConstantA.Text
        m_Settings.MathCalcConstantB = textBoxConstantB.Text
        m_Settings.MathCalcConstantC = textBoxConstantC.Text
        m_Settings.MathCalcGroupBy = textBoxGroupBy.Text
        m_Settings.MathCalcTarget = textBoxTarget.Text
        m_Settings.MathCalcFunctionRadio = GetRadioButton(groupBoxFunction)
        m_Settings.MathCalcFormulaInclude1 = GetCheckBoxValue(checkBoxFormulaInclude1)
        m_Settings.MathCalcFormulaInclude2 = GetCheckBoxValue(checkBoxFormulaInclude2)
        m_Settings.MathCalcFormulaInclude3 = GetCheckBoxValue(checkBoxFormulaInclude3)
        m_Settings.MathCalcFormula1 = textBoxFormula1.Text
        m_Settings.MathCalcFormula2 = textBoxFormula2.Text
        m_Settings.MathCalcFormula3 = textBoxFormula3.Text
        m_Settings.WriteIni()
    End Sub

    Private Function InterpretFormulaString(ByVal source As String) As Boolean
        'Note that we do not actually evaluate the validity of the formula except for some syntax checking
        Dim substring As String
        Dim testString As String
        Dim position As Integer
        Dim lastNode As Boolean = False

        mFormula = ""
        mFormulaCode = ""
        mParameterNameA = ""
        mParameterNameB = ""
        mParameterNameC = ""
        mConstantValueA = ""
        mConstantValueB = ""
        mConstantValueC = ""
        mParameterNameGroupBy = ""
        mValueNeededA = False
        mValueNeededB = False
        mValueNeededC = False
        mValueNeededGroupBy = False
        mParameterExistsA = False
        mParameterExistsB = False
        mParameterExistsC = False
        mConstantExistsA = False
        mConstantExistsB = False
        mConstantExistsC = False
        mParameterExistsGroupBy = False
        mAggregation = False

        Try

            'Get the formula
            position = source.IndexOf("|")
            If position = -1 Then
                MessageBox.Show("Syntaxt error. Stopping process.", m_Settings.ProgramName)
                Return False
            End If
            substring = source.Substring(0, position).Trim()
            mFormula = substring.Substring(9)
            'Strip the "Formula: "
            mFormulaCode = ToggleFunctionModeTitle(mFormula)
            If mFormulaCode = "" Then
                MessageBox.Show("Syntaxt error. Stopping process.", m_Settings.ProgramName)
                Return False
            End If

            'Get the parameters
            For i As Integer = 1 To 5
                'not actually using the index, just looping 4 times
                source = source.Substring(position + 2)
                'Strip the "||"
                position = source.IndexOf("|")
                If position = -1 Then
                    substring = source.Substring(1)
                    lastNode = True
                Else
                    substring = source.Substring(1, position - 1)
                End If
                'note, don't trim!
                If substring.Length < 14 Then
                    MessageBox.Show("Syntaxt error. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If

                testString = substring.Substring(0, 12)
                Select Case testString
                    Case "Parameter A:"
                        mParameterNameA = substring.Substring(13).Trim()
                        mParameterExistsA = True
                        Exit Select
                    Case "Parameter B:"
                        mParameterNameB = substring.Substring(13).Trim()
                        mParameterExistsB = True
                        Exit Select
                    Case "Parameter C:"
                        mParameterNameB = substring.Substring(13).Trim()
                        mParameterExistsB = True
                        Exit Select
                    Case "Constant A: "
                        mConstantValueA = substring.Substring(12).Trim()
                        mConstantExistsA = True
                        Exit Select
                    Case "Constant B: "
                        mConstantValueB = substring.Substring(12).Trim()
                        mConstantExistsB = True
                        Exit Select
                    Case "Constant C: "
                        mConstantValueC = substring.Substring(12).Trim()
                        mConstantExistsC = True
                        Exit Select
                    Case "Group By Par"
                        mParameterNameGroupBy = substring.Substring(20).Trim()
                        mParameterExistsGroupBy = True
                        Exit Select
                    Case "Target Param"
                        mParameterNameTarget = substring.Substring(18).Trim()
                        Exit Select
                    Case Else
                        MessageBox.Show("Unknown case.")
                        Return False
                End Select
                If lastNode Then
                    Exit For
                End If
            Next
            Return True
        Catch
            Return False
        End Try
    End Function

    Private Function SaveFormula(ByVal indexFormula As Integer) As Boolean
        Dim buildFormula As String

        'Check that user has input logical values
        If Not ValidateInput(False) Then
            Return False
        End If

        'Start string with formula then add parameters
        buildFormula = "Formula: " & mFormula
        If mValueNeededA Then
            If mParameterExistsA Then
                buildFormula = (buildFormula & " || Parameter A: ") + textBoxParameterA.Text
            Else
                buildFormula = (buildFormula & " || Constant A: ") + textBoxConstantA.Text
            End If
        End If
        If mValueNeededB Then
            If mParameterExistsB Then
                buildFormula = (buildFormula & " || Parameter B: ") + textBoxParameterB.Text
            Else
                buildFormula = (buildFormula & " || Constant B: ") + textBoxConstantB.Text
            End If
        End If
        If mValueNeededC Then
            If mParameterExistsC Then
                buildFormula = (buildFormula & " || Parameter C: ") + textBoxParameterC.Text
            Else
                buildFormula = (buildFormula & " || Constant C: ") + textBoxConstantC.Text
            End If
        End If
        If mValueNeededGroupBy Then
            buildFormula = (buildFormula & " || Group By Parameter: ") + textBoxGroupBy.Text
        End If
        buildFormula = (buildFormula & " || Target Parameter: ") + textBoxTarget.Text

        'Write the formula to the corresponding text box
        Select Case indexFormula
            Case 1
                textBoxFormula1.Text = buildFormula
                Exit Select
            Case 2
                textBoxFormula2.Text = buildFormula
                Exit Select
            Case 3
                textBoxFormula3.Text = buildFormula
                Exit Select
            Case Else
                MessageBox.Show("Unknown case.")
                Return False
        End Select
        Return True
    End Function

    Private Function EditFormula(ByVal indexFormula As Integer) As Boolean

        Select Case indexFormula
            Case 1
                If Not InterpretFormulaString(textBoxFormula1.Text) Then
                    textBoxFormula1.Text = ""
                    MessageBox.Show("Error interpreting formula 1.", m_Settings.ProgramName)
                    Return False
                End If
                Exit Select
            Case 2
                If Not InterpretFormulaString(textBoxFormula2.Text) Then
                    textBoxFormula2.Text = ""
                    MessageBox.Show("Error interpreting formula 2.", m_Settings.ProgramName)
                    Return False
                End If
                Exit Select
            Case 3
                If Not InterpretFormulaString(textBoxFormula3.Text) Then
                    MessageBox.Show("Error interpreting formula 3.", m_Settings.ProgramName)
                    textBoxFormula3.Text = ""
                    Return False
                End If
                Exit Select
            Case Else
                MessageBox.Show("Unknown case.")
                Return False
        End Select

        textBoxParameterA.Text = mParameterNameA
        textBoxConstantA.Text = mConstantValueA
        textBoxParameterB.Text = mParameterNameB
        textBoxConstantB.Text = mConstantValueB
        textBoxParameterC.Text = mParameterNameC
        textBoxConstantC.Text = mConstantValueC
        textBoxGroupBy.Text = mParameterNameGroupBy
        textBoxTarget.Text = mParameterNameTarget
        SetRadioButton(groupBoxFunction, "radioButton" & mFormulaCode)

        Return True
    End Function

    Private Function ValidateFormulaData(ByVal checkSelectionType As Boolean) As Boolean
        mValueNeededA = False
        mValueNeededB = False
        mValueNeededC = False
        mValueNeededGroupBy = False
        mAggregation = False

        'Check that both a parameter and a constant are not specified
        If mParameterExistsA AndAlso mConstantExistsA Then
            MessageBox.Show("A paramaer name and a constant value cannot both be provided for Source Value A. Stopping process.", m_Settings.ProgramName)
            Return False
        End If
        If mParameterExistsB AndAlso mConstantExistsB Then
            MessageBox.Show("A paramaer name and a constant value cannot both be provided for Source Value B. Stopping process.", m_Settings.ProgramName)
            Return False
        End If
        If mParameterExistsC AndAlso mConstantExistsC Then
            MessageBox.Show("A paramaer name and a constant value cannot both be provided for Source Value C. Stopping process.", m_Settings.ProgramName)
            Return False
        End If

        Select Case mFormulaCode
            Case "SumA"
                mValueNeededA = True
                If Not (mParameterExistsA OrElse mConstantExistsA) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'A'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                mAggregation = True
                Exit Select
            Case "SumAGroupBy"
                mValueNeededA = True
                mValueNeededGroupBy = True
                If Not (mParameterExistsGroupBy) Then
                    MessageBox.Show("A 'Group By' parameter name must be provided. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                If Not (mParameterExistsA OrElse mConstantExistsA) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'A'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                mAggregation = True
                Exit Select

            Case "AddAB", "SubAB", "MultAB", "DivAB", "SubABDivA", "SubABDivB"
                mValueNeededA = True
                mValueNeededB = True
                If Not (mParameterExistsA OrElse mConstantExistsA) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'A'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                If Not (mParameterExistsB OrElse mConstantExistsB) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'B'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                Exit Select

            Case "AddABC", "AddABSubC", "SubABC", "MultABC", "AddABDivC", "SubABDivC", _
            "MultABDivC", "DivABMultC", "DivABDivC"
                mValueNeededA = True
                mValueNeededB = True
                mValueNeededC = True
                If Not (mParameterExistsA OrElse mConstantExistsA) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'A'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                If Not (mParameterExistsB OrElse mConstantExistsB) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'B'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                If Not (mParameterExistsC OrElse mConstantExistsC) Then
                    MessageBox.Show("Either a parameter or a constant must be provided for source value 'C'. Stopping process.", m_Settings.ProgramName)
                    Return False
                End If
                Exit Select
            Case Else
                MessageBox.Show("Case not implemented yet.")
                Return False
        End Select

        'Check that a target parameter is provided
        If textBoxTarget.Text.Trim() = "" Then
            MessageBox.Show("A target parameter must be provided. Stopping process.", m_Settings.ProgramName)
            Return False
        End If

        'Set up for aggreation cases
        If mFormulaCode = "SumA" OrElse mFormulaCode = "SumAGroupBy" Then
            mAggregation = True
        End If
        If checkSelectionType Then
            If radioButtonProcessSelection.Checked AndAlso mAggregation Then
                MessageBox.Show("'Sum' functions cannot be run on a pre-selection since this would give unpredictable results. Stoppping process.", m_Settings.ProgramName)
                Return False
            End If
        End If

        'If here then OK
        Return True
    End Function

    Private Function ValidateInput(ByVal checkSelectionType As Boolean) As Boolean
        Dim radioButton As RadioButton
        Dim groupBox As GroupBox = DirectCast(Me.Controls("groupBoxFunction"), GroupBox)

        mParameterExistsA = False
        mParameterExistsB = False
        mParameterExistsC = False
        mConstantExistsA = False
        mConstantExistsB = False
        mConstantExistsC = False
        mParameterExistsGroupBy = False

        'Determine which parameters and constants are being used
        If textBoxParameterA.Text.Trim() <> "" Then
            mParameterExistsA = True
        Else
            mParameterExistsA = False
        End If
        If textBoxParameterB.Text.Trim() <> "" Then
            mParameterExistsB = True
        Else
            mParameterExistsB = False
        End If
        If textBoxParameterC.Text.Trim() <> "" Then
            mParameterExistsC = True
        Else
            mParameterExistsC = False
        End If
        If textBoxConstantA.Text.Trim() <> "" Then
            mConstantExistsA = True
        Else
            mConstantExistsA = False
        End If
        If textBoxConstantB.Text.Trim() <> "" Then
            mConstantExistsB = True
        Else
            mConstantExistsB = False
        End If
        If textBoxConstantC.Text.Trim() <> "" Then
            mConstantExistsC = True
        Else
            mConstantExistsC = False
        End If
        If textBoxGroupBy.Text.Trim() <> "" Then
            mParameterExistsGroupBy = True
        Else
            mParameterExistsGroupBy = False
        End If

        'Read radio buttons and get the formula we are going to use
        '!!!!Note that we are using a rigid convention for encoding the function in the radio button name
        For Each controlTest As System.Windows.Forms.Control In groupBox.Controls
            If controlTest.Name.Substring(0, 11) = "radioButton" Then
                radioButton = DirectCast(controlTest, RadioButton)
                If radioButton.Checked Then
                    mFormulaCode = radioButton.Name.Substring(11)
                    Exit For
                End If
            End If
        Next
        If mFormulaCode = "" Then
            MessageBox.Show("Failed to get 'mFormulaCode' value", m_Settings.ProgramName)
            Return False
        End If
        mFormula = ToggleFunctionModeTitle(mFormulaCode)

        'Check the actual data
        If Not ValidateFormulaData(False) Then
            Return False
        End If
        'parameter is false since we don't want to check if the selection set
        'is invalid for aggreagte at this time.
        'If here then OK
        Return True
    End Function

    Private Function ProcessFormulasAll() As Boolean
        Dim categoryFound As Boolean = False

        'Make sure that category selecttion is valid
        For Each categoryTest As Category In m_Settings.Document.Settings.Categories
            If categoryTest.Name = comboBoxElementType.Text Then
                mCategory = categoryTest
                categoryFound = True
                Exit For
            End If
        Next
        If Not categoryFound Then
            MessageBox.Show("Missing or invalid Element Type. Stoppping process.", m_Settings.ProgramName)
            Return False
        End If

        ' New filtered element collector by category, category must be an actual Category
        Dim filCollector As New FilteredElementCollector(m_Settings.Document)
        filCollector.OfCategory(mCategory.Id.IntegerValue)
        mElementsModel = filCollector.ToElements

        ' '' '' Old Way Removed...
        '' ''Dim filter As Filter
        '' ''filter = mSettings.Application.Create.Filter.NewCategoryFilter(mCategory)
        '' ''mSettings.Application.ActiveDocument.get_Elements(filter, mElementsModel)

        If radioButtonProcessSelection.Checked Then
            If m_Settings.ElementsPreSelected.IsEmpty Then
                MessageBox.Show("No elements were selected in Revit prior to running command." & vbLf & "Pre-select elements and rerun this command, or use the 'All Elements in Model' option.", m_Settings.ProgramName)
                Return False
            End If
            For Each element As Element In mElementsModel
                If m_Settings.ElementsPreSelected.Contains(element) Then
                    mElementsSelected.Add(element)
                End If
            Next
            If mElementsSelected.Count = 0 Then
                MessageBox.Show("None of the pre-selected elements were of type: " & comboBoxElementType.Text & "." & vbLf & "Re-select elements and rerun this command, or use the 'All Elements in Model' option.", m_Settings.ProgramName)
                Return False

            End If
        Else
            mElementsSelected = mElementsModel
        End If

        'process the formulas
        If checkBoxFormulaInclude1.Checked Then
            If textBoxFormula1.Text.Trim() = "" Then
                MessageBox.Show("No formula given for 'Formula 1'. Halting process.", m_Settings.ProgramName)
                Return False
            Else
                If Not InterpretFormulaString(textBoxFormula1.Text) Then
                    Return False
                End If
                If Not ValidateFormulaData(True) Then
                    Return False
                End If
                'parameter is true since we do want to check if the selection set
                If Not ProcessFormulaOne() Then
                    Return False
                    'is invalid for aggregate at this time.
                End If
            End If
        End If
        If checkBoxFormulaInclude2.Checked Then
            If textBoxFormula1.Text.Trim() = "" Then
                MessageBox.Show("No formula given for 'Formula 2'. Halting process.", m_Settings.ProgramName)
                Return False
            Else
                If Not InterpretFormulaString(textBoxFormula2.Text) Then
                    Return False
                End If
                If Not ValidateFormulaData(True) Then
                    Return False
                End If
                'parameter is true since we do want to check if the selection set
                If Not ProcessFormulaOne() Then
                    Return False
                    'is invalid for aggregate at this time.
                End If
            End If
        End If
        If checkBoxFormulaInclude3.Checked Then
            If textBoxFormula1.Text.Trim() = "" Then
                MessageBox.Show("No formula given for 'Formula 3'. Halting process.", m_Settings.ProgramName)
                Return False
            Else
                If Not InterpretFormulaString(textBoxFormula3.Text) Then
                    Return False
                End If
                If Not ValidateFormulaData(True) Then
                    Return False
                End If
                'parameter is true since we do want to check if the selection set
                If Not ProcessFormulaOne() Then
                    Return False
                    'is invalid for aggregate at this time.
                End If
            End If
        End If

        'If here then all successful
        Return True
    End Function

    Private Function ProcessFormulaOne() As Boolean
        Dim progressBarForm As form_ParamProgressBar

        Dim valueA As Double = 0
        Dim valueB As Double = 0
        Dim valueC As Double = 0
        Dim valueResult As Double = 0

        Dim valueGroupBy As String = ""
        'Used when summation is grouped by B.
        Dim noGroupByValue As Boolean = True


        'case where group by parameter is empty for that particular instancew
        Dim sumValues As New Dictionary(Of String, Double)()
        Dim firstElement As Boolean = True

        'Start the progress bar
        If mAggregation Then
            'double count since we have to process each entity twice
            progressBarForm = New form_ParamProgressBar(("Processing " & (mElementsSelected.Count * 2).ToString & " ") + comboBoxElementType.Text & ".", (mElementsSelected.Count * 2) + 1)
        Else
            progressBarForm = New form_ParamProgressBar(("Processing " & mElementsSelected.Count.ToString & " ") + comboBoxElementType.Text & ".", mElementsSelected.Count + 1)
        End If
        progressBarForm.Show()
        progressBarForm.Increment()
        'To avoid transparent look while waiting
        'Get the constant values
        If mValueNeededA AndAlso mConstantExistsA Then
            Try
                valueA = Convert.ToDouble(mConstantValueA)
            Catch exception As Exception
                MessageBox.Show(("Error reading constant value 'A'. Stopping process." & vbLf & vbLf & "System message: ") + exception.Message, m_Settings.ProgramName)
                progressBarForm.Close()
                Return False
            End Try
        End If
        If mValueNeededB AndAlso mConstantExistsB Then
            Try
                valueB = Convert.ToDouble(mConstantValueB)
            Catch exception As Exception
                MessageBox.Show(("Error reading constant value 'B'. Stopping process." & vbLf & vbLf & "System message: ") + exception.Message, m_Settings.ProgramName)
                progressBarForm.Close()
                Return False
            End Try
        End If
        If mValueNeededC AndAlso mConstantExistsC Then
            Try
                valueC = Convert.ToDouble(mConstantValueC)
            Catch exception As Exception
                MessageBox.Show(("Error reading constant value 'C'. Stopping process." & vbLf & vbLf & "System message: ") + exception.Message, m_Settings.ProgramName)
                progressBarForm.Close()
                Return False
            End Try
        End If

        For Each element As Element In mElementsSelected

            'Check the first element for valid parameters.
            If firstElement Then
                If Not ValidateParameters(element) Then
                    progressBarForm.Close()
                    Return False
                End If
                firstElement = False
            End If

            'Get the numerical values from the parameters
            Try
                If mValueNeededA AndAlso mParameterExistsA Then
                    If Not GetParameterValue(element, mParameterNameA, valueA) Then
                        MessageBox.Show("Error getting parameter value 'A'. Stopping process.", m_Settings.ProgramName)
                        progressBarForm.Close()
                        Return False
                    End If
                End If
                If mValueNeededB AndAlso mParameterExistsB Then
                    If Not GetParameterValue(element, mParameterNameB, valueB) Then
                        MessageBox.Show("Error getting parameter value 'B'. Stopping process.", m_Settings.ProgramName)
                        progressBarForm.Close()
                        Return False
                    End If
                End If
                If mValueNeededC AndAlso mParameterExistsC Then
                    If Not GetParameterValue(element, mParameterNameC, valueC) Then
                        MessageBox.Show("Error getting parameter value 'C'. Stopping process.", m_Settings.ProgramName)
                        progressBarForm.Close()
                        Return False
                    End If
                End If
                If mFormulaCode = "SumAGroupBy" Then
                    noGroupByValue = False
                    If Not GetParameterValue(element, mParameterNameGroupBy, valueGroupBy) Then
                        MessageBox.Show("Error getting parameter value 'Group By'. Stopping process.", m_Settings.ProgramName)
                        progressBarForm.Close()
                    End If
                    valueGroupBy = valueGroupBy.Trim()
                    If valueGroupBy = "" Then
                        'This is not an error; it mans that this instance doesn't have any value in the group-by paramater field
                        noGroupByValue = True
                    End If
                End If
            Catch exception As Exception
                MessageBox.Show(("Error getting parameter value. Stopping process." & vbLf & vbLf & "System message: ") + exception.Message, m_Settings.ProgramName)
                progressBarForm.Close()
                Return False
            End Try

            'Process equation
            Select Case mFormulaCode
                Case "AddAB"
                    valueResult = valueA + valueB
                    Exit Select
                Case "SubAB"
                    valueResult = valueA - valueB
                    Exit Select
                Case "MultAB"
                    valueResult = valueA * valueB
                    Exit Select
                Case "DivAB"
                    If valueB = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for B encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = valueA / valueB
                    Exit Select
                Case "SubABDivA"
                    If valueA = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for B encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA - valueB) / valueA
                    Exit Select
                Case "SubABDivB"
                    If valueB = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for B encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA - valueB) / valueB
                    Exit Select
                Case "SumA"
                    valueResult = valueResult + valueA
                    Exit Select
                Case "SumAGroupBy"
                    If noGroupByValue Then
                        Exit Select
                    End If
                    If sumValues.ContainsKey(valueGroupBy) Then
                        sumValues(valueGroupBy) = sumValues(valueGroupBy) + valueA
                    Else
                        sumValues.Add(valueGroupBy, valueA)
                    End If
                    Exit Select
                Case "AddABC"
                    valueResult = valueA + valueB + valueC
                    Exit Select
                Case "AddABSubC"
                    valueResult = (valueA + valueB) - valueC
                    Exit Select
                Case "SubABC"
                    valueResult = (valueA - valueB) - valueC
                    Exit Select
                Case "MultABC"
                    valueResult = valueA * valueB * valueC
                    Exit Select
                Case "AddABDivC"
                    If valueC = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for C encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA + valueB) / valueC
                    Exit Select
                Case "SubABDivC"
                    If valueC = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for C encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA - valueB) / valueC
                    Exit Select
                Case "MultABDivC"
                    If valueC = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for C encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA * valueB) / valueC
                    Exit Select

                Case "DivABMultC"
                    If valueB = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for C encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA / valueB) * valueC
                    Exit Select
                Case "DivABDivC"
                    If valueB = 0 OrElse valueC = 0 Then
                        valueResult = 0
                        'MessageBox.Show("Zero value for C encountered. Stopping process.", mSettings.ProgramName);
                        'return false;
                        Exit Select
                    End If
                    valueResult = (valueA / valueB) / valueC
                    Exit Select
                Case Else
                    MessageBox.Show("Case not implemented yet.")
                    progressBarForm.Close()
                    Return False
            End Select

            'Set the target values except in aggregate cases
            If Not mAggregation Then
                SetParameterValue(element, mParameterNameTarget, valueResult)
            End If

            'Increment the Progress Bar

            progressBarForm.Increment()
        Next

        'Write the aggregate values
        If mAggregation Then
            For Each element As Element In mElementsSelected
                Select Case mFormulaCode
                    Case "SumA"
                        SetParameterValue(element, mParameterNameTarget, valueResult)
                        Exit Select
                    Case "SumAGroupBy"
                        If GetParameterValue(element, mParameterNameGroupBy, valueGroupBy) Then
                            If sumValues.ContainsKey(valueGroupBy) Then
                                SetParameterValue(element, mParameterNameTarget, sumValues(valueGroupBy))
                            Else
                                SetParameterValue(element, mParameterNameTarget, 0)
                            End If
                        End If
                        Exit Select
                    Case Else
                        MessageBox.Show("Case not implemented yet.")
                        progressBarForm.Close()
                        Return False

                End Select
                'Increment the Progress Bar
                progressBarForm.Increment()
            Next
        End If

        'Close the progress bar.
        progressBarForm.Close()

        Return True
    End Function
#End Region

#Region "************************************************ Event Handlers *****************************************************"

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.Close()
    End Sub

    Private Sub buttonProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonProcess.Click
        Dim m_Trans As New Transaction(m_Settings.Document, "HOK Parameter Calculations")
        m_Trans.Start()
        ProcessFormulasAll()
        m_Trans.Commit()
    End Sub

    Private Sub buttonFormulaSave1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonFormulaSave1.Click
        SaveFormula(1)
    End Sub
    Private Sub buttonFormulaSave2_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonFormulaSave2.Click
        SaveFormula(2)
    End Sub
    Private Sub buttonFormulaSave3_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonFormulaSave3.Click
        SaveFormula(3)
    End Sub

    Private Sub buttonFormulaEdit1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonFormulaEdit1.Click
        EditFormula(1)
    End Sub
    Private Sub buttonFormulaEdit2_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonFormulaEdit2.Click
        EditFormula(2)
    End Sub
    Private Sub buttonFormulaEdit3_Click(ByVal sender As Object, ByVal e As EventArgs) Handles buttonFormulaEdit3.Click
        EditFormula(3)
    End Sub

#End Region

    Private Function GetRadioButton(ByVal groupBox As GroupBox) As String
        Dim radioButton As RadioButton
        For Each controlTest As System.Windows.Forms.Control In groupBox.Controls
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
        For Each controlTest As System.Windows.Forms.Control In groupBox.Controls
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

    Private Function ToggleFunctionModeTitle(ByVal input As String) As String
        Select Case input
            Case "AddAB"
                Return "A + B"
            Case "SubAB"
                Return "A - B"
            Case "MultAB"
                Return "A * B"
            Case "DivAB"
                Return "A / B"
            Case "SubABDivA"
                Return "(A - B) / A"
            Case "SubABDivB"
                Return "(A - B) / B"
            Case "AddABC"
                Return "A + B + C"
            Case "AddABSubC"
                Return "(A + B) - C"
            Case "SubABC"
                Return "(A - B) - C"
            Case "MultABC"
                Return "A * B * C"
            Case "AddABDivC"
                Return "(A + B) / C"
            Case "SubABDivC"
                Return "(A - B) / C"
            Case "MultABDivC"
                Return "(A * B) / C"
            Case "DivABMultC"
                Return "(A / B) * C"
            Case "DivABDivC"
                Return "(A / B) / C"
            Case "SumA"
                Return "Sum(A)"
            Case "SumAGroupBy"
                Return "Sum(A) Group-by"

            Case "A + B"
                Return "AddAB"
            Case "A - B"
                Return "SubAB"
            Case "A * B"
                Return "MultAB"
            Case "A / B"
                Return "DivAB"
            Case "(A - B) / A"
                Return "SubABDivA"
            Case "(A - B) / B"
                Return "SubABDivB"
            Case "A + B + C"
                Return "AddABC"
            Case "(A + B) - C"
                Return "AddABSubC"
            Case "(A - B) - C"
                Return "SubABC"
            Case "A * B * C"
                Return "MultABC"
            Case "(A + B) / C"
                Return "AddABDivC"
            Case "(A - B) / C"
                Return "SubABDivC"
            Case "(A * B) / C"
                Return "MultABDivC"
            Case "(A / B) * C"
                Return "DivABMultC"
            Case "(A / B) / C"
                Return "DivABDivC"
            Case "Sum(A)"
                Return "SumA"
            Case "Sum(A) Group-by"
                Return "SumAGroupBy"
            Case Else

                MessageBox.Show("Case not implemented yet.")
                Return ""
        End Select
    End Function

End Class