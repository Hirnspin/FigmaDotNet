using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FigmaDotNet.Models.Response;

public abstract class Node
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("visible")]
    public bool Visible { get; set; }

    [JsonPropertyName("children")]
    public List<Node> Children { get; set; }
}

public class DocumentNode : Node
{
    // Additional properties specific to DocumentNode
}

public class PageNode : Node
{
    // Additional properties specific to PageNode
}

public class FrameNode : Node
{
    [JsonPropertyName("background")]
    public List<Paint> Background { get; set; }

    [JsonPropertyName("backgroundColor")]
    public Color BackgroundColor { get; set; }

    // Additional properties specific to FrameNode
}

public class GroupNode : Node
{
    // Additional properties specific to GroupNode
}

public class ComponentNode : Node
{
    // Additional properties specific to ComponentNode
}

public class InstanceNode : Node
{
    // Additional properties specific to InstanceNode
}

public class BooleanOperationNode : Node
{
    // Additional properties specific to BooleanOperationNode
}

public class VectorNode : Node
{
    // Additional properties specific to VectorNode
}

public class StarNode : Node
{
    // Additional properties specific to StarNode
}

public class LineNode : Node
{
    // Additional properties specific to LineNode
}

public class EllipseNode : Node
{
    // Additional properties specific to EllipseNode
}

public class PolygonNode : Node
{
    // Additional properties specific to PolygonNode
}

public class RectangleNode : Node
{
    // Additional properties specific to RectangleNode
}

public class TextNode : Node
{
    [JsonPropertyName("characters")]
    public string Characters { get; set; }

    [JsonPropertyName("style")]
    public TypeStyle Style { get; set; }

    // Additional properties specific to TextNode
}

public class SliceNode : Node
{
    // Additional properties specific to SliceNode
}

public class ComponentSetNode : Node
{
    // Additional properties specific to ComponentSetNode
}