using Laraue.PdfQL.Expressions;
using Laraue.PdfQL.PdfObjects;
using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors;

public class SelectStageTokenVisitor : StageTokenVisitor<SelectStageToken>
{
    public override Stage Visit(SelectStageToken token, ParseContext context)
    {
        var selectExpressionPdfType = token.Selector switch
        {
            Selector.Pages => typeof(PdfPage),
            Selector.Tables => typeof(PdfTable),
            Selector.TableCells => typeof(PdfTableCell),
            Selector.TableRows => typeof(PdfTableRow),
            _ => throw new InvalidOperationException($"Unknown selector {token.Selector}")
        };

        var stage = new SelectStage
        {
            SelectExpression = new PsqlApplySelectorExpression
            {
                Selector = token.Selector,
                ObjectType = selectExpressionPdfType
            },
            ObjectType = context.CurrentPdfQueryType
        };
        
        context.CurrentPdfQueryType = selectExpressionPdfType;

        return stage;
    }
}