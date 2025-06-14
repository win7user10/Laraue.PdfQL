using Laraue.PdfQL.PdfObjects;

namespace Laraue.PdfQL;

/// <summary>
/// Class that runs PdfQL execution.
/// </summary>
public interface IPdfqlExecutor
{
    /// <summary>
    /// Execute PdfQL.
    /// </summary>
    /// <param name="pdfql">Text of PdfQL query.</param>
    /// <param name="document">PDF document.</param>
    /// <returns></returns>
    object ExecutePdfql(string pdfql, PdfDocument document);
    
    /// <summary>
    /// Execute PdfQL and cast the result to the passed generic type ot throw the error on wrong cast.
    /// </summary>
    /// <param name="pdfql"></param>
    /// <param name="document"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T ExecutePdfql<T>(string pdfql, PdfDocument document);
    
    /// <summary>
    /// Try to execute PdfQL and returns errors in the result if they are occured.
    /// </summary>
    /// <param name="pdfql">Text of PdfQL query.</param>
    /// <param name="document">PDF document.</param>
    /// <returns></returns>
    PsqlExecutionResult TryExecutePdfql(string pdfql, PdfDocument document);
    
    /// <summary>
    /// Check PdfQL syntax and returns found errors.
    /// </summary>
    /// <param name="pdfql"></param>
    /// <returns></returns>
    PSqlSyntaxCheckResult CheckSyntax(string pdfql);
}

public class PsqlExecutionResult : PSqlSyntaxCheckResult
{
    public object? Result { get; set; }
}

public class PSqlSyntaxCheckResult
{
    public List<PsqlCompileError> Errors { get; set; } = new ();
    public bool HasErrors => Errors.Any();
}

public class PsqlCompileError
{
    public required string Message { get; init; }
}