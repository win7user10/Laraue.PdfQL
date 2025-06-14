using System.Text;

namespace Laraue.PdfQL;

public class PdfqlCompileException : Exception
{
    public IReadOnlyList<PsqlCompileError> Errors { get; }

    public PdfqlCompileException(List<PsqlCompileError> errors)
        : base(GetErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string GetErrorMessage(List<PsqlCompileError> errors)
    {
        var sb = new StringBuilder("PSql compile error:");

        foreach (var error in errors)
        {
            sb.AppendLine();
            sb.Append(error.Message);
        }

        return sb.ToString();
    }
}