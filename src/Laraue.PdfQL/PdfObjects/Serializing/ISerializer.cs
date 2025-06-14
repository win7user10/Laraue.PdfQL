namespace Laraue.PdfQL.PdfObjects.Serializing;

public interface ISerializer
{
    object ToJsonObject(object obj);
}