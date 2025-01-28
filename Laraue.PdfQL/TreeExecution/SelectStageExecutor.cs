using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;

namespace Laraue.PQL.TreeExecution;

public class SelectStageExecutor : StageExecutor<SelectStage>
{
    public override StageResult Execute(StageResult currentValue, SelectStage stage)
    {
        return stage.Selector switch
        {
            Selector.Pages => currentValue.GetPagesContainerOrThrow(),
            Selector.Tables => currentValue.GetTablesContainerOrThrow(),
            _ => throw new NotImplementedException()
        };
    }
}