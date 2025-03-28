using System.Diagnostics;
using System.Text.RegularExpressions;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors;

public class FilterStageTokenVisitor : StageTokenVisitor<FilterStageToken>
{
    private const char StringToken = '\'';

    private static OrderedDictionary<string, string> Grammar = new()
    {
        // How to represent regex here?
        ["<Arguments>"] = "<LeftBracket><Argument>[<Comma><Argument>]?+<RightBracket>",
        ["<Argument>"] = "<String>|<Number>",
        ["<MethodCall>"] = "<Word><Arguments>",
        ["<MemberAccess>"] = "<Word><Dot>",
        
        ["<Expression>"] = "<MethodCallExpression>|<BinaryExpression>",
        ["<MethodCallExpression>"] = "<MemberAccess><MethodCall>|<Expression><MethodCall>",
        ["<BinaryExpression>"] = "<Expression><Operand><Expression>",
    };

    
    
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
        var nodeList = GetChildrenNodes2(tokens);
        
        // $item.CellAt(4).Text() = 'Лейкоциты (WBC)'
        
        // Is valid expression means it matches Grammar of "Expression"
        
        throw new NotImplementedException();
    }

    private Node? GetChildrenNodes2(TokenTreeIterator tokenTreeIterator)
    {
        while (true)
        {
            var nextNode = tokenTreeIterator.GetNextNodeToReduce();
            if (nextNode == null)
            {
                break;
            }
            
            var nextIterator = new TokenTreeIterator(nextNode);
            var processedNode = ProcessNextNode(nextIterator);
            tokenTreeIterator.ReplaceNextNode(processedNode);
        }
        
        Debug.WriteLine($"The final iterator is {tokenTreeIterator}");
        throw new NotImplementedException();
    }

    private Node ProcessNextNode(TokenTreeIterator tokenTreeIterator)
    {
        foreach (var grammar in Grammar)
        {
            // Debug.WriteLine($"Looking for suitable grammar. Try: {grammar.Key}. Tokens should match: {grammar.Value}");
            if (DoesMatchNode(tokenTreeIterator, grammar.Value))
            {
                Debug.WriteLine($"Node {grammar.Key} is chosen for {tokenTreeIterator}");
                return new Node { Value = grammar.Key };
            }

            // Debug.WriteLine($"Grammar: {grammar.Key} does not match iterator tokens: {tokenTreeIterator}");
            // Debug.WriteLine(string.Empty);
        }

        throw new InvalidSyntaxException($"Unknown tokens sequence {tokenTreeIterator}");
    }

    private bool DoesMatchNode(TokenTreeIterator tokenTreeIterator, string grammar)
    {
        var orGrammar = ParseNextOrGrammar(grammar);
        for (var index = 0; index < orGrammar.OrGrammars.Length; index++)
        {
            tokenTreeIterator.AddBreakPoint();
            var sequentialGrammar = orGrammar.OrGrammars[index];
            
            var sequentialGrammarParts = ParseSimpleGrammars(sequentialGrammar);
            foreach (var sequentialGrammarPart in sequentialGrammarParts)
            {
                //Debug.WriteLine($"Try match next token: {sequentialGrammarPart.Grammar}");
                var childNodeMatch = DoesMatchNode(tokenTreeIterator, sequentialGrammarPart);
                if (childNodeMatch == false)
                {
                    //Debug.WriteLine($"No token found for: {sequentialGrammarPart.Grammar}");
                    return false;
                }
                
                if (tokenTreeIterator.AllNodesParsed())
                {
                    return true;
                }
                
                tokenTreeIterator.ToNext();
            }

            if (tokenTreeIterator.AllNodesParsed())
            {
                return true;
            }
            
            tokenTreeIterator.ResetBreakPoint();
        }

        return false;
    }
    
    private bool DoesMatchNode(TokenTreeIterator tokenTreeIterator, NextGrammar grammar)
    {
        switch (grammar)
        {
            case NextExactGrammar when Grammar.TryGetValue(grammar.Grammar, out var grammarDefinition):
                // Debug.WriteLine($"Token: {grammar.Grammar} is consists of {grammarDefinition}");
                return DoesMatchNode(tokenTreeIterator, grammarDefinition);
            case NextExactGrammar:
                return true;
            case NextOnceOrNeverGrammar or NextAnyTimeGrammar:
                tokenTreeIterator.AddBreakPoint();
                try
                {
                    return DoesMatchNode(tokenTreeIterator, new NextExactGrammar
                    {
                        Grammar = grammar.Grammar,
                        RemainedString = grammar.RemainedString
                    });
                }
                catch (InvalidSyntaxException)
                {
                    tokenTreeIterator.ResetBreakPoint();
                    return false;
                }
        }
        
        throw new Exception();
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
    
    private record struct NextOrGrammar
    {
        public string[] OrGrammars { get; set; }
        public NextOrGrammar(string[] OrGrammars)
        {
            this.OrGrammars = OrGrammars;
        }

        public override string ToString()
        {
            return string.Join("|", OrGrammars);
        }
    }

    private TokenTreeIterator ParseTokens(ReadOnlySpan<char> input)
    {
        var result = new TokenTree();
        
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
            
            if (!Operands.ContainsOperand(currentToken))
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
                result.AddToken(new Token
                {
                    Name = GetTokenType(input[statement]),
                    Value = input[statement].ToString()
                });
            }
            
            var token = new Range(currentTokenIndex, ++currentTokenIndex);
            result.AddToken(new Token
            {
                Name = Operands.NameOf(currentToken),
                Value = input[token].ToString()
            });
            
            previousTokenIndex = lastNonWhiteSpaceIndex = currentTokenIndex;
            previousCharIsToken = true;
        }

        var lastTokenRange = new Range(previousTokenIndex, new Index(0, true));
        result.AddToken(new Token
        {
            Value = input[lastTokenRange].ToString(),
            Name = GetTokenType(input[lastTokenRange])
        });

        return new TokenTreeIterator(result);
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