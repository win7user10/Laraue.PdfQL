namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class Stage
{
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public int StartLineNumber { get; set; }
    public int EndLineNumber { get; set; }
}