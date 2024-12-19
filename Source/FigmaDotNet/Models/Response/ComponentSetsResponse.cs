namespace FigmaDotNet.Models.Response;

using System.Text.Json.Serialization;

public class ComponentSetsResponse : FigmaResponse
{
    [JsonPropertyName("meta")]
    public ComponentSetsMeta Meta { get; set; }
}
