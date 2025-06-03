using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;
using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing;

public class Parser : IParser
{
    public ParseResult ParseStatement(Token[] tokens)
    {
        return new ParserImpl(tokens).ParseStatement();
    }

    public ParseResult ParseEquality(Token[] tokens)
    {
        return new ParserImpl(tokens).ParseEquality();
    }
}

internal class ParserImpl
{
    private readonly Token[] _tokens;
    private readonly List<ParseError> _errors = [];
    private int _current;

    public ParserImpl(Token[] tokens)
    {
        _tokens = tokens;
    }

    public ParseResult ParseStatement()
    {
        return Parse(Expression);
    }
    
    public ParseResult ParseEquality()
    {
        return Parse(Equality);
    }
    
    public ParseResult Parse(Func<Expr> getExpression)
    {
        try
        {
            var expression = getExpression();
            return new ParseResult
            {
                Expression = expression,
                Errors = _errors.ToArray()
            };
        }
        catch (ParseException)
        {
            return new ParseResult
            {
                Errors = _errors.ToArray()
            };
        }
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
            if (Match(TokenType.LeftBracket))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.Dot))
            {
                // Here is wrong translation, may be method call
                var name = Consume(TokenType.Identifier, "Except property name after '.'");
                expr = new MemberAccessExpr(expr, name);
            }
            else
            {
                break;
            }
        }
        
        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenType.RightBracket))
        {
            do
            {
                arguments.Add(Expression());
            } while (Match(TokenType.Comma));
        }

        var paren = Consume(TokenType.RightBracket, "Except ) after arguments list");
        return new MethodCallExpr(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new LiteralExpr(false);
        if (Match(TokenType.True)) return new LiteralExpr(true);
        if (Match(TokenType.Null)) return new LiteralExpr(null);
        if (Match(TokenType.Identifier)) return new VariableExpr(Previous().Lexeme!);
        
        if (Match(TokenType.Number, TokenType.String))
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