using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public interface ITranslator
{
    TranslationResult Translate(Expr expr, TranslationContext translationContext);
}