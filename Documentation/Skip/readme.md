# Skip stage

Skip is the operation that skips passed number of elements in a sequence.

#### Take syntax
```antlr
Skip
  : 'skip' '(' ConstantExpression ')'  
  ;
```

Skip examples
1. Skip first cell of the sequence.
    ```csharp
    select(tableCells) // PdfTableCell[]
        ->skip(1) // PdfTableCell[]
    ```
2. Take only the second and the third rows.
    ```csharp
    select(tableCells) // PdfTableCell[]
        ->skip(1) // PdfTableCell[]
        ->take(2) // PdfTableCell[]
    ```