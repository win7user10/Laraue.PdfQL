namespace Laraue.PQL.PdfObjects.Interfaces;

public interface IHasTableRowsContainer
{
    public PdfObjectContainer<PdfTableRow> GetTableRowsContainer();
}