using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models;

public class DevResourceUpdatePayload
{
    [JsonPropertyName("dev_resources")]
    public IEnumerable<DevResourceUpdate> DevResources { get; set; }
}
