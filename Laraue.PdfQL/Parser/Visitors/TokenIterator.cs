using System.Text;

namespace Laraue.PdfQL.Parser.Visitors;

public class TokenIterator
{
    private readonly FilterStageTokenVisitor.Token[] _tokens;
    private int _index;
        
    private readonly Stack<int> _breakPoints = new();

    public TokenIterator(FilterStageTokenVisitor.Token[] tokens)
    {
        _tokens = tokens;
    }
        
    public FilterStageTokenVisitor.Token Current => _tokens.ElementAt(_index);
    public int CurrentIndex => _index;
    public void ToNext() => _index++;
    public void SetIndex(int index) => _index = index;
    public void AddBreakPoint() => _breakPoints.Push(_index);

    public int ResetBreakPoint()
    {
        var index = _index;
        _index = _breakPoints.Pop();
        return index;
    }

    public bool FullyParsed()
    {
        return _index == _tokens.Length;
    }

    public void ThrowIfNotAllTokensParsed()
    {
        if (!FullyParsed())
        {
            throw new InvalidSyntaxException($"Not all tokens parsed. Current symbols position: {ToString()}");
        }
    }
        
    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < _tokens.Length; i++)
        {
            var v = _tokens[i];
            sb.Append(i == _index ? $" -->{v.Type}<-- " : v.Type);
        }
            
        return sb.ToString();
    }
}