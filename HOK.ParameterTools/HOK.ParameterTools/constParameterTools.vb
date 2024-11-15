Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

''' <summary>
''' Various Parameter tools
''' </summary>
Public Class constParameterTools

    Public Sub New(ByVal cmdData As ExternalCommandData, _
                   ByVal Msg As String, _
                   ByVal eSet As ElementSet, _
                   ByVal myAppVer As String)

        Dim dlg As New form_ParamMenu(cmdData, Msg, eSet, myAppVer)
        dlg.ShowDialog()
    End Sub

End Class
