namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public class SelectStage : Stage
{
    public SelectStage(PdfElement selectElement)
    {
        SelectElement = selectElement;
    }

    PdfElement SelectElement { get; set; }
}