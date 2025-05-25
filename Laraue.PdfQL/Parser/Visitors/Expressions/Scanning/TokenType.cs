namespace Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

public enum TokenType
{
    Identifier,
    String,
    Number,
    
    Comma,
    Dot,
    
    LessThan,
    GreaterThan,
    LessOrEqualThan,
    GreaterOrEqualThan,
    Equal,
    NotEqual,
    Not,
    
    Minus,
    Plus,
    Divide,
    Multiply,
    
    LeftBracket,
    RightBracket,
    
    Lambda,
    False,
    True,
    Null,
    
    Eof,
}