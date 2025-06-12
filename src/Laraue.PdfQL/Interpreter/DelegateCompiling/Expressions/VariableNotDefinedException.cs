namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class VariableNotDefinedException(string name)
    : ExpressionCompileException($"Variable '{name}' is not defined")
{
}