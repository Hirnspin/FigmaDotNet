namespace FigmaDotNet.Models.Response;

using System.Text.Json.Serialization;

public class ComponentResponse : FigmaResponse
{
    [JsonPropertyName("meta")]
    public FigmaComponent Meta { get; set; }
}
