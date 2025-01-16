namespace FigmaDotNet.Models;

/// <summary>
/// Payload to create a dev resource in a file.
/// </summary>
public class DevResourceCreate
{
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
}
