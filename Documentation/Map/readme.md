# Map stage

Map is the operation that transforms sequence of objects to the sequence of other objects using map function.

#### MapStage syntax
```antlr
MapStage
  : 'map' '(' LambdaExpression ')'  
  ;
  
LambdaExpression
  : (Args '=>' BinaryExpression)
  ;
```

Map examples
1. For each table in a PDF take the text content.
```csharp
select(tables) // PdfTable[]
    ->map((item) => item.Text()) // string[]
```