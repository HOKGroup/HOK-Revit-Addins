Imports Autodesk.Revit
Imports Autodesk.Revit.DB
Imports System.Windows.Forms

Public Class form_ElemSelectRoomTag

    Private mTextbox As System.Windows.Forms.TextBox

    Public Sub New(ByVal settings As clsSettings, ByRef textbox As System.Windows.Forms.TextBox)
        InitializeComponent()

        mTextbox = textbox
        Dim collector As FilteredElementCollector = New FilteredElementCollector(settings.Document)
        Dim filter As New ElementCategoryFilter(BuiltInCategory.OST_RoomTags)
        Dim roomTags As IList(Of Element) = collector.WherePasses(filter).WhereElementIsElementType().ToElements()
        'Fill the list box
        For Each tagType As DB.Architecture.RoomTagType In roomTags
            listBoxSelect.Items.Add(tagType.Name.ToString)
        Next
        If listBoxSelect.Items.Count = 0 Then
            MessageBox.Show("There are no room tags in project.  Command cannot be used.", settings.ProgramName)
            Close()
        End If
    End Sub

    Private Sub listBoxSelect_Click(ByVal sender As Object, ByVal e As EventArgs) Handles listBoxSelect.Click
        mTextbox.Text = listBoxSelect.SelectedItem.ToString
        Close()
    End Sub


    Private Sub buttonCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonCancel.Click
        Me.Close()
    End Sub
End Class