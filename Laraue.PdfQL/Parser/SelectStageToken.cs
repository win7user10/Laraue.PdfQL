using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser;

public class SelectStageToken : StageToken
{
    public required Selector Selector { get; set; }
}