using System.Linq.Expressions;
using System.Reflection;
using Laraue.PQL.Expressions;
using Laraue.PQL.PdfObjects;
using Laraue.PQL.PdfObjects.Interfaces;
using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;

namespace Laraue.PQL.TreeExecution.Expressions;

public class PsqlApplySelectorExpressionVisitor : PSqlExpressionVisitor<PsqlApplySelectorExpression>
{
    public override Expression Visit(PsqlApplySelectorExpression expression)
    {
        return expression.Selector switch
        {
            Selector.Tables => GetContainerExpression<IHasTablesContainer>(
                expression.ObjectType,
                container => new PdfObjectStageResult(container.GetTablesContainer())),
            Selector.TableRows => GetContainerExpression<IHasTableRowsContainer>(
                expression.ObjectType,
                container => new PdfObjectStageResult(container.GetTableRowsContainer())),
            _ => throw new NotSupportedException($"Unsupported selector type: {expression.Selector.GetType().FullName}")
        };
    }

    private Expression GetContainerExpression<TContainer>(
        Type objectType,
        Expression<Func<TContainer, StageResult>> getContainerExpression)
    {
        if (!typeof(TContainer).IsAssignableFrom(objectType))
        {
            throw new NotSupportedException($"{objectType} does not implement {typeof(TContainer)}");
        }

        return getContainerExpression;
    }
}