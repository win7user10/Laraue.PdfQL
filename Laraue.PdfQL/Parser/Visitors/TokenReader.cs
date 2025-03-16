namespace Laraue.PdfQL.Parser.Visitors;

public class TokenReader
{
    private readonly OpenClosePair[] _openClosePairs;

    public TokenReader(OpenClosePair[] openClosePairs)
    {
        _openClosePairs = openClosePairs;
    }
    
    public string[] Split(string input, char delimiter)
    {
        var openClosePairStates = GetOpenClosePairStates();

        var words = new List<string>();
        var currentPosition = 0;
        var lastWordStartPosition = 0;
        while (currentPosition < input.Length)
        {
            var currentChar = input[currentPosition];
            if (!openClosePairStates.TryProcessOpenCharacter(currentChar)
                && !openClosePairStates.TryProcessCloseCharacter(currentChar))
            {
                if (currentChar == delimiter)
                {
                    if (!openClosePairStates.HasOpened)
                    {
                        words.Add(input[lastWordStartPosition..currentPosition]);
                        lastWordStartPosition = currentPosition + 1;
                    }
                }
            }
            
            currentPosition++;
        }

        words.Add(input[lastWordStartPosition..currentPosition]);
        return words.ToArray();
    }

    public NextGrammar ReadNext(string input)
    {
        var openClosePairStates = GetOpenClosePairStates();
        
        var currentPosition = 0;
        var currentChar = input[currentPosition];
        
        while (currentPosition < input.Length)
        {
            currentChar = input[currentPosition];
            try
            {
                if (!openClosePairStates.TryProcessOpenCharacter(currentChar))
                {
                    if (openClosePairStates.TryProcessCloseCharacter(currentChar))
                    {
                        if (!openClosePairStates.HasOpened)
                        {
                            break;
                        }
                    }
                }
                
                currentPosition++;
            }
            catch (InvalidSyntaxException e)
            {
                throw new InvalidSyntaxException($"Error while parsing token: '{input}' on position: {currentPosition}. {e.Message}", e);
            }
        }
        
        // try match regex modifiers. Try get next two symbols: "?" and "+"
        if (currentChar == ']')
        {
            var nextTwoChars = input[(currentPosition + 1)..(currentPosition + 3)];
            if (nextTwoChars == "?+")
            {
                return new NextAnyTimeGrammar
                {
                    Grammar = input[1..(currentPosition)],
                    RemainedString = input[(currentPosition + 3)..],
                };
            }

            if (nextTwoChars.Length > 0 && nextTwoChars[0] == '?')
            {
                return new NextOnceOrNeverGrammar
                {
                    Grammar = input[1..(currentPosition)],
                    RemainedString = input[(currentPosition + 2)..],
                };
            }
        }
        
        // other exact grammars
        var grammar = input[..(currentPosition + 1)];
        return new NextExactGrammar
        {
            Grammar = grammar,
            RemainedString = input[(currentPosition + 1)..],
        };
    }

    private OpenClosePairStates GetOpenClosePairStates()
    {
        return new OpenClosePairStates(_openClosePairs);
    }

    private class OpenClosePairStates
    {
        private readonly OpenClosePair[] _openClosePairStates;
        private Stack<char> _openedCharacters = new ();

        public OpenClosePairStates(OpenClosePair[] openClosePairStates)
        {
            _openClosePairStates = openClosePairStates;
        }
        
        public bool HasOpened => _openedCharacters.Count != 0;
    
        public bool TryProcessCloseCharacter(char currentChar)
        {
            foreach (var openClosePairState in _openClosePairStates)
            {
                // not closing character, skip
                if (currentChar != openClosePairState.CloseCharacter)
                {
                    continue;
                }

                // close character of some pair is found
                // if opened characters are not found
                if (!_openedCharacters.TryPeek(out var openedCharacter))
                {
                    throw new InvalidSyntaxException(
                        $"Excepted word or '{openClosePairState.OpenCharacter}', taken '{currentChar}'");
                }

                // if opened character is found, but it's not opened character of this closing symbol
                if (openedCharacter != openClosePairState.OpenCharacter)
                {
                    throw new InvalidSyntaxException(
                        $"Excepted closing of '{openedCharacter}', taken '{currentChar}'");
                }
                
                // closing is correct, remove character from stack
                _openedCharacters.Pop();
                return true;
            }

            return false;
        }
        
        public bool TryProcessOpenCharacter(char currentChar)
        {
            foreach (var openClosePairState in _openClosePairStates)
            {
                // not opening character, skip
                if (currentChar != openClosePairState.OpenCharacter)
                {
                    continue;
                }

                // no one character is opened, just open new
                if (!_openedCharacters.TryPeek(out var openedCharacter))
                {
                    _openedCharacters.Push(currentChar);
                    return true;
                }
                
                // if last opened is the same as opening char, fail
                if (openedCharacter == currentChar)
                {
                    throw new InvalidSyntaxException(
                        $"Excepted word or '{openClosePairState.CloseCharacter}', taken '{currentChar}'");
                }
                
                // if any character is already open that not allows nested openings
                var alsoOpenedCharacters = _openedCharacters
                    .Take(new Range(new Index(0), new Index(1, true)))
                    .ToArray();

                // no more characters, just open new
                if (alsoOpenedCharacters.Length == 0)
                {
                    _openedCharacters.Push(currentChar);
                    return true;
                }

                var characterRequireClose = alsoOpenedCharacters
                    .Join(_openClosePairStates,
                        c => c,
                        pair => pair.OpenCharacter,
                        (_, p) => new { p.EnableNestedOpen, p.OpenCharacter })
                    .FirstOrDefault(p => !p.EnableNestedOpen);

                if (characterRequireClose != null)
                {
                    throw new InvalidSyntaxException(
                        $"Excepted word or closing of '{characterRequireClose.OpenCharacter}', taken '{currentChar}'");
                }

                _openedCharacters.Push(currentChar);
                return true;
            }

            return false;
        }
    }
}

public record OpenClosePair(char OpenCharacter, char? CloseCharacter, bool EnableNestedOpen);
public record OpenClosePairState(OpenClosePair Pair, bool State)
{
    public bool State { get; set; } = State;
}


public abstract record NextGrammar
{
    public string Grammar { get; set; } = string.Empty;
    public string RemainedString { get; set; } = string.Empty;
}

public record NextExactGrammar : NextGrammar
{
}

public record NextOnceOrNeverGrammar : NextGrammar
{
    
}

public record NextAnyTimeGrammar : NextGrammar
{
}