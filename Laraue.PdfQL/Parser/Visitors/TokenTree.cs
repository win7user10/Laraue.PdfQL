using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Laraue.PdfQL.Parser.Visitors;

public class TokenTree
{
    private readonly TokenGroup _source;

    private readonly Stack<TokenGroup> _currentGroupPath = new();
    public TokenGroup CurrentGroup => _currentGroupPath.Peek();

    public TokenTree(TokenGroup source)
    {
        _source = source;
        _currentGroupPath.Push(source);
    }

    private void PushGroup(TokenGroup group)
    {
        _currentGroupPath.Push(group);
        _source.Tokens.Add(CurrentGroup);
    }

    private void WrapCurrentTokensInGroup()
    {
        // Move the whole group into a nested group
        var nestedGroup = new TokenGroup();
            
        CurrentGroup.Tokens.ForEach(nestedGroup.Tokens.Add);
        CurrentGroup.Tokens.RemoveAll(_ => true);
        CurrentGroup.Tokens.Add(nestedGroup);
    }

    public void AddToken(Token token)
    {
        if (token.Name == Operands.NameOf(Operands.LeftBracket))
        {
            PushGroup(new TokenGroup());
        }

        if (Operands.ContainsBinaryOperand(token.Name))
        {
            WrapCurrentTokensInGroup();
            CurrentGroup.MaxTokenPriority++;
        }
        
        CurrentGroup.Tokens.Add(new TokenWithPriority(token.Value)
        {
            Name = token.Name,
            Priority = CurrentGroup.MaxTokenPriority
        });
        
        if (token.Name == Operands.NameOf(Operands.Dot))
        {
            WrapCurrentTokensInGroup();
            CurrentGroup.MaxTokenPriority++;
        }
        
        if (token.Name == Operands.NameOf(Operands.RightBracket))
        {
            _currentGroupPath.Pop();
        }
    }

    public TokenTree? GetNextGroup()
    {
        var source = GetNextGroupTokens();
        return source == null
            ? null
            : new TokenTree(source);
    }

    public void ReplaceNextNode(Node newNode)
    {
        var path = new Stack<TokenGroup>();
        path.Push(_source);
        FillNextTokenGroupPath(_source.Tokens, path);

        var elementToRemove = path.Pop();
        var elementParent = path.Pop();
        
        // add node to parent instead of removed token
        var oldElementIndex = elementParent.Tokens.IndexOf(elementToRemove);
        elementParent.Tokens.Remove(elementToRemove);
        elementParent.Tokens.Insert(oldElementIndex, new TokenWithPriority(elementToRemove.Value)
        {
            Name = newNode.Value,
        });
        
        Debug.WriteLine($"New iterator state {this}");
    }

    public void MoveCurrentGroupTokensToUpperLevel()
    {
        var path = new Stack<TokenGroup>();
        path.Push(_source);
        FillNextTokenGroupPath(_source.Tokens, path);
        
        var groupToMove = path.Pop();
        var elementParent = path.Pop();
        
        var oldElementIndex = elementParent.Tokens.IndexOf(groupToMove);
        elementParent.Tokens.Remove(groupToMove);
        elementParent.Tokens.Insert(oldElementIndex, groupToMove);
        
        Debug.WriteLine($"New iterator state {this}");
    }

    private TokenGroup? GetNextGroupTokens()
    {
        return GetNextTokenGroup(_source.Tokens);
    }

    private static TokenGroup? GetNextTokenGroup(ICollection<TokenBase> tokens)
    {
        var path = new Stack<TokenGroup>();
        
        FillNextTokenGroupPath(tokens, path);
        
        return path.Count > 0 ? path.Peek() : null;
    }
    
    private static void FillNextTokenGroupPath(ICollection<TokenBase> tokens, Stack<TokenGroup> path)
    {
        foreach (var token in tokens)
        {
            if (token is not TokenGroup tokenGroupToken)
            {
                continue;
            }

            path.Push(tokenGroupToken);
            FillNextTokenGroupPath(tokenGroupToken.Tokens, path);
            break;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.Append(Environment.NewLine);
        FillGroupInfo(sb, _source, 0);
        sb.Append(Environment.NewLine);
        
        return sb.ToString();
    }

    private static void FillGroupInfo(StringBuilder stringBuilder, TokenGroup token, int level)
    {
        foreach (var child in token.Tokens)
        {
            switch (child)
            {
                case TokenGroup tokenGroup:
                    stringBuilder.AddNewLineIndent(level).Append('(');
                    FillGroupInfo(stringBuilder, tokenGroup, level + 1);
                    stringBuilder.AddNewLineIndent(level).Append(')');
                    break;
                case TokenWithPriority simpleToken:
                    stringBuilder.AddNewLineIndent(level).Append(simpleToken.Name);
                    break;
            }
        }
    }
}