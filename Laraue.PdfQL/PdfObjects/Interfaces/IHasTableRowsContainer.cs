namespace Laraue.PdfQL.PdfObjects.Interfaces;

public interface IHasTableRowsContainer
{
    public PdfObjectContainer<PdfTableRow> GetTableRowsContainer();
}