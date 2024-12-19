namespace FigmaDotNet.Models.Response;
using System.Text.Json.Serialization;

public class ClientMeta
{
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("node_offset")]
    public NodeOffset NodeOffset { get; set; }
}
