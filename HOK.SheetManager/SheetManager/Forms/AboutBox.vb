Public NotInheritable Class AboutBox

    Private Sub AboutBox_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' Set the title of the form.
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        Me.Text = String.Format("About {0}", ApplicationTitle)
        ' Initialize all of the text displayed on the About Box.
        ' TODO: Customize the application's assembly information in the "Application" pane of the project 
        '    properties dialog (under the "Project" menu).
        Me.LabelProductName.Text = My.Application.Info.ProductName
        Me.LabelVersion.Text = String.Format("Version {0}", My.Application.Info.Version.ToString)
        Me.LabelCopyright.Text = My.Application.Info.Copyright
        Me.LabelCompanyName.Text = My.Application.Info.CompanyName
        Me.TextBoxDescription.Text = My.Application.Info.Description
    End Sub

    Private Sub OKButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OKButton.Click
        Me.Close()
    End Sub

    Private Sub linkContact_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles linkContact.LinkClicked
        Dim outlookApplication As Microsoft.Office.Interop.Outlook.Application = New Microsoft.Office.Interop.Outlook.Application
        Dim name As Microsoft.Office.Interop.Outlook.NameSpace = outlookApplication.GetNamespace("MAPI")
        Dim folderInbox As Microsoft.Office.Interop.Outlook.MAPIFolder = name.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderInbox)
        Dim mailItem As Microsoft.Office.Interop.Outlook.MailItem = DirectCast(outlookApplication.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem), Microsoft.Office.Interop.Outlook.MailItem)
        mailItem.Subject = "Revit Problem Report: Sheet Manager"
        mailItem.Body = "**** This email will go to the Firmwide [_HOK BIM Support Request] team. ****" & vbCrLf & "What office are you in?" & vbCrLf & "What project are you working on?" & vbCrLf & "Describe the problem:"
        mailItem.Recipients.Add("dan.siroky@hok.com")
        mailItem.Display(False)

    End Sub
End Class
