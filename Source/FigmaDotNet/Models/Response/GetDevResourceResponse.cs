using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models.Response;

public class GetDevResourceResponse : FigmaResponse
{
    [JsonPropertyName("dev_resources")]
    public IEnumerable<DevResource> DevResources { get; set; }
}
