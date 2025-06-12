using System.Linq.Expressions;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class ParameterReplacer : ExpressionVisitor
{
    private readonly Dictionary<string, ParameterExpression> _parameters;
        
    public ParameterReplacer(IReadOnlyCollection<ParameterExpression> parameterExpressions)
    {
        _parameters = parameterExpressions
            .ToDictionary(p => p.Name!, p => p);
    }
        
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (!_parameters.TryGetValue(node.Name!, out var p))
        {
            throw new InvalidOperationException($"Unknown expression parameter {node.Name}");
        }

        return p;
    }
}