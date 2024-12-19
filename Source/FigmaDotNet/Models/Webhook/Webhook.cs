namespace FigmaDotNet.Models.Webhook;

using System.Text.Json.Serialization;

public class WebHook
{
    [JsonPropertyName("team_id")]
    public string TeamId { get; set; }

    [JsonPropertyName("event_type")]
    public string EventType { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; }

    [JsonPropertyName("passcode")]
    public string Passcode { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}
