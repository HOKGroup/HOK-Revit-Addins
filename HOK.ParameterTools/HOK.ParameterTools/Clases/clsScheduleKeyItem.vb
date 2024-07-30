Imports Autodesk.Revit.DB

Public Class clsScheduleKeyItem

    Private mElementId As ElementId
    Private mKeyValue As String
    Private mValueSet As New List(Of String)

    Public Sub New(ByVal keyValue As String)
        mKeyValue = keyValue
    End Sub

    Public Property ElementId As ElementId
        Get
            Return mElementId
        End Get
        Set(ByVal value As ElementId)
            mElementId = value
        End Set
    End Property
    Public Property KeyValue As String
        Get
            Return mKeyValue
        End Get
        Set(ByVal value As String)
            mKeyValue = value
        End Set
    End Property
    Public Property ValueSet As List(Of String)
        Get
            Return mValueSet
        End Get
        Set(ByVal value As List(Of String))
            mValueSet = value
        End Set
    End Property

End Class
