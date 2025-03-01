using System.Text.Json.Serialization;

namespace Laraue.PdfQL.Parser;

[JsonPolymorphic(TypeDiscriminatorPropertyName = ExpressionNames.ExpressionPropertyName)]
[JsonDerivedType(typeof(SelectStageToken), typeDiscriminator: ExpressionNames.Select)]
[JsonDerivedType(typeof(FilterStageToken), typeDiscriminator: ExpressionNames.Filter)]
[JsonDerivedType(typeof(SelectManyStageToken), typeDiscriminator: ExpressionNames.SelectMany)]
[JsonDerivedType(typeof(MapStageToken), typeDiscriminator: ExpressionNames.Map)]
public abstract class StageToken
{
}