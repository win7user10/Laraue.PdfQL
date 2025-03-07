﻿using System.Linq.Expressions;
using Laraue.PdfQL.StageResults;
using Laraue.PdfQL.Stages;
using Laraue.PdfQL.TreeExecution.Expressions;

namespace Laraue.PdfQL.TreeExecution;

public class SelectStageExecutor : StageExecutor<SelectStage>
{
    private readonly PSqlExpressionVisitorFactory _factory;

    public SelectStageExecutor(PSqlExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override StageResult Execute(StageResult currentValue, SelectStage stage)
    {
        var exp = _factory.Visit(stage.SelectExpression);

        // How to know what parameter to pass?
        var parameter = Expression.Parameter(stage.ObjectType, "container");
        var replacer = new ParameterReplacer([parameter]);
        exp = replacer.Visit(exp);
        
        var action = exp switch
        {
            LambdaExpression lambda => lambda.Compile(),
            MethodCallExpression methodCall => Expression.Lambda(methodCall, parameter).Compile(),
            _ => throw new NotImplementedException()
        };
        
        var res = action.DynamicInvoke(currentValue.GetContainerOrThrow().PdfObject);

        return res as StageResult;
    }
}