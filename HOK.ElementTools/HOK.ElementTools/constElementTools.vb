Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI

''' <summary>
''' Various Element Tools
''' </summary>
Public Class constElementTools

    Public Sub New(ByVal settings As clsSettings,
                   ByVal myAppVer As String)

        Dim dlg As New form_ElemMenu(settings, "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString)
        dlg.ShowDialog()

    End Sub

End Class
