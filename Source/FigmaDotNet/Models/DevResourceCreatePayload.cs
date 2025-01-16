using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models;

public class DevResourceCreatePayload
{
    [JsonPropertyName("dev_resources")]
    public IEnumerable<DevResourceCreate> DevResources { get; set; }
}
