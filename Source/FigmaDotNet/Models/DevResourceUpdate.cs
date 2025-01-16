using System;

namespace FigmaDotNet.Models;

/// <summary>
/// Payload to update a dev resource in a file.
/// </summary>
public class DevResourceUpdate
{
    /// <summary>
    /// The unique identifier of the resource.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the resource.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description of the resource.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The type of the resource.
    /// </summary>
    public string ResourceType { get; set; }

    /// <summary>
    /// The URL of the resource.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// The date and time when the resource was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
