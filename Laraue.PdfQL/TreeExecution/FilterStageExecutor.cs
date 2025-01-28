using System.Linq.Expressions;
using Laraue.PQL.PdfObjects;
using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;
using Laraue.PQL.TreeExecution.Expressions;

namespace Laraue.PQL.TreeExecution;

public class FilterStageExecutor : StageExecutor<FilterStage>
{
    private readonly PSqlExpressionVisitorFactory _factory;

    public FilterStageExecutor(PSqlExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override StageResult Execute(StageResult currentValue, FilterStage stage)
    {
        if (currentValue is not PdfObjectStageResult { PdfObject: PdfObjectContainer pdfObjectContainer })
        {
            throw new InvalidOperationException("Filter expression is available only on list of objects");
        }
        
        var binaryExpression = stage.BinaryExpression;
        
        var predicateExpression = _factory.Visit(binaryExpression);
        
        // How to know what parameter to pass?
        var parameter = Expression.Parameter(stage.ObjectType, "tables");
        var replacer = new ParameterReplacer([parameter]);
        
        predicateExpression = replacer.Visit(predicateExpression);
        
        var func = typeof (Func<,>);
        var genericFunc = func.MakeGenericType(stage.ObjectType, typeof(bool));
        
        var lambda = Expression.Lambda(genericFunc, predicateExpression, parameter);
        
        var compiled = lambda.Compile();

        var result = new List<PdfObject>();
        foreach (var pdfObject in pdfObjectContainer)
        {
            var interResult = (bool)compiled.DynamicInvoke(pdfObject)!;
            if (interResult)
            {
                result.Add(pdfObject);
            }
        }
        
        return new PdfObjectStageResult(new PdfObjectContainer<PdfObject>(result.ToArray()));
    }
    
    private class ParameterReplacer : ExpressionVisitor
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
}