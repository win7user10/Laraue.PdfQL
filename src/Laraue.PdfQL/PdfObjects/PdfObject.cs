namespace Laraue.PdfQL.PdfObjects;

public abstract class PdfObject
{
    /// <summary>
    /// Returns Text content of the object.
    /// </summary>
    /// <returns></returns>
    public abstract string Text();
}