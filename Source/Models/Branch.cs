using System.Text.Json.Serialization;

namespace FigmaDotNet.Models
{
    public class Branch
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonPropertyName("last_modified")]
        public string LastModified { get; set; }

        [JsonPropertyName("link_access")]
        public string LinkAccess { get; set; }
    }
}