Public Class form_ElemProgress
    Public Sub New(ByVal title As String, ByVal maximum As Integer)
        InitializeComponent()
        Me.Text = title
        progressBar1.Maximum = maximum
    End Sub
    Public Sub Increment()
        progressBar1.Increment(1)
    End Sub
    Public Sub Reset()
        progressBar1.Value = 0
        progressBar1.Refresh()
    End Sub
End Class