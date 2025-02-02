using Laraue.PdfQL.StageResults;
using Laraue.PdfQL.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PdfQL.TreeExecution;

public class StagesListExecutor : StageExecutor<StagesList>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ExecutorOptions _executorOptions;

    public StagesListExecutor(IServiceProvider serviceProvider, ExecutorOptions executorOptions)
    {
        _serviceProvider = serviceProvider;
        _executorOptions = executorOptions;
    }
    
    public override StageResult Execute(StageResult currentValue, StagesList stage)
    {
        for (var index = 0; index < stage.Stages.Count; index++)
        {
            var stageElement = stage.Stages[index];
            try
            {
                currentValue = stageElement switch
                {
                    SelectStage selectStage => Execute(currentValue, selectStage),
                    SelectManyStage selectManyStage => Execute(currentValue, selectManyStage),
                    FilterStage filterStage => Execute(currentValue, filterStage),
                    ApplyMethodForEachElementStage applyMethodStage => Execute(currentValue, applyMethodStage),
                    _ => throw new NotImplementedException($"Stage {stageElement.GetType().Name} is not supported"),
                };
            }
            catch (Exception e) when (_executorOptions.HandleErrors)
            {
                throw new InvalidOperationException($"Stage[{index}] ({stageElement.GetType().Name}) execution failed. {e.Message}.]", e);
            }
        }

        return currentValue;
    }

    private StageResult Execute<TStage>(StageResult currentValue, TStage stage)
        where TStage : Stage
    {
        var executor = _serviceProvider.GetService<Executor<TStage>>();
        if (executor == null)
        {
            throw new NullReferenceException($"No executor for {typeof(TStage).Name}");
        }
        
        return executor.Execute(currentValue, stage);
    }
}