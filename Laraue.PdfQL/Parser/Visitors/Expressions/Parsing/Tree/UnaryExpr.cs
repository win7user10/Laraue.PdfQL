﻿using Laraue.PdfQL.Parser.Visitors.Expressions.Scanning;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

public record UnaryExpr : Expr
{
    public UnaryExpr(Token @operator, Expr expr)
    {
        Operator = @operator;
        Expr = expr;
    }

    public Token Operator { get; init; }
    public Expr Expr { get; init; }
}