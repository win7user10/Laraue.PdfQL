﻿using Laraue.PdfQL.Interpreter.Scanning;

namespace Laraue.PdfQL.Interpreter.Parsing.Expressions;

public record BinaryExpr : Expr
{
    public BinaryExpr(Expr left, Token @operator, Expr right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public Expr Left { get; init; }
    public Token Operator { get; init; }
    public Expr Right { get; init; }

    public override string ToString()
    {
        return $"{Left} {Operator.Lexeme} {Right}";
    }
}