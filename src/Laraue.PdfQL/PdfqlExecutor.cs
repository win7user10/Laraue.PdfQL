using Laraue.PdfQL.Interpreter.DelegateCompiling;
using Laraue.PdfQL.Interpreter.Parsing;
using Laraue.PdfQL.Interpreter.Scanning;
using Laraue.PdfQL.PdfObjects;

namespace Laraue.PdfQL;

/// <inheritdoc />
public class PdfqlExecutor : IPdfqlExecutor
{
    private readonly Scanner _scanner;
    private readonly Parser _parser;
    private readonly DelegateCompiler _compiler;

    /// <summary>
    /// Initializes <see cref="PdfqlExecutor"/>.
    /// </summary>
    public PdfqlExecutor()
    {
        _scanner = new Scanner();
        _parser = new Parser();
        _compiler = new DelegateCompiler();
    }

    /// <inheritdoc />
    public object ExecutePdfql(string pdfql, PdfDocument document)
    {
        var result = TryExecutePdfql(pdfql, document);
        if (result.HasErrors)
        {
            throw new PdfqlCompileException(result.Errors);
        }
        
        return result.Result!;
    }

    /// <inheritdoc />
    public T ExecutePdfql<T>(string pdfql, PdfDocument document)
    {
        var result = ExecutePdfql(pdfql, document);
        return result is T typeResult 
            ? typeResult
            : throw new PdfqlRuntimeException($"Excepted result of type {typeof(T)} but taken {result.GetType()}.");
    }

    /// <inheritdoc />
    public PsqlExecutionResult TryExecutePdfql(string pdfql, PdfDocument document)
    {
        var result = GetCSharpDelegate(pdfql);
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

    /// <inheritdoc />
    public PSqlSyntaxCheckResult CheckSyntax(string pdfql)
    {
        var result = GetCSharpDelegate(pdfql);

        return new PSqlSyntaxCheckResult
        {
            Errors = result.Errors
        };
    }

    private GetCSharpDelegateResult GetCSharpDelegate(string psql)
    {
        var errors = new List<PsqlCompileError>();
        
        var scanResult = _scanner.ScanTokens(psql);
        if (scanResult.HasErrors)
        {
            foreach (var scanError in scanResult.Errors)
            {
                errors.Add(new PsqlCompileError
                {
                    Message = $"Position {scanError.StartPosition}: syntax error {scanError.Error}",
                    StartPosition = scanError.StartPosition,
                    EndPosition = scanError.EndPosition,
                    StartLineNumber = scanError.LineNumber,
                    EndLineNumber = scanError.LineNumber,
                });
            }
            
            return new GetCSharpDelegateResult { Errors = errors };
        }
        
        var parseResult = _parser.Parse(scanResult.Tokens);
        if (parseResult.HasErrors)
        {
            foreach (var parseError in parseResult.Errors)
            {
                errors.Add(new PsqlCompileError
                {
                    Message = $"Syntax error on token '{parseError.Token.Lexeme}'. {parseError.Error}",
                    StartPosition = parseError.Token.StartPosition,
                    EndPosition = parseError.Token.EndPosition,
                    StartLineNumber = parseError.Token.LineNumber,
                    EndLineNumber = parseError.Token.LineNumber
                });
            }
            
            return new GetCSharpDelegateResult { Errors = errors };
        }
        
        var delegateCompiler = new DelegateCompiler();
        var delegateCompilingResult = delegateCompiler.Compile(parseResult.Stages.ToArray());
        if (delegateCompilingResult.HasErrors)
        {
            foreach (var compileError in delegateCompilingResult.Errors)
            {
                errors.Add(new PsqlCompileError
                {
                    Message = $"Compile error on stage '{compileError.Stage.GetType().Name}' : {compileError.Error}",
                    StartPosition = compileError.Stage.StartPosition,
                    EndPosition = compileError.Stage.EndPosition,
                    StartLineNumber = compileError.Stage.StartLineNumber,
                    EndLineNumber = compileError.Stage.EndLineNumber,
                });
            }
        }
        
        return new GetCSharpDelegateResult { Errors = errors, Delegate = delegateCompilingResult.Delegate };
    }
    

    private class GetCSharpDelegateResult
    {
        public Func<PdfDocument, object?>? Delegate { get; init; }
        public required List<PsqlCompileError> Errors { get; init; }
    }
}
