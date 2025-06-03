namespace Laraue.PdfQL.Parser.Visitors.Expressions.Translating;

public class Environment
{
    private readonly Environment? _parentScope;
    private readonly Dictionary<string, Type> _variables = new ();

    public Environment(Environment? parentScope = null)
    {
        _parentScope = parentScope;
    }

    public Type GetType(string name)
    {
        if (_variables.TryGetValue(name, out var variable))
        {
            return variable;
        }

        if (_parentScope != null)
        {
            return _parentScope.GetType(name);
        }

        throw new VariableNotDefinedException(name);
    }
    
    public bool TryDefine(string name, Type value)
    {
        return _variables.TryAdd(name, value);
    }
}