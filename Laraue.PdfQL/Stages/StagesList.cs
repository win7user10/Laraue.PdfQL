namespace Laraue.PdfQL.Stages;

public class StagesList : Stage
{
    public required IList<Stage> Stages { get; init; }
}