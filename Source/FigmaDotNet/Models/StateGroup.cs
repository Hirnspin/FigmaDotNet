namespace FigmaDotNet.Models;

using System.Text.Json.Serialization;

public class StateGroup
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
