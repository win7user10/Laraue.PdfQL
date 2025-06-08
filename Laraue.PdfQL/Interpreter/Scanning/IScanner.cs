namespace Laraue.PdfQL.Interpreter.Scanning;

public interface IScanner
{
    ScanResult ScanTokens(string input);
}