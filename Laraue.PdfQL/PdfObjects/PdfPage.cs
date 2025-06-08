using UglyToad.PdfPig.Content;

namespace Laraue.PdfQL.PdfObjects;

public class PdfPage : PdfObject
{
    private readonly Page _page;

    public PdfPage(Page page)
    {
        _page = page;
    }

    public override string ToString()
    {
        return $"PdfPage #{_page.Number}";;
    }
}