namespace Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

public class ScanResult
{
    public required Token[] Tokens { get; set; }
    public required ScanError[] Errors { get; set; }
    public bool HasErrors => Errors.Any();
}