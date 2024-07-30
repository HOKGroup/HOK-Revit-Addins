Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports System.Windows.Forms

Public Class clsUtilityProperties

    ''' <summary>
    ''' Return Property names for an element
    ''' </summary>
    ''' <param name="elem"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetPropNames(ByVal elem As Element) As List(Of String)
        Dim m_PropNames As New List(Of String)
        Try
            For Each p As Parameter In elem.Parameters
                m_PropNames.Add(p.Definition.Name)
            Next
            Return m_PropNames
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try
    End Function

End Class
