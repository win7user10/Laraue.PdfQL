using Laraue.PdfQL.Interpreter.Parsing.Stages;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class DelegateCompilingError
{
    public required Stage Stage { get; set; }
    public required string Error { get; set; }
}