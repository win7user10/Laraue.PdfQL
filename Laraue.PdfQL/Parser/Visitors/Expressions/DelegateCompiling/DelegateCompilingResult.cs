using Laraue.PdfQL.PdfObjects;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;

public class DelegateCompilingResult
{
    public required Func<PdfDocument, object> Delegate { get; set; }
    public required DelegateCompilingError[] Errors { get; set; }
    public bool HasErrors => Errors.Any();
}