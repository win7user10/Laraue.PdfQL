namespace Laraue.PdfQL.Parser.Visitors;

public static class StackExtensions
{
    public static void UpdateLastValue<T>(this Stack<T> stack, Func<T, T> update)
    {
        stack.Push(update(stack.Pop()));
    }
}