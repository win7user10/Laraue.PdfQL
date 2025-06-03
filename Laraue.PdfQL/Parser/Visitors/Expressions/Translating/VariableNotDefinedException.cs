namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class VariableNotDefinedException(string name)
    : TranslationException($"Variable '{name}' is not defined")
{
}