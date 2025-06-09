# Select syntax

```antlr
SelectStage
  : 'select' '(' SelectStageArgs ')'  
  ;
  
SelectStageArgs
  : 'tables'
  | 'tableRows'
  | 'tableCells'
  ;
```