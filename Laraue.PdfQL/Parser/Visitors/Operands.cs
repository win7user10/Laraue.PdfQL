using System.Reflection;

namespace Laraue.PdfQL.Parser.Visitors;

public static class Operands
{
    public const char Plus = '+';
    public const char Minus = '-';
    public const char Multiply = '*';
    public const char Divider = '/';
    public const char Equal = '=';
    public const char LeftBracket = '(';
    public const char RightBracket = ')';
    public const char Comma = ',';
    public const char Dot = '.';
    public const char WhiteSpace = ' ';

    private static readonly Dictionary<char, string> TokensByValue;
    private static readonly Dictionary<string, char> TokensByName;
    private static char[] BinaryOperands = [Plus, Minus, Multiply, Divider, Equal];
    
    public static string NameOf(char operand) => TokensByValue[operand];
    
    static Operands()
    {
        TokensByValue = typeof(Operands)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .ToDictionary(field => (char)field.GetValue(null)!, field => OperandUtils.Tokenize(field.Name));
        TokensByName = TokensByValue.ToDictionary(field => field.Value, field => field.Key);
    }

    public static bool ContainsOperand(char operand)
    {
        return TokensByValue.ContainsKey(operand);
    }
    
    public static bool ContainsOperand(string operandName)
    {
        return TokensByName.ContainsKey(operandName);
    }
    
    public static bool ContainsBinaryOperand(string operandName)
    {
        return TokensByName.TryGetValue(operandName, out var operandValue)
            && BinaryOperands.Contains(operandValue);
    }
}