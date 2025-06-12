using System.Text;

namespace Laraue.PdfQL;

public class PSqlExecutionException : Exception
{
    public IReadOnlyList<PsqlExecutionError> Errors { get; }

    public PSqlExecutionException(List<PsqlExecutionError> errors)
        : base(GetErrorMessage(errors))
    {
        Errors = errors;
    }

    private static string GetErrorMessage(List<PsqlExecutionError> errors)
    {
        var sb = new StringBuilder("PSql execution error:");

        foreach (var error in errors)
        {
            sb.AppendLine();
            sb.Append(error.Message);
        }

        return sb.ToString();
    }
}