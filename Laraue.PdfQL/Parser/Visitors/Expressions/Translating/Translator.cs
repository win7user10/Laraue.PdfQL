using System.Linq.Expressions;
using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;
using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class Translator : ITranslator
{
    public TranslationResult Translate(Expr expr, TranslationContext translationContext)
    {
        return new TranslatorImpl(translationContext).Translate(expr);
    }
}

public class TranslationContext
{
    public List<Type> ParameterTypes { get; } = new();
}

public class TranslatorImpl
{
    private readonly List<TranslationError> _errors = new();
    private readonly TranslationContext _translationContext;
    private readonly Environment _environment = new ();

    public TranslatorImpl(TranslationContext translationContext)
    {
        _translationContext = translationContext;
    }

    public TranslationResult Translate(Expr expr)
    {
        try
        {
            return new TranslationResult
            {
                Expression = Expression(expr),
                Errors = _errors.ToArray()
            };
        }
        catch (TranslationException)
        {
            return new TranslationResult
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
            _errors.Add(new TranslationError { Error = "Invalid parameters count", Token = firstWrongParameter });

            return null;
        }
        
        // Parameters resolving
        for (var index = 0; index < expr.Parameters.Count; index++)
        {
            var parameter = expr.Parameters[index];
            if (!_environment.TryDefine(parameter.Lexeme!, _translationContext.ParameterTypes[index]))
            {
                _errors.Add(new TranslationError { Error = "Parameter with the same name already defined", Token = parameter });
                return null;
            }
        }

        // Make the further translation
        return Expression(expr.Body);
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
                _errors.Add(new TranslationError { Error = "Invalid method name", Token = expr.Method });
                return null;
            }
            
            var arguments = new List<Expression?>();
            foreach (var argument in expr.Arguments)
                arguments.Add(Expression(argument));
        
            if (arguments.Any(a => a == null))
                return null;

            return System.Linq.Expressions.Expression.Call(callee, methodInfo, arguments.Cast<Expression>());
        }
        catch (TranslationException e)
        {
            _errors.Add(new TranslationError { Error = e.Message });
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
            _errors.Add(new TranslationError { Error = e.Message});
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