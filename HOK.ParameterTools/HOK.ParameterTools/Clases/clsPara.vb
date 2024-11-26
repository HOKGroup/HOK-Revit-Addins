Imports Autodesk.Revit.DB

''' <summary>
''' A simple class used to work with parameters 
''' </summary>
''' <remarks></remarks>
Public Class clsPara

    Private m_parameter As Parameter
    Private m_PropName As String
    Private m_PropValue As String

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="parameter"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal parameter As Parameter)
        m_parameter = parameter
    End Sub

    ''' <summary>
    ''' For generating from a datasource (no element yet)
    ''' </summary>
    ''' <param name="pName"></param>
    ''' <param name="pValue"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal pName As String, ByVal pValue As String)
        m_PropName = pName
        m_PropValue = pValue
    End Sub

    Public ReadOnly Property DataSourceName As String
        Get
            Return m_PropName
        End Get
    End Property

    Public ReadOnly Property DataSourceValue As String
        Get
            Return m_PropValue
        End Get
    End Property

    ''' <summary>
    ''' The display unit type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DisplayUnitType() As String
        Get
            Try
#If REVIT2022_OR_GREATER Then
                Return m_parameter.Definition.GetDataType().ToString
#Else
                Return m_parameter.Definition.ParameterType.ToString
#End If
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' The parameter reference.
    ''' The element the parameter belongs to is accessible from this object
    ''' </summary>
    ''' <value></value>
    ''' <returns>DB.Parameter of para, to gain access to its parent element</returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ParameterObject() As Parameter
        Get
            Try
                Return m_parameter
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Is it a read only parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ParameterIsReadOnly() As Boolean
        Get
            Try
                Return m_parameter.IsReadOnly
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Is this a shared parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property ParameterIsShared() As Boolean
        Get
            Try
                Return m_parameter.IsShared
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' The type of parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Type() As String
        Get
            Try
                Return m_parameter.GetType.Name
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' The name of the parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Name() As String
        Get
            Try
                Return m_parameter.Definition.Name
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Gets the parameter format
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Format() As String
        Get
            Try
                Return m_parameter.StorageType.ToString
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Returns value of the parameter
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Value() As String
        Get
            Try
                Return GetParameterValue(m_parameter)
            Catch
                Return Nothing
            End Try
        End Get
        Set(ByVal value As String)
            Try
                SetParameterValue(m_parameter, value)
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Get the value of a parameter
    ''' </summary>
    ''' <param name="parameter">Parameter</param>
    ''' <returns>String representing the value</returns>
    ''' <remarks></remarks>
    Public Shared Function GetParameterValue(ByVal parameter As Parameter) As String
        Select Case parameter.StorageType
            Case StorageType.Double
                Return parameter.AsValueString
            Case StorageType.ElementId

                Dim elementId As ElementId
                elementId = parameter.AsElementId
                Dim paramElement As Element = parameter.Element.Document.GetElement(elementId)
                If paramElement IsNot Nothing Then
                    Return paramElement.Name
                Else
                    Return ""
                End If

            Case StorageType.Integer
                Return parameter.AsInteger
            Case StorageType.None
                Return parameter.AsValueString
            Case StorageType.String
                Return parameter.AsString
            Case Else
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' Set a value to a parameter
    ''' </summary>
    ''' <param name="parameter">Parameter</param>
    ''' <param name="value">Value does not have to be a string, Object</param>
    ''' <remarks></remarks>
    Public Shared Sub SetParameterValue(ByVal parameter As Parameter, ByVal value As Object)
        ' Don't do anything with Read-Only parameters 
        If parameter.IsReadOnly Then Exit Sub
        Try
            Select Case parameter.StorageType
                Case StorageType.Double
                    Dim m_Double As Double = value
                    parameter.Set(m_Double)
                Case StorageType.ElementId
                    Dim myElementId As ElementId = DirectCast(value, ElementId)
                    parameter.[Set](myElementId)
                Case StorageType.Integer
                    Dim m_Int As Integer = 0
                    Select Case value.ToString.ToUpper
                        Case "0"
                            m_Int = 0
                        Case "1"
                            m_Int = 1
                        Case "N"
                            m_Int = 0
                        Case "Y"
                            m_Int = 1
                        Case "NO"
                            m_Int = 0
                        Case "YES"
                            m_Int = 1
                        Case ""
                            m_Int = 0
                        Case "X"
                            m_Int = 1
                        Case Else
                            m_Int = value.ToString
                    End Select
                    parameter.Set(m_Int)
                Case StorageType.String
                    parameter.Set(TryCast(value, String))
                Case Else
            End Select
        Catch ex As Exception
            ' Do nothing
        End Try
    End Sub

End Class