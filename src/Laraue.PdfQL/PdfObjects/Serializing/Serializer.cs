﻿namespace Laraue.PdfQL.PdfObjects.Serializing;

public class Serializer : ISerializer
{
    /// <inheritdoc />
    public object ToJsonObject(object obj)
    {
        if (obj is PdfObject pdfObject)
        {
            return ToJsonObject(pdfObject);
        }

        if (obj is StageResult stageResult)
        {
            var result = new List<object>();
            foreach (var stageResultObject in stageResult)
            {
                result.Add(ToJsonObject(stageResultObject));
            }

            return result;
        }
        
        return obj;
    }

    private static object ToJsonObject(PdfObject obj)
    {
        return obj switch
        {
            PdfDocument pdfDocument => ToJsonObject(pdfDocument),
            PdfTable pdfTable => ToJsonObject(pdfTable),
            PdfTableRow pdfTableRow => ToJsonObject(pdfTableRow),
            PdfTableCell pdfTableCell => ToJsonObject(pdfTableCell),
            _ => throw new NotImplementedException()
        };
    }
    
    private static PdfDocumentJsonObject ToJsonObject(PdfDocument obj)
    {
        return new PdfDocumentJsonObject(((IEnumerable<PdfTable>)obj.GetTablesContainer()).Select(ToJsonObject).ToArray());
    }
    
    private static string[][] ToJsonObject(PdfTable obj)
    {
        return ((IEnumerable<PdfTableRow>)obj.GetTableRowsContainer()).Select(ToJsonObject).ToArray();
    }

    private static string[] ToJsonObject(PdfTableRow obj)
    {
        return ((IEnumerable<PdfTableCell>)obj.GetTableCellsContainer()).Select(ToJsonObject).ToArray();
    }
    
    private static string ToJsonObject(PdfTableCell obj)
    {
        return obj.Text();
    }
    
}