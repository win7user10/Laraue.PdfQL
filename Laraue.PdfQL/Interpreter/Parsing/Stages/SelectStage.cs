namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class SelectStage : Stage
{
    public SelectStage(PdfElement selectElement)
    {
        SelectElement = selectElement;
    }

    public PdfElement SelectElement { get; set; }

    public override string ToString()
    {
        return $"select({SelectElement})";
    }
}