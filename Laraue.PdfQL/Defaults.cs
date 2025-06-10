using Tabula.Extractors;

namespace Laraue.PdfQL;

public class Defaults
{
    public static readonly IExtractionAlgorithm ExtractionAlgorithm = new BasicExtractionAlgorithm();
}