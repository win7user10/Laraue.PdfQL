using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling.Expressions;
using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;
using Laraue.PdfQL.PdfObjects;
using Laraue.PdfQL.PdfObjects.Interfaces;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;

public class DelegateCompiler : IDelegateCompiler
{
    public DelegateCompilingResult Compile(Stage[] stages)
    {
        return new TypeResolverImpl(stages).Compile();
    }
}

internal class TypeResolverImpl
{
    private readonly Stage[] _stages;
    private Type _currentType;
    
    private readonly List<DelegateCompilingError> _errors = new ();
    private Func<PdfDocument, object> _result;

    private readonly ExpressionCompiler _expressionCompiler = new();

    public TypeResolverImpl(Stage[] stages)
    {
        _stages = stages;
        _currentType = typeof(PdfDocument);
        _result = document => document;
    }

    public DelegateCompilingResult Compile()
    {
        foreach (var stage in _stages)
        {
            switch (stage)
            {
                case SelectStage selectStage:
                    SelectStage(selectStage);
                    break;
                case FilterStage filterStage:
                    FilterStage(filterStage);
                    break;
                case SelectManyStage selectManyStage:
                    SelectManyStage(selectManyStage);
                    break;
                case MapStage mapStage:
                    MapStage(mapStage);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported stage: {stage.GetType().Name}");
            }

            if (_errors.Any())
            {
                break;
            }
        }

        return new DelegateCompilingResult
        {
            Errors = _errors.ToArray(),
            Delegate = _result
        };
    }

    private void SelectStage(SelectStage stage)
    {
        try
        {
            switch (stage.SelectElement)
            {
                case PdfElement.Table:
                    MapStageElement<IHasTablesContainer, PdfObjectContainer<PdfTable>>(c => c.GetTablesContainer());
                    _currentType = typeof(PdfObjectContainer<PdfTable>);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        catch (DelegateCompilingException)
        {
            _errors.Add(new DelegateCompilingError
            {
                Error = $"Selector '{stage.SelectElement}' is not applicable on the current element",
                Stage = stage,
            });
        }
    }
    
    private void SelectManyStage(SelectManyStage stage)
    {
        try
        {
            switch (stage.SelectElement)
            {
                case PdfElement.Table:
                    ApplyToEachElementAndUseSelectMany<IHasTablesContainer, PdfTable>(c => c.GetTablesContainer());
                    _currentType = typeof(PdfObjectContainer<PdfTable>);
                    break;
                case PdfElement.TableRow:
                    ApplyToEachElementAndUseSelectMany<IHasTableRowsContainer, PdfTableRow>(c => c.GetTableRowsContainer());
                    _currentType = typeof(PdfObjectContainer<PdfTableRow>);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        catch (DelegateCompilingException)
        {
            _errors.Add(new DelegateCompilingError
            {
                Error = $"Selector '{stage.SelectElement}' is not applicable on the current element",
                Stage = stage,
            });
        }
    }
    
    private void ApplyToEachElementAndUseSelectMany<TContainerElement, TResultElement>(
        Func<TContainerElement, IEnumerable<TResultElement>> @delegate)
        where TResultElement : PdfObject
    {
        SetNextStageResult<TContainerElement, TResultElement>(elements =>
        {
            var nextResult = new List<TResultElement>();
            
            foreach (var element in elements)
            {
                nextResult.AddRange(@delegate.Invoke(element));
            }
            
            return nextResult;
        });
    }

    private void FilterStage(FilterStage stage)
    {
        var expression = CompileLambda(stage, s => s.Filter);
        if (expression == null)
            return;

        var method = GetType().GetMethod(nameof(ApplyFilterToEachContainerElement), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var genericMethod = method.MakeGenericMethod(expression.Parameters[0].Type);
        genericMethod.Invoke(this, [expression.Compile()]);
    }
    
    private void ApplyFilterToEachContainerElement<TContainerElement>(Func<TContainerElement, bool> @delegate)
        where TContainerElement : PdfObject
    {
        SetNextStageResult<TContainerElement, TContainerElement>(elements =>
        {
            var nextResult = new List<TContainerElement>();
            
            foreach (var element in elements)
            {
                if (@delegate.Invoke(element))
                {
                    nextResult.Add(element);
                }
            }
            
            return nextResult;
        });
    }

    private void MapStage(MapStage stage)
    {
        var expression = CompileLambda(stage, s => s.Projection);
        if (expression == null)
            return;
        
        var method = GetType().GetMethod(nameof(ApplyMapToEachContainerElement), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var genericMethod = method.MakeGenericMethod(expression.Parameters[0].Type, expression.ReturnType);
        
        genericMethod.Invoke(this, [expression.Compile()]);
    }
    
    private void ApplyMapToEachContainerElement<TContainerElement, TResult>(Func<TContainerElement, TResult> @delegate)
        where TContainerElement : PdfObject
        where TResult : PdfObject
    {
        SetNextStageResult<TContainerElement, TResult>(elements =>
        {
            var nextResult = new List<TResult>();
            
            foreach (var element in elements)
            {
                nextResult.Add(@delegate.Invoke(element));
            }
            
            return nextResult;
        });
    }

    private LambdaExpression? CompileLambda<TStage>(TStage stage, Func<TStage, Expr> getLambdaExpression)
        where TStage : Stage
    {
        var expr = getLambdaExpression(stage);
        if (expr is not LambdaExpr projection)
        {
            _errors.Add(new DelegateCompilingError { Error = "Lambda expression excepted", Stage = stage });
            return null;
        }
        
        // Type checking
        var containerType = _currentType;
        if (!containerType.IsGenericType && containerType.GenericTypeArguments.Length != 0)
        {
            _errors.Add(new DelegateCompilingError { Error = "Filtration operation is not allowed here", Stage = stage });
            return null;
        }
        
        var containerElementType = containerType.GenericTypeArguments[0];
        var compileResult = _expressionCompiler.Compile(projection, new ExpressionCompilerContext
        {
            ParameterTypes = { containerElementType }
        });

        if (compileResult.HasErrors)
        {
            foreach (var error in compileResult.Errors)
            {
                _errors.Add(new DelegateCompilingError { Error = error.Error, Stage = stage });
            }
            
            return null;
        }

        if (compileResult.Expression is not LambdaExpression expression)
        {
            throw new DelegateCompilingException($"Lambda expression expected, but {compileResult.Expression?.GetType().Name} taken");
        }

        return expression;
    }

    private void SetNextStageResult<TContainerElement, TResultElement>(
        Func<IEnumerable<TContainerElement>, IEnumerable<TResultElement>> @delegate)
        where TResultElement : PdfObject
    {
        _result = document =>
        {
            var collection = _result(document);

            if (collection is not IEnumerable<TContainerElement> enumerable)
            {
                throw new DelegateRuntimeException($"Collection was excepted but {collection.GetType().Name} taken");
            }

            var nextResult = @delegate(enumerable);

            return new PdfObjectContainer<TResultElement>(nextResult.ToArray());
        };
    }

    private void MapStageElement<TExceptedType, TResult>(Func<TExceptedType, TResult> selector)
        where TResult : class
    {
        if (!typeof(TExceptedType).IsAssignableFrom(_currentType))
        {
            throw new DelegateCompilingException($"Method is not exists on the instance of type {typeof(TExceptedType)}");
        }

        _result = document =>
        {
            var temp = _result(document);

            if (temp is not TExceptedType exceptedType)
            {
                throw new DelegateRuntimeException($"Method is not exists on the object of type {temp.GetType()}");
            }

            return selector(exceptedType);
        };
    }
}