using System.Collections.ObjectModel;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling;

/// <summary>
/// The storage for the PdfQL anonymous types.
/// </summary>
public class AnonymousTypeRegistry
{
    private readonly string _moduleName;
    private readonly HashSet<AnonymousTypeProperties> _declaredProperties = new ();
    private readonly Dictionary<AnonymousTypeProperties, Type> _anonymousClassNames = new ();

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="moduleName"></param>
    public AnonymousTypeRegistry(string moduleName)
    {
        _moduleName = moduleName;
    }
    
    /// <summary>
    /// Returns new anonymous types or returns existing.
    /// </summary>
    /// <param name="anonymousTypeProperties"></param>
    /// <returns></returns>
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

/// <summary>
/// Properties of anonymous type.
/// </summary>
public class AnonymousTypeProperties : ReadOnlyDictionary<string, Type>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="properties"></param>
    public AnonymousTypeProperties(IDictionary<string, Type> properties)
        : base(properties)
    {
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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