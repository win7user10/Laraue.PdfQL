using Laraue.PdfQL.Extensions;
using Laraue.PdfQL.Parser;
using Laraue.PdfQL.Stages;
using Laraue.PdfQL.TreeExecution;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PdfQL;

public static class PdfQLInstance
{
    public static Executor<StagesList> GetTreeExecutor(ExecutorOptions options)
    {
        var services = new ServiceCollection();

        services.AddExecutorServices(options);
        
        return services.BuildServiceProvider().GetRequiredService<Executor<StagesList>>();
    }
    
    public static PdfExpressionParser GetTreeBuilder()
    {
        var services = new ServiceCollection();

        services.AddParserServices();
        
        return services.BuildServiceProvider().GetRequiredService<PdfExpressionParser>();
    }
}