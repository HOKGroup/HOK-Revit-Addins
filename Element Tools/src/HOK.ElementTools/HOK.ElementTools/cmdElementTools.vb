' (C) Copyright 2011 HOK SF
'
' Code originally managed by Don Rudder, SF BIM Manager
' Code modified by Jinsol Kim, HK Custom Application Developer 
' Added functions by Jinsol Kim: Create Views from Areas, Create Tagged Views from Areas


Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.Attributes

''' <summary>
''' Various Element Tools
''' </summary>
<Transaction(TransactionMode.Manual)> _
<Regeneration(RegenerationOption.Manual)> _
Public Class cmdElementTools

    Implements IExternalCommand

    Private m_Ver As String = "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

    Public Function Execute(ByVal commandData As ExternalCommandData, _
                            ByRef message As String, _
                            ByVal elements As ElementSet) As Result Implements IExternalCommand.Execute
        Try
            Dim myDlg As New form_ElemMenu(commandData, message, elements, m_Ver)
            myDlg.ShowDialog()

            Return Result.Succeeded
        Catch ex As Exception
            message = "HOK Customization Error: Element Tools Failed"
            Return Result.Failed
        End Try
    End Function
End Class
