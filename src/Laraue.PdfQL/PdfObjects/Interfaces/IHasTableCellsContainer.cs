namespace Laraue.PdfQL.PdfObjects.Interfaces;

public interface IHasTableCellsContainer
{
    StageResult<PdfTableCell> GetTableCellsContainer();
}