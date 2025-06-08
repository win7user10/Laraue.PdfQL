using Laraue.PdfQL.Stages;

namespace Laraue.PdfQL.Parser.Visitors.Expressions.DelegateCompiling;

public class TypeResolvedStage
{
    public TypeResolvedStage(Stage stage, Type[] inputTypes, Type outputType)
    {
        Stage = stage;
        InputTypes = inputTypes;
        OutputType = outputType;
    }

    public Stage Stage { get; set; }
    
    public Type[] InputTypes { get; set; }
    
    public Type OutputType { get; set; }
}