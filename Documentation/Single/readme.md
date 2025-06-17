# Single stage

Single is the operation that returns single object from the sequence or throws an exception if the sequence contains more 
or less elements.

#### SingleStage syntax
```antlr
First
  : 'single' '(' LambdaExpression? ')'  
  ;
  
LambdaExpression
  : (Args '=>' BinaryExpression)
  ;
```

Single examples
1. Find a table cell with the text 'Alex'. Throws when not found or more than one record returns.
    ```csharp
    select(tableCells) // PdfTableCell[]
        ->single((item) => item.Text() == 'Alex') // PdfTableCell
    ```