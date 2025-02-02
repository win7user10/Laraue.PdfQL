using System.Linq.Expressions;

namespace Laraue.PdfQL.TreeExecution.Expressions.MethodCalls;

public abstract class MethodCallVisitor
{
    public abstract string MethodName { get; }

    public abstract Expression Visit(Expression objectExpression, Expression[] argumentExpressions);
}