namespace Laraue.PdfQL.PdfObjects;

public abstract class PdfObject
{
    public abstract object ToJson();

    public bool TryParse(ScalarType type)
    {
        var value = ToJson();
        if (value is not string json)
        {
            return false;
        }

        return type switch
        {
            ScalarType.Boolean => bool.TryParse(json, out _),
            ScalarType.Float => float.TryParse(json, out _),
            ScalarType.Int64 => long.TryParse(json, out _),
            ScalarType.String => true,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}