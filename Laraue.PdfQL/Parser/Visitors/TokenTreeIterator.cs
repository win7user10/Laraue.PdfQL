using System.Diagnostics;
using System.Text;

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

    public bool IsCurrentElementMatches(string tokenName)
    {
        if (_tokenTree.CurrentGroup.Tokens.Count <= _index)
        {
            return false;
        }
        
        var valueToCheck = _tokenTree.CurrentGroup.Tokens[_index];

        if (valueToCheck is not TokenWithPriority token)
        {
            throw new InvalidOperationException();
        }

        return token.Name == tokenName;
    }

    public bool TryReduceNodes(NextExactGrammar[] tokenNames, string newTokenName)
    {
        using var tokenNamesEnumerator = tokenNames.AsEnumerable().GetEnumerator();
        tokenNamesEnumerator.MoveNext();
        
        var tokensToReplace = new List<TokenWithPriority>();
        
        foreach (var tokenName in _tokenTree.CurrentGroup.Tokens)
        {
            if (tokenName is TokenWithPriority tokenWithPriority
                && tokenWithPriority.Name == tokenNamesEnumerator.Current.Grammar)
            {
                var moveNextResult = tokenNamesEnumerator.MoveNext();
                tokensToReplace.Add(tokenWithPriority);
                
                Debug.WriteLine($"Run simplify: {newTokenName}");
                if (!moveNextResult)
                {
                    _tokenTree.CurrentGroup.Tokens.ReplaceTokens(tokensToReplace, new TokenWithPriority("Don't know")
                    {
                        Name = newTokenName
                    });

                    if (_tokenTree.CurrentGroup.Tokens.Count == 1)
                    {
                        _tokenTree.MoveCurrentGroupTokensToUpperLevel();
                    }
                    
                    return true;
                }
                
                continue;
            }

            tokenNamesEnumerator.Reset();
            tokenNamesEnumerator.MoveNext();
            tokensToReplace.Clear();
        }

        return false;
    }

    public TokenTree? GetNextNodeToReduce()
    {
        return _tokenTree.GetNextGroup();
    }
    
    public void ReplaceNextNode(Node newNode)
    {
        _tokenTree.ReplaceNextNode(newNode);
    }
    
    public bool AllNodesParsed() => _index == _tokenTree.CurrentGroup.Tokens.Count;

    public void ToNext()
    {
        _index++;

        // Debug.WriteLine($"Iterator switched: {previous} ({_index}) -> {Current.Name} ({_index})");
    }

    public void AddBreakPoint()
    {
        // Debug.WriteLine($"Added Break Point ({_breakPoints.Count + 1}): {this}");
        _breakPoints.Push(_index);
    }

    public void ResetBreakPoint()
    {
        _index = _breakPoints.Pop();
        // Debug.WriteLine($"Reset Break point ({_breakPoints.Count}): {this}");
    }
        
    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var index = 0; index < _tokenTree.CurrentGroup.Tokens.Count; index++)
        {
            var token = _tokenTree.CurrentGroup.Tokens[index];
            if (token is TokenWithPriority tokenWithPriority)
            {
                sb.Append(index == _index ? $" -->{tokenWithPriority.Name}<-- " : tokenWithPriority.Name);
            }
        }

        if (_index == _tokenTree.CurrentGroup.Tokens.Count)
        {
            sb.Append(" (completed)");
        }
        
        return sb.ToString();
    }
}