Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Windows.Forms

''' <summary>
''' A class used to create a parent child relationship
''' </summary>
''' <remarks></remarks>
Public Class form_ParamParent

    Private m_Settings As clsSettings

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()
        ' Settings Class
        m_Settings = settings
        ' Add Valid Category Selections
        comboBoxCatParent.Items.Add("Areas")
        comboBoxCatParent.Items.Add("Property Lines")
        comboBoxCatParent.Items.Add("Rooms")
        comboBoxCatChild.Items.Add("Mass")
        comboBoxCatChild.Items.Add("Rooms")
        ' Populate saved settings to Dialog
        comboBoxCatParent.Text = m_Settings.ElementParentCatParent
        textBoxParamParentKey.Text = m_Settings.ElementParentParamParentKey
        comboBoxCatChild.Text = m_Settings.ElementParentCatChild

        textBoxParamChildKey.Text = m_Settings.ElementParentParamChildKey
    End Sub

    ''' <summary>
    ''' Save the Settings to th INI file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SaveSettings()
        m_Settings.ElementParentCatParent = comboBoxCatParent.Text
        m_Settings.ElementParentParamParentKey = textBoxParamParentKey.Text
        m_Settings.ElementParentCatChild = comboBoxCatChild.Text
        m_Settings.ElementParentParamChildKey = textBoxParamChildKey.Text
        m_Settings.WriteIni()
    End Sub

    ''' <summary>
    ''' Do the Relationships
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub DoRelation()
        Dim m_ParentElems As New List(Of Element)
        Dim m_ChildElems As New List(Of Element)
        Dim boundingBoxParent As BoundingBoxXYZ
        Dim boundingBoxChild As BoundingBoxXYZ
        Dim keyValueParent As String

        ' Form Qualifications
        If comboBoxCatParent.Text = "" Then
            MessageBox.Show("Parent element category must be set before running this command.", m_Settings.ProgramName)
            Exit Sub
        End If
        If comboBoxCatChild.Text = "" Then
            MessageBox.Show("Child element category must be set before running this command.", m_Settings.ProgramName)
            Exit Sub
        End If
        If textBoxParamParentKey.Text = "" Then
            MessageBox.Show("Parent key parameter name must be set before running this command.", m_Settings.ProgramName)
            Exit Sub
        End If
        If textBoxParamChildKey.Text = "" Then
            MessageBox.Show("Child key parameter name must be set before running this command.", m_Settings.ProgramName)
            Exit Sub
        End If
        If comboBoxCatParent.Text = comboBoxCatChild.Text Then
            MessageBox.Show("Parent and child element category cannot be the same.", m_Settings.ProgramName)
            Exit Sub
        End If

        ' Get all Instance Elements
        Dim m_Elements As New List(Of Element)
        Dim m_Col As New FilteredElementCollector(m_Settings.Document)
        m_Col.WhereElementIsNotElementType()
        m_Elements = m_Col.ToElements

        ' Find the matching elements
        For Each e As Element In m_Elements
            If e.Category IsNot Nothing Then
                If e.Category.Name.ToUpper = comboBoxCatParent.SelectedItem.ToUpper Then
                    ' Parent Instances Collection by Category
                    m_ParentElems.Add(e)
                ElseIf e.Category.Name.ToUpper = comboBoxCatChild.SelectedItem.ToUpper Then
                    ' Child Instances Collection by Category
                    m_ChildElems.Add(e)
                End If
            End If
        Next

        'Start the progress bar
        Dim progressBarForm As New form_ParamProgressBar("Processing " & _
                                                         m_ChildElems.Count.ToString & " " & _
                                                         comboBoxCatParent.Text & ".", _
                                                         m_ChildElems.Count + 1)
        progressBarForm.Show()
        progressBarForm.Increment()

        ' Process the Parent Elements
        For Each elementParent As Element In m_ParentElems
            keyValueParent = ""

            ' Start a New Transaction... per element
            Dim m_Trans As New Transaction(m_Settings.Document, "HOK Parameter Define Parent Child")
            m_Trans.Start()

            Try

                ' Get the parameter if it exists 
#If RELEASE2013 Or RELEASE2014 Then
                Dim m_ParentKeyParam As Parameter = elementParent.Parameter(textBoxParamParentKey.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                Dim m_ParentKeyParam As Parameter = elementParent.LookupParameter(textBoxParamParentKey.Text)
#End If

                If m_ParentKeyParam IsNot Nothing Then
                    Dim m_para As New clsPara(m_ParentKeyParam)
                    keyValueParent = m_para.Value
                End If

                ' Look inside the bounding box: 
                boundingBoxParent = elementParent.BoundingBox(m_Settings.ActiveView)
                If boundingBoxParent Is Nothing Then Continue For

                ' Probably Unplaced 
                For Each elementChild As Element In m_ChildElems
                    boundingBoxChild = elementChild.BoundingBox(m_Settings.ActiveView)
                    Dim xyzChild As New XYZ((boundingBoxChild.Max.X + boundingBoxChild.Min.X) / 2, (boundingBoxChild.Max.Y + boundingBoxChild.Min.Y) / 2, 0)
                    ' General Logic
                    If xyzChild.X > boundingBoxParent.Min.X And xyzChild.X < boundingBoxParent.Max.X Then
                        If xyzChild.Y > boundingBoxParent.Min.Y And xyzChild.Y < boundingBoxParent.Max.Y Then

                            ' Set the parameter if it exists
#If RELEASE2013 Or RELEASE2014 Then
                            Dim m_ChildKeyParam As Parameter = elementChild.Parameter(textBoxParamChildKey.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Then
                            Dim m_ChildKeyParam As Parameter = elementChild.LookupParameter(textBoxParamChildKey.Text)
#End If

                            If m_ChildKeyParam IsNot Nothing Then
                                Dim m_para As New clsPara(m_ChildKeyParam)
                                m_para.Value = keyValueParent
                            End If

                        End If

                    End If

                Next

                ' Commit the transaction
                m_Trans.Commit()

            Catch ex As Exception
                ' On Failure...
                m_Trans.RollBack()
            End Try

            'Increment the Progress Bar
            progressBarForm.Increment()

        Next

        'Close the progress bar.
        progressBarForm.Close()
    End Sub

#Region "Form Controls and Events"

    ''' <summary>
    ''' Close the Utility
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
    ''' Process the Parent Relations 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonProcess_Click(ByVal sender As System.Object, _
                                    ByVal e As System.EventArgs) _
                                Handles buttonProcess.Click
        DoRelation()
    End Sub

#End Region

End Class