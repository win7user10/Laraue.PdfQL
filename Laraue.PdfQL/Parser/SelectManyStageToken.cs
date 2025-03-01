using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser;

public class SelectManyStageToken : StageToken
{
    public required Selector Selector { get; set; }
}