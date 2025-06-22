namespace Laraue.PdfQL.Interpreter.Parsing.Stages;

public class TakeStage : Stage
{
    public const string Name = "take";
    
    public TakeStage(int count)
    {
        Count = count;
    }

    public int Count { get; }

    public override string ToString()
    {
        return $"{Name}({Count})";
    }
}