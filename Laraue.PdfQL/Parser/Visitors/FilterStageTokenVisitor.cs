using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors;

public static class Operands
{
    public const char StartParameter = '$';
    public const char Plus = '+';
    public const char Minus = '-';
    public const char Multiply = '*';
    public const char Divider = '/';
    public const char Equal = '=';
    public const char LeftBracket = '(';
    public const char RightBracket = ')';
    public const char Comma = ',';
    public const char Dot = '.';
    public const char WhiteSpace = ' ';
}

public class FilterStageTokenVisitor : StageTokenVisitor<FilterStageToken>
{
    private const char StringToken = '\'';

    public FilterStageTokenVisitor()
    {
        _tokens = typeof(Operands)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .ToDictionary(field => (char)field.GetValue(null)!, field => $"<{field.Name}>");
        
        _tokenNames = _tokens.Values.ToHashSet();
    }

    private readonly Dictionary<char, string> _tokens;
    private readonly HashSet<string> _tokenNames;
    private static Dictionary<string, string> Grammar = new()
    {
        // How to represent regex here?
        ["<Argument>"] = "<String>|<Number>",
        ["<Arguments>"] = "<Argument>[<Comma><Argument>]?+",
        ["<Parameter>"] = "<StartParameter><Word>",
        ["<Expression>"] = "<MethodCallExpression>|<BinaryExpression>",
        ["<MethodCallExpression>"] = "<Parameter><Dot><Word><LeftBracket>[<Arguments>]?<RightBracket>",
        ["<BinaryExpression>"] = "<Expression><Operand><Expression>",
    };

    private class Node
    {
        public string Value { get; set; } = string.Empty;
        public List<Node> Children { get; set; } = [];

        public string StringRepresentation
        {
            get
            {
                var sb = new StringBuilder();
                AppendNodeName(sb, this, 0);
                return sb.ToString();
            }
        }

        private void AppendNodeName(StringBuilder stringBuilder, Node node, int currentDeepLevel)
        {
            if (currentDeepLevel > 0)
            {
                stringBuilder.Append(Environment.NewLine);
            }
            
            for (var i = 0; i < currentDeepLevel * 2; i++)
            {
                stringBuilder.Append(' ');
            }
            
            stringBuilder.Append($"{{{node.Value}}}");
            AppendChildrenNodes(stringBuilder, node.Children, currentDeepLevel);
        }
        
        private void AppendChildrenNodes(StringBuilder stringBuilder, List<Node> children, int currentDeepLevel)
        {
            foreach (var child in children)
            {
                AppendNodeName(stringBuilder, child, currentDeepLevel + 1);
            }
        }

        public override string ToString()
        {
            return StringRepresentation;
        }
    }
    
    public override Stage Visit(FilterStageToken token, ParseContext context)
    {
        var stage = new FilterStage
        {
            BinaryExpression = GetExpression(token.Expression, context) as PsqlBinaryExpression,
            ObjectType = context.CurrentPdfQueryType
        };
        
        throw new NotImplementedException();
    }

    // []
    private PsqlExpression GetExpression(ReadOnlySpan<char> expression, ParseContext context)
    {
        // make a regex for each expression and try match?
        // each sub expression should deep the tree? BinaryExpression {  } ??
        
        var tokens = ParseTokens(expression);
        var nodeList = GetChildrenNodes(tokens, new NextExactGrammar { Grammar = "<Expression>" });
        
        // $item.CellAt(4).Text() = 'Лейкоциты (WBC)'
        
        // Is valid expression means it matches Grammar of "Expression"
        
        throw new NotImplementedException();
    }

    private Node? GetChildrenNodes(TokenIterator tokenIterator, NextGrammar grammar)
    {
        switch (grammar)
        {
            case NextExactGrammar when Grammar.TryGetValue(grammar.Grammar, out var nextGrammar):
            {
                var parsed = ParseNextOrGrammar(nextGrammar);

                for (var index = 0; index < parsed.OrGrammars.Length; index++)
                {
                    var orGrammar = parsed.OrGrammars[index];
                    // TODO - it should be in tokens parser??
                    tokenIterator.AddBreakPoint();

                    Node? result = null;
                    try
                    {
                        var simpleGrammars = ParseSimpleGrammars(orGrammar);
                        foreach (var nextSimpleGrammar in simpleGrammars)
                        {
                            var child = GetChildrenNodes(tokenIterator, nextSimpleGrammar);
                            if (child != null)
                            {
                                result ??= new Node { Value = grammar.Grammar };
                                result.Children.Add(child);
                            }
                        }
                    }
                    catch (InvalidSyntaxException) when (index < parsed.OrGrammars.Length - 1)
                    {
                        // or grammar is not valid, but the next grammar can be correct. Continue the cycle.
                        tokenIterator.ResetBreakPoint();
                        continue;
                    }

                    // parsing is finished. Check excepted and real elements count. Return result
                    if (result != null)
                    {
                        return result;
                    }
                    
                    // No one token returned. Exception.
                    tokenIterator.ResetBreakPoint();
                    throw new InvalidSyntaxException("No one token found");
                }

                break;
            }
            case NextExactGrammar:
            {
                var tokenType = tokenIterator.Current.Type;
                if (tokenType != grammar.Grammar)
                {
                    throw new InvalidSyntaxException($"Excepted {grammar.Grammar} in {tokenIterator}");
                }
            
                tokenIterator.ToNext();
                return new Node { Value = grammar.Grammar };
            }
            case NextOnceOrNeverGrammar or NextAnyTimeGrammar:
                tokenIterator.AddBreakPoint();
                try
                {
                    return GetChildrenNodes(tokenIterator, new NextExactGrammar
                    {
                        Grammar = grammar.Grammar,
                        RemainedString = grammar.RemainedString
                    });
                }
                catch (InvalidSyntaxException)
                {
                    tokenIterator.ResetBreakPoint();
                    return null;
                }
        }

        throw new InvalidSyntaxException($"Excepted {grammar.Grammar} in {tokenIterator}");
    }

    private class TokenIterator
    {
        private readonly Token[] _tokens;
        private int _index;
        
        private readonly Stack<int> _breakPoints = new();

        public TokenIterator(Token[] tokens)
        {
            _tokens = tokens;
        }
        
        public Token Current => _tokens.ElementAt(_index);
        public void ToNext() => _index++;
        
        public void AddBreakPoint() => _breakPoints.Push(_index);
        public void ResetBreakPoint() => _index = _breakPoints.Pop();
        
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

    private const char StartGrammar = '<';
    private const char EndGrammar = '>';
    private const char OrGrammar = '|';

    private readonly TokenReader _tokenReader = new TokenReader(
    [
        new OpenClosePair(StartGrammar, EndGrammar, false),
        new OpenClosePair('[', ']', true),
        new OpenClosePair('\'', null, true),
    ]);
    
    private NextOrGrammar ParseNextOrGrammar(string currentGrammar)
    {
        var orTokens = _tokenReader.Split(currentGrammar, OrGrammar);
        return new NextOrGrammar(orTokens);
    }

    private NextGrammar[] ParseSimpleGrammars(string simpleGrammar)
    {
        var result = new List<NextGrammar>();

        while (true)
        {
            var nextGrammar = _tokenReader.ReadNext(simpleGrammar);
            result.Add(nextGrammar);
            if (string.IsNullOrEmpty(nextGrammar.RemainedString))
            {
                break;
            }
            
            simpleGrammar = nextGrammar.RemainedString;
        }
        
        return result.ToArray();
    }
    
    private record struct NextOrGrammar(
        string[] OrGrammars);

    private TokenIterator ParseTokens(ReadOnlySpan<char> input)
    {
        var tokens = new List<Token>();
        var isStringStarted = false;
        var previousTokenIndex = 0;
        var lastNonWhiteSpaceIndex = 0;
        var currentTokenIndex = 0;
        var previousCharIsToken = true;

        while (currentTokenIndex < input.Length)
        {
            // if not delimiter, index++
            // if delimiter, finish word, add it, add delimiter
            var currentToken = input[currentTokenIndex];

            if (currentToken == StringToken)
            {
                isStringStarted = !isStringStarted;
            }
            
            if (isStringStarted)
            {
                currentTokenIndex++;
                previousCharIsToken = false;
                continue;
            }
            
            if (!_tokens.ContainsKey(currentToken))
            {
                currentTokenIndex++;
                lastNonWhiteSpaceIndex++;
                previousCharIsToken = false;
                continue;
            }
            
            // if whitespace - skip the cycle
            if (currentToken == Operands.WhiteSpace)
            {
                currentTokenIndex++;
                continue;
            }

            if (!previousCharIsToken)
            {
                var statement = new Range(previousTokenIndex, lastNonWhiteSpaceIndex);
                tokens.Add(new Token(statement, GetTokenType(input[statement])));
            }
            
            var token = new Range(currentTokenIndex, ++currentTokenIndex);
            tokens.Add(new Token(token, _tokens[currentToken]));
            
            previousTokenIndex = lastNonWhiteSpaceIndex = currentTokenIndex;
            previousCharIsToken = true;
        }

        var lastTokenRange = new Range(previousTokenIndex, new Index(0, true));
        tokens.Add(new Token(lastTokenRange, GetTokenType(input[lastTokenRange])));

        return new TokenIterator(tokens.ToArray());
    }

    public struct Token(Range range, string type)
    {
        public Range Range { get; } = range;
        public string Type { get; } = type;
    }

    private static string GetTokenType(ReadOnlySpan<char> token)
    {
        if (Regex.IsMatch(token, @"\'.*\'"))
        {
            return "<String>";
        }
        
        if (Regex.IsMatch(token, "[\\w][\\w|\\d]+"))
        {
            return "<Word>";
        }
        
        if (Regex.IsMatch(token, "[\\d]+"))
        {
            return "<Number>";
        }

        return "<Unknown>";
    }
}