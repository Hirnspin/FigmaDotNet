using System.Text.Json.Serialization;

public class TypeStyle
{
    [JsonPropertyName("fontFamily")]
    public string FontFamily { get; set; }

    [JsonPropertyName("fontPostScriptName")]
    public string FontPostScriptName { get; set; }

    [JsonPropertyName("fontWeight")]
    public int FontWeight { get; set; }

    [JsonPropertyName("fontSize")]
    public float FontSize { get; set; }

    [JsonPropertyName("textAlignHorizontal")]
    public string TextAlignHorizontal { get; set; }

    [JsonPropertyName("textAlignVertical")]
    public string TextAlignVertical { get; set; }

    [JsonPropertyName("letterSpacing")]
    public float LetterSpacing { get; set; }

    [JsonPropertyName("lineHeightPx")]
    public float LineHeightPx { get; set; }

    [JsonPropertyName("lineHeightPercent")]
    public float LineHeightPercent { get; set; }

    [JsonPropertyName("lineHeightUnit")]
    public string LineHeightUnit { get; set; }
}
