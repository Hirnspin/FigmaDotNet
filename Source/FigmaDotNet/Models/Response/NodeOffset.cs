namespace FigmaDotNet.Models.Response;

using System.Text.Json.Serialization;

public class NodeOffset
{
    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }
}
