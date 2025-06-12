namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class VariableAlreadyDefinedException(string name)
    : ExpressionCompileException($"Variable '{name}' already defined")
{
}