# Filter stage

Filter is the operation that returns objects from a sequence that matches the passed condition.

#### FilterStage syntax
```antlr
FilterStage
  : 'filter' '(' LambdaExpression ')'  
  ;
  
LambdaExpression
  : (Args '=>' BinaryExpression)
  ;
```

Map examples
1. For each table cell returns only those where text is equal to 'Title'.
    ```csharp
    select(tableCells) // PdfTableCell[]
        ->filter((item) => item.Text() == 'Title') // PdfTableCell[]
    ```