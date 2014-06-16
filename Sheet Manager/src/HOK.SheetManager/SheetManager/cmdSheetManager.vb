' (C) Copyright 2011 HOK SF
'
' Code managed by Don
' Code modified by Jinsol Kim, Hong Kong

Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.Attributes

''' <summary>
''' Implements the Revit add-in interface IExternalCommand
''' </summary>
''' <remarks></remarks>
<Transaction(TransactionMode.Manual)> _
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

    Public Function Execute(ByVal commandData As ExternalCommandData, _
                            ByRef message As String, _
                            ByVal elements As ElementSet) As Result Implements IExternalCommand.Execute
        Try

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
