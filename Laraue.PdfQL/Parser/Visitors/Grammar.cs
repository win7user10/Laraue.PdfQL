namespace Laraue.PdfQL.Parser.Visitors;

public static class Grammar
{
    public static readonly OrderedDictionary<string, string> Definitions = new()
    {
        // How to represent regex here?
        ["<Arguments>"] = "<LeftBracket>[<Argument>]?[<Comma><Argument>]?+<RightBracket>",
        ["<Argument>"] = "<String>|<Number>",
        ["<MethodCall>"] = "<Identifier><Arguments>",
        ["<MemberAccess>"] = "<Identifier><Dot>|<MethodCallExpression><Dot>",
        
        //["<Expression>"] = "<MethodCallExpression>|<BinaryExpression>",
        ["<MethodCallExpression>"] = "<MemberAccess><MethodCall>",
        //["<BinaryExpression>"] = "<Expression><Operand><Expression>",
    };

    public static HashSet<string> ReservedWords = new()
    {
        Tokens.String,
        Tokens.Number,
        Tokens.Identifier,
    };

    public static class Tokens
    {
        public const string String = "<String>";
        public const string Number = "<Number>";
        public const string Identifier = "<Identifier>";
    }
}