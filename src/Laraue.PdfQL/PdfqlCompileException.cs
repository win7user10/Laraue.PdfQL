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
        var sb = new StringBuilder();
        
        sb.AppendJoin(Environment.NewLine, errors.Select(e => e.Message));

        return sb.ToString();
    }
}