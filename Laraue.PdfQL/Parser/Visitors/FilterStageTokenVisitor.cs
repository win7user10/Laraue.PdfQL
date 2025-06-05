using System.Diagnostics;
using System.Text.RegularExpressions;
using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors;

public class FilterStageTokenVisitor : StageTokenVisitor<FilterStageToken>
{
    private readonly Interpreter _interpreter = new();
    
    public override Stage Visit(FilterStageToken token, ParseContext context)
    {
        var expression = _interpreter.ParseBinary(token.Expression);
        
        var stage = new FilterStage
        {
            BinaryExpression = null!,
            ObjectType = context.CurrentPdfQueryType
        };
        
        throw new NotImplementedException();
    }
}