using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PdfQL.TreeExecution.Expressions.MethodCalls;

public class MethodCallVisitorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public MethodCallVisitorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public MethodCallVisitor? Get(string methodName)
    {
        return _serviceProvider.GetKeyedService<MethodCallVisitor>(methodName);
    }
}