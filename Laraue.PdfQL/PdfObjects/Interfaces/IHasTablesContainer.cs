namespace Laraue.PQL.PdfObjects.Interfaces;

public interface IHasTablesContainer
{
    public PdfObjectContainer<PdfTable> GetTablesContainer();
}