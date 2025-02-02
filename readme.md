# Laraue PDF query language

## Introduction
The library should allow to make queries to PDF. E.g. select table rows from tables where first cell text is "Number".
The library should execute the passed sequence of operations, like stages in MongoDB.

## Why?
1. Sometimes extracting data from PDF require write a lot of boilerplate C# code 
2. To get practice with AST.

## Development plan
1. Define operations for PdfQL syntax tree, write code that use the tree to make queries. 
Write tests allows to make queries using code-defined tree
2. Write PdfQL translator that will translate code to PdfQL syntax tree

### Prototype of syntax tree

Open PDF
```csharp
var pdfBytes = File.ReadAllBytes("my.pdf");
var pdfContainer = new PdfDocument(pdfBytes);
```

Define syntax tree
```csharp
Stage[] stages =
    [
        new SelectStage
        {
            SelectExpression = new PsqlApplySelectorExpression
            {
                Selector = Selector.Tables,
                ObjectType = typeof(PdfDocument)
            },
            ObjectType = typeof(IHasTablesContainer)
        }
    ];
```

Run the tree execution
```csharp
var executor = PdfQLInstance.GetTreeExecutor(new ExecutorOptions { HandleErrors = false });
var sourceResult = new PdfObjectStageResult(pdfContainer);

var stageResult = executor.Execute(sourceResult, new StagesList { Stages = stages });
var result = stageResult.ToJsonObject();

var serialized = JsonSerializer.Serialize(result);
```