# Map stage

Map is the operation that transforms sequence of objects to the sequence of other objects using map function.

#### MapStage syntax
```antlr
MapStage
  : 'map' '(' LambdaExpression ')'  
  ;
  
LambdaExpression
  : (Args '=>' MemberExpression)
  ;
  
MemberExpression
    : InstanceMethodCallExpression
    | NewExpression
    ;
```

Map examples
1. For each table in a PDF take the text content
    ```csharp
    select(tables) // PdfTable[]
        ->map((item) => item.Text()) // string[]
    ```
2. For each table row get first cell text as 'Name' and second cell as 'Description'.
    ```csharp
    select(tableRows) // PdfTable[]
        ->map((row) => new { Title = row.GetCell(1).Text(), Description = row.GetCell(2).Text() }) // object[]
    ```