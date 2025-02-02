namespace Laraue.PdfQL.PdfObjects.Interfaces;

public interface IHasTablesContainer
{
    public PdfObjectContainer<PdfTable> GetTablesContainer();
}