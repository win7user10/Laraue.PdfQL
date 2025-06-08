using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;

public class DelegateCompilingError
{
    public Stage? Stage { get; set; }
    public required string Error { get; set; }
}