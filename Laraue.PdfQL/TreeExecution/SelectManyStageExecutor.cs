using Laraue.PQL.PdfObjects;
using Laraue.PQL.StageResults;
using Laraue.PQL.Stages;

namespace Laraue.PQL.TreeExecution;

public class SelectManyStageExecutor : StageExecutor<SelectManyStage>
{
    public override StageResult Execute(StageResult currentValue, SelectManyStage stage)
    {
        if (currentValue is not PdfObjectStageResult { PdfObject: PdfObjectContainer pdfObjectContainer })
        {
            throw new InvalidOperationException();
        }
        
        return stage.Selector switch
        {
            Selector.TableRows => pdfObjectContainer.GetTablesRowsContainerOrThrow(),
            _ => throw new NotImplementedException()
        };
    }
}