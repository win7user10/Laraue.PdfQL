using System.Collections.ObjectModel;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

public class AnonymousTypeRegistry
{
    private readonly string _moduleName;
    private readonly HashSet<AnonymousTypeProperties> _declaredProperties = new ();
    private readonly Dictionary<AnonymousTypeProperties, Type> _anonymousClassNames = new ();

    public AnonymousTypeRegistry(string moduleName)
    {
        _moduleName = moduleName;
    }
    
    public Type GetAnonymousType(AnonymousTypeProperties anonymousTypeProperties)
    {
        if (_declaredProperties.Add(anonymousTypeProperties))
        {
            var typeName = $"PdfQLAnonymousType_{_declaredProperties.Count}";
            var type = AnonymousTypeBuilder.CreateType(_moduleName, typeName, anonymousTypeProperties);
            _anonymousClassNames.Add(anonymousTypeProperties, type);
        }
        
        return _anonymousClassNames[anonymousTypeProperties];
    }
}

public class AnonymousTypeProperties : ReadOnlyDictionary<string, Type>
{
    public AnonymousTypeProperties(ReadOnlyDictionary<string, Type> properties) : base(properties)
    {
    }

    public override int GetHashCode()
    {
        var hashCode = 0;

        foreach (var property in this)
        {
            hashCode ^= property.Key.GetHashCode();
            hashCode ^= property.Value.GetHashCode();
        }
        
        return hashCode;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AnonymousTypeProperties anonymousTypeProperties)
        {
            return false;
        }

        return anonymousTypeProperties.Keys.SequenceEqual(Keys) 
            && anonymousTypeProperties.Values.SequenceEqual(Values);
    }
}