namespace Laraue.PdfQL.Interpreter.Scanning;

public enum TokenType
{
    Identifier,
    String,
    Integer,
    
    Comma,
    Dot,
    
    LessThan,
    GreaterThan,
    LessOrEqualThan,
    GreaterOrEqualThan,
    Assign,
    Equal,
    NotEqual,
    Not,
    
    Minus,
    Plus,
    Divide,
    Multiply,
    
    LeftParentheses,
    RightParentheses,
    LeftBracket,
    RightBracket,
    
    Lambda,
    False,
    True,
    Null,
    NextPipeline,
    
    New,
    
    Eof,
}