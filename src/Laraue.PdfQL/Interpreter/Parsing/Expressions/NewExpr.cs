using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

public record NewExpr : Expr
{
    public NewExpr(ICollection<MemberInitMember> members)
    {
        Members = members;
    }

    public ICollection<MemberInitMember> Members { get; }
}

public record MemberInitMember(Token Token, Expr InitExpression);