using System.Text;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class Utils
{
    public static string GetReadableTypeName(Type type)
    {
        var sb = new StringBuilder(type.Name);
        if (type.IsGenericType)
        {
            sb.Append("[");
            sb.Append(string.Join(",", type.GenericTypeArguments.Select(a => a.Name)));
            sb.Append("]");
        }

        return sb.ToString();
    }
}