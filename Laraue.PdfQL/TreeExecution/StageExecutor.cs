using Laraue.PQL.Stages;

namespace Laraue.PQL.TreeExecution;

public abstract class StageExecutor<TStage> : Executor<TStage>
    where TStage : Stage
{
}