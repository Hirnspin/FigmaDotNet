namespace FigmaDotNet.Models;

using System.Text.Json.Serialization;

public class User
{
    [JsonPropertyName("handle")]
    public string Handle { get; set; }

    [JsonPropertyName("img_url")]
    public string ImgUrl { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }
}
