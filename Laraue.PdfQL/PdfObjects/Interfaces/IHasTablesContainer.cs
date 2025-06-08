namespace Laraue.PdfQL.PdfObjects.Interfaces;

public interface IHasTablesContainer
{
    public StageResult<PdfTable> GetTablesContainer();
}