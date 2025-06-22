namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class SkipStage : Stage
{
    public const string Name = "skip";
    
    public SkipStage(int count)
    {
        Count = count;
    }

    public int Count { get; }

    public override string ToString()
    {
        return $"{Name}({Count})";
    }
}