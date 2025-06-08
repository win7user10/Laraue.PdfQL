namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class SelectManyStage : Stage
{
    public SelectManyStage(PdfElement selectElement)
    {
        SelectElement = selectElement;
    }

    public PdfElement SelectElement { get; set; }
}