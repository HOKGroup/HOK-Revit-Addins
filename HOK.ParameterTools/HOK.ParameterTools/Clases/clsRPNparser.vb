'' ''Imports System.Collections
'' ''Imports System.Text
'' ''Imports System.Text.RegularExpressions

'' ''#Region "RPN"
'' '' ''' <summary>
'' '' ''' Summary description for RPNParser.
'' '' ''' </summary>
'' ''Public Class RPNParser
'' ''    Public Sub New()
'' ''    End Sub
'' ''    Public Function EvaluateExpression(ByVal szExpr As String, ByVal varType As Type, ByVal bFormula As Boolean, ByVal htValues As Hashtable) As Object
'' ''        Dim arrExpr As ArrayList = GetPostFixNotation(szExpr, varType, bFormula)
'' ''        Return EvaluateRPN(arrExpr, varType, htValues)
'' ''    End Function

'' ''#Region "RPN_Parser"

'' ''    ''' <summary>
'' ''    ''' Algo of GetPostFixNotation (source : Expression Evaluator : using RPN by lallous 
'' ''    ''' in the C++/MFC section of www.CodeProject.com.
'' ''    ''' 1.	Initialize an empty stack (string stack), prepare input infix expression and clear RPN string 
'' ''    '''	2.	Repeat until we reach end of infix expression 
'' ''    '''		I.	Get token (operand or operator); skip white spaces 
'' ''    '''		II.	If token is: 
'' ''    '''			a.	Left parenthesis: Push it into stack 
'' ''    '''			b.	Right parenthesis: Keep popping from the stack and appending to 
'' ''    '''				RPN string until we reach the left parenthesis.
'' ''    '''				If stack becomes empty and we didn't reach the left parenthesis 
'' ''    '''				then break out with error "Unbalanced parenthesis" 
'' ''    '''			c.	Operator: If stack is empty or operator has a higher precedence than 
'' ''    '''				the top of the stack then push operator into stack. 
'' ''    '''				Else if operator has lower precedence then we keep popping and 
'' ''    '''				appending to RPN string, this is repeated until operator in stack 
'' ''    '''				has lower precedence than the current operator. 
'' ''    '''			d.	An operand: we simply append it to RPN string. 
'' ''    '''		III.	When the infix expression is finished, we start popping off the stack and 
'' ''    '''				appending to RPN string till stack becomes empty. 
'' ''    ''' </summary>
'' ''    ''' <param name="szExpr"></param>
'' ''    ''' <param name="varType"></param>
'' ''    ''' <param name="bFormula"></param>
'' ''    ''' <returns></returns>
'' ''    ''' <remarks></remarks>
'' ''    Public Function GetPostFixNotation(ByVal szExpr As String, ByVal varType As Type, ByVal bFormula As Boolean) As ArrayList
'' ''        Dim stkOp As New Stack()
'' ''        Dim arrFinalExpr As New ArrayList()
'' ''        Dim szResult As String = ""

'' ''        Dim tknzr As New Tokenizer(szExpr)
'' ''        For Each token As Token In tknzr
'' ''            Dim szToken As String = token.Value.Trim()
'' ''            If szToken.Length = 0 Then
'' ''                Continue For
'' ''            End If
'' ''            If Not OperatorHelper.IsOperator(szToken) Then
'' ''                Dim oprnd As Operand = OperandHelper.CreateOperand(szToken, varType)
'' ''                oprnd.ExtractAndSetValue(szToken, bFormula)
'' ''                arrFinalExpr.Add(oprnd)

'' ''                szResult += szToken
'' ''                Continue For
'' ''            End If
'' ''            Dim szOp As String = szToken
'' ''            If szOp = "(" Then
'' ''                stkOp.Push(szOp)
'' ''            ElseIf szOp = ")" Then
'' ''                Dim szTop As String
'' ''                While (InlineAssignHelper(szTop, DirectCast(stkOp.Pop(), String))) <> "("
'' ''                    Dim oprtr As IOperator = OperatorHelper.CreateOperator(szTop)
'' ''                    arrFinalExpr.Add(oprtr)

'' ''                    szResult += szTop

'' ''                    If stkOp.Count = 0 Then
'' ''                        Throw New RPN_Exception("Unmatched braces!")
'' ''                    End If
'' ''                End While
'' ''            Else
'' ''                If stkOp.Count = 0 OrElse DirectCast(stkOp.Peek(), String) = "(" OrElse OperatorHelper.IsHigherPrecOperator(szOp, DirectCast(stkOp.Peek(), String)) Then
'' ''                    stkOp.Push(szOp)
'' ''                Else
'' ''                    While stkOp.Count <> 0
'' ''                        If OperatorHelper.IsLowerPrecOperator(szOp, DirectCast(stkOp.Peek(), String)) OrElse OperatorHelper.IsEqualPrecOperator(szOp, DirectCast(stkOp.Peek(), String)) Then
'' ''                            Dim szTop As String = DirectCast(stkOp.Peek(), String)
'' ''                            If szTop = "(" Then
'' ''                                Exit While
'' ''                            End If
'' ''                            szTop = DirectCast(stkOp.Pop(), String)

'' ''                            Dim oprtr As IOperator = OperatorHelper.CreateOperator(szTop)
'' ''                            arrFinalExpr.Add(oprtr)

'' ''                            szResult += szTop
'' ''                        Else
'' ''                            Exit While
'' ''                        End If
'' ''                    End While
'' ''                    stkOp.Push(szOp)
'' ''                End If
'' ''            End If
'' ''        Next
'' ''        While stkOp.Count <> 0
'' ''            Dim szTop As String = DirectCast(stkOp.Pop(), String)
'' ''            If szTop = "(" Then
'' ''                Throw New RPN_Exception("Unmatched braces")
'' ''            End If

'' ''            Dim oprtr As IOperator = OperatorHelper.CreateOperator(szTop)
'' ''            arrFinalExpr.Add(oprtr)

'' ''            szResult += szTop
'' ''        End While
'' ''        Return arrFinalExpr
'' ''    End Function

'' ''#End Region

'' ''    Public Function Convert2String(ByVal arrExpr As ArrayList) As String
'' ''        Dim szResult As String = ""
'' ''        For Each obj As Object In arrExpr
'' ''            szResult += obj.ToString()
'' ''        Next
'' ''        Return szResult
'' ''    End Function


'' ''#Region "RPN_Evaluator"
'' ''    ''' <summary>
'' ''    ''' Algo of EvaluateRPN (source : Expression Evaluator : using RPN by lallous 
'' ''    ''' in the C++/MFC section of www.CodeProject.com.
'' ''    ''' 1.	Initialize stack for storing results, prepare input postfix (or RPN) expression. 
'' ''    '''	2.	Start scanning from left to right till we reach end of RPN expression 
'' ''    '''	3.	Get token, if token is: 
'' ''    '''		I.	An operator: 
'' ''    '''			a.	Get top of stack and store into variable op2; Pop the stack 
'' ''    '''			b.	Get top of stack and store into variable op1; Pop the stack 
'' ''    '''			c.	Do the operation expression in operator on both op1 and op2 
'' ''    '''			d.	Push the result into the stack 
'' ''    '''		II.	An operand: stack its numerical representation into our numerical stack. 
'' ''    '''	4.	At the end of the RPN expression, the stack should only have one value and 
'' ''    '''	that should be the result and can be retrieved from the top of the stack.
'' ''    ''' </summary>
'' ''    ''' <param name="arrExpr"></param>
'' ''    ''' <param name="varType"></param>
'' ''    ''' <param name="htValues"></param>
'' ''    ''' <returns></returns>
'' ''    ''' <remarks></remarks>
'' ''    Public Function EvaluateRPN(ByVal arrExpr As ArrayList, ByVal varType As Type, ByVal htValues As Hashtable) As Object
'' ''        ' initialize stack (integer stack) for results
'' ''        Dim stPad As New Stack()
'' ''        ' begin loop : scan from left to right till end of RPN expression
'' ''        For Each var As Object In arrExpr
'' ''            Dim op1 As Operand = Nothing
'' ''            Dim op2 As Operand = Nothing
'' ''            Dim oprtr As IOperator = Nothing
'' ''            ' Get token
'' ''            ' if token is 
'' ''            If TypeOf var Is IOperand Then
'' ''                ' Operand : push onto top of numerical stack
'' ''                stPad.Push(var)
'' ''            ElseIf TypeOf var Is IOperator Then
'' ''                ' Operator :	
'' ''                '		Pop top of stack into var 1 - op2 first as top of stack is rhs
'' ''                op2 = DirectCast(stPad.Pop(), Operand)
'' ''                If htValues IsNot Nothing Then
'' ''                    op2.Value = htValues(op2.Name)
'' ''                End If
'' ''                '		Pop top of stack into var 2
'' ''                op1 = DirectCast(stPad.Pop(), Operand)
'' ''                If htValues IsNot Nothing Then
'' ''                    op1.Value = htValues(op1.Name)
'' ''                End If
'' ''                '		Do operation exp for 'this' operator on var1 and var2
'' ''                oprtr = DirectCast(var, IOperator)
'' ''                Dim opRes As IOperand = oprtr.Eval(op1, op2)
'' ''                '		Push results onto stack
'' ''                stPad.Push(opRes)
'' ''            End If
'' ''        Next
'' ''        '	end loop
'' ''        ' stack ends up with single value with final result
'' ''        Return DirectCast(stPad.Pop(), Operand).Value
'' ''    End Function
'' ''    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
'' ''        target = value
'' ''        Return value
'' ''    End Function
'' ''#End Region
'' ''End Class
'' ''#End Region

'' ''#Region "UtilClasses"

'' '' ''' <summary>
'' '' ''' The given expression can be parsed as either Arithmetic or Logical or 
'' '' ''' Comparison ExpressionTypes.  This is controlled by the enums 
'' '' ''' ExpressionType::ET_ARITHMETIC, ExpressionType::ET_COMPARISON and
'' '' ''' ExpressionType::ET_LOGICAL.  A combination of these enum types can also be given.
'' '' ''' E.g. To parse the expression as all of these, pass 
'' '' ''' ExpressionType.ET_ARITHMETIC|ExpressionType.ET_COMPARISON|ExpressionType.ET_LOGICAL 
'' '' ''' to the Tokenizer c'tor.
'' '' ''' </summary>
'' ''<Flags()> _
'' ''Public Enum ExpressionType
'' ''    ET_ARITHMETIC = &H1
'' ''    ET_COMPARISON = &H2
'' ''    ET_LOGICAL = &H4
'' ''End Enum
'' '' ''' <summary>
'' '' ''' Currently not used.
'' '' ''' </summary>
'' ''Public Enum TokenType
'' ''    TT_OPERATOR
'' ''    TT_OPERAND
'' ''End Enum
'' '' ''' <summary>
'' '' ''' Represents each token in the expression
'' '' ''' </summary>
'' ''Public Class Token
'' ''    Public Sub New(ByVal szValue As String)
'' ''        m_szValue = szValue
'' ''    End Sub
'' ''    Public ReadOnly Property Value() As String
'' ''        Get
'' ''            Return m_szValue
'' ''        End Get
'' ''    End Property
'' ''    Private m_szValue As String
'' ''End Class
'' '' ''' <summary>
'' '' ''' Is the tokenizer which does the actual parsing of the expression.
'' '' ''' </summary>
'' ''Public Class Tokenizer
'' ''    Implements IEnumerable
'' ''    Public Sub New(ByVal szExpression As String)
'' ''        Me.New(szExpression, ExpressionType.ET_ARITHMETIC Or ExpressionType.ET_COMPARISON Or ExpressionType.ET_LOGICAL)
'' ''    End Sub
'' ''    Public Sub New(ByVal szExpression As String, ByVal exType As ExpressionType)
'' ''        m_szExpression = szExpression
'' ''        m_exType = exType
'' ''        m_RegEx = New Regex(OperatorHelper.GetOperatorsRegEx(m_exType))
'' ''        m_strarrTokens = SplitExpression(szExpression)
'' ''    End Sub
'' ''    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
'' ''        Return New TokenEnumerator(m_strarrTokens)
'' ''    End Function
'' ''    Public Function SplitExpression(ByVal szExpression As String) As String()
'' ''        Return m_RegEx.Split(szExpression)
'' ''    End Function
'' ''    Private m_exType As ExpressionType
'' ''    Private m_szExpression As String
'' ''    Private m_strarrTokens As String()
'' ''    Private m_RegEx As Regex
'' ''End Class

'' '' ''' <summary>
'' '' ''' Enumerator to enumerate over the tokens.
'' '' ''' </summary>
'' ''Public Class TokenEnumerator
'' ''    Implements IEnumerator
'' ''    Private m_Token As Token
'' ''    Private m_nIdx As Integer
'' ''    Private m_strarrTokens As String()

'' ''    Public Sub New(ByVal strarrTokens As String())
'' ''        m_strarrTokens = strarrTokens
'' ''        Reset()
'' ''    End Sub
'' ''    Public ReadOnly Property Current() As Object Implements IEnumerator.Current
'' ''        Get
'' ''            Return m_Token
'' ''        End Get
'' ''    End Property
'' ''    Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
'' ''        If m_nIdx >= m_strarrTokens.Length Then
'' ''            Return False
'' ''        End If

'' ''        m_Token = New Token(m_strarrTokens(m_nIdx))
'' ''        m_nIdx += 1
'' ''        Return True
'' ''    End Function
'' ''    Public Sub Reset() Implements IEnumerator.Reset
'' ''        m_nIdx = 0
'' ''    End Sub
'' ''End Class

'' ''#Region "Exceptions"
'' '' ''' <summary>
'' '' ''' For the exceptions thrown by this module.
'' '' ''' </summary>
'' ''Public Class RPN_Exception
'' ''    Inherits ApplicationException
'' ''    Public Sub New()
'' ''    End Sub
'' ''    Public Sub New(ByVal szMessage As String)
'' ''        MyBase.New(szMessage)
'' ''    End Sub
'' ''    Public Sub New(ByVal szMessage As String, ByVal innerException As Exception)
'' ''        MyBase.New(szMessage, innerException)
'' ''    End Sub
'' ''End Class
'' ''#End Region

'' ''#End Region

'' ''#Region "Interfaces"
'' ''Public Interface IOperand
'' ''End Interface
'' ''Public Interface IOperator
'' ''    Function Eval(ByVal lhs As IOperand, ByVal rhs As IOperand) As IOperand
'' ''End Interface

'' ''Public Interface IArithmeticOperations
'' ''    ' to support {"+", "-", "*", "/", "%"} operators
'' ''    Function Plus(ByVal rhs As IOperand) As IOperand
'' ''    Function Minus(ByVal rhs As IOperand) As IOperand
'' ''    Function Multiply(ByVal rhs As IOperand) As IOperand
'' ''    Function Divide(ByVal rhs As IOperand) As IOperand
'' ''    Function Modulo(ByVal rhs As IOperand) As IOperand
'' ''End Interface
'' ''Public Interface IComparisonOperations
'' ''    ' to support {"==", "!=","<", "<=", ">", ">="} operators
'' ''    Function EqualTo(ByVal rhs As IOperand) As IOperand
'' ''    Function NotEqualTo(ByVal rhs As IOperand) As IOperand
'' ''    Function LessThan(ByVal rhs As IOperand) As IOperand
'' ''    Function LessThanOrEqualTo(ByVal rhs As IOperand) As IOperand
'' ''    Function GreaterThan(ByVal rhs As IOperand) As IOperand
'' ''    Function GreaterThanOrEqualTo(ByVal rhs As IOperand) As IOperand
'' ''End Interface
'' ''Public Interface ILogicalOperations
'' ''    ' to support {"||", "&&" } operators
'' ''    Function [OR](ByVal rhs As IOperand) As IOperand
'' ''    Function [AND](ByVal rhs As IOperand) As IOperand
'' ''End Interface
'' ''#End Region

'' ''#Region "Operands"
'' '' ''' <summary>
'' '' ''' Base class for all Operands.  Provides datastorage
'' '' ''' </summary>
'' ''Public MustInherit Class Operand
'' ''    Implements IOperand
'' ''    Public Sub New(ByVal szVarName As String, ByVal varValue As Object)
'' ''        m_szVarName = szVarName
'' ''        m_VarValue = varValue
'' ''    End Sub
'' ''    Public Sub New(ByVal szVarName As String)
'' ''        m_szVarName = szVarName
'' ''    End Sub
'' ''    Public Overrides Function ToString() As String
'' ''        Return m_szVarName
'' ''    End Function
'' ''    Public MustOverride Sub ExtractAndSetValue(ByVal szValue As String, ByVal bFormula As Boolean)
'' ''    Public Property Name() As String
'' ''        Get
'' ''            Return m_szVarName
'' ''        End Get
'' ''        Set(ByVal value As String)
'' ''            m_szVarName = value
'' ''        End Set
'' ''    End Property
'' ''    Public Property Value() As Object
'' ''        Get
'' ''            Return m_VarValue
'' ''        End Get
'' ''        Set(ByVal value As Object)
'' ''            m_VarValue = value
'' ''        End Set
'' ''    End Property
'' ''    Protected m_szVarName As String = ""
'' ''    Protected m_VarValue As Object = Nothing
'' ''End Class

'' '' ''' <summary>
'' '' ''' Operand corresponding to the Long (Int32/Int64) datatypes.
'' '' ''' </summary>
'' ''Public Class LongOperand
'' ''    Inherits Operand
'' ''    Implements IArithmeticOperations
'' ''    Implements IComparisonOperations
'' ''    Public Sub New(ByVal szVarName As String, ByVal varValue As Object)
'' ''        MyBase.New(szVarName, varValue)
'' ''    End Sub
'' ''    Public Sub New(ByVal szVarName As String)
'' ''        MyBase.New(szVarName)
'' ''    End Sub
'' ''    Public Overrides Function ToString() As String
'' ''        Return m_szVarName
'' ''    End Function
'' ''    Public Overrides Sub ExtractAndSetValue(ByVal szValue As String, ByVal bFormula As Boolean)
'' ''        m_VarValue = If(Not bFormula, Convert.ToInt64(szValue), 0)
'' ''    End Sub
'' ''    ''' IArithmeticOperations methods.  Return of these methods is again a LongOperand
'' ''    Public Function Plus(ByVal rhs As IOperand) As IOperand Implements IArithmeticOperations.Plus
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.Plus : rhs")
'' ''        End If
'' ''        Dim oprResult As New LongOperand("Result", Type.[GetType]("System.Int64"))
'' ''        oprResult.Value = CLng(Me.Value) + CLng(DirectCast(rhs, Operand).Value)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function Minus(ByVal rhs As IOperand) As IOperand Implements IArithmeticOperations.Minus
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.Minus : rhs")
'' ''        End If
'' ''        Dim oprResult As New LongOperand("Result", Type.[GetType]("System.Int64"))
'' ''        oprResult.Value = CLng(Me.Value) - CLng(DirectCast(rhs, Operand).Value)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function Multiply(ByVal rhs As IOperand) As IOperand Implements IArithmeticOperations.Multiply
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New ArgumentException("Argument invalid in LongOperand.Multiply : rhs")
'' ''        End If
'' ''        Dim oprResult As New LongOperand("Result", Type.[GetType]("System.Int64"))
'' ''        oprResult.Value = CLng(Me.Value) * CLng(DirectCast(rhs, Operand).Value)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function Divide(ByVal rhs As IOperand) As IOperand Implements IArithmeticOperations.Divide
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.Divide : rhs")
'' ''        End If
'' ''        Dim oprResult As New LongOperand("Result", Type.[GetType]("System.Int64"))
'' ''        oprResult.Value = CLng(Me.Value) \ CLng(DirectCast(rhs, Operand).Value)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function Modulo(ByVal rhs As IOperand) As IOperand Implements IArithmeticOperations.Modulo
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.Modulo : rhs")
'' ''        End If
'' ''        Dim oprResult As New LongOperand("Result", Type.[GetType]("System.Int64"))
'' ''        oprResult.Value = CLng(Me.Value) Mod CLng(DirectCast(rhs, Operand).Value)
'' ''        Return oprResult
'' ''    End Function

'' ''    ''' IComparisonOperators methods.  Return values are always BooleanOperands type
'' ''    Public Function EqualTo(ByVal rhs As IOperand) As IOperand Implements IComparisonOperations.EqualTo
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.== : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = CLng(Me.Value) = CLng(DirectCast(rhs, Operand).Value)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function NotEqualTo(ByVal rhs As IOperand) As IOperand Implements IComparisonOperations.NotEqualTo
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.!= : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CLng(Me.Value) <> CLng(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function LessThan(ByVal rhs As IOperand) As IOperand Implements IComparisonOperations.LessThan
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.< : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CLng(Me.Value) < CLng(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function LessThanOrEqualTo(ByVal rhs As IOperand) As IOperand Implements IComparisonOperations.LessThanOrEqualTo
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.<= : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CLng(Me.Value) <= CLng(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function GreaterThan(ByVal rhs As IOperand) As IOperand Implements IComparisonOperations.GreaterThan
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.> : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CLng(Me.Value) > CLng(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function GreaterThanOrEqualTo(ByVal rhs As IOperand) As IOperand Implements IComparisonOperations.GreaterThanOrEqualTo
'' ''        If Not (TypeOf rhs Is LongOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in LongOperand.>= : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CLng(Me.Value) >= CLng(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''End Class
'' '' ''' <summary>
'' '' ''' Operand corresponding to Boolean Type
'' '' ''' </summary>
'' ''Public Class BoolOperand
'' ''    Inherits Operand
'' ''    Implements ILogicalOperations
'' ''    Public Sub New(ByVal szVarName As String, ByVal varValue As Object)
'' ''        MyBase.New(szVarName, varValue)
'' ''    End Sub
'' ''    Public Sub New(ByVal szVarName As String)
'' ''        MyBase.New(szVarName)
'' ''    End Sub
'' ''    Public Overrides Function ToString() As String
'' ''        Return Me.Value.ToString()
'' ''    End Function
'' ''    Public Overrides Sub ExtractAndSetValue(ByVal szValue As String, ByVal bFormula As Boolean)
'' ''        m_VarValue = If(Not bFormula, Convert.ToBoolean(szValue), False)
'' ''    End Sub
'' ''    Public Function [AND](ByVal rhs As IOperand) As IOperand Implements ILogicalOperations.[AND]
'' ''        If Not (TypeOf rhs Is BoolOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in BoolOperand.&& : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CBool(Me.Value) AndAlso CBool(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''    Public Function [OR](ByVal rhs As IOperand) As IOperand Implements ILogicalOperations.[OR]
'' ''        If Not (TypeOf rhs Is BoolOperand) Then
'' ''            Throw New RPN_Exception("Argument invalid in BoolOperand.|| : rhs")
'' ''        End If
'' ''        Dim oprResult As New BoolOperand("Result")
'' ''        oprResult.Value = If((CBool(Me.Value) OrElse CBool(DirectCast(rhs, Operand).Value)), True, False)
'' ''        Return oprResult
'' ''    End Function
'' ''End Class

'' ''Public Class OperandHelper
'' ''    ''' <summary>
'' ''    ''' Factory method to create corresponding Operands.
'' ''    ''' Extended this method to create newer datatypes.
'' ''    ''' </summary>
'' ''    ''' <param name="szVarName"></param>
'' ''    ''' <param name="varType"></param>
'' ''    ''' <param name="varValue"></param>
'' ''    ''' <returns></returns>
'' ''    Public Shared Function CreateOperand(ByVal szVarName As String, ByVal varType As Type, ByVal varValue As Object) As Operand
'' ''        Dim oprResult As Operand = Nothing
'' ''        Select Case varType.ToString()
'' ''            Case "System.Int32", "System.Int64"
'' ''                oprResult = New LongOperand(szVarName, varValue)
'' ''                Return oprResult
'' ''                'case System.Decimal:
'' ''                'case System.Single:
'' ''                '	oprResult = new DecimalOperand( szVarName, varValue );
'' ''                '	return oprResult;
'' ''                '	break;
'' ''        End Select
'' ''        Throw New RPN_Exception("Unhandled type : " & varType.ToString())
'' ''    End Function
'' ''    Public Shared Function CreateOperand(ByVal szVarName As String, ByVal varType As Type) As Operand
'' ''        Return OperandHelper.CreateOperand(szVarName, varType, Nothing)
'' ''    End Function

'' ''End Class

'' ''#End Region

'' ''#Region "Operators"
'' '' ''' <summary>
'' '' ''' Base class of all operators.  Provides datastorage
'' '' ''' </summary>
'' ''Public MustInherit Class [Operator]
'' ''    Implements IOperator

'' ''    Public Sub New(ByVal cOperator As Char)
'' ''        m_szOperator = New [String](cOperator, 1)
'' ''    End Sub
'' ''    Public Sub New(ByVal szOperator As String)
'' ''        m_szOperator = szOperator
'' ''    End Sub
'' ''    Public Overrides Function ToString() As String
'' ''        Return m_szOperator
'' ''    End Function

'' ''    '' ''Public MustOverride Function Eval(ByVal lhs As IOperand, ByVal rhs As IOperand) As IOperand

'' ''    Public Property Value() As String
'' ''        Get
'' ''            Return m_szOperator
'' ''        End Get
'' ''        Set(ByVal value As String)
'' ''            m_szOperator = value
'' ''        End Set
'' ''    End Property
'' ''    Protected m_szOperator As String = ""

'' ''    Public Overridable Function Eval(ByVal lhs As IOperand, ByVal rhs As IOperand) As IOperand Implements IOperator.Eval

'' ''    End Function
'' ''End Class

'' '' ''' <summary>
'' '' ''' Arithmetic Operator Class providing evaluation services for "+-/*%" operators.
'' '' ''' </summary>
'' ''Public Class ArithmeticOperator
'' ''    Inherits [Operator]

'' ''    Public Sub New(ByVal cOperator As Char)
'' ''        MyBase.New(cOperator)
'' ''    End Sub

'' ''    Public Sub New(ByVal szOperator As String)
'' ''        MyBase.New(szOperator)
'' ''    End Sub

'' ''    Public Overrides Function Eval(ByVal lhs As IOperand, ByVal rhs As IOperand) As IOperand
'' ''        If Not (TypeOf lhs Is IArithmeticOperations) Then
'' ''            Throw New RPN_Exception("Argument invalid in ArithmeticOperator.Eval - Invalid Expression : lhs")
'' ''        End If
'' ''        Select Case m_szOperator
'' ''            Case "+"
'' ''                Return DirectCast(lhs, IArithmeticOperations).Plus(rhs)
'' ''            Case "-"
'' ''                Return DirectCast(lhs, IArithmeticOperations).Minus(rhs)
'' ''            Case "*"
'' ''                Return DirectCast(lhs, IArithmeticOperations).Multiply(rhs)
'' ''            Case "/"
'' ''                Return DirectCast(lhs, IArithmeticOperations).Divide(rhs)
'' ''            Case "%"
'' ''                Return DirectCast(lhs, IArithmeticOperations).Modulo(rhs)
'' ''        End Select
'' ''        Throw New RPN_Exception("Unsupported Arithmetic operation " & m_szOperator)
'' ''    End Function
'' ''End Class

'' ''Public Class ComparisonOperator
'' ''    Inherits [Operator]
'' ''    Public Sub New(ByVal cOperator As Char)
'' ''        MyBase.New(cOperator)
'' ''    End Sub
'' ''    Public Sub New(ByVal szOperator As String)
'' ''        MyBase.New(szOperator)
'' ''    End Sub

'' ''    Public Overrides Function Eval(ByVal lhs As IOperand, ByVal rhs As IOperand) As IOperand
'' ''        If Not (TypeOf lhs Is IComparisonOperations) Then
'' ''            Throw New RPN_Exception("Argument invalid in ComparisonOperator.Eval - Invalid Expression : lhs")
'' ''        End If
'' ''        Select Case m_szOperator
'' ''            Case "=="
'' ''                Return DirectCast(lhs, IComparisonOperations).EqualTo(rhs)
'' ''            Case "!="
'' ''                Return DirectCast(lhs, IComparisonOperations).NotEqualTo(rhs)
'' ''            Case "<"
'' ''                Return DirectCast(lhs, IComparisonOperations).LessThan(rhs)
'' ''            Case "<="
'' ''                Return DirectCast(lhs, IComparisonOperations).LessThanOrEqualTo(rhs)
'' ''            Case ">"
'' ''                Return DirectCast(lhs, IComparisonOperations).GreaterThan(rhs)
'' ''            Case ">="
'' ''                Return DirectCast(lhs, IComparisonOperations).GreaterThanOrEqualTo(rhs)
'' ''        End Select
'' ''        Throw New RPN_Exception("Unsupported Comparison operation " & m_szOperator)
'' ''    End Function
'' ''End Class

'' ''Public Class LogicalOperator
'' ''    Inherits [Operator]
'' ''    Public Sub New(ByVal cOperator As Char)
'' ''        MyBase.New(cOperator)
'' ''    End Sub
'' ''    Public Sub New(ByVal szOperator As String)
'' ''        MyBase.New(szOperator)
'' ''    End Sub

'' ''    Public Overrides Function Eval(ByVal lhs As IOperand, ByVal rhs As IOperand) As IOperand
'' ''        If Not (TypeOf lhs Is ILogicalOperations) Then
'' ''            Throw New RPN_Exception("Argument invalid in LogicalOperator.Eval - Invalid Expression : lhs")
'' ''        End If
'' ''        Select Case m_szOperator
'' ''            Case "&&"
'' ''                Return DirectCast(lhs, ILogicalOperations).[AND](rhs)
'' ''            Case "||"
'' ''                Return DirectCast(lhs, ILogicalOperations).[OR](rhs)
'' ''        End Select
'' ''        Throw New RPN_Exception("Unsupported Logical operation " & m_szOperator)
'' ''    End Function
'' ''End Class

'' ''Public Class OperatorHelper
'' ''    ''' <summary>
'' ''    ''' Factory method to create Operator objects.
'' ''    ''' </summary>
'' ''    ''' <param name="szOperator"></param>
'' ''    ''' <returns></returns>
'' ''    Public Shared Function CreateOperator(ByVal szOperator As String) As IOperator
'' ''        Dim oprtr As IOperator = Nothing
'' ''        If OperatorHelper.IsArithmeticOperator(szOperator) Then
'' ''            oprtr = New ArithmeticOperator(szOperator)
'' ''            Return oprtr
'' ''        End If
'' ''        If OperatorHelper.IsComparisonOperator(szOperator) Then
'' ''            oprtr = New ComparisonOperator(szOperator)
'' ''            Return oprtr
'' ''        End If
'' ''        If OperatorHelper.IsLogicalOperator(szOperator) Then
'' ''            oprtr = New LogicalOperator(szOperator)
'' ''            Return oprtr
'' ''        End If
'' ''        Throw New RPN_Exception("Unhandled Operator : " & szOperator)
'' ''    End Function

'' ''    Public Shared Function CreateOperator(ByVal cOperator As Char) As IOperator
'' ''        Return CreateOperator(New String(cOperator, 1))
'' ''    End Function

'' ''    ''' <summary>
'' ''    ''' Some helper functions
'' ''    ''' </summary>
'' ''    ''' <param name="currentOp"></param>
'' ''    ''' <returns></returns>
'' ''    ''' <remarks></remarks>
'' ''    Public Shared Function IsOperator(ByVal currentOp As String) As Boolean
'' ''        Dim nPos As Integer = Array.IndexOf(m_AllOps, currentOp.Trim())
'' ''        If nPos <> -1 Then
'' ''            Return True
'' ''        Else
'' ''            Return False
'' ''        End If
'' ''    End Function
'' ''    Public Shared Function IsArithmeticOperator(ByVal currentOp As String) As Boolean
'' ''        Dim nPos As Integer = Array.IndexOf(m_AllArithmeticOps, currentOp)
'' ''        If nPos <> -1 Then
'' ''            Return True
'' ''        Else
'' ''            Return False
'' ''        End If
'' ''    End Function
'' ''    Public Shared Function IsComparisonOperator(ByVal currentOp As String) As Boolean
'' ''        Dim nPos As Integer = Array.IndexOf(m_AllComparisonOps, currentOp)
'' ''        If nPos <> -1 Then
'' ''            Return True
'' ''        Else
'' ''            Return False
'' ''        End If
'' ''    End Function
'' ''    Public Shared Function IsLogicalOperator(ByVal currentOp As String) As Boolean
'' ''        Dim nPos As Integer = Array.IndexOf(m_AllLogicalOps, currentOp)
'' ''        If nPos <> -1 Then
'' ''            Return True
'' ''        Else
'' ''            Return False
'' ''        End If
'' ''    End Function

'' ''#Region "Precedence"

'' ''    ''' <summary>
'' ''    ''' Summary of IsLowerPrecOperator
'' ''    ''' </summary>
'' ''    ''' <param name="currentOp"></param>
'' ''    ''' <param name="prevOp"></param>
'' ''    ''' <returns></returns>
'' ''    ''' <remarks></remarks>
'' ''    Public Shared Function IsLowerPrecOperator(ByVal currentOp As String, ByVal prevOp As String) As Boolean
'' ''        Dim nCurrIdx As Integer
'' ''        Dim nPrevIdx As Integer
'' ''        GetCurrentAndPreviousIndex(m_AllOps, currentOp, prevOp, nCurrIdx, nPrevIdx)
'' ''        If nCurrIdx < nPrevIdx Then
'' ''            Return True
'' ''        End If
'' ''        Return False
'' ''    End Function

'' ''    ''' <summary>
'' ''    ''' Summary of IsHigherPrecOperator
'' ''    ''' </summary>
'' ''    ''' <param name="currentOp"></param>
'' ''    ''' <param name="prevOp"></param>
'' ''    ''' <returns></returns>
'' ''    ''' <remarks></remarks>
'' ''    Public Shared Function IsHigherPrecOperator(ByVal currentOp As String, ByVal prevOp As String) As Boolean
'' ''        Dim nCurrIdx As Integer
'' ''        Dim nPrevIdx As Integer
'' ''        GetCurrentAndPreviousIndex(m_AllOps, currentOp, prevOp, nCurrIdx, nPrevIdx)
'' ''        If nCurrIdx > nPrevIdx Then
'' ''            Return True
'' ''        End If
'' ''        Return False
'' ''    End Function

'' ''    ''' <summary>
'' ''    ''' Summary of IsEqualPrecOperator
'' ''    ''' </summary>
'' ''    ''' <param name="currentOp"></param>
'' ''    ''' <param name="prevOp"></param>
'' ''    ''' <returns></returns>
'' ''    ''' <remarks></remarks>
'' ''    Public Shared Function IsEqualPrecOperator(ByVal currentOp As String, ByVal prevOp As String) As Boolean
'' ''        Dim nCurrIdx As Integer
'' ''        Dim nPrevIdx As Integer
'' ''        GetCurrentAndPreviousIndex(m_AllOps, currentOp, prevOp, nCurrIdx, nPrevIdx)
'' ''        If nCurrIdx = nPrevIdx Then
'' ''            Return True
'' ''        End If
'' ''        Return False
'' ''    End Function

'' ''    ''' <summary>
'' ''    ''' Summary of GetCurrentAndPreviousIndex
'' ''    ''' </summary>
'' ''    ''' <param name="allOps"></param>
'' ''    ''' <param name="currentOp"></param>
'' ''    ''' <param name="prevOp"></param>
'' ''    ''' <param name="nCurrIdx"></param>
'' ''    ''' <param name="nPrevIdx"></param>
'' ''    ''' <remarks></remarks>
'' ''    Private Shared Sub GetCurrentAndPreviousIndex(ByVal allOps() As String, ByVal currentOp As String, ByVal prevOp As String, ByRef nCurrIdx As Integer, ByRef nPrevIdx As Integer)
'' ''        nCurrIdx = -1
'' ''        nPrevIdx = -1
'' ''        For nIdx As Integer = 0 To allOps.Length - 1
'' ''            If allOps(nIdx) = currentOp Then
'' ''                nCurrIdx = nIdx
'' ''            End If
'' ''            If allOps(nIdx) = prevOp Then
'' ''                nPrevIdx = nIdx
'' ''            End If
'' ''            If nPrevIdx <> -1 AndAlso nCurrIdx <> -1 Then
'' ''                Exit For
'' ''            End If
'' ''        Next
'' ''        If nCurrIdx = -1 Then
'' ''            Throw New RPN_Exception("Unknown operator - " & currentOp)
'' ''        End If
'' ''        If nPrevIdx = -1 Then
'' ''            Throw New RPN_Exception("Unknown operator - " & prevOp)
'' ''        End If

'' ''    End Sub
'' ''#End Region

'' ''#Region "RegEx"
'' ''    ''' <summary>
'' ''    ''' This gets the regular expression used to find operators in the input
'' ''    ''' expression.
'' ''    ''' </summary>
'' ''    ''' <param name="exType"></param>
'' ''    ''' <returns></returns>
'' ''    Public Shared Function GetOperatorsRegEx(ByVal exType As ExpressionType) As String
'' ''        Dim strRegex As New StringBuilder()
'' ''        If (exType And ExpressionType.ET_ARITHMETIC).Equals(ExpressionType.ET_ARITHMETIC) Then
'' ''            If strRegex.Length = 0 Then
'' ''                strRegex.Append(m_szArthmtcRegEx)
'' ''            Else
'' ''                strRegex.Append("|" & m_szArthmtcRegEx)
'' ''            End If
'' ''        End If
'' ''        If (exType And ExpressionType.ET_COMPARISON).Equals(ExpressionType.ET_COMPARISON) Then
'' ''            If strRegex.Length = 0 Then
'' ''                strRegex.Append(m_szCmprsnRegEx)
'' ''            Else
'' ''                strRegex.Append("|" & m_szCmprsnRegEx)
'' ''            End If
'' ''        End If
'' ''        If (exType And ExpressionType.ET_LOGICAL).Equals(ExpressionType.ET_LOGICAL) Then
'' ''            If strRegex.Length = 0 Then
'' ''                strRegex.Append(m_szLgclRegEx)
'' ''            Else
'' ''                strRegex.Append("|" & m_szLgclRegEx)
'' ''            End If
'' ''        End If
'' ''        If strRegex.Length = 0 Then
'' ''            Throw New RPN_Exception("Invalid combination of ExpressionType value")
'' ''        End If
'' ''        Return "(" & strRegex.ToString() & ")"
'' ''    End Function

'' ''    ''' <summary>
'' ''    ''' Expression to pattern match various operators
'' ''    ''' </summary>
'' ''    Shared m_szArthmtcRegEx As String = "[+\-*/%()]{1}"
'' ''    Shared m_szCmprsnRegEx As String = "[=<>!]{1,2}"
'' ''    Shared m_szLgclRegEx As String = "[&|]{2}"
'' ''#End Region

'' ''    Public Shared ReadOnly Property AllOperators() As String()
'' ''        Get
'' ''            Return m_AllOps
'' ''        End Get
'' ''    End Property

'' ''    ''' <summary>
'' ''    ''' All Operators supported by this module currently.
'' ''    ''' Modify here to add more operators IN ACCORDANCE WITH their precedence.
'' ''    ''' Additionally add into individual variables to support some helper methods above.
'' ''    ''' </summary>
'' ''    Shared m_AllOps As String() = {"||", "&&", "|", "^", "&", "==", "!=", "<", "<=", ">", ">=", "+", "-", "*", "/", "%", "(", ")"}
'' ''    Shared m_AllArithmeticOps As String() = {"+", "-", "*", "/", "%"}
'' ''    Shared m_AllComparisonOps As String() = {"==", "!=", "<", "<=", ">", ">="}
'' ''    Shared m_AllLogicalOps As String() = {"&&", "||"}
'' ''End Class

'' ''#End Region