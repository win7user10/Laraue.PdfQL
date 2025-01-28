using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PQL.TreeExecution;

public class StagesListExecutor : StageExecutor<StagesList>
{
    private readonly IServiceProvider _serviceProvider;

    public StagesListExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public override StageResult Execute(StageResult currentValue, StagesList stage)
    {
        foreach (var stageElement in stage.Stages)
        {
            currentValue = stageElement switch
            {
                SelectStage selectStage => Execute(currentValue, selectStage),
                SelectManyStage selectManyStage => Execute(currentValue, selectManyStage),
                FilterStage filterStage => Execute(currentValue, filterStage),
                _ => throw new NotImplementedException(),
            };
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