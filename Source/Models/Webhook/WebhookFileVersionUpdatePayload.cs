namespace FigmaDotNet.Models.Webhook;

using System;
using System.Text.Json.Serialization;

public class WebhookFileVersionUpdatePayload
{
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("event_type")]
    public string EventType { get; set; }

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("passcode")]
    public string Passcode { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("triggered_by")]
    public User TriggeredBy { get; set; }

    [JsonPropertyName("version_id")]
    public string VersionId { get; set; }

    [JsonPropertyName("webhook_id")]
    public string WebhookId { get; set; }
}
