using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models.Response;

public class PutDevResourceResponse : FigmaResponse
{
    [JsonPropertyName("links_updated")]
    public IEnumerable<string> LinksUpdated { get; set; }

    [JsonPropertyName("errors")]
    public IEnumerable<PutDevResourceError> Errors { get; set; }
}

public class PutDevResourceError
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}
