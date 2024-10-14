''' <summary>
''' A SQL helper class for apostraphe handling and DBNULL
''' </summary>
''' <remarks></remarks>
Public Class clsUtilitySQL

    ''' <summary>
    ''' Empty string to NULL
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LiteralOrNull(ByVal input As String) As String
        If input = "" Then
            Return "NULL"
        Else
            Return "'" & input.Replace("'", "''") & "'"
        End If
    End Function

    ''' <summary>
    ''' Single apostraphe handling
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReplaceQuote(ByVal input As String) As String
        Return input.Replace("'", "''")
    End Function

End Class
