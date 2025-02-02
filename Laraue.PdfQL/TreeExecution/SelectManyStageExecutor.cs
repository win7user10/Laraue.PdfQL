using System.Linq.Expressions;
using Laraue.PdfQL.PdfObjects;
using Laraue.PdfQL.StageResults;
using Laraue.PdfQL.Stages;
using Laraue.PdfQL.TreeExecution.Expressions;

namespace Laraue.PdfQL.TreeExecution;

public class SelectManyStageExecutor : StageExecutor<SelectManyStage>
{
    private readonly PSqlExpressionVisitorFactory _factory;

    public SelectManyStageExecutor(PSqlExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override StageResult Execute(StageResult currentValue, SelectManyStage stage)
    {
        if (currentValue is not PdfObjectStageResult { PdfObject: PdfObjectContainer pdfObjectContainer })
        {
            throw new InvalidOperationException();
        }
        
        var exp = _factory.Visit(stage.SelectExpression);
        
        var parameter = Expression.Parameter(stage.ObjectType, "container");
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
            if (localResult is not PdfObjectStageResult { PdfObject: PdfObjectContainer localResultContainer })
            {
                throw new InvalidOperationException("Attemp to make SelectMany on non of Container lists. May be Select stage required?");
            }
            
            result.AddRange(localResultContainer.Select(localResultContainer => localResultContainer));
        }

        return new PdfObjectStageResult(new PdfObjectContainer(result.ToArray()));
    }
}