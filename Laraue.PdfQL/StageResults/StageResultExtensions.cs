using Laraue.PQL.PdfObjects;
using Tabula;

namespace Laraue.PQL.StageResults;

public static class StageResultExtensions
{
    public static StageResult GetPagesContainerOrThrow(this StageResult stageResult)
    {
        var container = stageResult.GetContainerOrThrow();

        var pdfObject = container.PdfObject.CastPdfObjectOrThrow<PdfDocument>();
        
        var pages = pdfObject.SourceDocument.GetPages()
            .Select(p => new PdfPage(p))
            .ToArray();

        return new PdfObjectStageResult(new PdfObjectContainer<PdfPage>(pages));
    }
    
    public static StageResult GetTablesContainerOrThrow(this StageResult stageResult)
    {
        var container = stageResult.GetContainerOrThrow();

        var pdfObject = container.PdfObject.CastPdfObjectOrThrow<PdfDocument>();
        
        var tables = pdfObject.SourceDocument.GetPages()
            .SelectMany(p =>
            {
                var oe = ObjectExtractor.ExtractPage(p);
                return Defaults.ExtractionAlgorithm.Extract(oe);
            })
            .Select(t => new PdfTable(t))
            .ToArray();

        return new PdfObjectStageResult(new PdfObjectContainer<PdfTable>(tables));
    }
    
    public static StageResult GetTablesRowsContainerOrThrow(this PdfObjectContainer stageResult)
    {
        var container = stageResult.CastPdfObjectContainerOrThrow<PdfTable>();

        var result = new List<PdfTableRow>();
        foreach (var val in container)
        {
            result.AddRange(val.GetTableRows());
        }

        return new PdfObjectStageResult(new PdfObjectContainer<PdfTableRow>(result.ToArray()));
    }
    
    public static PdfObjectStageResult GetContainerOrThrow(this StageResult stageResult)
    {
        if (stageResult is not PdfObjectStageResult result)
        {
            throw new InvalidOperationException("Can't get container from the current stage result");
        }

        return result;
    }
    
    public static TObject CastPdfObjectOrThrow<TObject>(this PdfObject source) where TObject : PdfObject
    {
        if (source is not TObject result)
        {
            throw new InvalidOperationException(
                $"Current object {source.GetType()} is not permitted to make the cast to {typeof(TObject)}");
        }

        return result;
    }
    
    public static PdfObjectContainer<TObject> CastPdfObjectContainerOrThrow<TObject>(this PdfObjectContainer container)
        where TObject : PdfObject
    {
        var result = new List<TObject>();
        
        foreach (var pdfObject in container)
        {
            if (pdfObject is not TObject typedObject)
            {
                throw new InvalidOperationException(
                    $"Current object {pdfObject.GetType()} is not permitted to make the cast to {typeof(TObject)}");
            }
            
            result.Add(typedObject);
        }

        return new PdfObjectContainer<TObject>(result.ToArray());
    }
}