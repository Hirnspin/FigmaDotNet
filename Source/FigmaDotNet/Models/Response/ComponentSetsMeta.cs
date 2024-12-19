namespace FigmaDotNet.Models.Response;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ComponentSetsMeta
{
    [JsonPropertyName("component_sets")]
    public IEnumerable<ComponentSet> ComponentSets { get; set; }
}
