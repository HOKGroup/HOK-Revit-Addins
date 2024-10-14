Imports System.Windows.Forms
Imports System.IO

Public Class clsUtilityIni

    Private mIniFileName As String

    ''' <summary>
    ''' Class Constructor
    ''' </summary>
    ''' <param name="iniFileName"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal iniFileName As String)
        mIniFileName = iniFileName
    End Sub

    ''' <summary>
    ''' Write the settings to the INI
    ''' </summary>
    ''' <param name="iniPath"></param>
    ''' <param name="listSettings"></param>
    ''' <remarks></remarks>
    Public Sub WriteIniFile(ByVal iniPath As String, ByVal listSettings As List(Of String))
        If iniPath = "" Then
            Exit Sub
        End If
        'no warning, just no action
        Try
            'Note, if file doesn't exist it is created
            ' The using statement also closes the StreamWriter.
            Using sw As New StreamWriter(iniPath)
                For Each stringItem As String In listSettings
                    sw.WriteLine(stringItem)
                Next
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Read the settings from the INI file
    ''' </summary>
    ''' <param name="iniPath"></param>
    ''' <param name="listSettings"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReadIniFile(ByVal iniPath As String, ByRef listSettings As List(Of String)) As Boolean
        'Read the ini file if possible if not it will get written later when user sets database path
        Try
            If File.Exists(iniPath) Then
                listSettings.Clear()
                Dim input As String = ""
                Using sr As StreamReader = File.OpenText(iniPath)
                    For i As Integer = 0 To 999
                        'just to prevent endless loop
                        input = sr.ReadLine()
                        If input Is Nothing Then
                            Exit For
                        End If
                        listSettings.Add(input)
                    Next
                    sr.Close()
                End Using
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return False
        End Try
    End Function

End Class
