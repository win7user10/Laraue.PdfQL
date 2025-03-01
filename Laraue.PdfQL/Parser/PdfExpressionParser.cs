using System.Text.Json;
using System.Text.Json.Serialization;
using Laraue.PdfQL.Parser.Visitors;
using Laraue.PdfQL.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace Laraue.PdfQL.Parser;

public class PdfExpressionParser
{
    private readonly IServiceProvider _serviceProvider;

    public PdfExpressionParser(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Stage[] ParseStages(string stagesJson)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        
        var stages = JsonSerializer.Deserialize<StageToken[]>(
            stagesJson,
            jsonOptions) ?? [];

        var result = new Stage[stages.Length];
        var context = new ParseContext();

        for (var index = 0; index < stages.Length; index++)
        {
            var stage = stages[index];
            result[index] = stage switch
            {
                SelectStageToken selectStageToken => Visit(selectStageToken, context),
                FilterStageToken filterStageToken => Visit(filterStageToken, context),
                MapStageToken mapStageToken => Visit(mapStageToken, context),
                SelectManyStageToken selectManyStageToken => Visit(selectManyStageToken, context),
                _ => throw new InvalidOperationException(),
            };
        }

        return result;
    }

    private Stage Visit<TToken>(TToken token, ParseContext context) where TToken : StageToken
    {
        var visitor = _serviceProvider.GetService<StageTokenVisitor<TToken>>();
        if (visitor == null)
            throw new NotImplementedException($"Visitor for {typeof(TToken)} was not found");
        
        return visitor.Visit(token, context);
    }
}