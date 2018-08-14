Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

Imports System.Windows.Forms

Public Class clsSettings

    Private Const cProgramName As String = "ParameterTools"
    Private Const cIniFileName As String = "ParameterTools.ini"

    Private m_CategoryParameters As New List(Of String)
    Private m_ParamList As New List(Of String)

    'Local usage
    Private m_CmdData As ExternalCommandData
    Private m_App As UIApplication
    Private m_Doc As Document
    Private mActiveView As Autodesk.Revit.DB.View
    Private m_ActiveViewPlan As ViewPlan
    Private m_CurrentLevel As Level
    Private m_CurrentPhase As Phase
    Private m_CurrentViewIsPlan As Boolean
    Private mProjectFolderPath As String
    Private mElementsPreSelected As ElementSet

    Private mUtilityIni As clsUtilityIni
    Private mListSettings As New List(Of String)()
    Private mIniPath As String

    'Managed with INI file (note that all are strings with no validation)

    'Strings Calculation and Concatenation
    Private mStringCalcElementType As String
    'Type of Revit element to be updated
    Private mStringCalcProcessRadio As String
    'Group box groupBoxElementsToProcess selection
    Private mStringCalcNode1Radio As String
    'Group box radio button set ("Not Used", "Constant", etc.)
    Private mStringCalcNode1Constant As String
    'String value to use as constant
    Private mStringCalcNode1Source As String
    'Parameter name for source value
    Private mStringCalcNode1Target As String
    'Parameter name for target value
    Private mStringCalcNode1Include As String
    Private mStringCalcNode2Radio As String
    Private mStringCalcNode2Constant As String
    Private mStringCalcNode2Source As String
    Private mStringCalcNode2Target As String
    Private mStringCalcNode2Include As String
    Private mStringCalcNode3Radio As String
    Private mStringCalcNode3Constant As String
    Private mStringCalcNode3Source As String
    Private mStringCalcNode3Target As String
    Private mStringCalcNode3Include As String
    Private mStringCalcNode4Radio As String
    Private mStringCalcNode4Constant As String
    Private mStringCalcNode4Source As String
    Private mStringCalcNode4Target As String
    Private mStringCalcNode4Include As String
    Private mStringCalcNode5Radio As String
    Private mStringCalcNode5Constant As String
    Private mStringCalcNode5Source As String
    Private mStringCalcNode5Target As String
    Private mStringCalcNode5Include As String
    Private mStringCalcConcatTarget As String

    'Parameter to be updated with concatenated string
    Private mMathCalcElementType As String

    'Type of Revit element to be updated
    Private mMathCalcProcessRadio As String
    Private mMathCalcParameterA As String
    Private mMathCalcParameterB As String
    Private mMathCalcParameterC As String
    Private mMathCalcConstantA As String
    Private mMathCalcConstantB As String
    Private mMathCalcConstantC As String
    Private mMathCalcGroupBy As String
    Private mMathCalcTarget As String
    Private mMathCalcFunctionRadio As String
    Private mMathCalcFormulaInclude1 As String
    Private mMathCalcFormulaInclude2 As String
    Private mMathCalcFormulaInclude3 As String
    Private mMathCalcFormula1 As String
    Private mMathCalcFormula2 As String
    Private mMathCalcFormula3 As String

    Private mElementParentCatParent As String
    Private mElementParentParamParentKey As String
    Private mElementParentCatChild As String
    Private mElementParentParamChildKey As String

    Private mElementRollUpCatParent As String
    Private mElementRollUpParamParentKey As String
    Private mElementRollUpParamRollUp As String
    Private mElementRollUpCatChild As String
    Private mElementRollUpParamChildKey As String
    Private mElementRollUpParamSource As String

    Private mExternalWriteElementType As String 'Category of Revit element to be updated
    Private mExternalWriteProcessRadio As String 'Group box groupBoxElementsToProcess selection (pre-selected/all)
    Private mExternalWriteParametersRadio As String 'Group box groupBoxParameters (type/instance/both)
    Private mExternalKeyPath As String 'Path to Excel file
    Private mExternalKeyParamKey1 As String 'Parameter name of key 1
    Private mExternalKeyParamList1 As String 'List with ";" separators of dependent parameter names for key 1
    Private mExternalKeyParamKey2 As String 'Parameter name of key 2
    Private mExternalKeyParamList2 As String 'List with ";" separators of dependent parameter names for key 2
    Private mExternalKeyParamKey3 As String 'Parameter name of key 3
    Private mExternalKeyParamList3 As String 'List with ";" separators of dependent parameter names for key 3

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="commandData"></param>
    ''' <param name="message"></param>
    ''' <param name="elementSet"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal commandData As ExternalCommandData, ByRef message As String, ByVal elementSet As ElementSet)
        Try
            m_CmdData = commandData
            RefreshRevitValues()
            mUtilityIni = New clsUtilityIni(cIniFileName)
            mProjectFolderPath = CalculateProjectFolderPath()
            If mProjectFolderPath = "" Then
                mIniPath = ""
            Else
                mIniPath = (mProjectFolderPath & "\") + cIniFileName
            End If

            ' This is required to access the preselected elements in the model viewer
            Dim uiDoc As UIDocument = commandData.Application.ActiveUIDocument
#If RELEASE2013 Or RELEASE2014 Then
            mElementsPreSelected = uiDoc.Selection.Elements
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
            For Each elementId As ElementId In uiDoc.Selection.GetElementIds
                Dim element As Element = m_Doc.GetElement(elementId)
                mElementsPreSelected.Insert(element)
            Next
#End If

                ParametersBoundByCategory()
                RefreshIniValues()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "clsSettings:Constructor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
        
    End Sub

    ''' <summary>
    ''' Gather a list of custom project and shared parameters by category
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ParametersBoundByCategory() As Boolean
        Dim iterCategory As Category
        Try
            ' Binding Map
            Dim bindingsMap As BindingMap
            bindingsMap = m_Doc.ParameterBindings
            ' Definition Map
            Dim iterator As DefinitionBindingMapIterator
            iterator = bindingsMap.ForwardIterator
            ' Cycle the whole binding map
            Do While (iterator.MoveNext)
                ' Get the binding object
                Dim elementBinding As ElementBinding
                elementBinding = iterator.Current
                ' get the name of the parameter 
                Dim definition As Definition
                definition = iterator.Key
                m_ParamList.Add(definition.Name)
                ' add the category name.  
                If (Not elementBinding Is Nothing) Then
                    Dim categories As CategorySet
                    categories = elementBinding.Categories

                    If (categories.Size > 0) Then
                        ' Get all parameters for the category
                        For Each category As Category In categories
                            iterCategory = category
                            If (Not category Is Nothing And category.AllowsBoundParameters = True) Then 'And category.Name.ToUpper <> "DETAIL ITEMS"
                                m_CategoryParameters.Add(category.Name & vbTab & definition.Name)
                            End If
                        Next
                    End If
                End If
            Loop
            ' Alpha sort the category list
            m_CategoryParameters.Sort()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "clsSettings:ParametersBoundByCategory", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try

    End Function

    ''' <summary>
    ''' Gets Revit settings for document, view, etc; 
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub RefreshRevitValues()
        Try
            m_App = m_CmdData.Application
            m_Doc = m_App.ActiveUIDocument.Document
            mActiveView = m_Doc.ActiveView
            m_CurrentViewIsPlan = True
            Try
                m_ActiveViewPlan = DirectCast(mActiveView, ViewPlan)
            Catch
                m_CurrentViewIsPlan = False
            End Try
            m_CurrentLevel = mActiveView.GenLevel
            Dim parameter As Parameter = mActiveView.Parameter(BuiltInParameter.VIEW_PHASE)
            Dim elementId As ElementId = parameter.AsElementId()
            m_CurrentPhase = TryCast(m_Doc.GetElement(elementId), Phase)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "clsSettings:RefreshRevitValues", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
        
    End Sub

    ''' <summary>
    ''' Read the INI file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub RefreshIniValues()
        Try
            'reads ini if possible; creates ini and sets defaults if necessary.
            If mUtilityIni.ReadIniFile(mIniPath, mListSettings) Then
                ReadListSettings()
            Else
                WriteDefaultIniValues()
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "clsSettings:RefreshIniValues", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
        
    End Sub

    ''' <summary>
    ''' Where is the project model saved?
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CalculateProjectFolderPath() As String
        Dim path As String = ""
        Try
            path = m_Doc.PathName
            If path <> "" Then
                Dim pos As Integer = path.LastIndexOf("\")
                path = path.Substring(0, pos)
            End If
        Catch
            path = ""
        End Try
        Return path
    End Function

    ''' <summary>
    ''' Refresh Everything
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Refresh()
        RefreshRevitValues()
        RefreshIniValues()
    End Sub

    ''' <summary>
    ''' Save and update the INI file
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WriteIni()
        If mIniPath = "" Then
            MessageBox.Show("Note: Settings cannot be saved until project is saved.", cProgramName)
            Exit Sub
        End If
        WriteListSettings()
        mUtilityIni.WriteIniFile(mIniPath, mListSettings)
    End Sub

    ''' <summary>
    ''' Read the contents of the INI file
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReadIni()
        mUtilityIni.ReadIniFile(mIniPath, mListSettings)
        ReadListSettings()
    End Sub

    ''' <summary>
    ''' Reset to defaults
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ReloadDefaults()
        WriteDefaultIniValues()
        mUtilityIni.ReadIniFile(mIniPath, mListSettings)
        ReadListSettings()
    End Sub

#Region "INI File Related"

    Private Sub WriteDefaultIniValues()
        'These are the hard-coded defaults
        mStringCalcElementType = ""
        mStringCalcProcessRadio = "radioButtonProcessSelection"
        mStringCalcNode1Radio = "radioButtonNode1NotUsed"
        mStringCalcNode1Constant = ""
        mStringCalcNode1Source = ""
        mStringCalcNode1Target = ""
        mStringCalcNode1Include = "false"
        mStringCalcNode2Radio = "radioButtonNode2NotUsed"
        mStringCalcNode2Constant = ""
        mStringCalcNode2Source = ""
        mStringCalcNode2Target = ""
        mStringCalcNode2Include = "false"
        mStringCalcNode3Radio = "radioButtonNode3NotUsed"
        mStringCalcNode3Constant = ""
        mStringCalcNode3Source = ""
        mStringCalcNode3Target = ""
        mStringCalcNode3Include = "false"
        mStringCalcNode4Radio = "radioButtonNode4NotUsed"
        mStringCalcNode4Constant = ""
        mStringCalcNode4Source = ""
        mStringCalcNode4Target = ""
        mStringCalcNode4Include = "false"
        mStringCalcNode5Radio = "radioButtonNode5NotUsed"
        mStringCalcNode5Constant = ""
        mStringCalcNode5Source = ""
        mStringCalcNode5Target = ""
        mStringCalcNode5Include = "false"
        mStringCalcConcatTarget = ""

        mMathCalcElementType = ""
        mMathCalcProcessRadio = "radioButtonProcessSelection"
        mMathCalcParameterA = ""
        mMathCalcParameterB = ""
        mMathCalcParameterC = ""
        mMathCalcConstantA = ""
        mMathCalcConstantB = ""
        mMathCalcConstantC = ""
        mMathCalcGroupBy = ""
        mMathCalcTarget = ""
        mMathCalcFunctionRadio = "radioButtonAddAB"
        mMathCalcFormulaInclude1 = "false"
        mMathCalcFormulaInclude2 = "false"
        mMathCalcFormulaInclude3 = "false"
        mMathCalcFormula1 = ""
        mMathCalcFormula2 = ""
        mMathCalcFormula3 = ""

        mElementParentCatParent = "Areas"
        mElementParentParamParentKey = "AreaId"
        mElementParentCatChild = "Mass"
        mElementParentParamChildKey = "ParentAreaId"

        mElementRollUpCatParent = "Areas"
        mElementRollUpParamParentKey = "AreaId"
        mElementRollUpParamRollUp = "AreaSumChildFloorAreas"
        mElementRollUpCatChild = "Mass"
        mElementRollUpParamChildKey = "ParentAreaId"
        mElementRollUpParamSource = "Gross Floor Area"

        mExternalWriteElementType = ""
        mExternalWriteProcessRadio = "radioButtonProcessSelection"
        mExternalWriteParametersRadio = "radioButtonBoth"

        mExternalKeyPath = ""
        'mExternalKeyElementType = "";
        mExternalKeyParamKey1 = ""
        mExternalKeyParamList1 = ""
        mExternalKeyParamKey2 = ""
        mExternalKeyParamList2 = ""
        mExternalKeyParamKey3 = ""
        mExternalKeyParamList3 = ""

        WriteListSettings()
        mUtilityIni.WriteIniFile(mIniPath, mListSettings)
    End Sub

    Private Sub ReadListSettings()
        Try
            Dim i As Integer = 0

            mStringCalcElementType = mListSettings(i)
            i += 1
            mStringCalcProcessRadio = mListSettings(i)
            i += 1
            mStringCalcNode1Radio = mListSettings(i)
            i += 1
            mStringCalcNode1Constant = mListSettings(i)
            i += 1
            mStringCalcNode1Source = mListSettings(i)
            i += 1
            mStringCalcNode1Target = mListSettings(i)
            i += 1
            mStringCalcNode1Include = mListSettings(i)
            i += 1
            mStringCalcNode2Radio = mListSettings(i)
            i += 1
            mStringCalcNode2Constant = mListSettings(i)
            i += 1
            mStringCalcNode2Source = mListSettings(i)
            i += 1
            mStringCalcNode2Target = mListSettings(i)
            i += 1
            mStringCalcNode2Include = mListSettings(i)
            i += 1
            mStringCalcNode3Radio = mListSettings(i)
            i += 1
            mStringCalcNode3Constant = mListSettings(i)
            i += 1
            mStringCalcNode3Source = mListSettings(i)
            i += 1
            mStringCalcNode3Target = mListSettings(i)
            i += 1
            mStringCalcNode3Include = mListSettings(i)
            i += 1
            mStringCalcNode4Radio = mListSettings(i)
            i += 1
            mStringCalcNode4Constant = mListSettings(i)
            i += 1
            mStringCalcNode4Source = mListSettings(i)
            i += 1
            mStringCalcNode4Target = mListSettings(i)
            i += 1
            mStringCalcNode4Include = mListSettings(i)
            i += 1
            mStringCalcNode5Radio = mListSettings(i)
            i += 1
            mStringCalcNode5Constant = mListSettings(i)
            i += 1
            mStringCalcNode5Source = mListSettings(i)
            i += 1
            mStringCalcNode5Target = mListSettings(i)
            i += 1
            mStringCalcNode5Include = mListSettings(i)
            i += 1
            mStringCalcConcatTarget = mListSettings(i)
            i += 1

            mMathCalcElementType = mListSettings(i)
            i += 1
            mMathCalcProcessRadio = mListSettings(i)
            i += 1
            mMathCalcParameterA = mListSettings(i)
            i += 1
            mMathCalcParameterB = mListSettings(i)
            i += 1
            mMathCalcParameterC = mListSettings(i)
            i += 1
            mMathCalcConstantA = mListSettings(i)
            i += 1
            mMathCalcConstantB = mListSettings(i)
            i += 1
            mMathCalcConstantC = mListSettings(i)
            i += 1
            mMathCalcGroupBy = mListSettings(i)
            i += 1
            mMathCalcTarget = mListSettings(i)
            i += 1
            mMathCalcFunctionRadio = mListSettings(i)
            i += 1
            mMathCalcFormulaInclude1 = mListSettings(i)
            i += 1
            mMathCalcFormulaInclude2 = mListSettings(i)
            i += 1
            mMathCalcFormulaInclude3 = mListSettings(i)
            i += 1
            mMathCalcFormula1 = mListSettings(i)
            i += 1
            mMathCalcFormula2 = mListSettings(i)
            i += 1
            mMathCalcFormula3 = mListSettings(i)
            i += 1

            mElementParentCatParent = mListSettings(i)
            i += 1
            mElementParentParamParentKey = mListSettings(i)
            i += 1
            mElementParentCatChild = mListSettings(i)
            i += 1
            mElementParentParamChildKey = mListSettings(i)
            i += 1

            mElementRollUpCatParent = mListSettings(i)
            i += 1
            mElementRollUpParamParentKey = mListSettings(i)
            i += 1
            mElementRollUpParamRollUp = mListSettings(i)
            i += 1
            mElementRollUpCatChild = mListSettings(i)
            i += 1
            mElementRollUpParamChildKey = mListSettings(i)
            i += 1
            mElementRollUpParamSource = mListSettings(i)
            i += 1

            mExternalWriteElementType = mListSettings(i)
            i += 1
            mExternalWriteProcessRadio = mListSettings(i)
            i += 1
            mExternalWriteParametersRadio = mListSettings(i)
            i += 1

            mExternalKeyPath = mListSettings(i)
            i += 1
            'mExternalKeyElementType = mListSettings[i]; i++;
            mExternalKeyParamKey1 = mListSettings(i)
            i += 1
            mExternalKeyParamList1 = mListSettings(i)
            i += 1
            mExternalKeyParamKey2 = mListSettings(i)
            i += 1
            mExternalKeyParamList2 = mListSettings(i)
            i += 1
            mExternalKeyParamKey3 = mListSettings(i)
            i += 1
            mExternalKeyParamList3 = mListSettings(i)

            i += 1
        Catch
            'Assume failure is due to an out-of-date ini file so make a new one.
            WriteDefaultIniValues()
        End Try
    End Sub

    Private Sub WriteListSettings()
        mListSettings.Clear()

        mListSettings.Add(mStringCalcElementType)
        mListSettings.Add(mStringCalcProcessRadio)
        mListSettings.Add(mStringCalcNode1Radio)
        mListSettings.Add(mStringCalcNode1Constant)
        mListSettings.Add(mStringCalcNode1Source)
        mListSettings.Add(mStringCalcNode1Target)
        mListSettings.Add(mStringCalcNode1Include)
        mListSettings.Add(mStringCalcNode2Radio)
        mListSettings.Add(mStringCalcNode2Constant)
        mListSettings.Add(mStringCalcNode2Source)
        mListSettings.Add(mStringCalcNode2Target)
        mListSettings.Add(mStringCalcNode2Include)
        mListSettings.Add(mStringCalcNode3Radio)
        mListSettings.Add(mStringCalcNode3Constant)
        mListSettings.Add(mStringCalcNode3Source)
        mListSettings.Add(mStringCalcNode3Target)
        mListSettings.Add(mStringCalcNode3Include)
        mListSettings.Add(mStringCalcNode4Radio)
        mListSettings.Add(mStringCalcNode4Constant)
        mListSettings.Add(mStringCalcNode4Source)
        mListSettings.Add(mStringCalcNode4Target)
        mListSettings.Add(mStringCalcNode4Include)
        mListSettings.Add(mStringCalcNode5Radio)
        mListSettings.Add(mStringCalcNode5Constant)
        mListSettings.Add(mStringCalcNode5Source)
        mListSettings.Add(mStringCalcNode5Target)
        mListSettings.Add(mStringCalcNode5Include)
        mListSettings.Add(mStringCalcConcatTarget)

        mListSettings.Add(mMathCalcElementType)
        mListSettings.Add(mMathCalcProcessRadio)
        mListSettings.Add(mMathCalcParameterA)
        mListSettings.Add(mMathCalcParameterB)
        mListSettings.Add(mMathCalcParameterC)
        mListSettings.Add(mMathCalcConstantA)
        mListSettings.Add(mMathCalcConstantB)
        mListSettings.Add(mMathCalcConstantC)
        mListSettings.Add(mMathCalcGroupBy)
        mListSettings.Add(mMathCalcTarget)
        mListSettings.Add(mMathCalcFunctionRadio)
        mListSettings.Add(mMathCalcFormulaInclude1)
        mListSettings.Add(mMathCalcFormulaInclude2)
        mListSettings.Add(mMathCalcFormulaInclude3)
        mListSettings.Add(mMathCalcFormula1)
        mListSettings.Add(mMathCalcFormula2)
        mListSettings.Add(mMathCalcFormula3)

        mListSettings.Add(mElementParentCatParent)
        mListSettings.Add(mElementParentParamParentKey)
        mListSettings.Add(mElementParentCatChild)
        mListSettings.Add(mElementParentParamChildKey)

        mListSettings.Add(mElementRollUpCatParent)
        mListSettings.Add(mElementRollUpParamParentKey)
        mListSettings.Add(mElementRollUpParamRollUp)
        mListSettings.Add(mElementRollUpCatChild)
        mListSettings.Add(mElementRollUpParamChildKey)
        mListSettings.Add(mElementRollUpParamSource)

        mListSettings.Add(mExternalWriteElementType)
        mListSettings.Add(mExternalWriteProcessRadio)
        mListSettings.Add(mExternalWriteParametersRadio)

        mListSettings.Add(mExternalKeyPath)
        'mListSettings.Add(mExternalKeyElementType);
        mListSettings.Add(mExternalKeyParamKey1)
        mListSettings.Add(mExternalKeyParamList1)
        mListSettings.Add(mExternalKeyParamKey2)
        mListSettings.Add(mExternalKeyParamList2)
        mListSettings.Add(mExternalKeyParamKey3)


        mListSettings.Add(mExternalKeyParamList3)
    End Sub

#End Region

#Region "********************************************** Public Properties ****************************************************"
    Public ReadOnly Property ProgramName() As String
        Get
            Return cProgramName
        End Get
    End Property
    Public ReadOnly Property IniFileName() As String
        Get
            Return cIniFileName
        End Get
    End Property

    'Common Revit values
    Public ReadOnly Property Application() As UIApplication
        Get
            Return m_App
        End Get
    End Property
    Public ReadOnly Property Document() As Document
        Get
            Return m_Doc
        End Get
    End Property
    Public ReadOnly Property ActiveView() As Autodesk.Revit.DB.View
        Get
            Return mActiveView
        End Get
    End Property
    Public ReadOnly Property ActiveViewPlan() As ViewPlan
        Get
            Return m_ActiveViewPlan
        End Get
    End Property
    Public ReadOnly Property CurrentLevel() As Level
        Get
            Return m_CurrentLevel
        End Get
    End Property
    Public ReadOnly Property CurrentPhase() As Phase
        Get
            Return m_CurrentPhase
        End Get
    End Property
    Public ReadOnly Property CurrentViewIsPlan() As Boolean
        Get
            Return m_CurrentViewIsPlan
        End Get
    End Property
    Public ReadOnly Property ProjectFolderPath() As String
        Get
            Return mProjectFolderPath
        End Get
    End Property
    Public ReadOnly Property ElementsPreSelected() As ElementSet
        Get
            Return mElementsPreSelected
        End Get
    End Property

    'Values from dialog boxes
    Public Property StringCalcElementType() As String
        Get
            Return mStringCalcElementType
        End Get
        Set(ByVal value As String)
            mStringCalcElementType = value
        End Set
    End Property
    Public Property StringCalcProcessRadio() As String
        Get
            Return mStringCalcProcessRadio
        End Get
        Set(ByVal value As String)
            mStringCalcProcessRadio = value
        End Set
    End Property
    Public Property StringCalcNode1Radio() As String
        Get
            Return mStringCalcNode1Radio
        End Get
        Set(ByVal value As String)
            mStringCalcNode1Radio = value
        End Set
    End Property
    Public Property StringCalcNode1Constant() As String
        Get
            Return mStringCalcNode1Constant
        End Get
        Set(ByVal value As String)
            mStringCalcNode1Constant = value
        End Set
    End Property
    Public Property StringCalcNode1Source() As String
        Get
            Return mStringCalcNode1Source
        End Get
        Set(ByVal value As String)
            mStringCalcNode1Source = value
        End Set
    End Property
    Public Property StringCalcNode1Target() As String
        Get
            Return mStringCalcNode1Target
        End Get
        Set(ByVal value As String)
            mStringCalcNode1Target = value
        End Set
    End Property
    Public Property StringCalcNode1Include() As String
        Get
            Return mStringCalcNode1Include
        End Get
        Set(ByVal value As String)
            mStringCalcNode1Include = value
        End Set
    End Property
    Public Property StringCalcNode2Radio() As String
        Get
            Return mStringCalcNode2Radio
        End Get
        Set(ByVal value As String)
            mStringCalcNode2Radio = value
        End Set
    End Property
    Public Property StringCalcNode2Constant() As String
        Get
            Return mStringCalcNode2Constant
        End Get
        Set(ByVal value As String)
            mStringCalcNode2Constant = value
        End Set
    End Property
    Public Property StringCalcNode2Source() As String
        Get
            Return mStringCalcNode2Source
        End Get
        Set(ByVal value As String)
            mStringCalcNode2Source = value
        End Set
    End Property
    Public Property StringCalcNode2Target() As String
        Get
            Return mStringCalcNode2Target
        End Get
        Set(ByVal value As String)
            mStringCalcNode2Target = value
        End Set
    End Property
    Public Property StringCalcNode2Include() As String
        Get
            Return mStringCalcNode2Include
        End Get
        Set(ByVal value As String)
            mStringCalcNode2Include = value
        End Set
    End Property
    Public Property StringCalcNode3Radio() As String
        Get
            Return mStringCalcNode3Radio
        End Get
        Set(ByVal value As String)
            mStringCalcNode3Radio = value
        End Set
    End Property
    Public Property StringCalcNode3Constant() As String
        Get
            Return mStringCalcNode3Constant
        End Get
        Set(ByVal value As String)
            mStringCalcNode3Constant = value
        End Set
    End Property
    Public Property StringCalcNode3Source() As String
        Get
            Return mStringCalcNode3Source
        End Get
        Set(ByVal value As String)
            mStringCalcNode3Source = value
        End Set
    End Property
    Public Property StringCalcNode3Target() As String
        Get
            Return mStringCalcNode3Target
        End Get
        Set(ByVal value As String)
            mStringCalcNode3Target = value
        End Set
    End Property
    Public Property StringCalcNode3Include() As String
        Get
            Return mStringCalcNode3Include
        End Get
        Set(ByVal value As String)
            mStringCalcNode3Include = value
        End Set
    End Property
    Public Property StringCalcNode4Radio() As String
        Get
            Return mStringCalcNode4Radio
        End Get
        Set(ByVal value As String)
            mStringCalcNode4Radio = value
        End Set
    End Property
    Public Property StringCalcNode4Constant() As String
        Get
            Return mStringCalcNode4Constant
        End Get
        Set(ByVal value As String)
            mStringCalcNode4Constant = value
        End Set
    End Property
    Public Property StringCalcNode4Source() As String
        Get
            Return mStringCalcNode4Source
        End Get
        Set(ByVal value As String)
            mStringCalcNode4Source = value
        End Set
    End Property
    Public Property StringCalcNode4Target() As String
        Get
            Return mStringCalcNode4Target
        End Get
        Set(ByVal value As String)
            mStringCalcNode4Target = value
        End Set
    End Property
    Public Property StringCalcNode4Include() As String
        Get
            Return mStringCalcNode4Include
        End Get
        Set(ByVal value As String)
            mStringCalcNode4Include = value
        End Set
    End Property
    Public Property StringCalcNode5Radio() As String
        Get
            Return mStringCalcNode5Radio
        End Get
        Set(ByVal value As String)
            mStringCalcNode5Radio = value
        End Set
    End Property
    Public Property StringCalcNode5Constant() As String
        Get
            Return mStringCalcNode5Constant
        End Get
        Set(ByVal value As String)
            mStringCalcNode5Constant = value
        End Set
    End Property
    Public Property StringCalcNode5Source() As String
        Get
            Return mStringCalcNode5Source
        End Get
        Set(ByVal value As String)
            mStringCalcNode5Source = value
        End Set
    End Property
    Public Property StringCalcNode5Target() As String
        Get
            Return mStringCalcNode5Target
        End Get
        Set(ByVal value As String)
            mStringCalcNode5Target = value
        End Set
    End Property
    Public Property StringCalcNode5Include() As String
        Get
            Return mStringCalcNode5Include
        End Get
        Set(ByVal value As String)
            mStringCalcNode5Include = value
        End Set
    End Property
    Public Property StringCalcConcatTarget() As String
        Get
            Return mStringCalcConcatTarget
        End Get
        Set(ByVal value As String)
            mStringCalcConcatTarget = value
        End Set
    End Property

    Public Property MathCalcElementType() As String
        Get
            Return mMathCalcElementType
        End Get
        Set(ByVal value As String)
            mMathCalcElementType = value
        End Set
    End Property
    Public Property MathCalcProcessRadio() As String
        Get
            Return mMathCalcProcessRadio
        End Get
        Set(ByVal value As String)
            mMathCalcProcessRadio = value
        End Set
    End Property
    Public Property MathCalcParameterA() As String
        Get
            Return mMathCalcParameterA
        End Get
        Set(ByVal value As String)
            mMathCalcParameterA = value
        End Set
    End Property
    Public Property MathCalcParameterB() As String
        Get
            Return mMathCalcParameterB
        End Get
        Set(ByVal value As String)
            mMathCalcParameterB = value
        End Set
    End Property
    Public Property MathCalcParameterC() As String
        Get
            Return mMathCalcParameterC
        End Get
        Set(ByVal value As String)
            mMathCalcParameterC = value
        End Set
    End Property
    Public Property MathCalcConstantA() As String
        Get
            Return mMathCalcConstantA
        End Get
        Set(ByVal value As String)
            mMathCalcConstantA = value
        End Set
    End Property
    Public Property MathCalcConstantB() As String
        Get
            Return mMathCalcConstantB
        End Get
        Set(ByVal value As String)
            mMathCalcConstantB = value
        End Set
    End Property
    Public Property MathCalcConstantC() As String
        Get
            Return mMathCalcConstantC
        End Get
        Set(ByVal value As String)
            mMathCalcConstantC = value
        End Set
    End Property
    Public Property MathCalcFormulaInclude1() As String
        Get
            Return mMathCalcFormulaInclude1
        End Get
        Set(ByVal value As String)
            mMathCalcFormulaInclude1 = value
        End Set
    End Property
    Public Property MathCalcFormulaInclude2() As String
        Get
            Return mMathCalcFormulaInclude2
        End Get
        Set(ByVal value As String)
            mMathCalcFormulaInclude2 = value
        End Set
    End Property
    Public Property MathCalcFormulaInclude3() As String
        Get
            Return mMathCalcFormulaInclude3
        End Get
        Set(ByVal value As String)
            mMathCalcFormulaInclude3 = value
        End Set
    End Property
    Public Property MathCalcFormula1() As String
        Get
            Return mMathCalcFormula1
        End Get
        Set(ByVal value As String)
            mMathCalcFormula1 = value
        End Set
    End Property
    Public Property MathCalcFormula2() As String
        Get
            Return mMathCalcFormula2
        End Get
        Set(ByVal value As String)
            mMathCalcFormula2 = value
        End Set
    End Property
    Public Property MathCalcFormula3() As String
        Get
            Return mMathCalcFormula3
        End Get
        Set(ByVal value As String)
            mMathCalcFormula3 = value
        End Set
    End Property

    Public Property MathCalcGroupBy() As String
        Get
            Return mMathCalcGroupBy
        End Get
        Set(ByVal value As String)
            mMathCalcGroupBy = value
        End Set
    End Property
    Public Property MathCalcTarget() As String
        Get
            Return mMathCalcTarget
        End Get
        Set(ByVal value As String)
            mMathCalcTarget = value
        End Set
    End Property
    Public Property MathCalcFunctionRadio() As String
        Get
            Return mMathCalcFunctionRadio
        End Get
        Set(ByVal value As String)
            mMathCalcFunctionRadio = value
        End Set
    End Property

    Public Property ElementParentCatParent() As String
        Get
            Return mElementParentCatParent
        End Get
        Set(ByVal value As String)
            mElementParentCatParent = value
        End Set
    End Property
    Public Property ElementParentParamParentKey() As String
        Get
            Return mElementParentParamParentKey
        End Get
        Set(ByVal value As String)
            mElementParentParamParentKey = value
        End Set
    End Property
    Public Property ElementParentCatChild() As String
        Get
            Return mElementParentCatChild
        End Get
        Set(ByVal value As String)
            mElementParentCatChild = value
        End Set
    End Property
    Public Property ElementParentParamChildKey() As String
        Get
            Return mElementParentParamChildKey
        End Get
        Set(ByVal value As String)
            mElementParentParamChildKey = value
        End Set
    End Property

    Public Property ElementRollUpCatParent() As String
        Get
            Return mElementRollUpCatParent
        End Get
        Set(ByVal value As String)
            mElementRollUpCatParent = value
        End Set
    End Property
    Public Property ElementRollUpParamParentKey() As String
        Get
            Return mElementRollUpParamParentKey
        End Get
        Set(ByVal value As String)
            mElementRollUpParamParentKey = value
        End Set
    End Property
    Public Property ElementRollUpParamRollUp() As String
        Get
            Return mElementRollUpParamRollUp
        End Get
        Set(ByVal value As String)
            mElementRollUpParamRollUp = value
        End Set
    End Property
    Public Property ElementRollUpCatChild() As String
        Get
            Return mElementRollUpCatChild
        End Get
        Set(ByVal value As String)
            mElementRollUpCatChild = value
        End Set
    End Property
    Public Property ElementRollUpParamChildKey() As String
        Get
            Return mElementRollUpParamChildKey
        End Get
        Set(ByVal value As String)
            mElementRollUpParamChildKey = value
        End Set
    End Property
    Public Property ElementRollUpParamSource() As String
        Get
            Return mElementRollUpParamSource
        End Get
        Set(ByVal value As String)
            mElementRollUpParamSource = value
        End Set
    End Property

    Public Property ExternalWriteElementType() As String
        Get
            Return mExternalWriteElementType
        End Get
        Set(ByVal value As String)
            mExternalWriteElementType = value
        End Set
    End Property
    Public Property ExternalWriteProcessRadio() As String
        Get
            Return mExternalWriteProcessRadio
        End Get
        Set(ByVal value As String)
            mExternalWriteProcessRadio = value
        End Set
    End Property
    Public Property ExternalWriteParametersRadio() As String
        Get
            Return mExternalWriteParametersRadio
        End Get
        Set(ByVal value As String)
            mExternalWriteParametersRadio = value
        End Set
    End Property

    Public Property ExternalKeyPath() As String
        Get
            Return mExternalKeyPath
        End Get
        Set(ByVal value As String)
            mExternalKeyPath = value
        End Set
    End Property
    Public Property ExternalKeyParamKey1() As String
        Get
            Return mExternalKeyParamKey1
        End Get
        Set(ByVal value As String)
            mExternalKeyParamKey1 = value
        End Set
    End Property
    Public Property ExternalKeyParamList1() As String
        Get
            Return mExternalKeyParamList1
        End Get
        Set(ByVal value As String)
            mExternalKeyParamList1 = value
        End Set
    End Property
    Public Property ExternalKeyParamKey2() As String
        Get
            Return mExternalKeyParamKey2
        End Get
        Set(ByVal value As String)
            mExternalKeyParamKey2 = value
        End Set
    End Property
    Public Property ExternalKeyParamList2() As String
        Get
            Return mExternalKeyParamList2
        End Get
        Set(ByVal value As String)
            mExternalKeyParamList2 = value
        End Set
    End Property
    Public Property ExternalKeyParamKey3() As String
        Get
            Return mExternalKeyParamKey3
        End Get
        Set(ByVal value As String)
            mExternalKeyParamKey3 = value
        End Set
    End Property
    Public Property ExternalKeyParamList3() As String
        Get
            Return mExternalKeyParamList3
        End Get
        Set(ByVal value As String)
            mExternalKeyParamList3 = value
        End Set
    End Property
#End Region

End Class
