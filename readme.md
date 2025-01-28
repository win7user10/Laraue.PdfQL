# Laraue PDF query language

## Introduction
The library should allow to make queries to PDF. E.g. select table rows from tables where first cell text is "Number".

## Why?
1. Sometimes extracting data from PDF require write a lot of boilerplate C# code 
2. To get practice with AST.

## Development plan
1. Define operations for PdfQL syntax tree, write code that use the tree to make queries. 
Write tests allows to make queries using code-defined tree
2. Write PdfQL translator that will translate code to PdfQL syntax tree