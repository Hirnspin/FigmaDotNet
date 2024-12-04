namespace FigmaDotNet.Models.Webhook;

using System.Text.Json.Serialization;

public class WebHookV2
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("team_id")]
    public string TeamId { get; set; }

    [JsonPropertyName("event_type")]
    public string EventType { get; set; }

    [JsonPropertyName("client_id")]
    public object ClientId { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    [JsonPropertyName("passcode")]
    public string Passcode { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("protocol_version")]
    public string ProtocolVersion { get; set; }
}
