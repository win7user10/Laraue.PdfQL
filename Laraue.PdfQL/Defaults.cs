using Tabula.Extractors;

namespace Laraue.PQL;

public class Defaults
{
    public static readonly IExtractionAlgorithm ExtractionAlgorithm = new SpreadsheetExtractionAlgorithm();
}