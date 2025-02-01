using Laraue.PQL.PdfObjects.Interfaces;
using Tabula;
using UglyToad.PdfPig;

namespace Laraue.PQL.PdfObjects;

public class PdfDocument : PdfObject, IHasTablesContainer
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

    public PdfObjectContainer<PdfTable> GetTablesContainer()
    {
        var tables = SourceDocument.GetPages()
            .SelectMany(p =>
            {
                var oe = ObjectExtractor.ExtractPage(p);
                return Defaults.ExtractionAlgorithm.Extract(oe);
            })
            .Select(t => new PdfTable(t))
            .ToArray();

        return new PdfObjectContainer<PdfTable>(tables);
    }
}