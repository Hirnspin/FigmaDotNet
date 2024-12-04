namespace FigmaDotNet.Models.Response;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class CommentsResponse : FigmaResponse
{
    [JsonPropertyName("comments")]
    public IEnumerable<Comment> Comments { get; set; }
}
