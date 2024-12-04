namespace FigmaDotNet.Models.Response;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ComponentsMeta
{
    [JsonPropertyName("components")]
    public IEnumerable<FigmaComponent> Components { get; set; }
}
