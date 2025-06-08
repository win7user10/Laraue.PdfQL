using Laraue.PdfQL.Interpreter.Parsing.Stages;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public interface IDelegateCompiler
{
    DelegateCompilingResult Compile(Stage[] stages);
}