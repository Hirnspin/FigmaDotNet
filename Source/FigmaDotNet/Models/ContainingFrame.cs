namespace FigmaDotNet.Models;

using System.Text.Json.Serialization;

public partial class ContainingFrame
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; }

    [JsonPropertyName("pageId")]
    public string PageId { get; set; }

    [JsonPropertyName("pageName")]
    public string PageName { get; set; }

    [JsonPropertyName("backgroundColor")]
    public string BackgroundColor { get; set; }
}
