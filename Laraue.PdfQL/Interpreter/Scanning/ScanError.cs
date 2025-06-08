namespace Laraue.PdfQL.Interpreter.Scanning;

public class ScanError
{
    public int Position { get; set; }
    public required string Error { get; set; }
}