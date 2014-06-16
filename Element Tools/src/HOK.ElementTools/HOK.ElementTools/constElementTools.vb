Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

''' <summary>
''' Various Element Tools
''' </summary>
Public Class constElementTools

    Public Sub New(ByVal cmdData As ExternalCommandData, _
                   ByVal Msg As String, _
                   ByVal eSet As ElementSet, _
                   ByVal myAppVer As String)

        Dim dlg As New form_ElemMenu(cmdData, Msg, eSet, "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString)
        dlg.ShowDialog()

    End Sub

End Class
