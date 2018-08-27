' (C) Copyright 2011 HOK SF
'
' Code managed by Don
' Code modified by Jinsol Kim, Hong Kong
' Code modified by Konrad K Sobon, New York

Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.Attributes
Imports HOK.MissionControl.Core.Schemas
Imports HOK.MissionControl.Core.Utils

''' <summary>
''' Implements the Revit add-in interface IExternalCommand
''' </summary>
''' <remarks></remarks>
<Transaction(TransactionMode.Manual)>
Public Class cmdSheetManager

    Implements IExternalCommand

    ''' <summary>
    ''' Command Entry Point
    ''' </summary>
    ''' <param name="commandData"></param>
    ''' <param name="message"></param>
    ''' <param name="elements"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 

    Public Function Execute(ByVal commandData As ExternalCommandData,
                            ByRef message As String,
                            ByVal elements As ElementSet) As Result Implements IExternalCommand.Execute
        Try
            AddinUtilities.PublishAddinLog(New AddinLog("Sheet Manager-Excel", commandData.Application.Application.VersionNumber))

            ' Construct the settings class used to manage the Revit environment and document
            Dim m_Settings As New clsSettings(commandData)

            ' Construct and display the main dialog
            Using m_dlg As New form_SheetManager(m_Settings)
                m_dlg.ShowDialog()

                Return Result.Succeeded
            End Using

        Catch ex As Exception

            ' Display a sweet failure message
            message = ex.Message
            Return Result.Failed

        End Try

        ' Dialog was Cancelled
        Return Result.Cancelled

    End Function

End Class
