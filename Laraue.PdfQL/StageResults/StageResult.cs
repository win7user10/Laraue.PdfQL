namespace Laraue.PdfQL.StageResults;

public abstract class StageResult
{
    /// <summary>
    /// Object that can be serialized.
    /// </summary>
    public abstract object ToJsonObject();
}