Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

Public Class form_ParameterCalculation

    Private m_Settings As clsSettings
    Private m_SelectedElements As New List(Of Element)

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()
        ' The Settings Class
        m_Settings = settings
        ' General Form Visibilities
        Me.ProgressBarMain.Visible = False
        Me.ButtonOk.Enabled = False
        ' Populate the categories
        FillElementTypeList()
    End Sub

#Region "Private Functions and Routines"

    ''' <summary>
    ''' Retrieve all Parameters from First Element of a Category
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetParametersByCategory(ByVal CatName As String) As Dictionary(Of String, clsParaMini)
        ' The main source dictionary
        Dim m_Params As New Dictionary(Of String, clsParaMini)

        Try
            Dim m_Elements As New List(Of Element)
            Dim m_Col As New FilteredElementCollector(m_Settings.Document)
            ' Build the List from the category name
            For Each cat As Category In m_Settings.Document.Settings.Categories
                If cat.Name.ToUpper = CatName.ToUpper Then
                    m_Col.OfCategory(cat.Id.Value)
                    m_Elements = m_Col.ToElements
                End If
            Next

            ' List the type and instance parameters
            Dim m_DoneInst As Boolean = False
            Dim m_DoneType As Boolean = False

            For Each e As Element In m_Elements
                ' Type Element
                If TypeOf e Is FamilySymbol And m_DoneType = False Then

                    For Each p As Parameter In e.Parameters
                        Try
                            Dim m_Para As New clsParaMini(p)
#If RELEASE2022 Or RELEASE2023 Or RELEASE2024 Then
                            Select Case p.Definition.GetDataType()
                                Case New ForgeTypeId() ' Invalid
                                    Continue For
                                Case SpecTypeId.Boolean.YesNo ' YesNo
                                    Continue For
                                Case SpecTypeId.String.Url ' URL
                                    Continue For
                                Case UnitTypeId.Currency ' Currency
                                    Continue For
#Else
                            Select Case p.Definition.ParameterType
                                Case 0 ' Invalid
                                    Continue For
                                Case 8 ' URL
                                    Continue For
                                Case 10 ' YesNo
                                    Continue For
                                Case 172 ' Currency
                                    Continue For
#End If
                            End Select
                            ' Add it to the dictionary
                            If m_Para.Format.ToUpper <> "ELEMENTID" Then m_Params.Add(m_Para.Name, m_Para)
                        Catch ex2 As Exception
                            ' Parameter already present
                        End Try
                    Next

                ElseIf TypeOf e Is FamilyInstance And m_DoneInst = False Then
                    ' Instance Element
                    For Each p As Parameter In e.Parameters
                        Try
                            Dim m_Para As New clsParaMini(p)
#If RELEASE2022 Or RELEASE2023 Or RELEASE2024 Then
                            Select Case p.Definition.GetDataType()
                                Case New ForgeTypeId() ' Invalid
                                    Continue For
                                Case SpecTypeId.Boolean.YesNo ' YesNo
                                    Continue For
                                Case SpecTypeId.String.Url ' URL
                                    Continue For
                                Case UnitTypeId.Currency ' Currency
                                    Continue For
#Else
                            Select Case p.Definition.ParameterType
                                Case 0
                                    Continue For
                                Case 8
                                    Continue For
                                Case 10
                                    Continue For
                                Case 172
                                    Continue For
#End If
                            End Select
                            ' Add it to the dictionary
                            If m_Para.Format.ToUpper <> "ELEMENTID" Then m_Params.Add(m_Para.Name, m_Para)
                        Catch ex2 As Exception
                            ' Parameter already present
                        End Try
                    Next

                End If

                ' If we've satisfied everything, exit the loopage
                If m_DoneInst = True And m_DoneType = True Then Exit For

            Next

        Catch ex1 As Exception

        End Try
        ' Populate to the datagrid?
        Return m_Params
    End Function

    ''' <summary>
    ''' Process the selected formulas
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ProcessFormulasAll() As Boolean
        ' Validate Formula
        ' - Parameter Names must be enclosed in []
        ' - () must be matched... no open parenthese ; count each... must be equal

        Dim m_FormulaString As String = Me.TextBoxFormula.Text



        ' Verify the parameters (Necessary here?)

        ' Select the Elements
        m_SelectedElements = ElementCollector(Me.comboBoxCategory.SelectedItem)





        Dim m_Trans As New Transaction(m_Settings.Document, "HOK Parameter Calculations")
        m_Trans.Start()

        m_Trans.Commit()
    End Function

    ''' <summary>
    ''' Grab all elements by category
    ''' </summary>
    ''' <param name="CategoryName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ElementCollector(ByVal CategoryName As String) As List(Of Element)
        Dim m_Elements As New List(Of Element)
        For Each cat As Category In m_Settings.Document.Settings.Categories
            If cat.Name.ToUpper = CategoryName.ToUpper Then
                Dim m_Col As New FilteredElementCollector(m_Settings.Document)
                m_Col.OfCategory(cat.Id.Value)
                m_Elements = m_Col.ToElements
                ' This will return the contents of the category
                Return m_Elements
            End If
        Next
        ' This will be empty... :(
        Return m_Elements
    End Function

    ''' <summary>
    ''' List all categories in the pull-down
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub FillElementTypeList()
        'This isn't finding all the categories; for example "Property Lines" is a subset of Site
        'Convert to style like ElementParent where we use a button to search for all valid values in current model
        Dim categoryList As New List(Of String)
        For Each category As Category In m_Settings.Document.Settings.Categories
            If category.Name.Contains("Tag") Then Continue For
            categoryList.Add(category.Name)
        Next
        categoryList.Sort()
        Me.comboBoxCategory.DataSource = categoryList
        Me.comboBoxCategory.SelectedIndex = 0
    End Sub

#End Region

#Region "Form Controls and Events"

    ''' <summary>
    ''' When the user leaves the text box...
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub TextBoxFormula_Leave(ByVal sender As Object, _
                                     ByVal e As System.EventArgs) _
                                 Handles TextBoxFormula.Leave
        ' Allow the update function...
        If Me.TextBoxFormula.Text <> "" Then
            Me.ButtonOk.Enabled = True
        Else
            Me.ButtonOk.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Close and Exit
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonClose_Click(ByVal sender As System.Object, _
                                  ByVal e As System.EventArgs) _
                              Handles ButtonClose.Click
        ' Save Settings?

        Me.Close()
    End Sub

    ''' <summary>
    ''' Load parameters from first mathing element (type and instance?)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub comboBoxCategory_SelectedIndexChanged(ByVal sender As System.Object, _
                                                      ByVal e As System.EventArgs) _
                                                  Handles comboBoxCategory.SelectedIndexChanged
        ' From Dictionary to Sortable List...
        Dim m_ParaDictionary As New Dictionary(Of String, clsParaMini)
        m_ParaDictionary = GetParametersByCategory(Me.comboBoxCategory.SelectedItem)
        ' Generate a sortable list from the dictionary
        Dim m_List As New SortableBindingList(Of clsParaMini)
        For Each x As clsParaMini In m_ParaDictionary.Values
            m_List.Add(x)
        Next
        ' Refresh the datagrid
        Me.DataGridViewParameters.DataSource = Nothing
        Me.DataGridViewParameters.DataSource = m_List
    End Sub

    ''' <summary>
    ''' Run the Calculation
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonOk_Click(ByVal sender As System.Object, _
                               ByVal e As System.EventArgs) _
                           Handles ButtonOk.Click
        ' Preview the Formula Results
        ProcessFormulasAll()
    End Sub

    ''' <summary>
    ''' Show the Formuala Help Dialog
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub ButtonHelp_Click(ByVal sender As System.Object, _
                                 ByVal e As System.EventArgs) _
                             Handles ButtonHelp.Click
        Using m_Dlg As New form_FormulaHelp
            m_Dlg.ShowDialog()
        End Using
    End Sub

#End Region

End Class