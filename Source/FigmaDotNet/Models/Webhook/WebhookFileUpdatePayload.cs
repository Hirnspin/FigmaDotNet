namespace FigmaDotNet.Models.Webhook;

using System;
using System.Text.Json.Serialization;

class WebhookFileUpdatePayload
{
    [JsonPropertyName("event_type")]
    public string EventType { get; set; }

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; }

    [JsonPropertyName("passcode")]
    public string Passcode { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("webhook_id")]
    public string WebhookId { get; set; }
}
