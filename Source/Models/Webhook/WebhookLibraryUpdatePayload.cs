namespace FigmaDotNet.Models.Webhook;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FigmaDotNet.Models;

public class WebhookLibraryUpdatePayload
{
    [JsonPropertyName("created_components")]
    public List<UpdateElement> CreatedComponents { get; set; }

    [JsonPropertyName("created_styles")]
    public List<UpdateElement> CreatedStyles { get; set; }

    [JsonPropertyName("deleted_components")]
    public List<UpdateElement> DeletedComponents { get; set; }

    [JsonPropertyName("deleted_styles")]
    public List<UpdateElement> DeletedStyles { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("event_type")]
    public string EventType { get; set; }

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("file_name")]
    public string FileName { get; set; }

    [JsonPropertyName("modified_components")]
    public List<UpdateElement> ModifiedComponents { get; set; }

    [JsonPropertyName("modified_styles")]
    public List<UpdateElement> ModifiedStyles { get; set; }

    [JsonPropertyName("passcode")]
    public string Passcode { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("triggered_by")]
    public User TriggeredBy { get; set; }

    [JsonPropertyName("webhook_id")]
    public string WebhookId { get; set; }
}

public class UpdateElement
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

