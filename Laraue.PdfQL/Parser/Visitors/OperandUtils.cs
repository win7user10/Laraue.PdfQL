namespace Laraue.PdfQL.Parser.Visitors;

public static class OperandUtils
{
    public static string Tokenize(string input)
    {
        return $"<{input}>";
    }
}