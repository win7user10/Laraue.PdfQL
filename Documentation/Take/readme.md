# Take stage

Take is the operation that limits a sequence to the passed number of elements. 

#### Take syntax
```antlr
Take
  : 'take' '(' ConstantExpression ')'  
  ;
```

Take examples
1. Returns only 3 cells from the sequence.
    ```csharp
    select(tableCells) // PdfTableCell[]
        ->take(3) // PdfTableCell[]
    ```