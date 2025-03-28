using System.Diagnostics;

namespace Laraue.PdfQL.Parser.Visitors;

public class TokenTreeIterator
{
    private readonly TokenTree _tokenTree;
    private int _index;
        
    private readonly Stack<int> _breakPoints = new();

    public TokenTreeIterator(TokenTree tokenTree)
    {
        _tokenTree = tokenTree;
        
        Debug.WriteLine($"Initialized Token Tree Iterator: {this}");
    }

    public TokenTree? GetNextNodeToReduce()
    {
        return _tokenTree.GetNextGroup();
    }
    
    public void ReplaceNextNode(Node newNode)
    {
        _tokenTree.ReplaceNextNode(newNode);
    }
    
    public bool AllNodesParsed() => _index == _tokenTree.Tokens.Count - 1;
        
    public Token Current => _tokenTree.Tokens.ElementAt(_index);

    public void ToNext()
    {
        var previous = Current.Name;
        
        _index++;

        // Debug.WriteLine($"Iterator switched: {previous} ({_index}) -> {Current.Name} ({_index})");
    }
    
    public void AddBreakPoint() => _breakPoints.Push(_index);

    public void ResetBreakPoint()
    {
        var previous = Current.Name;
        var previousIndex = _index;
        
        _index = _breakPoints.Pop();
        
        // Debug.WriteLine($"Iterator reset: {previous} ({previousIndex}) -> {Current.Name} ({_index})");
    }
        
    public override string ToString()
    {
        return _tokenTree.ToString();
    }
}