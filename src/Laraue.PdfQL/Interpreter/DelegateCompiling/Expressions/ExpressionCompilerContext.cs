namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public class ExpressionCompilerContext
{
    public List<Type> ParameterTypes { get; } = new();
    public required AnonymousTypeRegistry AnonymousTypeRegistry { get; set; }
}