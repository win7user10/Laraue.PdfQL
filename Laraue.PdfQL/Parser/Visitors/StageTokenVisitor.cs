using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors;

public abstract class StageTokenVisitor<TToken> where TToken : StageToken
{
    public abstract Stage Visit(TToken token, ParseContext context);
}