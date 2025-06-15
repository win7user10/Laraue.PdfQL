using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Python.Runtime;

namespace Laraue.PdfQL.Interpreter.DelegateCompiling.Expressions;

public static class AnonymousTypeBuilder
{
    public static Type CreateType(Dictionary<string, Type> propertyTypes)
    {
        var tb = GetTypeBuilder();
        tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

        foreach (var propertyType in propertyTypes)
            CreateProperty(tb, propertyType.Key, propertyType.Value);

        var objectType = tb.CreateType();
        return objectType;
    }
    
    private static TypeBuilder GetTypeBuilder()
    {
        var typeSignature = "MyDynamicType";
        var an = new AssemblyName(typeSignature);
        var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var tb = moduleBuilder.DefineType(typeSignature,
            TypeAttributes.Public |
            TypeAttributes.Class |
            TypeAttributes.AutoClass |
            TypeAttributes.AnsiClass |
            TypeAttributes.BeforeFieldInit |
            TypeAttributes.AutoLayout,
            null);
        return tb;
    }
    
    private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
    {
        var debuggerAttributeParametersTypes = new[] { typeof(DebuggerBrowsableState) };
        var debuggerBrowsableAttributeConstructor = typeof(DebuggerBrowsableAttribute).GetConstructor(debuggerAttributeParametersTypes)!;
        var customAttributeBuilder = new CustomAttributeBuilder(debuggerBrowsableAttributeConstructor, [DebuggerBrowsableState.Never]);
        
        var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private | FieldAttributes.InitOnly);
        fieldBuilder.SetCustomAttribute(customAttributeBuilder);

        var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        var getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
        var getIl = getPropMthdBldr.GetILGenerator();

        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        var setPropMthdBldr =
            tb.DefineMethod("set_" + propertyName,
                MethodAttributes.Public |
                MethodAttributes.SpecialName |
                MethodAttributes.HideBySig,
                null, new[] { propertyType });
        
        var setIl = setPropMthdBldr.GetILGenerator();
        var modifyProperty = setIl.DefineLabel();
        var exitSet = setIl.DefineLabel();

        setIl.MarkLabel(modifyProperty);
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);

        setIl.Emit(OpCodes.Nop);
        setIl.MarkLabel(exitSet);
        setIl.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getPropMthdBldr);
        propertyBuilder.SetSetMethod(setPropMthdBldr);
    }
}