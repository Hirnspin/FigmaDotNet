namespace FigmaDotNet.Models;

using System.Text.Json.Serialization;

public class FrameInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("pageId")]
    public string PageId { get; set; }

    [JsonPropertyName("pageName")]
    public string PageName { get; set; }

    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; }

    [JsonPropertyName("containingStateGroup")]
    public StateGroup ContainingStateGroup { get; set; }
}
