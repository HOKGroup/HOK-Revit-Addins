''' <summary>
''' A simple progress floater
''' </summary>
''' <remarks></remarks>
Public Class form_Progress

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="maximum"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal maximum As Integer)
        InitializeComponent()
        progressBar1.Maximum = maximum
    End Sub

    ''' <summary>
    ''' Increment one step
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Increment()
        progressBar1.Increment(1)
    End Sub

    ''' <summary>
    ''' Reset the controls
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Reset()
        progressBar1.Value = 0
        progressBar1.Refresh()
    End Sub

End Class