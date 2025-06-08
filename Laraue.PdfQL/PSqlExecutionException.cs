namespace Laraue.PdfQL;

public class PSqlExecutionException : Exception
{
    public IReadOnlyList<PsqlExecutionError> Errors { get; }

    public PSqlExecutionException(List<PsqlExecutionError> errors) : base("PSql execution error")
    {
        Errors = errors;
    }
}