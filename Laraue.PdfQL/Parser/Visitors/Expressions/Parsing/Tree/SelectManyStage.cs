namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class SelectManyStage : Stage
{
    public SelectManyStage(PdfElement selectElement)
    {
        SelectElement = selectElement;
    }

    PdfElement SelectElement { get; set; }
}