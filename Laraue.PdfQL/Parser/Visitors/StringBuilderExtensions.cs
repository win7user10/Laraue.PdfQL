using System.Text;

namespace Laraue.PdfQL.Parser.Visitors;

public static class StringBuilderExtensions
{
    public static StringBuilder AddNewLineIndent(this StringBuilder sb, int count)
    {
        sb.Append(Environment.NewLine);
        for (var i = 0; i < count; i++)
        {
            sb.Append(" ");
        }

        return sb;
    }
}