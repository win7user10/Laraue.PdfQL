# Laraue PDF query language

[![latest version](https://img.shields.io/nuget/v/Laraue.PdfQL)](https://www.nuget.org/packages/Laraue.PdfQL)
[![latest version](https://img.shields.io/nuget/dt/Laraue.PdfQL)](https://www.nuget.org/packages/Laraue.PdfQL)

## Introduction
The library should allow to make queries to PDF. E.g. select table rows from tables where first cell text is "Number".
The library should execute the passed sequence of operations, like stages in MongoDB.

## Why?
Sometimes extracting data from PDF require write a lot of boilerplate C# code.

### How to use

Open a PDF
```csharp
var pdfBytes = File.ReadAllBytes("my.pdf");
var pdfContainer = new PdfDocument(pdfBytes);
```

Define the operations sequence
```
var pdfql = @"
    select(tables)
        ->filter((item) => item.GetCell(4).Text() == 'Name')
        ->selectMany(tableRows)
        ->map((item) => item.GetCell(1))";
```

Run the tree execution
```csharp
var executor = new PdfqlExecutor();
var result = executor.ExecutePsql(pdfql, pdfContainer);
```

[Full available stages documentation](Documentation)