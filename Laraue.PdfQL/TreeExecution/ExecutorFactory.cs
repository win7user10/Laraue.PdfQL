using Laraue.PdfQL.StageResults;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PdfQL.TreeExecution;

public class ExecutorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExecutorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public StageResult Execute<TCommand>(StageResult currentValue, TCommand selectExpression)
    {
        var executor = _serviceProvider.GetService<Executor<TCommand>>();
        if (executor == null)
        {
            throw new NullReferenceException($"No executor for {typeof(TCommand).Name}");
        }
        
        return executor.Execute(currentValue, selectExpression);
    }
}