namespace FigmaDotNet.Models.Webhook;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public class WebHookListV2
{
    [JsonPropertyName("webhooks")]
    public IEnumerable<WebHookV2> WebHooks { get; set; }
}
