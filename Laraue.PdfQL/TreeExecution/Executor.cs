using Laraue.PdfQL.StageResults;

namespace Laraue.PdfQL.TreeExecution;

public abstract class Executor<TCommand>
{
    public abstract StageResult Execute(StageResult currentValue, TCommand command);
}