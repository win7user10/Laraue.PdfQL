namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class VariableAlreadyDefinedException(string name)
    : TranslationException($"Variable '{name}' already defined")
{
}