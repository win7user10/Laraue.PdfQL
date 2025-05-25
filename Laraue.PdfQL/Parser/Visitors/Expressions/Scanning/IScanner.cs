namespace Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

public interface IScanner
{
    ScanResult ScanTokens(string input);
}