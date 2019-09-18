Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Windows.Forms
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

Public Class form_ParamRollUp

    Private m_Settings As clsSettings

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="settings"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal settings As clsSettings)
        InitializeComponent()

        AddinUtilities.PublishAddinLog(New AddinLog("ParameterTools-RollUp", settings.Document.Application.VersionNumber))

        ' Main Settings Class
        m_Settings = settings
        ' Category Selections
        comboBoxCatParent.Items.Add("Areas")
        comboBoxCatParent.Items.Add("Property Lines")
        comboBoxCatParent.Items.Add("Rooms")
        comboBoxCatChild.Items.Add("Mass")
        comboBoxCatChild.Items.Add("Rooms")
        ' Saved Settings
        comboBoxCatParent.Text = m_Settings.ElementRollUpCatParent
        textBoxParamParentKey.Text = m_Settings.ElementRollUpParamParentKey
        textBoxParamRollUp.Text = m_Settings.ElementRollUpParamRollUp
        comboBoxCatChild.Text = m_Settings.ElementRollUpCatChild
        textBoxParamChildKey.Text = m_Settings.ElementRollUpParamChildKey
        textBoxParamSource.Text = m_Settings.ElementRollUpParamSource
    End Sub

    ''' <summary>
    ''' Save the settings and write to the INI file
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub SaveSettings()
        m_Settings.ElementRollUpCatParent = comboBoxCatParent.Text
        m_Settings.ElementRollUpParamParentKey = textBoxParamParentKey.Text
        m_Settings.ElementRollUpParamRollUp = textBoxParamRollUp.Text
        m_Settings.ElementRollUpCatChild = comboBoxCatChild.Text
        m_Settings.ElementRollUpParamChildKey = textBoxParamChildKey.Text
        m_Settings.ElementRollUpParamSource = textBoxParamSource.Text
        ' Write the settings to INI
        m_Settings.WriteIni()
    End Sub

    ''' <summary>
    ''' Close the application
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        SaveSettings()
        Me.Close()
    End Sub

    ''' <summary>
    ''' Process the Values
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub buttonProcess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonProcess.Click
        
        ' Message Qualifiers
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
        If textBoxParamRollUp.Text = "" Then
            MessageBox.Show("Roll up value parameter name must be set before running this command.", m_Settings.ProgramName)
            Exit Sub
        End If
        If textBoxParamSource.Text = "" Then
            MessageBox.Show("Source value parameter name must be set before running this command.", m_Settings.ProgramName)
            Exit Sub
        End If
        If comboBoxCatParent.Text = comboBoxCatChild.Text Then
            MessageBox.Show("Parent and child element category cannot be the same.", m_Settings.ProgramName)
            Exit Sub
        End If

        ' Element Collection
        Dim myCol As FilteredElementCollector = New FilteredElementCollector(m_Settings.Document)
        myCol.WhereElementIsNotElementType()
        Dim m_TestElements As New List(Of Element)
        m_TestElements = myCol.ToElements

        ' Lists etc.
        Dim elementsParent As New List(Of Element)
        Dim m_ChildElementList As New List(Of Element)
        Dim nameCategory As String
        Dim m_KeyValue As String
        Dim areaRollUp As Double

        ' Find matching elements
        For Each elem As Element In m_TestElements
            If elem.Category Is Nothing Then
                Continue For
            End If
            nameCategory = elem.Category.Name
            If nameCategory = comboBoxCatParent.Text Then
                elementsParent.Add(elem)
            ElseIf nameCategory = comboBoxCatChild.Text Then
                m_ChildElementList.Add(elem)
            End If
        Next

        'Start the progress bar
        Dim progressBarForm As New form_ParamProgressBar(("Processing " & m_ChildElementList.Count.ToString & " ") + comboBoxCatParent.Text & ".", m_ChildElementList.Count + 1)
        progressBarForm.Show()
        progressBarForm.Increment()

        ' Iterate through each parent elements...
        For Each m_ParentElem As Element In elementsParent
            ' New Transaction
            Dim m_Trans As New Transaction(m_Settings.Document, "HOK Parameter Roll up to Parent")
            m_Trans.Start()

            m_KeyValue = ""
            Try
#If RELEASE2013 Or RELEASE2014 Then
                Dim m_ParameterParent As Parameter = m_ParentElem.Parameter(textBoxParamParentKey.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Or RELEASE2020 Then
                Dim m_ParameterParent As Parameter = m_ParentElem.LookupParameter(textBoxParamParentKey.Text)
#End If

                If m_ParameterParent IsNot Nothing Then
                    Dim m_ParaP As New clsPara(m_ParameterParent)
                    m_KeyValue = m_ParaP.Value
                End If

                ' Reset the rollup value to zero
                areaRollUp = 0

                ' Iterate the Child elements
                For Each m_ChildElem As Element In m_ChildElementList
#If RELEASE2013 Or RELEASE2014 Then
                    Dim m_ParameterKeyChild As Parameter = m_ChildElem.Parameter(textBoxParamChildKey.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Or RELEASE2020 Then
                    Dim m_ParameterKeyChild As Parameter = m_ChildElem.LookupParameter(textBoxParamChildKey.Text)
#End If
                    If m_ParameterKeyChild IsNot Nothing Then
                        Dim m_para As New clsPara(m_ParameterKeyChild)
                        If m_para.Value = m_KeyValue Then
#If RELEASE2013 Or RELEASE2014 Then
                            Dim m_ParaFinal As Parameter = m_ChildElem.Parameter(textBoxParamSource.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Or RELEASE2020 Then
                            Dim m_ParaFinal As Parameter = m_ChildElem.LookupParameter(textBoxParamSource.Text)
#End If

                            Dim m_ParaAdder As New clsPara(m_ParaFinal)
                            areaRollUp = areaRollUp + m_ParaAdder.Value
                        End If
                    End If

                Next
#If RELEASE2013 Or RELEASE2014 Then
                Dim m_Param As Parameter = m_ParentElem.Parameter(textBoxParamRollUp.Text)
#ElseIf RELEASE2015 Or RELEASE2016 Or RELEASE2017 Or RELEASE2018 Or RELEASE2019 Or RELEASE2020 Then
                Dim m_Param As Parameter = m_ParentElem.LookupParameter(textBoxParamRollUp.Text)
#End If

                If m_Param IsNot Nothing Then
                    Dim m_para As New clsPara(m_Param)
                    m_para.Value = areaRollUp
                End If

                ' Commit the transaction 
                m_Trans.Commit()
            Catch ex As Exception
                m_Trans.RollBack()
            End Try
            'Increment the Progress Bar
            progressBarForm.Increment()
        Next


        'Close the progress bar.
        progressBarForm.Close()
    End Sub
End Class