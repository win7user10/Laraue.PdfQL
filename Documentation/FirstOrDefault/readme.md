# FirstOrDefault stage

FirstOrDefault is the operation that returns first object from the sequence or null if the sequence is empty.

#### FirstOrDefaultStage syntax
```antlr
FirstOrDefaultStage
  : 'firstOrDefault' '(' LambdaExpression? ')'  
  ;
  
LambdaExpression
  : (Args '=>' BinaryExpression)
  ;
```

FirstOrDefault examples
1. Find a table cell with the text 'Alex'. Returns ```null``` if not found.
```csharp
select(tableCells) // PdfTableCell[]
    ->firstOrDefault((item) => item.Text() = 'Alex') // PdfTableCell?
```