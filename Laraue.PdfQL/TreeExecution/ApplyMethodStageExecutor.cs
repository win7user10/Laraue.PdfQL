using System.Linq.Expressions;
using Laraue.PQL.PdfObjects;
using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;
using Laraue.PQL.TreeExecution.Expressions;

namespace Laraue.PQL.TreeExecution;

public class ApplyMethodStageExecutor : StageExecutor<ApplyMethodForEachElementStage>
{
    private readonly PSqlExpressionVisitorFactory _factory;

    public ApplyMethodStageExecutor(PSqlExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override StageResult Execute(StageResult currentValue, ApplyMethodForEachElementStage forEachElementStage)
    {
        if (currentValue is not PdfObjectStageResult { PdfObject: PdfObjectContainer pdfObjectContainer })
        {
            throw new InvalidOperationException("Filter expression is available only on list of objects");
        }
        
        var exp = _factory.Visit(forEachElementStage.MethodCallExpression);
        
        var parameter = Expression.Parameter(forEachElementStage.ObjectType, "container");
        var replacer = new ParameterReplacer([parameter]);
        exp = replacer.Visit(exp);

        var action = exp switch
        {
            LambdaExpression lambda => lambda.Compile(),
            MethodCallExpression methodCall => Expression.Lambda(methodCall, parameter).Compile(),
            _ => throw new NotImplementedException()
        };

        var result = new List<PdfObject>();
        foreach (var pdfObject in pdfObjectContainer)
        {
            var localResult = action.DynamicInvoke(pdfObject)!;
            if (localResult is not PdfObject localResultObject)
            {
                throw new InvalidOperationException("Attempt to execute method on non PdfObject");
            }
            
            result.Add(localResultObject);
        }

        return new PdfObjectStageResult(new PdfObjectContainer(result.ToArray()));
    }
}