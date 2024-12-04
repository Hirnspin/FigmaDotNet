namespace FigmaDotNet.Models.Response;

using System.Text.Json.Serialization;

public class ComponentsResponse : FigmaResponse
{
    [JsonPropertyName("meta")]
    public ComponentsMeta Meta { get; set; }
}
