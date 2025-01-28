namespace Laraue.PQL.Stages;

public class SelectStage : Stage
{
    public required Selector Selector { get; init; }
}