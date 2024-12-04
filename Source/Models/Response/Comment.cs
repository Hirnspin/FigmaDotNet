namespace FigmaDotNet.Models.Response;

using System;
using System.Text.Json.Serialization;

public class Comment
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("parent_id")]
    public string ParentId { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("resolved_at")]
    public object ResolvedAt { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("client_meta")]
    public ClientMeta ClientMeta { get; set; }

    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }
}
