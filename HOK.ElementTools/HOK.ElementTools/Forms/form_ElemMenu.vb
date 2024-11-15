﻿Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

Imports System.Windows.Forms

Public Class form_ElemMenu

    Private m_wndToolTip As System.Windows.Forms.ToolTip
    Private m_Settings As clsSettings
    Private m_command As cmdElementTools.ToolType = cmdElementTools.ToolType.None

    Property CommandToolType() As cmdElementTools.ToolType
        Get
            Return m_command
        End Get
        Set(value As cmdElementTools.ToolType)
            m_command = value
        End Set
    End Property

    Public Sub New(ByRef settings As clsSettings, ByVal appV As String)
        InitializeComponent()
        m_Settings = settings
        Me.Text = "Element Tools - " & appV
    End Sub

    Public Sub New(ByRef settings As clsSettings, ByVal appV As String, ByVal command As cmdElementTools.ToolType)
        InitializeComponent()
        m_Settings = settings
        Me.Text = "Element Tools - " & appV
        m_command = command
    End Sub

    Private Sub Menu_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.m_wndToolTip = New System.Windows.Forms.ToolTip
        m_wndToolTip.SetToolTip(Me.ButtonPlaceUnplacedAreas, _
                                "Lists all unplaced (and placed but not enclosed) areas." & vbCr & _
                                "Selections from the list are arrayed in the current view," & vbCr & _
                                "with area boundary line enclosures." & vbCr & _
                                "(Current view must be an area plan.)")
        m_wndToolTip.SetToolTip(Me.ButtonPlaceUnplacedRooms, _
                                "Lists all unplaced (and placed but not enclosed) rooms." & vbCr & _
                                "Selections from the list are arrayed in the current view, " & vbCr & _
                                "with enclosures of the current wall type.")
        m_wndToolTip.SetToolTip(Me.ButtonCreateRoomsFromAreas, _
                                "Lists all areas." & vbCr & _
                                "Unplaced (and placed but not enclosed)" & vbCr & _
                                "selections from the list are used to create unplaced rooms." & vbCr & _
                                "Placed selections are used to create a room at the same location." & vbCr & _
                                "(Current view must be an area plan showing area.)")
        m_wndToolTip.SetToolTip(Me.ButtonCreateViewsFromRooms, _
                               "Lists all rooms with an option to include rooms" & vbCr & _
                               "that already have a view of the same name." & vbCr & _
                               "Views are created from the list selection," & vbCr & _
                               "replacing any that already exist," & vbCr & _
                               "showing only the associated room.")
        m_wndToolTip.SetToolTip(Me.ButtonCreateTaggedViewsFromRooms, _
                               "Lists all untagged views with an option to include" & vbCr & _
                               "tagged views that already exist with the same room name." & vbCr & _
                               "Views are created as copies from the list selection," & vbCr & _
                               "replacing any that already exist, with a room tag added.")
        m_wndToolTip.SetToolTip(Me.ButtonCreateViewsFromAreas, _
                               "Lists all areas with an option to include areas" & vbCr & _
                               "that already have a view of the same name." & vbCr & _
                               "Views are created from the list selection," & vbCr & _
                               "replacing any that already exist," & vbCr & _
                               "showing only the associated areas.")
        m_wndToolTip.SetToolTip(Me.ButtonCreateTaggedViewsFromAreas, _
                               "Lists all untagged views with an option to include" & vbCr & _
                               "tagged views that already exist with the same area name." & vbCr & _
                               "Views are created as copies from the list selection," & vbCr & _
                               "replacing any that already exist, with a area tag added.")
        m_wndToolTip.SetToolTip(Me.ButtonCreateSheetsFromViews, _
                               "Lists all views that correspond to a room," & vbCr & _
                               "with an option to include views that already" & vbCr & _
                               "have a sheet of the same name." & vbCr & _
                               "Sheets are created from the list selection," & vbCr & _
                               "replacing any that already exist.")
        m_wndToolTip.SetToolTip(Me.ButtonManageAttachmentLinks, _
                               "Lists all of the DWG/DXF/DGN/SKP/SAT attachments," & vbCr & _
                               "both definitions and instances," & vbCr & _
                               "by their element ID." & vbCr & _
                               "Creates a text file of the list and provides an option" & vbCr & _
                               "to select attachment(s) to be modified on return to Revit. ")
        If (m_command <> cmdElementTools.ToolType.None) Then
            RunCommand(m_command)
        End If
    End Sub

    Public Sub RunCommand(ByVal toolToRun As cmdElementTools.ToolType)
        Try
            Select Case toolToRun
                Case cmdElementTools.ToolType.CreateRoomsFromAreas
                    RunCreateRoomsFromAres()
                Case cmdElementTools.ToolType.CreateSheetsFromViews
                    RunCreateSheetsFromViews()
                Case cmdElementTools.ToolType.CreateTaggedViewsFromAreas
                    RunCreateTaggedViewsFromAreas()
                Case cmdElementTools.ToolType.CreateTaggedViewsFromRooms
                    RunCreateTaggedViewsFromRooms()
                Case cmdElementTools.ToolType.CreateViewsFromAreas
                    RunCreateViewsFromArea()
                Case cmdElementTools.ToolType.CreateViewsFromRooms
                    RunCreateViewsFromRooms()
                Case cmdElementTools.ToolType.ManageAttachmentLinks
                    RunManageAttachmentLinks()
                Case cmdElementTools.ToolType.PlaceUnplacedAreas
                    RunPlaceUnplacedAreas()
                Case cmdElementTools.ToolType.PlaceUnplacedRooms
                    RunPlaceUnplacedRooms()
            End Select
        Catch ex As Exception
            Dim message As String = ex.Message
        End Try
    End Sub

    Private Sub ButtonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCancel.Click
        Me.Close()
    End Sub

    Private Sub buttonReloadSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonReloadSettings.Click
        m_Settings.ReloadDefaults()
    End Sub

    Private Sub RunPlaceUnplacedAreas()
        Dim dialog As New form_ElemPlaceUnplacedAreas(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.PlaceUnplacedAreas
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonPlaceUnplacedAreas_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPlaceUnplacedAreas.Click
        RunPlaceUnplacedAreas()
    End Sub

    Private Sub RunPlaceUnplacedRooms()
        Dim dialog As New form_ElemPlaceUnplacedRooms(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.PlaceUnplacedRooms
            dialog.Close()
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonPlaceUnplacedRooms_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonPlaceUnplacedRooms.Click
        RunPlaceUnplacedRooms()
    End Sub

    Private Sub RunCreateRoomsFromAres()
        Dim dialog As New form_ElemRoomsFromAreas(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.CreateRoomsFromAreas
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonCreateRoomsFromAreas_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCreateRoomsFromAreas.Click
        RunCreateRoomsFromAres()
    End Sub

    Private Sub RunCreateViewsFromRooms()
        Dim dialog As New form_ElemViewsFromRooms(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.CreateViewsFromRooms
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonCreateViewsFromRooms_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCreateViewsFromRooms.Click
        RunCreateViewsFromRooms()
    End Sub

    Private Sub RunCreateTaggedViewsFromRooms()
        Dim dialog As New form_ElemTagViews(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.CreateTaggedViewsFromRooms
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonCreateTaggedViewsFromRooms_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCreateTaggedViewsFromRooms.Click
        RunCreateTaggedViewsFromRooms()
    End Sub

    Private Sub RunCreateViewsFromArea()
        Dim dialog As New form_ElemViewsFromAreas(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.CreateViewsFromAreas
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonCreateViewsFromAreas_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCreateViewsFromAreas.Click
        RunCreateViewsFromArea()
    End Sub

    Private Sub RunCreateTaggedViewsFromAreas()
        Dim dialog As New form_ElemTagViewsArea(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.CreateTaggedViewsFromAreas
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonCreateTaggedViewsFromAreas_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCreateTaggedViewsFromAreas.Click
        RunCreateTaggedViewsFromAreas()
    End Sub

    Private Sub RunCreateSheetsFromViews()
        Dim dialog As New form_ElemSheetsFromViews(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.CreateSheetsFromViews
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonCreateSheetsFromViews_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonCreateSheetsFromViews.Click
        RunCreateSheetsFromViews()
    End Sub

    Private Sub RunManageAttachmentLinks()
        Dim dialog As New form_ElemAttachmentManager(m_Settings)
        Me.WindowState = FormWindowState.Minimized
        Dim dResult As DialogResult = dialog.ShowDialog()
        If (DialogResult.OK = dResult Or DialogResult.Retry = dResult) Then
            m_command = cmdElementTools.ToolType.ManageAttachmentLinks
            Me.DialogResult = dResult
        End If
    End Sub

    Private Sub ButtonManageAttachmentLinks_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonManageAttachmentLinks.Click
        RunManageAttachmentLinks()
    End Sub



End Class