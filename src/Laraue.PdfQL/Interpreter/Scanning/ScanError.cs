namespace Laraue.PdfQL.Interpreter.Scanning;

public class ScanError
{
    public required int StartPosition { get; set; }
    public required int EndPosition { get; set; }
    public required int LineNumber { get; set; }
    public required string Error { get; set; }
}