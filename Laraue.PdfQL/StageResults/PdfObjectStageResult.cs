using Laraue.PQL.PdfObjects;

namespace Laraue.PQL.StageResults;

public class PdfObjectStageResult : StageResult
{
    internal readonly PdfObject PdfObject;

    public PdfObjectStageResult(PdfObject pdfObject)
    {
        PdfObject = pdfObject;
    }

    public override object ToJsonObject()
    {
        return PdfObject.ToJson();
    }

    public override string ToString()
    {
        return PdfObject.ToString()!;
    }
}