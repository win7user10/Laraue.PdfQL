using System.Diagnostics.CodeAnalysis;

namespace Laraue.PdfQL.Interpreter.Scanning;

public sealed class Scanner : IScanner
{
    public ScanResult ScanTokens(string input)
    {
        return new ScannerImplementation(input).ScanTokens();
    }

    private class ScannerImplementation
    {
        private readonly string _input;
        private int _startAbsolutePosition;
        private int _startRelativePosition;
        private int _currentAbsolutePosition;
        private int _currentRelativePosition;
        private int _lineNumber;
        private readonly List<Token> _tokens = new ();
        private readonly List<ScanError> _errors = new ();
        
        private const char StringStart = '\'';

        private Dictionary<string, TokenType> _keywords = new()
        {
            ["new"] = TokenType.New,
        };

        public ScannerImplementation(string input)
        {
            _input = input;
        }

        public ScanResult ScanTokens()
        {
            while (!IsScanCompleted)
            {
                _startAbsolutePosition = _currentAbsolutePosition;
                _startRelativePosition = _currentRelativePosition;
                ScanToken();
            }
            
            _tokens.Add(new Token
            {
                TokenType = TokenType.Eof,
                Lexeme = null,
                StartPosition = _startRelativePosition,
                EndPosition = _currentRelativePosition,
                LineNumber = _lineNumber,
            });
            
            return new ScanResult
            {
                Tokens = _tokens.ToArray(),
                Errors = _errors.ToArray()
            };
        }

        private void ScanToken()
        {
            var nextChar = Advance();
            switch (nextChar)
            {
                case ' ': break;
                case '\r':
                    if (PopNextCharIf(c => c == '\n'))
                        ToNextLine();
                    break;
                case '\n':
                    ToNextLine();
                    break;
                case '\t':
                    break;
                case '(': AddToken(TokenType.LeftParentheses); break;
                case ')': AddToken(TokenType.RightParentheses); break;
                case '{': AddToken(TokenType.LeftBracket); break;
                case '}': AddToken(TokenType.RightBracket); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '+': AddToken(TokenType.Plus); break;
                case '-': 
                    AddToken(PopNextCharIf(c => c == '>') ? TokenType.NextPipeline : TokenType.Minus); break;
                case '/': AddToken(TokenType.Divide); break;
                case '*': AddToken(TokenType.Multiply); break;
                case '=':
                    AddToken(PopNextCharIf(c => c == '>') 
                        ? TokenType.Lambda
                        : PopNextCharIf(c => c == '=') 
                            ? TokenType.Equal
                            : TokenType.Assign); break;
                case '!':
                    AddToken(PopNextCharIf(c => c == '=') ? TokenType.NotEqual : TokenType.Not); break;
                case '<':
                    AddToken(PopNextCharIf(c => c == '=') ? TokenType.LessOrEqualThan : TokenType.LessThan); break;
                case '>':
                    AddToken(PopNextCharIf(c => c == '=') ? TokenType.GreaterOrEqualThan : TokenType.GreaterThan); break;
                case StringStart: AddString(); break;
                    
                default:
                    if (IsDigit(nextChar))
                        AddNumber();
                    else if (IsAlpha(nextChar))
                        AddIdentifier();
                    else
                        _errors.Add(new ScanError
                        {
                            StartPosition = _startRelativePosition,
                            EndPosition = _currentRelativePosition,
                            Error = $"Unknown character '{nextChar}'.",
                            LineNumber = _lineNumber
                        });
                    break;
            }
        }

        private void ToNextLine()
        {
            _lineNumber++;
            _currentRelativePosition = 0;
        }
        
        private bool IsDigit (char c) => c is >= '0' and <= '9';
        private bool IsAlpha (char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

        private void AddString()
        {
            while (TryPeekNextChar(out var nextChar) && nextChar != StringStart && !IsScanCompleted)
            {
                Advance();
            }

            if (IsScanCompleted)
            {
                _errors.Add(new ScanError
                {
                    StartPosition = _startRelativePosition,
                    Error = "Unterminated string.",
                    EndPosition = _currentRelativePosition,
                    LineNumber = _lineNumber
                });
            }

            // consume last '"'
            Advance();
            
            var value = _input[(_startAbsolutePosition + 1)..(_currentAbsolutePosition - 1)];
            AddToken(TokenType.String, value);
        }

        private void AddNumber()
        {
            while (PopNextCharIf(IsDigit))
            {
            }

            if (TryPeekNextChar(out var nextChar) && nextChar == '.'
                && TryPeekInOneChar(out var inOneChar) && IsDigit(inOneChar.Value))
            {
                Advance();
                
                while (PopNextCharIf(IsDigit))
                {
                }
            }
            
            AddToken(
                TokenType.Integer,
                int.Parse(_input[_startAbsolutePosition.._currentAbsolutePosition]));
        }
        
        private void AddIdentifier()
        {
            while (PopNextCharIf(ch => IsAlpha(ch) || IsDigit(ch)))
            {
            }
            
            var text = _input[_startAbsolutePosition.._currentAbsolutePosition];
            AddToken(_keywords.GetValueOrDefault(text, TokenType.Identifier));
        }

        private char Advance()
        {
            _currentRelativePosition++;
            return _input[_currentAbsolutePosition++];
        }
        
        private bool TryPeekNextChar([NotNullWhen(true)] out char? nextChar)
        {
            nextChar = null;
            
            if (IsScanCompleted)
            {
                return false;
            }
            
            nextChar = _input[_currentAbsolutePosition];
            return true;
        }

        private bool TryPeekInOneChar([NotNullWhen(true)] out char? inOneChar)
        {
            inOneChar = null;
            
            if (_currentAbsolutePosition + 1 >= _input.Length)
            {
                return false;
            }
            
            inOneChar = _input[_currentAbsolutePosition + 1];
            return true;
        }
        
        private bool PopNextCharIf(Func<char, bool> predicate)
        {
            if (IsScanCompleted)
            {
                return false;
            }

            if (!predicate(_input[_currentAbsolutePosition]))
            {
                return false;
            }
            
            _currentAbsolutePosition++;
            _currentRelativePosition++;
            return true;
        }

        private void AddToken(TokenType tokenType, object? literal = null)
        {
            var lexeme = _input[_startAbsolutePosition.._currentAbsolutePosition];
            
            _tokens.Add(new Token
            {
                TokenType = tokenType,
                Lexeme = lexeme,
                Literal = literal,
                StartPosition = _startRelativePosition,
                EndPosition = _currentRelativePosition,
                LineNumber = _lineNumber
            });
        }
        
        private bool IsScanCompleted => _currentAbsolutePosition >= _input.Length;
    }
}