using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models.Response;

public class CreateDevResourceResponse : FigmaResponse
{
    [JsonPropertyName("links_created")]
    public IEnumerable<DevResource> LinksCreated { get; set; }

    [JsonPropertyName("errors")]
    public IEnumerable<CreateDevResourceError> Errors { get; set; }
}

public class CreateDevResourceError
{
    [JsonPropertyName("file_key")]
    public string FileKey { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}
