' (C) Copyright 2011 HOK SF
'
' Code originally managed by Don Rudder, SF BIM Manager
' Code modified by Jinsol Kim, HK Custom Application Developer 
' Added functions by Jinsol Kim: Create Views from Areas, Create Tagged Views from Areas


Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.Attributes
Imports System.Windows.Forms

''' <summary>
''' Various Element Tools
''' </summary>
<Transaction(TransactionMode.Manual)> _
<Regeneration(RegenerationOption.Manual)> _
Public Class cmdElementTools

    Implements IExternalCommand

    Enum ToolType
        None
        PlaceUnplacedAreas
        PlaceUnplacedRooms
        CreateRoomsFromAreas
        CreateViewsFromRooms
        CreateTaggedViewsFromRooms
        CreateViewsFromAreas
        CreateTaggedViewsFromAreas
        CreateSheetsFromViews
        ManageAttachmentLinks
    End Enum

    Private m_Ver As String = "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
    Private m_command As ToolType = ToolType.None
    Private m_settings As clsSettings
    Private m_dialog As form_ElemMenu

    Public Function Execute(ByVal commandData As ExternalCommandData, _
                            ByRef message As String, _
                            ByVal elements As ElementSet) As Result Implements IExternalCommand.Execute
        Try
            m_settings = New clsSettings(commandData, message, elements)

            m_dialog = New form_ElemMenu(m_settings, m_Ver)
            Dim dResult As DialogResult = m_dialog.ShowDialog()
            If (dResult = DialogResult.OK) Then
                m_dialog.Close()
            ElseIf (dResult = DialogResult.Retry) Then
                m_command = m_dialog.CommandToolType
                'save setting
                Select Case m_command
                    Case ToolType.PlaceUnplacedRooms
                        m_dialog.Close()
                        PickStartPoint(m_command)
                        OpenCommandForm(m_command)
                End Select
            End If


            Return Result.Succeeded
        Catch ex As Exception
            message = "HOK Customization Error: Element Tools Failed"
            Return Result.Failed
        End Try
    End Function

    Private Sub OpenCommandForm(ByVal commandRun As ToolType)
        'when command forms should be closed and reopened for the UI interaction of Revit elements.
        m_dialog = New form_ElemMenu(m_settings, m_Ver, commandRun)
        Dim dResult As DialogResult = m_dialog.ShowDialog()
        If (dResult = DialogResult.OK) Then
            m_dialog.Close()
        ElseIf (dResult = DialogResult.Retry) Then
            m_command = m_dialog.CommandToolType
            'save setting
            Select Case m_command
                Case ToolType.PlaceUnplacedRooms
                    m_dialog.Close()
                    PickStartPoint(m_command)
                    OpenCommandForm(m_command)
            End Select
        End If

    End Sub

    Private Sub PickStartPoint(ByVal selectedTool As ToolType)
        Try
            If (selectedTool = ToolType.PlaceUnplacedRooms) Then
                Dim point As XYZ = m_settings.UIdoc.Selection.PickPoint("Please pick a point to place component.")
                If point IsNot Nothing Then
                    m_settings.RoomsPlaceStartX = point.X.ToString
                    m_settings.RoomsPlaceStartY = point.Y.ToString
                End If
                
            End If
        Catch ex As Exception
            Dim message As String = ex.Message
        End Try
    End Sub

End Class


