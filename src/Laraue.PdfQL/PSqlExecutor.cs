using Laraue.PdfQL.Interpreter.DelegateCompiling;
using Laraue.PdfQL.Interpreter.Parsing;
using Laraue.PdfQL.Interpreter.Scanning;
using Laraue.PdfQL.PdfObjects;

namespace Laraue.PdfQL;

public class PSqlExecutor : IPsqlExecutor
{
    private readonly Scanner _scanner;
    private readonly Parser _parser;
    private readonly DelegateCompiler _delegateCompiler;

    public PSqlExecutor()
    {
        _scanner = new Scanner();
        _parser = new Parser();
        _delegateCompiler = new DelegateCompiler();
    }


    public object ExecutePsql(string psql, PdfDocument document)
    {
        var result = TryExecutePsql(psql, document);
        if (result.HasErrors)
        {
            throw new PSqlExecutionException(result.Errors);
        }
        
        return result.Result!;
    }

    public PsqlExecutionResult TryExecutePsql(string psql, PdfDocument document)
    {
        var result = GetCSharpDelegate(psql);
        if (result.Errors.Count > 0)
        {
            return new PsqlExecutionResult
            {
                Errors = result.Errors,
            };
        }
        
        var @delegate = result.Delegate!;
        return new PsqlExecutionResult
        {
            Result = @delegate(document),
            Errors = result.Errors,
        };
    }

    public PSqlSyntaxCheckResult CheckSyntax(string psql)
    {
        var result = GetCSharpDelegate(psql);

        return new PSqlSyntaxCheckResult
        {
            Errors = result.Errors
        };
    }

    private GetCSharpDelegateResult GetCSharpDelegate(string psql)
    {
        var errors = new List<PsqlExecutionError>();
        
        var scanResult = _scanner.ScanTokens(psql);
        if (scanResult.HasErrors)
        {
            foreach (var scanError in scanResult.Errors)
            {
                errors.Add(new PsqlExecutionError { Message = $"Position {scanError.Position}: syntax error {scanError}" });
            }
            
            return new GetCSharpDelegateResult { Errors = errors };
        }
        
        var parseResult = _parser.Parse(scanResult.Tokens);
        if (parseResult.HasErrors)
        {
            foreach (var parseError in parseResult.Errors)
            {
                errors.Add(new PsqlExecutionError { Message = $"Syntax error position {parseError.Position} on token '{parseError.Token.Lexeme}'. {parseError.Error}" });
            }
            
            return new GetCSharpDelegateResult { Errors = errors };
        }
        
        var delegateCompilingResult = _delegateCompiler.Compile(parseResult.Stages.ToArray());
        if (delegateCompilingResult.HasErrors)
        {
            foreach (var compileError in delegateCompilingResult.Errors)
            {
                errors.Add(new PsqlExecutionError { Message = $"Compile error on stage '{compileError.Stage.GetType().Name}' : {compileError.Error}" });
            }
        }
        
        return new GetCSharpDelegateResult { Errors = errors, Delegate = delegateCompilingResult.Delegate };
    }
    

    private class GetCSharpDelegateResult
    {
        public Func<PdfDocument, object>? Delegate { get; init; }
        public required List<PsqlExecutionError> Errors { get; init; }
    }
}

public interface IPsqlExecutor
{
    object ExecutePsql(string psql, PdfDocument document);
    PsqlExecutionResult TryExecutePsql(string psql, PdfDocument document);
    PSqlSyntaxCheckResult CheckSyntax(string psql);
}

public class PsqlExecutionResult : PSqlSyntaxCheckResult
{
    public object? Result { get; set; }
}

public class PSqlSyntaxCheckResult
{
    public List<PsqlExecutionError> Errors { get; set; } = new ();
    public bool HasErrors => Errors.Any();
}

public class PsqlExecutionError
{
    public required string Message { get; init; }
}