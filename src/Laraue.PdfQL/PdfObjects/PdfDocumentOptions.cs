using Tabula.Extractors;

namespace Laraue.PdfQL.PdfObjects;

public class PdfDocumentOptions
{
    public IExtractionAlgorithm ExtractionAlgorithm { get; set; } = new BasicExtractionAlgorithm();
}