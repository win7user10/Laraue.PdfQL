using System.Collections;
using System.Text;

namespace Laraue.PdfQL.PdfObjects;

public class PdfObjectContainer : PdfObject, IEnumerable<PdfObject>
{
    private readonly PdfObject[] _values;

    public PdfObjectContainer(PdfObject[] values)
    {
        _values = values;
    }

    public IEnumerator<PdfObject> GetEnumerator()
    {
        return _values.Cast<PdfObject>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"PdfContainer[{_values.Length}]");
        if (_values.Length > 0)
        {
            sb.Append(": [")
                .Append(_values[0])
                .Append("...]");
        }
        
        return sb.ToString();
    }
}

public class PdfObjectContainer<TPdfObject> : PdfObjectContainer, IEnumerable<TPdfObject>
    where TPdfObject : PdfObject
{
    private readonly TPdfObject[] _values;

    public PdfObjectContainer(TPdfObject[] values)
        : base(values.Cast<PdfObject>().ToArray())
    {
        _values = values;
    }

    public new IEnumerator<TPdfObject> GetEnumerator()
    {
        return _values.AsEnumerable().GetEnumerator();
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"PdfContainer[{_values.Length}]");
        if (_values.Length > 0)
        {
            sb.Append(": [")
                .Append(_values[0])
                .Append("...]");
        }
        
        return sb.ToString();
    }
}