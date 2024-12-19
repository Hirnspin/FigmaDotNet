namespace FigmaDotNet.Models;

using System;
using System.Text.Json.Serialization;

public partial class Meta
{
    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public Uri ThumbnailUrl { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("description_rt")]
    public string DescriptionRt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("containing_frame")]
    public ContainingFrame ContainingFrame { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}
