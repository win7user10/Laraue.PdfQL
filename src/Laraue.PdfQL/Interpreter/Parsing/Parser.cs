using Laraue.PdfQL.Interpreter.Parsing.Expressions;
using Laraue.PdfQL.Interpreter.Parsing.Stages;
using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing;

public class Parser : IParser
{
    public ParseResult Parse(Token[] tokens)
    {
        return new ParserImpl(tokens).ParseStatement();
    }
}

internal class ParserImpl
{
    private readonly Token[] _tokens;
    private readonly List<ParseError> _errors = [];
    private int _current;
    
    private static readonly string[] TableSelectors = [
        Definitions.TableSelector,
        Definitions.TableRowsSelector,
        Definitions.TableCellsSelector
    ];

    public ParserImpl(Token[] tokens)
    {
        _tokens = tokens;
    }

    public ParseResult ParseStatement()
    {
        return Parse();
    }
    
    public ParseResult Parse()
    {
        try
        {
            var stages = Stages();
            return new ParseResult
            {
                Stages = stages,
                Errors = _errors.ToArray()
            };
        }
        catch (ParseException)
        {
            return new ParseResult
            {
                Errors = _errors.ToArray(),
                Stages = new List<Stage>()
            };
        }
    }

    private List<Stage> Stages()
    {
        var stages = new List<Stage>();
        
        while (Match(TokenType.Identifier))
        {
            var stageName = Previous();
            Consume(TokenType.LeftBracket, "'(' excepted after stage definition.");

            switch (stageName.Lexeme)
            {
                case Parsing.Stages.SelectStage.Name:
                    stages.Add(SelectStage());
                    break;
                case Parsing.Stages.FilterStage.Name:
                    stages.Add(FilterStage());
                    break;
                case Parsing.Stages.SelectManyStage.Name:
                    stages.Add(SelectManyStage());
                    break;
                case Parsing.Stages.MapStage.Name:
                    stages.Add(MapStage());
                    break;
                case Parsing.Stages.SingleStage.Name:
                    stages.Add(SingleStage());
                    break;
                case Parsing.Stages.FirstStage.Name:
                    stages.Add(FirstStage());
                    break;
                case Parsing.Stages.FirstOrDefaultStage.Name:
                    stages.Add(FirstOrDefaultStage());
                    break;
                default:
                    throw Error(stageName, $"Unknown stage name: '{stageName.Lexeme}'");
            };
            
            Consume(TokenType.RightBracket, "')' excepted after stage definition.");
            if (!Match(TokenType.NextPipeline))
            {
                break;
            }
        }
        
        return stages;
    }
    
    private SelectStage SelectStage()
    {
        var selector = ConsumePdfSelector();
        
        return new SelectStage(selector);
    }
    
    private FilterStage FilterStage()
    {
        var expr = Equality();

        return new FilterStage(expr);
    }
    
    private SelectManyStage SelectManyStage()
    {
        var selector = ConsumePdfSelector();
        
        return new SelectManyStage(selector);
    }
    
    private MapStage MapStage()
    {
        var projection = Expression();
        
        return new MapStage(projection);
    }
    
    private SingleStage SingleStage()
    {
        Expr? filter = null;
        if (!Check(TokenType.RightBracket))
            filter = Equality();

        return new SingleStage(filter);
    }
    
    private FirstStage FirstStage()
    {
        Expr? filter = null;
        if (!Check(TokenType.RightBracket))
            filter = Equality();

        return new FirstStage(filter);
    }
    
    private FirstOrDefaultStage FirstOrDefaultStage()
    {
        Expr? filter = null;
        if (!Check(TokenType.RightBracket))
            filter = Equality();

        return new FirstOrDefaultStage(filter);
    }

    private PdfElement ConsumePdfSelector()
    {
        var errorText = $"Excepted {string.Join("|", TableSelectors.Select(s => $"'{s}'"))}.";
        var token = Consume(TokenType.Identifier, errorText);
        if (!TableSelectors.Contains(token.Lexeme))
        {
            throw Error(token, errorText);
        }
        
        return token.Lexeme switch
        {
            Definitions.TableSelector => PdfElement.Table,
            Definitions.TableRowsSelector => PdfElement.TableRow,
            Definitions.TableCellsSelector => PdfElement.TableCell,
            _ => throw new ParseException()
        };
    }

    private Expr Expression()
    {
        return Equality();
    }
    
    private Expr Equality()
    {
        if (Match(TokenType.LeftBracket))
            return Lambda();
        
        var expr = Comparison();
        
        while (Match(TokenType.Equal, TokenType.NotEqual))
        {
            var @operator = Previous();
            var right = Term();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }
    
    private Expr Lambda()
    {
        var parameters = new List<Token>();
        if (!Check(TokenType.RightBracket))
        {
            do
            {
                var parameter = Consume(TokenType.Identifier, "identifier expected.");
                parameters.Add(parameter);
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.RightBracket, "Except ) after arguments list");
        Consume(TokenType.Lambda, "Except => after arguments declaration");

        var body = Expression();
        return new LambdaExpr(parameters, body);
    }
    
    private Expr Comparison()
    {
        var expr = Term();

        while (Match(
            TokenType.GreaterThan,
            TokenType.GreaterOrEqualThan,
            TokenType.LessThan,
            TokenType.LessOrEqualThan))
        {
            var @operator = Previous();
            var right = Term();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var @operator = Previous();
            var right = Factor();
            expr = new BinaryExpr(expr, @operator, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(TokenType.Multiply, TokenType.Divide))
        {
            var @operator = Previous();
            var right = Unary();
            expr = new BinaryExpr(expr, @operator, right);
        }
        
        return expr;
    }

    private Expr Unary()
    {
        if (!Match(TokenType.Not, TokenType.Minus))
            return Call();
        
        var @operator = Previous();
        var right = Unary();

        return new UnaryExpr(@operator, right);
    }

    private Expr Call()
    {
        var expr = Primary();
        
        while (true)
        {
            if (Match(TokenType.Dot))
            {
                var propertyName = Consume(TokenType.Identifier, "Except property or method name after '.'");
                Consume(TokenType.LeftBracket, "'(' excepted");
                expr = FinishCall(expr, propertyName);
            }
            else
            {
                break;
            }
        }
        
        return expr;
    }

    private Expr FinishCall(Expr callee, Token methodToken)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenType.RightBracket))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightBracket, "Except ) after arguments list");
        return new InstanceMethodCallExpr(callee, arguments, methodToken);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new LiteralExpr(false);
        if (Match(TokenType.True)) return new LiteralExpr(true);
        if (Match(TokenType.Null)) return new LiteralExpr(null);
        if (Match(TokenType.Identifier)) return new VariableExpr(Previous().Lexeme!);
        
        if (Match(TokenType.Integer, TokenType.String))
            return new LiteralExpr(Previous().Literal);

        if (!Match(TokenType.LeftBracket))
            throw Error(Peek(), "Unexpected expression");
        
        var exp = Expression();
        Consume(TokenType.RightBracket, "Expect right bracket after expression.");
        return new GroupingExpr(exp);
    }

    private ParseException Error(Token token, string message)
    {
        _errors.Add(new ParseError { Error = message, Token = token, Position = _current });

        return new ParseException();
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        if (!tokenTypes.Any(Check)) 
            return false;
        
        Advance();
        return true;
    }

    private Token Consume(TokenType tokenType, string message)
    {
        if (Check(tokenType))
            return Advance();
        
        throw Error(Peek(), message);
    }

    private bool Check(TokenType tokenType) => !IsParseCompleted && Peek().TokenType == tokenType;

    private bool IsParseCompleted => Peek().TokenType == TokenType.Eof;
    
    private Token Peek() => _tokens[_current];

    private Token Advance()
    {
        if (!IsParseCompleted)
            _current++;

        return Previous();
    }
    
    private Token Previous() => _tokens[_current - 1];
}