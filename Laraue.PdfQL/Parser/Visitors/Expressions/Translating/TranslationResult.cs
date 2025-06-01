using System.Linq.Expressions;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class TranslationResult
{
    public Expression? Expression { get; set; }
    public required TranslationError[] Errors { get; set; }
    public bool HasErrors => Errors.Any();
}