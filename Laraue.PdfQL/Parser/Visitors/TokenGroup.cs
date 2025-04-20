namespace Laraue.PdfQL.Parser.Visitors;

public class TokenGroup : TokenBase
{
    public TokensCollection Tokens { get; } = new ();
    public override string Value => string.Concat(Tokens.Select(t => t.Value));
    public int MaxTokenPriority { get; set; }
}

public class TokensCollection : List<TokenBase>
{
    public void ReplaceTokens(IReadOnlyList<TokenWithPriority> tokens, TokenWithPriority newTokenWithPriority)
    {
        var lastIndex = IndexOf(tokens.Last());
        Insert(lastIndex + 1, newTokenWithPriority);

        foreach (var token in tokens)
        {
            Remove(token);
        }
    }
}