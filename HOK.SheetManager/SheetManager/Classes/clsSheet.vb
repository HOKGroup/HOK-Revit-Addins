Imports Autodesk.Revit.DB

Public Class clsSheet

    Private m_SheetParameters As New List(Of clsPara)
    Private m_Sheet As ViewSheet

    ''' <summary>
    ''' For data source sheet only
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal paras As List(Of clsPara), ByVal sht As ViewSheet)
        m_SheetParameters = paras
        m_Sheet = sht
    End Sub

    ''' <summary>
    ''' The Sheet Element
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Sheet As ViewSheet
        Get
            Return m_Sheet
        End Get
    End Property

    ''' <summary>
    ''' Parameters and/or properties
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Parameters As List(Of clsPara)
        Get
            Return m_SheetParameters
        End Get
    End Property

    ''' <summary>
    ''' Get a named para from the collection
    ''' </summary>
    ''' <param name="p_Name"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Parameter(ByVal p_Name As String) As clsPara
        Get
            If p_Name = "" Then Return Nothing
            For Each p As clsPara In m_SheetParameters
                If p.Name.ToUpper = "" Then Return p
            Next
            Return Nothing
        End Get
    End Property

End Class

Public Class clsTblk

    Private m_SheetParameters As New List(Of clsPara)
    Private m_Element As Element

    ''' <summary>
    ''' For data source TB only
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New(ByVal paras As List(Of clsPara), ByVal e As Element)
        m_SheetParameters = paras
        m_Element = e
    End Sub

    ''' <summary>
    ''' The TB Element
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Element As Element
        Get
            Return m_Element
        End Get
    End Property

    ''' <summary>
    ''' Parameters and/or properties
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Parameters As List(Of clsPara)
        Get
            Return m_SheetParameters
        End Get
    End Property

End Class

