Imports Autodesk.Revit.DB

''' <summary>
''' A simple class used to work with parameters 
''' </summary>
''' <remarks></remarks>
Public Class clsParaMini

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
    ''' The display unit type
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property DisplayUnitType() As String
        Get
            Try
#If 2022 Or 2023 Then
                Return m_parameter.Definition.GetDataType().ToString
#Else
                Return m_parameter.Definition.ParameterType.ToString
#End If
            Catch
                Return Nothing
            Catch
                Return Nothing
            End Try
        End Get
    End Property

    ''' <summary>
    ''' Get the value of a parameter
    ''' </summary>
    ''' <param name="parameter">Parameter</param>
    ''' <returns>String representing the value</returns>
    ''' <remarks></remarks>
    Public Shared Function GetParameterValue(ByVal parameter As Parameter) As String
        Select Case parameter.StorageType
            Case StorageType.[Double]
                Return parameter.AsDouble
            Case StorageType.ElementId
                Return parameter.AsElementId.IntegerValue
            Case StorageType.[Integer]
                Return parameter.AsInteger
            Case StorageType.None
                Return parameter.AsValueString
            Case StorageType.[String]
                Return parameter.AsString
            Case Else
                Return ""
        End Select
    End Function

End Class