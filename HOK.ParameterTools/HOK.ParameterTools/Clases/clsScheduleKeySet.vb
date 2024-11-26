Public Class clsScheduleKeySet

    Private mName As String
    Private mParameterNames As New List(Of String)
    Private mScheduleKeyItems As New List(Of clsScheduleKeyItem)

    Public Sub New(ByVal name As String)
        mName = name
    End Sub

    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property
    Public Property ParameterNames() As List(Of String)
        Get
            Return mParameterNames
        End Get
        Set(ByVal value As List(Of String))
            mParameterNames = value
        End Set
    End Property
    Public Property ScheduleKeyItems() As List(Of clsScheduleKeyItem)
        Get
            Return mScheduleKeyItems
        End Get
        Set(ByVal value As List(Of clsScheduleKeyItem))
            mScheduleKeyItems = value
        End Set
    End Property

End Class
