using System.Linq.Expressions;
using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class Translator : ITranslator
{
    public TranslationResult Translate(Expr expr)
    {
        return new TranslatorImpl().Translate(expr);
    }
}

public class TranslatorImpl
{
    private readonly List<TranslationError> _errors = new();

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

    private Expression Expression(Expr expr)
    {
        return expr switch
        {
            LambdaExpr lambdaExpr => LambdaExpression(lambdaExpr),
            _ => throw new NotImplementedException()
        };
    }
    
    private Expression LambdaExpression(LambdaExpr expr)
    {
        // Add parameters with overriding. When the parameter is requested, use it.
        throw new NotImplementedException();
    }
}