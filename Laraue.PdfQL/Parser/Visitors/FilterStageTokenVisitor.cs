using System.Diagnostics;
using System.Text.RegularExpressions;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors;

public class FilterStageTokenVisitor : StageTokenVisitor<FilterStageToken>
{
    private const char StringToken = '\'';
    
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
            if (TryReduceNextNode(nextIterator))
            {
                Debug.WriteLine($"Token tree reduced: {tokenTreeIterator}");
            }
            
            var processedNode = ProcessNextNode(nextIterator);
            tokenTreeIterator.ReplaceNextNode(processedNode);
        }
        
        Debug.WriteLine($"The final iterator is {tokenTreeIterator}");
        throw new NotImplementedException();
    }

    private Node ProcessNextNode(TokenTreeIterator tokenTreeIterator)
    {
        foreach (var grammar in Grammar.Definitions)
        {
            Debug.WriteLine($"Try: {grammar.Key}. Tokens should match: {grammar.Value}");
            if (DoesMatchOrGrammar(tokenTreeIterator, grammar.Value))
            {
                Debug.WriteLine($"Node {grammar.Key} is chosen for {tokenTreeIterator}");
                return new Node { Value = grammar.Key };
            }

            Debug.WriteLine($"- No matched: {grammar.Key} for: {tokenTreeIterator}");
            Debug.WriteLine(string.Empty);
        }

        throw new InvalidSyntaxException($"Unknown tokens sequence {tokenTreeIterator}");
    }
    
    private Dictionary<string, NextExactGrammar[]>? _exactGrammarsDictionary;
    
    private bool TryReduceNextNode(TokenTreeIterator tokenTreeIterator)
    {
        if (_exactGrammarsDictionary == null)
        {
            _exactGrammarsDictionary = new Dictionary<string, NextExactGrammar[]>();
            
            foreach (var grammar in Grammar.Definitions)
            {
                var orGrammar = ParseNextOrGrammar(grammar.Value);
                if (orGrammar.OrGrammars.Length > 1)
                {
                    continue;
                }
                
                var sequentialGrammarParts = ParseSimpleGrammars(orGrammar.OrGrammars[0]);
                var exactGrammarParts = sequentialGrammarParts
                    .OfType<NextExactGrammar>()
                    .ToArray();

                if (exactGrammarParts.Length == sequentialGrammarParts.Length)
                {
                    _exactGrammarsDictionary.Add(grammar.Key, exactGrammarParts);
                }
            }
        }

        var somethingReduced = false;
        
        TryReduceGrammar:
        foreach (var exactGrammarList in _exactGrammarsDictionary)
        {
            if (tokenTreeIterator.TryReduceNodes(exactGrammarList.Value, exactGrammarList.Key))
            {
                somethingReduced = true;
                goto TryReduceGrammar;
            }
        }
        
        // Try detect full reduce and return if it was
        return somethingReduced;
    }

    private bool DoesMatchOrGrammar(TokenTreeIterator tokenTreeIterator, string grammar)
    {
        var orGrammar = ParseNextOrGrammar(grammar);
        foreach (var sequentialGrammar in orGrammar.OrGrammars)
        {
            if (DoesMatchSequentialGrammar(tokenTreeIterator, sequentialGrammar))
            {
                return true;
            }
        }
        
        return false;
    }

    private bool DoesMatchSequentialGrammar(TokenTreeIterator tokenTreeIterator, string sequentialGrammar)
    {
        tokenTreeIterator.AddBreakPoint();
        
        var sequentialGrammarParts = ParseSimpleGrammars(sequentialGrammar);
        var breakDetected = false;

        foreach (var sequentialGrammarPart in sequentialGrammarParts)
        {
            Debug.WriteLine($"Try match next token as: {sequentialGrammarPart.Grammar}");
            var childNodeMatch = DoesMatchGrammar(tokenTreeIterator, sequentialGrammarPart);
            if (childNodeMatch == false)
            {
                // Debug.WriteLine($"- No matched: {sequentialGrammarPart.Grammar}");
                // Debug.WriteLine($"Iterator: {tokenTreeIterator}");
                breakDetected = true;
                break;
            }

            // Debug.WriteLine($"+ Matched: {sequentialGrammarPart.Grammar}");
            // Debug.WriteLine($"Iterator: {tokenTreeIterator}");
        }
        
        // It can be top level iterator. This level don't know is it completed.
        if (!breakDetected)
        {
            return true;
        }
            
        tokenTreeIterator.ResetBreakPoint();
        return false;
    }

    private bool DoesMatchNextExactGrammar(TokenTreeIterator tokenTreeIterator, NextExactGrammar grammar)
    {
        var result = tokenTreeIterator.IsCurrentElementMatches(grammar.Grammar);
        if (result)
        {
            tokenTreeIterator.ToNext();
            return true;
        }
        
        if (Grammar.Definitions.TryGetValue(grammar.Grammar, out var grammarDefinition))
        {
            return DoesMatchOrGrammar(tokenTreeIterator, grammarDefinition);
        }

        if (!Operands.ContainsOperand(grammar.Grammar) && !Grammar.ReservedWords.Contains(grammar.Grammar))
        {
            throw new InvalidSyntaxException($"Unknown grammar {grammar.Grammar}");
        }

        return false;
    }
    
    private bool DoesMatchNextExactAnyTimeGrammar(TokenTreeIterator tokenTreeIterator, RegexGrammar grammar)
    {
        tokenTreeIterator.AddBreakPoint();
        var result = DoesMatchSequentialGrammar(tokenTreeIterator, grammar.Grammar);

        if (!result)
        {
            tokenTreeIterator.ResetBreakPoint();
        }

        // It is no matter for Regex, was match or no.
        return true;
    }
    

    private bool DoesMatchGrammar(TokenTreeIterator tokenTreeIterator, NextGrammar grammar)
    {
        return grammar switch
        {
            NextExactGrammar nextExactGrammar => DoesMatchNextExactGrammar(tokenTreeIterator, nextExactGrammar),
            RegexGrammar regexGrammar => DoesMatchNextExactAnyTimeGrammar(tokenTreeIterator, regexGrammar),
            _ => throw new InvalidOperationException(),
        };
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
        var result = new TokenTree(new TokenGroup());
        
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
                result.AddToken(new Token(input[statement].ToString())
                {
                    Name = GetTokenType(input[statement]),
                });
            }
            
            var token = new Range(currentTokenIndex, ++currentTokenIndex);
            result.AddToken(new Token(input[token].ToString())
            {
                Name = Operands.NameOf(currentToken),
            });
            
            previousTokenIndex = lastNonWhiteSpaceIndex = currentTokenIndex;
            previousCharIsToken = true;
        }

        var lastTokenRange = new Range(previousTokenIndex, new Index(0, true));
        result.AddToken(new Token(input[lastTokenRange].ToString())
        {
            Name = GetTokenType(input[lastTokenRange])
        });
        
        Debug.WriteLine($"Initialized tree {result}");

        return new TokenTreeIterator(result);
    }

    private static string GetTokenType(ReadOnlySpan<char> token)
    {
        if (Regex.IsMatch(token, @"\'.*\'"))
        {
            return Grammar.Tokens.String;
        }
        
        if (Regex.IsMatch(token, "[\\w][\\w|\\d]+"))
        {
            return Grammar.Tokens.Identifier;
        }
        
        if (Regex.IsMatch(token, "[\\d]+"))
        {
            return Grammar.Tokens.Number;
        }

        return "<Unknown>";
    }
}