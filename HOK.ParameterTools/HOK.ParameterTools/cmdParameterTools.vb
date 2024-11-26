' (C) Copyright 2011 HOK SF
'
' Code managed by 
' Code modified by Jinsol Kim, Hong Kong

Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.Attributes

''' <summary>
''' Various Parameter tools
''' </summary>
<Transaction(TransactionMode.Manual)> _
<Regeneration(RegenerationOption.Manual)> _
Public Class cmdParameterTools

    Implements ExternalCommand

    Public Const appVer As String = "V2011.03.30"

    Public Function Execute(ByVal commandData As ExternalCommandData, _
                            ByRef message As String, _
                            ByVal elements As ElementSet) As Result _
                            Implements IExternalCommand.Execute
        Try

            Using dlg As New form_ParamMenu(commandData, message, elements, appVer)
                dlg.ShowDialog()
            End Using
            Return Result.Succeeded
        Catch e As Exception
            message = e.Message
            Return Result.Failed
        End Try
    End Function

End Class
