namespace Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

public class ScanError
{
    public int Position { get; set; }
    public required string Error { get; set; }
}