using System.Text.Json.Serialization;

namespace AsvtTPL.Components.Pages.DEMO2.DEMO212.widgets;

/// <summary>
/// 決策樹節點資料模型
/// </summary>
public class TreeNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("condField")]
    public string? CondField { get; set; }

    [JsonPropertyName("condOp")]
    public string? CondOp { get; set; }

    [JsonPropertyName("condValue")]
    public string? CondValue { get; set; }

    [JsonPropertyName("assignment")]
    public string Assignment { get; set; } = string.Empty;

    [JsonPropertyName("children")]
    public List<TreeNode> Children { get; set; } = new();

    [JsonIgnore]
    public bool Collapsed { get; set; } = false;

    public TreeNode() : this("None")
    {
    }

    public TreeNode(string type, string? id = null)
    {
        Id = id ?? GenerateId();
        Type = type;
    }

    private string GenerateId()
    {
        return "node_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "_" + Guid.NewGuid().ToString("N")[..9];
    }

    public void AddChild(TreeNode node)
    {
        Children.Add(node);
    }

    public void RemoveChild(string nodeId)
    {
        Children.RemoveAll(child => child.Id == nodeId);
    }

    public TreeNode ToJSON()
    {
        return this; // 直接返回自身，JsonSerializer 會處理序列化
    }
}
