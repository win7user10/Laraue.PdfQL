using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;
using Laraue.PdfQL.Interpreter.Parsing;
using Laraue.PdfQL.Interpreter.Parsing.Expressions;
using Laraue.PdfQL.Interpreter.Parsing.Stages;
using Laraue.PdfQL.PdfObjects;
using Laraue.PdfQL.PdfObjects.Interfaces;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class DelegateCompiler : IDelegateCompiler
{
    public DelegateCompilingResult Compile(Stage[] stages)
    {
        return new DelegateCompilerImpl(stages).Compile();
    }
}

internal class DelegateCompilerImpl
{
    private readonly Stage[] _stages;
    private Type _currentType;
    
    private readonly List<DelegateCompilingError> _errors = new ();
    private Func<PdfDocument, object?> _result;

    private readonly ExpressionCompiler _expressionCompiler = new();

    public DelegateCompilerImpl(Stage[] stages)
    {
        _stages = stages;
        _currentType = typeof(PdfDocument);
        _result = document => document;
    }

    public DelegateCompilingResult Compile()
    {
        try
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
                    case SingleStage singleStage:
                        SingleStage(singleStage);
                        break;
                    case FirstStage firstStage:
                        FirstStage(firstStage);
                        break;
                    case FirstOrDefaultStage firstOrDefaultStage:
                        FirstOrDefaultStage(firstOrDefaultStage);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported stage: {stage.GetType().Name}");
                }
            }
        }
        catch (DelegateCompilingException)
        {
            return new DelegateCompilingResult
            {
                Errors = _errors.ToArray(),
            };
        }

        return new DelegateCompilingResult
        {
            Errors = _errors.ToArray(),
            Delegate = _result
        };
    }

    private void SelectStage(SelectStage stage)
    {
        switch (stage.SelectElement)
        {
            case PdfElement.Table:
                AppendStageDelegate<IHasTablesContainer, StageResult<PdfTable>>(c => c.GetTablesContainer(), stage);
                break;
            case PdfElement.TableRow:
                AppendStageDelegate<IHasTableRowsContainer, StageResult<PdfTableRow>>(c => c.GetTableRowsContainer(), stage);
                break;
            case PdfElement.TableCell:
                AppendStageDelegate<IHasTableCellsContainer, StageResult<PdfTableCell>>(c => c.GetTableCellsContainer(), stage);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private void SelectManyStage(SelectManyStage stage)
    {
        switch (stage.SelectElement)
        {
            case PdfElement.Table:
                ApplyToEachElementAndUseSelectMany<IHasTablesContainer, PdfTable>(c => c.GetTablesContainer(), stage);
                break;
            case PdfElement.TableRow:
                ApplyToEachElementAndUseSelectMany<IHasTableRowsContainer, PdfTableRow>(c => c.GetTableRowsContainer(), stage);
                break;
            case PdfElement.TableCell:
                ApplyToEachElementAndUseSelectMany<IHasTableCellsContainer, PdfTableCell>(c => c.GetTableCellsContainer(), stage);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private void ApplyToEachElementAndUseSelectMany<TContainerElement, TResultElement>(
        Func<TContainerElement, IEnumerable<TResultElement>> @delegate,
        Stage stage)
        where TResultElement : PdfObject
    {
        AppendStageDelegate<TContainerElement, TResultElement>(elements =>
        {
            var nextResult = new List<TResultElement>();
            
            foreach (var element in elements)
            {
                nextResult.AddRange(@delegate.Invoke(element));
            }
            
            return nextResult;
        }, stage);
    }

    private void FilterStage(FilterStage stage)
    {
        var expression = CompileLambda(stage, s => s.Filter);
        if (expression == null)
            return;

        ApplyFilter(expression, stage);
    }

    private void ApplyFilter(LambdaExpression lambdaExpression, Stage stage)
    {
        CallInstanceMethod(
            nameof(ApplyFilterToEachContainerElement),
            [lambdaExpression.Parameters[0].Type],
            [lambdaExpression.Compile(), stage],
            stage);
    }

    private void CallInstanceMethod(string name, Type[] genericParameters, object[] parameters, Stage stage)
    {
        var method = FindGenericMethod(GetType(), name, genericParameters, parameters);
        if (method == null)
        {
            throw CompilingError($"Method {name} is not found", stage);
        }
        
        method.Invoke(this, parameters);
    }

    private static MethodInfo? FindGenericMethod(Type type, string name, Type[] genericParameters, object[] parameters)
    {
        var methods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.Name == name);
        
        foreach (var method in methods)
        {
            if (method.GetGenericArguments().Length != genericParameters.Length)
            {
                continue;
            }

            if (method.GetParameters().Length != parameters.Length)
            {
                continue;
            }
            
            return method.MakeGenericMethod(genericParameters);
        }

        return null;
    }
    
    private void ApplyFilterToEachContainerElement<TContainerElement>(Func<TContainerElement, bool> @delegate, Stage stage)
        where TContainerElement : PdfObject
    {
        AppendStageDelegate<TContainerElement, TContainerElement>(elements =>
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
        }, stage);
    }
    
    private void SingleStage(SingleStage stage)
    {
        OneElementStage(nameof(Single), stage);
    }
    
    private void FirstStage(FirstStage stage)
    {
        OneElementStage(nameof(First), stage);
    }
    
    private void FirstOrDefaultStage(FirstOrDefaultStage stage)
    {
        OneElementStage(nameof(FirstOrDefault), stage);
    }

    private void OneElementStage<TStage>(string methodName, TStage stage)
        where TStage : OneElementStage
    {
        if (stage.Filter is not null)
        {
            var expression = CompileLambda(stage, s => s.Filter!);
            if (expression == null)
                return;

            ApplyFilter(expression, stage);
        }
        
        var stageResultType = GetStageResultType(_currentType);
        if (stageResultType is null)
        {
            throw CompilingError("Call on collection excepted", stage);
        }
        
        CallInstanceMethod(
            methodName,
            [stageResultType],
            [stage],
            stage);
    }

    private void MapStage(MapStage stage)
    {
        var expression = CompileLambda(stage, s => s.Projection);
        if (expression == null)
            return;
        
        var method = GetType().GetMethod(nameof(ApplyMapToEachContainerElement), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var genericMethod = method.MakeGenericMethod(expression.Parameters[0].Type, expression.ReturnType);
        
        genericMethod.Invoke(this, [expression.Compile(), stage]);
    }
    
    private void ApplyMapToEachContainerElement<TContainerElement, TResult>(Func<TContainerElement, TResult> @delegate, Stage stage)
        where TContainerElement : PdfObject
    {
        AppendStageDelegate<TContainerElement, TResult>(elements =>
        {
            var nextResult = new List<TResult>();
            
            foreach (var element in elements)
            {
                nextResult.Add(@delegate.Invoke(element));
            }
            
            return nextResult;
        }, stage);
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
            throw CompilingError($"Lambda expression expected, but {compileResult.Expression?.GetType().Name} taken", stage);
        }

        return expression;
    }
    
    private void AppendStageDelegate<TContainerElement, TResultElement>(
        Func<IEnumerable<TContainerElement>, IEnumerable<TResultElement>> @delegate,
        Stage stage)
    {
        CheckStageTypesAreCorrect<TContainerElement>(stage);
        
        var resultRef = _result;
        _result = document =>
        {
            var collection = resultRef(document);

            if (collection is not IEnumerable<TContainerElement> enumerable)
            {
                throw new PdfqlRuntimeException($"Collection was excepted but {collection.GetType().Name} taken");
            }

            var nextResult = @delegate(enumerable);

            return new StageResult<TResultElement>(nextResult.ToArray());
        };
        
        _currentType = typeof(StageResult<TResultElement>);
    }

    private void FirstOrDefault<T>(Stage stage)
    {
        ApplyGetElement<T>(elements => elements.FirstOrDefault(), stage);
    }
    
    private void Single<T>(Stage stage)
    {
        ApplyGetElement<T>(elements =>
        {
            var result = elements.Take(2).ToArray();
            return result.Length switch
            {
                0 => throw new PdfqlRuntimeException("Sequence contains no elements"),
                2 => throw new PdfqlRuntimeException("Sequence contains more than one element"),
                _ => result[0]
            };
        }, stage);
    }
    
    private void First<T>(Stage stage)
    {
        ApplyGetElement<T>(elements =>
        {
            var result = elements.Take(1).ToArray();
            return result.Length switch
            {
                0 => throw new PdfqlRuntimeException("Sequence contains no elements"),
                _ => result[0]
            };
        }, stage);
    }
    
    private void ApplyGetElement<T>(
        Func<IEnumerable<T>, T?> getElement,
        Stage stage)
    {
        var stageResultType = GetStageResultType(_currentType);
        if (stageResultType is null)
        {
            throw CompilingError("Appliable only on collections", stage);
        }
        
        var resultRef = _result;
        _result = document =>
        {
            var collection = resultRef(document);
            if (collection is not IEnumerable<T> enumerable)
            {
                throw new PdfqlRuntimeException($"Collection was excepted");
            }

            return getElement(enumerable);
        };
        
        _currentType = stageResultType;
    }

    private void CheckStageTypesAreCorrect<TContainerElement>(Stage stage)
    {
        var stageResultType = GetStageResultType(_currentType);
        if (stageResultType is not null && typeof(TContainerElement).IsAssignableFrom(stageResultType))
        {
            return;
        }
        
        if (stageResultType != null)
        {
            throw CompilingError( $"'{stageResultType.Name}' is not assignable to '{typeof(TContainerElement).Name}'.", stage);
        }
            
        var error = new StringBuilder()
            .Append(Utils.GetReadableTypeName(_currentType))
            .Append("' is not assignable to '")
            .Append(Utils.GetReadableTypeName(typeof(StageResult<TContainerElement>)))
            .Append("'.");

        throw CompilingError(error.ToString(), stage);
    }

    private Type? GetStageResultType(Type type)
    {
        if (!typeof(StageResult).IsAssignableFrom(type))
        {
            return null;
        }

        if (!type.IsGenericType || type.GenericTypeArguments.Length != 1)
        {
            return null;
        }

        return type.GenericTypeArguments[0];
    }
    
    private void AppendStageDelegate<TExceptedType, TResult>(Func<TExceptedType, TResult> selector, Stage stage)
        where TResult : class
    {
        if (!typeof(TExceptedType).IsAssignableFrom(_currentType))
        {
            throw CompilingError($"Method is not exists on the instance of type {typeof(TExceptedType)}", stage);
        }

        var resultRef = _result;
        _result = document =>
        {
            var temp = resultRef(document);

            if (temp is not TExceptedType exceptedType)
            {
                throw new PdfqlRuntimeException($"Method is not exists on the object of type {temp.GetType()}");
            }

            return selector(exceptedType);
        };
        
        _currentType = typeof(TResult);
    }

    private DelegateCompilingException CompilingError(string message, Stage stage)
    {
        _errors.Add(new DelegateCompilingError { Error = message, Stage = stage });
        
        return new DelegateCompilingException();
    }
}