# SelectMany stage

SelectMany is the operation that can get from the objects collection the sequence of requested objects.

#### SelectManyStage syntax
```antlr
SelectManyStage
  : 'selectMany' '(' Selector ')'  
  ;
  
Selector
  : 'tableRows'
  | 'tableCells'
  ;
```

Selectors definition
1. Selector ```tableRows``` returns all table rows from the objects collection.
```PdfTable[]``` -> ```PdfTableRow[]```
2. Selector ```tableCells``` returns all table cells from the objects collection.
```PdfTable[]``` | ```PdfTableRow[]``` -> ```PdfTableCell[]```