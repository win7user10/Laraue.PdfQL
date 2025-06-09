# Laraue PDF query language

## Introduction
The library should allow to make queries to PDF. E.g. select table rows from tables where first cell text is "Number".
The library should execute the passed sequence of operations, like stages in MongoDB.

## Current status
A little test example from the documentation below runs successfully.

## Why?
Sometimes extracting data from PDF require write a lot of boilerplate C# code.

## Development plan
1. Define operations for PdfQL syntax tree, write code that use the tree to make queries. 
Write tests allows to make queries using code-defined tree
2. Write PdfQL translator that will translate code to PdfQL syntax tree

### Prototype of syntax tree

Open a PDF
```csharp
var pdfBytes = File.ReadAllBytes("my.pdf");
var pdfContainer = new PdfDocument(pdfBytes);
```

Define the operations sequence
```
var psql = @"
    select(tables)
        ->filter((item) => item.CellAt(4).Text() = 'Name')
        ->selectMany(tableRows)
        ->map((item) => item.CellAt(1))";
```

Run the tree execution
```csharp
var pdfBytes = File.ReadAllBytes("document.pdf");
var pdfContainer = new PdfDocument(pdfBytes);
        
var executor = new PSqlExecutor();
var result = executor.ExecutePsql(psql, pdfContainer);
```

[Full stages documentation](Documentation/readme.md)