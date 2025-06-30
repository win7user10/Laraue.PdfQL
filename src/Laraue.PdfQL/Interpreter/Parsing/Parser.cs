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
            Consume(TokenType.LeftParentheses, "'(' excepted after stage definition.");

            Stage stage = stageName.Lexeme switch
            {
                Parsing.Stages.SelectStage.Name => SelectStage(),
                Parsing.Stages.FilterStage.Name => FilterStage(),
                Parsing.Stages.SelectManyStage.Name => SelectManyStage(),
                Parsing.Stages.MapStage.Name => MapStage(),
                Parsing.Stages.SingleStage.Name => SingleStage(),
                Parsing.Stages.FirstStage.Name => FirstStage(),
                Parsing.Stages.FirstOrDefaultStage.Name => FirstOrDefaultStage(),
                Parsing.Stages.SkipStage.Name => SkipStage(),
                Parsing.Stages.TakeStage.Name => TakeStage(),
                _ => throw Error(stageName, $"Unknown stage name: '{stageName.Lexeme}'"),
            };
            
            stage.StartPosition = stageName.StartPosition;
            stage.StartLineNumber = stageName.LineNumber;
            stage.EndPosition = _current;
            stage.EndLineNumber = Previous().LineNumber;
            
            stages.Add(stage);
            
            Consume(TokenType.RightParentheses, "')' excepted after stage definition.");
            if (!Match(TokenType.NextPipeline))
            {
                break;
            }
        }

        if (!IsParseCompleted)
        {
            throw Error(Peek(), "Excepted next stage definition or end of pipeline.");
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
        if (!Check(TokenType.RightParentheses))
            filter = Equality();

        return new SingleStage(filter);
    }
    
    private FirstStage FirstStage()
    {
        Expr? filter = null;
        if (!Check(TokenType.RightParentheses))
            filter = Equality();

        return new FirstStage(filter);
    }
    
    private FirstOrDefaultStage FirstOrDefaultStage()
    {
        Expr? filter = null;
        if (!Check(TokenType.RightParentheses))
            filter = Equality();

        return new FirstOrDefaultStage(filter);
    }
    
    private SkipStage SkipStage()
    {
        var value = Consume(TokenType.Integer, "'Skip' except integer argument.");

        return new SkipStage((int)value.Literal);
    }
    
    private TakeStage TakeStage()
    {
        var value = Consume(TokenType.Integer, "'Take' except integer argument.");

        return new TakeStage((int)value.Literal);
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
        if (Check(TokenType.LeftParentheses) || (Check(TokenType.Identifier) && CheckNext(TokenType.Lambda)))
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
        if (Match(TokenType.LeftParentheses))
        {
            if (!Check(TokenType.RightParentheses))
            {
                do
                {
                    var parameter = Consume(TokenType.Identifier, "identifier expected.");
                    parameters.Add(parameter);
                } while (Match(TokenType.Comma));
            }
            
            Consume(TokenType.RightParentheses, "Except ) after arguments list");
        }
        else
        {
            var parameter = Consume(TokenType.Identifier, "identifier expected.");
            parameters.Add(parameter);
        }
        
        
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
                Consume(TokenType.LeftParentheses, "'(' excepted");
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
        if (!Check(TokenType.RightParentheses))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightParentheses, "Except ) after arguments list");
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

        if (Match(TokenType.New))
            return New();

        if (!Match(TokenType.LeftParentheses))
            throw Error(Peek(), "Unknown token type.");
        
        var exp = Expression();
        Consume(TokenType.RightParentheses, "Expect right bracket after expression.");
        return new GroupingExpr(exp);
    }

    private Expr New()
    {
        Consume(TokenType.LeftBracket, "Except '{' after 'new' declaration");

        var members = new Dictionary<string, MemberInitMember>();
        
        while (true)
        {
            if (Match(TokenType.Identifier))
            {
                var memberNameToken = Previous();
                var memberName = memberNameToken.Lexeme!;
                if (members.ContainsKey(memberName))
                {
                    throw Error(memberNameToken, $"Duplicate member name '{memberName}'");
                }
                
                Consume(TokenType.Assign, "'=' excepted");
                var expr = Expression();
                members.Add(memberName, new MemberInitMember(memberNameToken, expr));
                
                if (!Match(TokenType.Comma))
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        Consume(TokenType.RightBracket, "Except '}' after 'new' members declaration'");

        return new NewExpr(members.Values);
    }

    private ParseException Error(Token token, string message)
    {
        _errors.Add(new ParseError
        {
            Error = message,
            Token = token,
        });

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
    private bool CheckNext(TokenType tokenType) => _tokens.Length > _current + 1 && _tokens[_current + 1].TokenType == tokenType;

    private bool IsParseCompleted => Peek().TokenType == TokenType.Eof;
    
    private Token Peek() => _tokens[_current];

    private Token Advance()
    {
        if (!IsParseCompleted)
            _current++;

        return Previous();
    }
    
    private Token Previous() => _tokens[_current - 1];
    
    private bool HasPrevious() => _current > 0;
}