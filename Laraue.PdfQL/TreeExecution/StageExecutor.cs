using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.TreeExecution;

public abstract class StageExecutor<TStage> : Executor<TStage>
    where TStage : Stage
{
}