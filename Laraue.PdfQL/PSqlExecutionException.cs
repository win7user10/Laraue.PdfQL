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
        var sb = new StringBuilder("PSql execution errors:");

        for (var index = 0; index < errors.Count; index++)
        {
            var error = errors[index];
            sb.AppendLine();
            sb.Append(index + 1);
            sb.Append(". ");
            sb.Append(error.Message);
        }

        return sb.ToString();
    }
}