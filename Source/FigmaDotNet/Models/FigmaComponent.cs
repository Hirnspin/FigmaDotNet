namespace FigmaDotNet.Models;

using System;
using System.Text.Json.Serialization;

public class FigmaComponent
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string ThumbnailUrl { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("containing_frame")]
    public FrameInfo ContainingFrame { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}
