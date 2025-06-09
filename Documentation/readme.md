# Introduction
PdfQL is the language to describe how to get data from PDF document.

## PdfQL pipeline

Each instruction in the PdfQL is the stage that transform data from one format to another.
Typical PdfQL declaration looks like

```csharp
select(tables) // PdfTable[] - Get all tables from a document
    ->filter((item) => item.CellAt(4).Text() = 'Name') // PdfTable[] - Returns only tables where cell #4 contains text 'Name'
    ->selectMany(tableRows) // PdfTableRow[] - Get all table rows from tables, and transaform two-dimension array to one dimension
    ->map((item) => item.CellAt(1).Text()) // string - From table rows get cell #1 text.
```

## PdfQL stage specification

[SELECT](Select)