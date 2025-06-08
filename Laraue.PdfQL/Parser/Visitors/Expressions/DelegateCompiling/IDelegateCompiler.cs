using Laraue.PdfQL.Parser.Visitors.Expressions.Parsing.Tree;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;

public interface IDelegateCompiler
{
    DelegateCompilingResult Compile(Stage[] stages);
}