namespace Laraue.PdfQL.PdfObjects.Interfaces;

public interface IHasTableCellsContainer
{
    public StageResult<PdfTableCell> GetTableCellsContainer();
}