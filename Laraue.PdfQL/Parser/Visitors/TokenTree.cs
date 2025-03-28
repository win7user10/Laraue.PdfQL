using System.Diagnostics;
using System.Text;

namespace Laraue.PdfQL.Parser.Visitors;

public class TokenTree
{
    public IList<TokenWithGroupNumber> Tokens { get; set; } = new List<TokenWithGroupNumber>();
    private readonly Stack<int> _bracketDotNumbers = new ();

    private readonly string[] _highPriorityOperands =
    [
        Operands.NameOf(Operands.Multiply),
        Operands.NameOf(Operands.Divider)
    ];

    public TokenTree()
    {
        _bracketDotNumbers.Push(0);
    }
    
    public void AddToken(Token token)
    {
        if (token.Name == Operands.NameOf(Operands.LeftBracket))
        {
            _bracketDotNumbers.Push(0);
        }
        
        if (token.Name == Operands.NameOf(Operands.Dot))
        {
            _bracketDotNumbers.UpdateLastValue(v => v + 1);
            var previousToken = Tokens.LastOrDefault();
            if (previousToken != null && previousToken.GroupNumber == _bracketDotNumbers.Count)
            {
                previousToken.Priority++;
            }
        }

        if (Operands.ContainsBinaryOperand(token.Name))
        {
            _bracketDotNumbers.UpdateLastValue(v => v + 1);
        }
        
        Tokens.Add(new TokenWithGroupNumber
        {
            Name = token.Name,
            Value = token.Value,
            GroupNumber = _bracketDotNumbers.Count,
            Priority = _bracketDotNumbers.Peek(),
        });
        
        if (token.Name == Operands.NameOf(Operands.RightBracket))
        {
            _bracketDotNumbers.Pop();
        }
    }
        
    public int GetMaxGroupNumber() => Tokens.Max(t => t.GroupNumber);

    public TokenTree? GetNextGroup()
    {
        var tokens = GetNextGroupTokens();
        return tokens == null
            ? null
            : new TokenTree
            {
                Tokens = tokens
                    .Select(token => new TokenWithGroupNumber
                    {
                        Name = token.Name,
                        Value = token.Value,
                        Priority = 0,
                        GroupNumber = 0
                    })
                    .ToList(),
            };
    }

    private IReadOnlyCollection<TokenWithGroupNumber>? GetNextGroupTokens()
    {
        var maxGroupNumber = GetMaxGroupNumber();
        var groupStarted = false;

        var result = new List<TokenWithGroupNumber>();
        foreach (var token in Tokens)
        {
            if (token.GroupNumber < maxGroupNumber && !groupStarted)
            {
                continue;
            }
                    
            if (token.GroupNumber < maxGroupNumber && groupStarted)
            {
                break;
            }
                
            groupStarted = true;
            result.Add(token);
        }
        
        var minPriorityLevel = result.Min(t => t.Priority);
        result = result
            .Where(i => i.Priority == minPriorityLevel)
            .ToList();
        
        return Tokens.Count == result.Count
            ? null
            : result;
    }

    public void ReplaceNextNode(Node newNode)
    {
        var tokensToRemove = GetNextGroupTokens();
        if (tokensToRemove == null)
        {
            throw new InvalidOperationException("Iterator doesn't have next node to replace");
        }
        
        var firstTokenToRemove = tokensToRemove.First();
        var removeFromIndex = Tokens.IndexOf(firstTokenToRemove);
        
        foreach (var tokenToRemove in tokensToRemove)
        {
            Tokens.Remove(tokenToRemove);
        }

        var previousToken = Tokens.ElementAtOrDefault(removeFromIndex - 1);
        Tokens.Insert(removeFromIndex, new TokenWithGroupNumber
        {
            Name = newNode.Value,
            Value = "Unknown?",
            GroupNumber = firstTokenToRemove.GroupNumber - 1,
            Priority = previousToken?.Priority ?? 0
        });

        // if only one token and more than one remained - decrease level of nodes of the next group and their dot levels
        if (previousToken != null && Tokens.Count > 1)
        {
            var nextIndex = 1;
            int? groupNumberToProcess = null;

            do
            {
                var nextToken = Tokens.ElementAtOrDefault(nextIndex);
                if (nextToken == null)
                {
                    break;
                }
                
                if (groupNumberToProcess == null)
                {
                    groupNumberToProcess = nextToken.GroupNumber;
                }
                
                else if (nextToken.GroupNumber != groupNumberToProcess)
                {
                    break;
                }
                
                nextToken.GroupNumber = previousToken.GroupNumber;
                nextToken.Priority = previousToken.Priority;
                nextIndex++;
            } while (true);
        }
        
        Debug.WriteLine($"New iterator state {this}");
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Environment.NewLine);
        
        foreach (var token in Tokens)
        {
            for (var i = 0; i < token.GroupNumber; i++)
            {
                sb.Append(' ');
            }
            
            sb.Append(token.Name);
            sb.Append($"({token.Priority})");
            sb.Append(Environment.NewLine);
        }
        
        return sb.ToString();
    }
}