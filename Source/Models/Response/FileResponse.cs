namespace FigmaDotNet.Models.Response;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FileResponse : FigmaResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; }

    [JsonPropertyName("editorType")]
    public string EditorType { get; set; }

    [JsonPropertyName("thumbnailUrl")]
    public string ThumbnailUrl { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("components")]
    public IDictionary<string, FigmaComponent> Components { get; set; }

    [JsonPropertyName("componentSets")]
    public IDictionary<string, ComponentSet> ComponentSets { get; set; }

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("styles")]
    public IDictionary<string, dynamic> Styles { get; set; }

    [JsonPropertyName("mainFileKey")]
    public string MainFileKey { get; set; }

    [JsonPropertyName("branches")]
    public IEnumerable<dynamic> Branches { get; set; }
}
