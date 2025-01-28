using Laraue.PQL.StageResults;

namespace Laraue.PQL.TreeExecution;

public abstract class Executor<TCommand>
{
    public abstract StageResult Execute(StageResult currentValue, TCommand command);
}