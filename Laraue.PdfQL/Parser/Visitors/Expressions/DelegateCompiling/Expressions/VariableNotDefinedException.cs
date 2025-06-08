namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling.Expressions;

public class VariableNotDefinedException(string name)
    : ExpressionCompileException($"Variable '{name}' is not defined")
{
}