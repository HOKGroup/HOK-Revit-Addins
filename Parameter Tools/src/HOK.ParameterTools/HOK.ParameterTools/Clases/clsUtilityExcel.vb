Imports System.Windows.Forms
Imports System.IO
Imports Microsoft.Office.Interop

Public Class clsUtilityExcel

    Private m_ExcelSession As Excel.Application = New Excel.ApplicationClass

    ''' <summary>
    ''' Bind data to Excel
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function BindToExcel(ByVal path As String) As Excel.Application
        Dim m_Excel As Excel.Application
        Dim m_WB As Excel.Workbook
        If path.Trim() = "" Then
            Try
                m_WB = m_ExcelSession.Workbooks.Add(Type.Missing)
                While m_ExcelSession.Worksheets.Count > 1
                    Dim m_WS As Excel.Worksheet
                    m_WS = TryCast(m_ExcelSession.Worksheets(1), Excel.Worksheet)
                    m_WS.Delete()
                End While
                m_Excel = m_WB.Application
                m_Excel.Visible = True
                Return m_Excel
            Catch exception As Exception
                MessageBox.Show("Unable to start Excel session with file. " & _
                                vbCr & vbCr & _
                                "System Error Message: " & vbCr & _
                                exception.Message, "Error")
                Return Nothing
            End Try
        Else
            If Not File.Exists(path) Then
                MessageBox.Show("Unable to locate file: " & path & ".", "Error")
                Return Nothing
            End If
            Try
                m_WB = DirectCast(System.Runtime.InteropServices.Marshal.BindToMoniker(path), Excel.Workbook)
                If m_WB.Application.ActiveWorkbook Is Nothing Then
                    Dim m_FileName As String
                    m_FileName = path.Substring(path.LastIndexOf("\") + 1)
                    m_WB.Application.Windows(m_FileName).Visible = True
                End If
                m_Excel = m_WB.Application
                m_Excel.Visible = True
                Return m_Excel
            Catch exception As Exception
                MessageBox.Show("Unable to find or start Excel with file: " & path & ". " & _
                                vbLf & vbLf & "System Error Message: " & vbLf & _
                                exception.Message, "Error")
                Return Nothing
            End Try
        End If
    End Function

    ''' <summary>
    ''' Launch an Excel Session
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LaunchExcel() As Excel.Application
        Dim m_Worksheet As Excel.Worksheet
        Dim m_Workbook As Excel.Workbook = m_ExcelSession.Workbooks.Add(Type.Missing)
        Try
            If m_ExcelSession Is Nothing Then
                Return Nothing
            End If
            While m_ExcelSession.Worksheets.Count > 1
                m_Worksheet = TryCast(m_ExcelSession.Worksheets(1), Excel.Worksheet)
                m_Worksheet.Delete()
            End While
            Return m_ExcelSession
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Add a Worksheet to the Workbook
    ''' </summary>
    ''' <param name="p_isFirst"></param>
    ''' <param name="p_ShtName"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function AddWorksheet(ByVal p_isFirst As Boolean, ByVal p_ShtName As String) As Excel.Worksheet
        Dim worksheet As Excel.Worksheet
        Try
            If p_isFirst Then
                worksheet = TryCast(m_ExcelSession.ActiveSheet, Excel.Worksheet)
            Else
                worksheet = TryCast(m_ExcelSession.Worksheets.Add(Type.Missing, _
                            m_ExcelSession.Worksheets(m_ExcelSession.Worksheets.Count), _
                            Type.Missing, Type.Missing), Excel.Worksheet)
            End If
            If worksheet Is Nothing Then
                Return Nothing
            End If
            worksheet.Name = GetUniqueWSName(p_ShtName)
            Return worksheet
        Catch ex As Exception
            MessageBox.Show(ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Get the Unique Sheet Name
    ''' </summary>
    ''' <param name="p_Name"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetUniqueWSName(ByVal p_Name As String) As String
        Dim nameNotUnique As Boolean = False
        Dim worksheetName As String = p_Name
        While nameNotUnique
            nameNotUnique = False
            Dim increment As Integer = 2
            For Each ws As Excel.Worksheet In m_ExcelSession.Worksheets
                If ws.Name = worksheetName Then
                    nameNotUnique = True
                    Dim stringIncrement As String
                    stringIncrement = increment.ToString
                    worksheetName = (p_Name & " (") + stringIncrement & ")"
                    increment += 1
                    Continue For
                End If
            Next
        End While
        Return worksheetName
    End Function

End Class
