namespace Laraue.PdfQL.Parser;

public class FilterStageToken : StageToken
{
    public required string Expression { get; set; }
}