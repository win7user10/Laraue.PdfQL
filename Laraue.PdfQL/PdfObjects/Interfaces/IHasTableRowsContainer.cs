namespace Laraue.PdfQL.PdfObjects.Interfaces;

public interface IHasTableRowsContainer
{
    public StageResult<PdfTableRow> GetTableRowsContainer();
}