using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models.Response;

public class ChangeDevResourceResponse : FigmaResponse
{
    [JsonPropertyName("links_updated")]
    public IEnumerable<string> LinksUpdated { get; set; }

    [JsonPropertyName("errors")]
    public IEnumerable<ChangeDevResourceError> Errors { get; set; }
}

public class ChangeDevResourceError
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}
