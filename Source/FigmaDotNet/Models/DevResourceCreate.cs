using System.Text.Json.Serialization;

namespace FigmaDotNet.Models;

/// <summary>
/// Payload to create a dev resource in a file.
/// </summary>
public class DevResourceCreate
{
    /// <summary>
    /// The name of the dev resource.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The file key where the dev resource belongs.
    /// </summary>
    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    /// <summary>
    /// The target node to attach the dev resource to.
    /// </summary>
    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    /// <summary>
    /// The URL of the dev resource.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }
}
