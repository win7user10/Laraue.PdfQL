namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling.Expressions;

public class VariableAlreadyDefinedException(string name)
    : ExpressionCompileException($"Variable '{name}' already defined")
{
}