namespace FigmaDotNet.Models.Response;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ImageResponse
{
    [JsonPropertyName("images")]
    public Dictionary<string, string> Images { get; set; }
}
