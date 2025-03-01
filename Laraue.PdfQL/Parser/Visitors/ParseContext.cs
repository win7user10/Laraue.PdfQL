using Laraue.PdfQL.PdfObjects;

namespace Laraue.PdfQL.Parser.Visitors;

public class ParseContext
{
    public Type CurrentPdfQueryType { get; set; } = typeof(PdfDocument);
}