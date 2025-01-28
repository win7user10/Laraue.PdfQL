using UglyToad.PdfPig;

namespace Laraue.PQL.PdfObjects;

public class PdfDocument : PdfObject
{
    internal readonly UglyToad.PdfPig.PdfDocument SourceDocument;

    public PdfDocument(byte[] pdfBytes)
    {
        SourceDocument = UglyToad.PdfPig.PdfDocument.Open(pdfBytes, new ParsingOptions { ClipPaths = true });
    }
    
    public override object ToJson()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return $"PdfDocument {SourceDocument.Information}";
    }
}