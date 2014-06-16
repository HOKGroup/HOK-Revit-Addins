Imports Autodesk.Revit
Imports Autodesk.Revit.UI

''' <summary>
''' Creates a Sheet Manager Application Class
''' </summary>
''' <remarks></remarks>
Public Class constSheetManager

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="cmdData"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal cmdData As ExternalCommandData)
        Dim m_Settings As New clsSettings(cmdData)
        Dim m_Dlg As New form_SheetManager(m_Settings)
        m_Dlg.ShowDialog()
    End Sub

End Class
