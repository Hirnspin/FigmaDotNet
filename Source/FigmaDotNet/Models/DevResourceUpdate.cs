using System;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models;

/// <summary>
/// Payload to update a dev resource in a file.
/// </summary>
public class DevResourceUpdate
{
    /// <summary>
    /// Unique identifier of the dev resource
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// The name of the dev resource.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// The URL of the dev resource.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; }
}
