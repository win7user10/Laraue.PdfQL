# Select stage

Select is the operation that can get from the single object the sequence of requested objects.

#### SelectStage syntax
```antlr
SelectStage
  : 'select' '(' Selector ')'  
  ;
  
Selector
  : 'tables'
  | 'tableRows'
  | 'tableCells'
  ;
```

Selectors definition
1. Selector 'tables' returns all tables from the current object.
PdfDocument -> PdfTable[]
2. Selector 'tableRows' returns all table rows from the current object.
PdfDocument | PdfTable -> PdfTableRow[]
3. Selector 'tableCells' returns all table cells from the current object.
PdfDocument | PdfTable | PdfTableRow -> PdfTableCell[]