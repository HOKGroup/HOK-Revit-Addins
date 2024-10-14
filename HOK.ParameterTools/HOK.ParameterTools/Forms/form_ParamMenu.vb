Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Windows.Forms

Public Class form_ParamMenu

    Private m_Settings As clsSettings
    Private m_wndToolTip As ToolTip

    Public Sub New(ByVal commandData As ExternalCommandData, ByRef message As String, ByRef elementSet As ElementSet, ByVal aVer As String)
        InitializeComponent()
        m_Settings = New clsSettings(commandData, message, elementSet)
        ' Form Title
        Me.Text = "Parameter Tools - " & aVer
    End Sub

    Private Sub Menu_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.m_wndToolTip = New System.Windows.Forms.ToolTip

        m_wndToolTip.SetToolTip(Me.buttonClose, "Exit without changes")
        m_wndToolTip.SetToolTip(Me.buttonElementParent, "Determines which elements of a given class are located insde an instance of another class." & vbCr & _
                                "Assigns ID value of single parent instance to parameter of many child elements.")
        m_wndToolTip.SetToolTip(Me.buttonElementRollUp, "Adds up all of the values of multiple child instances as defined by one-many parameter values." & vbCr & _
                                "Updates an aggregate parameter in the parent element with the total value.")
        m_wndToolTip.SetToolTip(Me.buttonMathCalculate, "Calculates and assigns special values for parameters." & vbCr & _
                                "Uses mathematical functions from several source to a single target parameter.")
        m_wndToolTip.SetToolTip(Me.buttonReloadSettings, "Reload all default settings...")
        m_wndToolTip.SetToolTip(Me.buttonScheduleKey, "Starts a new session of Excel." & vbCr & _
                                "Reads and writes Schedule Key values.")
        m_wndToolTip.SetToolTip(Me.buttonStringCalculate, "Calculates and assigns special values for parameters." & vbCr & _
                                "Combines (concatentates) several values into one parameter.")
        m_wndToolTip.SetToolTip(Me.buttonWriteToExcel, "Starts a new session of Excel." & vbCr & _
                                "Writes out all parameter values for a selected type of element.")
    End Sub

#Region "Form Controls"

    Private Sub buttonClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClose.Click
        Me.Close()
    End Sub

    Private Sub buttonReloadSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonReloadSettings.Click
        m_Settings.ReloadDefaults()
    End Sub

    Private Sub buttonScheduleKey_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonScheduleKey.Click
        Using m_dlg As New form_ParamScheduleKey(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

    Private Sub buttonWriteToExcel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonWriteToExcel.Click
        Using m_dlg As New form_ParamWriteToExcel(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

    Private Sub buttonElementRollUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonElementRollUp.Click
        Using m_dlg As New form_ParamRollUp(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

    Private Sub buttonElementParent_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonElementParent.Click
        Using m_dlg As New form_ParamParent(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

    Private Sub buttonMathCalculate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonMathCalculate.Click
        Using m_dlg As New form_ParamCalculate(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

    Private Sub buttonStringCalculate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonStringCalculate.Click
        Using m_dlg As New form_ParamStringCalculate(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

    Private Sub ButtonSuperCalc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonSuperCalc.Click
        Using m_dlg As New form_ParameterCalculation(m_Settings)
            m_dlg.ShowDialog()
        End Using
    End Sub

#End Region

End Class