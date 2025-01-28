namespace Laraue.PQL.Stages;

public class SelectManyStage : Stage
{
    public required Selector Selector { get; init; }
}