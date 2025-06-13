namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class SelectManyStage : Stage
{
    public const string Name = "selectMany";

    public SelectManyStage(PdfElement selectElement)
    {
        SelectElement = selectElement;
    }

    public PdfElement SelectElement { get; set; }
    
    public override string ToString()
    {
        return $"{Name}({SelectElement})";
    }
}