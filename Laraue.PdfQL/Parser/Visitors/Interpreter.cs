using System.Linq.Expressions;
using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;
using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;
using Laraue.PdfQL.PdfObjects;

namespace Laraue.PdfQL.Parser.Visitors;

public class Interpreter
{
    private readonly Scanner _scanner = new ();
    private readonly Expressions.Parsing.Parser _parser = new ();

    public BinaryExpression ParseBinary(string expression)
    {
        var result = ParseExpression(expression);
        if (result is not BinaryExpression binaryExpression)
            throw new Exception("Invalid binary expression");
        
        return binaryExpression;
    }

    private Expression ParseExpression(string expression)
    {
        var scanResult = _scanner.ScanTokens(expression);
        if (scanResult.HasErrors)
            throw new Exception(string.Join<ScanError>(", ", scanResult.Errors));

        var parseResult = _parser.Parse(scanResult.Tokens);
        if (parseResult.HasErrors)
            throw new Exception(string.Join<ParseError>(", ", parseResult.Errors));

        /*var translationResult = _translator.Translate(
            parseResult.Expression!,
            new TranslationContext { ParameterTypes = { typeof(PdfTable) }});
        
        if (translationResult.HasErrors)
            throw new Exception(string.Join<TranslationError>(", ", translationResult.Errors));

        return translationResult.Expression!;*/

        return null!;
    }
}