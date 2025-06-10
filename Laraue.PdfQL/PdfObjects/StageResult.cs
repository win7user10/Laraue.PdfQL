using System.Collections;
using System.Text;

namespace Laraue.PdfQL.PdfObjects;

public class StageResult : PdfObject, IEnumerable<object>
{
    private readonly object[] _values;

    public StageResult(object[] values)
    {
        _values = values;
    }

    public IEnumerator<object> GetEnumerator()
    {
        return _values.Cast<object>().GetEnumerator();
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

public class StageResult<TPdfObject> : StageResult, IEnumerable<TPdfObject>
{
    private readonly TPdfObject[] _values;

    public StageResult(TPdfObject[] values)
        : base(values.Cast<object>().ToArray())
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
    
    public int Count => _values.Length; 
}