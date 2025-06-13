using System.Text;

namespace Laraue.PdfQL;

public class PSqlCompileException : Exception
{
    public IReadOnlyList<PsqlCompileError> Errors { get; }

    public PSqlCompileException(List<PsqlCompileError> errors)
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