namespace FigmaDotNet.Models.Response;

using System.Text.Json.Serialization;
  
public class FigmaResponse
{
    [JsonPropertyName("err")]
    public string Err { get; set; }

    [JsonPropertyName("error")]
    public bool Error { get; set; } = false;

    [JsonPropertyName("status")]
    public int Status { get; set; }
}
