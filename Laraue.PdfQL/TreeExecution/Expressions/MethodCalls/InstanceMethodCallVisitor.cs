using System.Linq.Expressions;
using System.Reflection;
using Laraue.PdfQL.TreeExecution.Expressions.Exceptions;

namespace Laraue.PdfQL.TreeExecution.Expressions.MethodCalls;

public abstract class InstanceMethodCallVisitor : MethodCallVisitor
{
    public override Expression Visit(Expression objectExpression, Expression[] argumentExpressions)
    {
        var method = objectExpression.Type
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == MethodName);

        if (method == null)
        {
            throw new UnknownMethodCallException($"Method {MethodName} is not exists on {objectExpression.Type}");
        }
        
        return Expression.Call(objectExpression, method, argumentExpressions);
    }
}