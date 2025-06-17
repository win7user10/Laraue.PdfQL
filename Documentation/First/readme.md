# First stage

First is the operation that returns first object from the sequence or throws an exception if the sequence
does not contain elements.

#### FirstStage syntax
```antlr
First
  : 'first' '(' LambdaExpression? ')'  
  ;
  
LambdaExpression
  : (Args '=>' BinaryExpression)
  ;
```

First examples
1. Find a table cell with the text 'Alex'. Throws when not found.
    ```csharp
    select(tableCells) // PdfTableCell[]
        ->first((item) => item.Text() == 'Alex') // PdfTableCell
    ```