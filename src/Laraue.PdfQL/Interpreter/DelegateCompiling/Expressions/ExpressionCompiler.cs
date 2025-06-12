using System.Linq.Expressions;
using System.Text;
using Laraue.PdfQL.Interpreter.Parsing.Expressions;
using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class ExpressionCompiler
{
    public ExpressionCompileResult Compile(Expr expr, ExpressionCompilerContext context)
    {
        return new TranslatorImpl(context).Translate(expr);
    }
}

public class TranslatorImpl
{
    private readonly List<ExpressionCompileError> _errors = new();
    private readonly ExpressionCompilerContext _translationContext;
    private readonly Environment _environment = new ();

    public TranslatorImpl(ExpressionCompilerContext translationContext)
    {
        _translationContext = translationContext;
    }

    public ExpressionCompileResult Translate(Expr expr)
    {
        try
        {
            return new ExpressionCompileResult
            {
                Expression = Expression(expr),
                Errors = _errors.ToArray()
            };
        }
        catch (ExpressionCompileException)
        {
            return new ExpressionCompileResult
            {
                Errors = _errors.ToArray()
            };
        }
    }

    private Expression? Expression(Expr expr)
    {
        return expr switch
        {
            LambdaExpr lambdaExpr => LambdaExpression(lambdaExpr),
            BinaryExpr binaryExpr => BinaryExpression(binaryExpr),
            InstanceMethodCallExpr instanceMethodCallExpr => InstanceMethodCallExpression(instanceMethodCallExpr),
            MemberAccessExpr memberAccessExpr => MemberAccessExpression(memberAccessExpr),
            VariableExpr variableExpr => VariableExpression(variableExpr),
            LiteralExpr literalExpr => LiteralExpression(literalExpr),
            _ => throw new NotImplementedException(expr.GetType().ToString())
        };
    }
    
    private Expression? LambdaExpression(LambdaExpr expr)
    {
        // Check porameters count
        if (expr.Parameters.Count > _translationContext.ParameterTypes.Count)
        {
            var firstWrongParameter = expr.Parameters[_translationContext.ParameterTypes.Count + 1];
            _errors.Add(new ExpressionCompileError { Error = "Invalid parameters count", Token = firstWrongParameter });

            return null;
        }
        
        var paremeters = new List<ParameterExpression>();
        
        // Parameters resolving
        for (var index = 0; index < expr.Parameters.Count; index++)
        {
            var parameter = expr.Parameters[index];
            var parameterName = parameter.Lexeme!;
            var parameterType = _translationContext.ParameterTypes[index];
            
            if (!_environment.TryDefine(parameterName, parameterType))
            {
                _errors.Add(new ExpressionCompileError { Error = "Parameter with the same name already defined", Token = parameter });
                return null;
            }
            
            paremeters.Add(System.Linq.Expressions.Expression.Parameter(parameterType, parameterName));
        }

        // Make the further translation
        var body = Expression(expr.Body);
        if (body == null)
        {
            return null;
        }

        var lambda = System.Linq.Expressions.Expression.Lambda(body, paremeters);
        var replacer = new ParameterReplacer(paremeters);
        return (LambdaExpression) replacer.Visit(lambda);
    }

    private Expression? BinaryExpression(BinaryExpr expr)
    {
        var left = Expression(expr.Left);
        var right = Expression(expr.Right);
        var type = GetExpressionType(expr.Operator.TokenType);
        
        if (left == null || right == null)
            return null;
        
        return System.Linq.Expressions.Expression.MakeBinary(type, left, right);
    }

    private Expression? InstanceMethodCallExpression(InstanceMethodCallExpr expr)
    {
        try
        {
            var callee = Expression(expr.Object);
            if (callee == null)
            {
                return null;
            }
            
            // Check method signature
            var methodInfo = callee.Type.GetMethod(expr.Method.Lexeme!);
            if (methodInfo == null)
            {
                _errors.Add(new ExpressionCompileError
                {
                    Error = $"Method is not found on {Utils.GetReadableTypeName(callee.Type)}",
                    Token = expr.Method
                });
                return null;
            }

            var exceptedParameters = methodInfo.GetParameters();
            if (exceptedParameters.Length != expr.Arguments.Count)
            {
                var sb = new StringBuilder($"Excepted call with signature '{methodInfo.Name}(")
                    .Append(string.Join(", ", exceptedParameters.Select(p => $"{p.ParameterType.Name} {p.Name}")))
                    .Append($")' got '{methodInfo.Name}(")
                    .Append(string.Join(", ", expr.Arguments))
                    .Append(")'");
                
                _errors.Add(new ExpressionCompileError
                {
                    Error = sb.ToString(),
                    Token = expr.Method
                });
                return null;
            }
            
            var arguments = new List<Expression?>();
            foreach (var argument in expr.Arguments)
                arguments.Add(Expression(argument));
        
            if (arguments.Any(a => a == null))
                return null;

            return System.Linq.Expressions.Expression.Call(callee, methodInfo, arguments.Cast<Expression>());
        }
        catch (ExpressionCompileException e)
        {
            _errors.Add(new ExpressionCompileError { Error = e.Message });
            return null;
        }
    }

    private Expression? MemberAccessExpression(MemberAccessExpr expr)
    {
        return Expression(expr.Expr);
    }
    
    private Expression? VariableExpression(VariableExpr expr)
    {
        try
        {
            var variableType = _environment.GetType(expr.Name);
            return System.Linq.Expressions.Expression.Variable(variableType, expr.Name);
        }
        catch (Exception e)
        {
            _errors.Add(new ExpressionCompileError { Error = e.Message});
            return null;
        }
    }
    
    private Expression? LiteralExpression(LiteralExpr expr)
    {
        return System.Linq.Expressions.Expression.Constant(expr.Value);
    }

    private ExpressionType GetExpressionType(TokenType tokenType)
    {
        return tokenType switch
        {
            TokenType.Divide => ExpressionType.Divide,
            TokenType.Equal => ExpressionType.Equal,
            TokenType.False => ExpressionType.IsFalse,
            TokenType.Minus => ExpressionType.Negate,
            TokenType.Multiply => ExpressionType.Multiply,
            TokenType.NotEqual => ExpressionType.NotEqual,
            TokenType.Not => ExpressionType.Not,
            TokenType.Plus => ExpressionType.Add,
            TokenType.GreaterThan => ExpressionType.GreaterThan,
            TokenType.LessThan => ExpressionType.LessThan,
            TokenType.GreaterOrEqualThan => ExpressionType.GreaterThanOrEqual,
            TokenType.LessOrEqualThan => ExpressionType.LessThanOrEqual,
            _ => throw new NotSupportedException(tokenType.ToString())
        };
    }
}