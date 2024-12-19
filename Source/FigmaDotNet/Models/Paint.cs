using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models;

public class Paint
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("visible")]
    public bool Visible { get; set; }

    [JsonPropertyName("opacity")]
    public float Opacity { get; set; }

    [JsonPropertyName("color")]
    public Color Color { get; set; }

    [JsonPropertyName("blendMode")]
    public string BlendMode { get; set; }

    [JsonPropertyName("gradientHandlePositions")]
    public List<Vector> GradientHandlePositions { get; set; }

    [JsonPropertyName("gradientStops")]
    public List<ColorStop> GradientStops { get; set; }

    [JsonPropertyName("scaleMode")]
    public string ScaleMode { get; set; }
}

public class Color
{
    [JsonPropertyName("r")]
    public float R { get; set; }

    [JsonPropertyName("g")]
    public float G { get; set; }

    [JsonPropertyName("b")]
    public float B { get; set; }

    [JsonPropertyName("a")]
    public float A { get; set; }
}

public class Vector
{
    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }
}

public class ColorStop
{
    [JsonPropertyName("position")]
    public float Position { get; set; }

    [JsonPropertyName("color")]
    public Color Color { get; set; }
}
