using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsvtTPL.Components.Pages.DEMO2.DEMO212.widgets;

/// <summary>
/// 決策樹編輯器 - Code Behind
/// </summary>
public partial class DecisionTreeEditor : ComponentBase, IAsyncDisposable
{
    #region Dependency Injection

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    #endregion

    #region JavaScript Interop

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<DecisionTreeEditor>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./Components/Pages/DEMO2/DEMO212/widgets/DecisionTreeEditor.razor.js");

            _dotNetRef = DotNetObjectReference.Create(this);

            await _jsModule.InvokeVoidAsync("initializeDragDrop", _dotNetRef);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
        }
        _dotNetRef?.Dispose();
    }

    #endregion

    #region State Management

    private TreeNode Root { get; set; } = new TreeNode("Root", "root");
    private TreeNode? SelectedNode { get; set; }

    // 拖拉狀態（由 JavaScript 傳入）
    private string? _draggedNodeId;
    private string? _draggedNodeParentId;

    #endregion

    #region Lifecycle - Initialization

    protected override void OnInitialized()
    {
        // 不再自動載入範例資料
        // 父元件可透過 @ref 呼叫 LoadSampleData() 或 ImportTree() 進行初始化
    }

    /// <summary>
    /// 載入範例決策樹資料
    /// </summary>
    public void LoadSampleData()
    {
        // 範例一的結構
        var if1 = new TreeNode("IfCondition")
        {
            Description = "這是一個範例條件。",
            CondField = "A",
            CondOp = "=",
            CondValue = "3"
        };
        var assign1 = new TreeNode("Assignment") { Assignment = "'3A'" };
        if1.AddChild(assign1);

        var if2 = new TreeNode("IfCondition")
        {
            CondField = "B",
            CondOp = ">",
            CondValue = "4"
        };
        var assign2 = new TreeNode("Assignment") { Assignment = "'Bar'" };
        if2.AddChild(assign2);

        var if3 = new TreeNode("IfCondition")
        {
            CondField = "A",
            CondOp = "<",
            CondValue = "10"
        };
        var assign3 = new TreeNode("Assignment") { Assignment = "'少數'" };
        if3.AddChild(assign3);

        var else1 = new TreeNode("ElseBranch");
        var assign4 = new TreeNode("Assignment") { Assignment = "'otherwise'" };
        else1.AddChild(assign4);

        Root.AddChild(if1);
        Root.AddChild(if2);
        Root.AddChild(if3);
        Root.AddChild(else1);

        StateHasChanged();
    }

    #endregion

    #region Node Operations - CRUD

    private void AddRootNode()
    {
        var newNode = new TreeNode("None");
        Root.AddChild(newNode);
        SelectNode(newNode);
    }

    private void AddChildNode(TreeNode parentNode)
    {
        var newNode = new TreeNode("None");
        parentNode.AddChild(newNode);
        SelectNode(newNode);
    }

    private void AddSiblingNode(TreeNode currentNode)
    {
        var parent = FindParentNode(Root, currentNode.Id);
        if (parent == null)
        {
            ShowNotification("根節點無法新增兄弟節點！", "warning");
            return;
        }

        var newNode = new TreeNode("None");
        var currentIndex = parent.Children.FindIndex(child => child.Id == currentNode.Id);
        parent.Children.Insert(currentIndex + 1, newNode);

        SelectNode(newNode);
    }

    private void DeleteNode(TreeNode node)
    {
        var parent = FindParentNode(Root, node.Id);
        if (parent == null)
        {
            ShowNotification("無法刪除根節點！", "error");
            return;
        }

        parent.RemoveChild(node.Id);
        SelectedNode = null;
    }

    private void ClearTree()
    {
        if (Root.Children.Count == 0)
        {
            ShowNotification("⚠️ 樹已經是空的！", "warning");
            return;
        }

        Root = new TreeNode("Root", "root");
        SelectedNode = null;
        ShowNotification("✓ 決策樹已清空！", "success");
    }

    private void SelectNode(TreeNode node)
    {
        SelectedNode = node;
        StateHasChanged();
    }

    private void ToggleNode(string nodeId)
    {
        var node = FindNodeById(Root, nodeId);
        if (node != null)
        {
            node.Collapsed = !node.Collapsed;
        }
    }

    #endregion

    #region Helper Methods

    private TreeNode? FindNodeById(TreeNode node, string id)
    {
        if (node.Id == id) return node;
        foreach (var child in node.Children)
        {
            var found = FindNodeById(child, id);
            if (found != null) return found;
        }
        return null;
    }

    private TreeNode? FindParentNode(TreeNode parent, string childId)
    {
        foreach (var child in parent.Children)
        {
            if (child.Id == childId) return parent;
            var found = FindParentNode(child, childId);
            if (found != null) return found;
        }
        return null;
    }

    #endregion

    #region Drag & Drop (Called from JavaScript)

    [JSInvokable]
    public void OnDragStart(string nodeId, string? parentId)
    {
        _draggedNodeId = nodeId;
        _draggedNodeParentId = parentId;
    }

    [JSInvokable]
    public async Task<bool> OnDrop(string targetNodeId, string dropPosition)
    {
        if (string.IsNullOrEmpty(_draggedNodeId) || string.IsNullOrEmpty(targetNodeId))
            return false;

        var draggedNode = FindNodeById(Root, _draggedNodeId);
        var targetNode = FindNodeById(Root, targetNodeId);
        var draggedParent = string.IsNullOrEmpty(_draggedNodeParentId)
            ? Root
            : FindNodeById(Root, _draggedNodeParentId);

        if (draggedNode == null || targetNode == null || draggedParent == null)
            return false;

        if (draggedNode.Id == targetNode.Id)
            return false;

        // 檢查循環依賴
        if (IsDescendant(draggedNode, targetNode))
        {
            await ShowNotificationAsync("⚠️ 無法將節點移動到其子節點下！", "error");
            return false;
        }

        // 根據位置執行插入
        if (dropPosition == "above")
        {
            return HandleDropAsSibling(draggedNode, draggedParent, targetNode);
        }
        else
        {
            return HandleDropAsFirstChild(draggedNode, draggedParent, targetNode);
        }
    }

    private bool HandleDropAsSibling(TreeNode draggedNode, TreeNode draggedParent, TreeNode targetNode)
    {
        var targetParent = FindParentNode(Root, targetNode.Id);
        if (targetParent == null)
        {
            ShowNotification("⚠️ 根層級節點無法插入兄弟節點！", "warning");
            return false;
        }

        var targetIndex = targetParent.Children.FindIndex(c => c.Id == targetNode.Id);

        // 從原父節點移除
        draggedParent.RemoveChild(draggedNode.Id);

        // 處理同父節點移動的索引調整
        var insertIndex = targetIndex;
        if (draggedParent.Id == targetParent.Id)
        {
            var originalIndex = targetParent.Children.FindIndex(c => c.Id == draggedNode.Id);
            if (originalIndex != -1 && originalIndex < targetIndex)
            {
                insertIndex = targetIndex - 1;
            }
        }

        // 在目標節點上方插入
        targetParent.Children.Insert(insertIndex, draggedNode);
        StateHasChanged();
        return true;
    }

    private bool HandleDropAsFirstChild(TreeNode draggedNode, TreeNode draggedParent, TreeNode targetNode)
    {
        // 從原父節點移除
        draggedParent.RemoveChild(draggedNode.Id);

        // 插入為第一個子節點
        targetNode.Children.Insert(0, draggedNode);
        StateHasChanged();
        return true;
    }

    private bool IsDescendant(TreeNode parent, TreeNode child)
    {
        if (parent.Id == child.Id) return true;
        foreach (var node in parent.Children)
        {
            if (IsDescendant(node, child)) return true;
        }
        return false;
    }

    #endregion

    #region Validation

    private List<string> _validationErrors = new();
    private List<string> _validationWarnings = new();
    private bool _showValidation = false;

    private void ValidateTree()
    {
        _validationErrors.Clear();
        _validationWarnings.Clear();
        _showValidation = true;

        ValidateNode(Root, _validationErrors, _validationWarnings, new List<string>());
        StateHasChanged();
    }

    private void ValidateNode(TreeNode node, List<string> errors, List<string> warnings, List<string> path)
    {
        var currentPath = new List<string>(path) { GetNodeLabel(node) };

        // 檢查節點類型
        if (node.Type == "None" && node.Children.Count == 0 && node.Id != "root")
        {
            warnings.Add($"路徑 {string.Join(" > ", currentPath)}: 節點類型未設定");
        }

        // 檢查 IF 條件
        if (node.Type == "IfCondition")
        {
            if (string.IsNullOrWhiteSpace(node.CondField))
            {
                errors.Add($"路徑 {string.Join(" > ", currentPath)}: IF 節點缺少條件欄位");
            }
            if (node.Children.Count == 0)
            {
                warnings.Add($"路徑 {string.Join(" > ", currentPath)}: IF 節點沒有子節點（結果）");
            }
        }

        // 檢查 ELSE 分支
        if (node.Type == "ElseBranch" && node.Children.Count == 0)
        {
            warnings.Add($"路徑 {string.Join(" > ", currentPath)}: ELSE 節點沒有子節點（結果）");
        }

        // 檢查 Assignment
        if (node.Type == "Assignment")
        {
            if (string.IsNullOrWhiteSpace(node.Assignment))
            {
                errors.Add($"路徑 {string.Join(" > ", currentPath)}: Assignment 節點缺少指定值");
            }
            // 新增規則：Assignment 是終止點，不應該有子節點
            if (node.Children.Count > 0)
            {
                errors.Add($"路徑 {string.Join(" > ", currentPath)}: Assignment 是結構終止點，不應該有子節點（發現 {node.Children.Count} 個子節點）");
            }
        }

        // 檢查同一層是否有多個 ELSE
        var elseCount = node.Children.Count(child => child.Type == "ElseBranch");
        if (elseCount > 1)
        {
            errors.Add($"路徑 {string.Join(" > ", currentPath)}: 同一層級有 {elseCount} 個 ELSE 節點，只能有一個");
        }

        // 檢查 ELSE 是否在最後
        var elseIndex = node.Children.FindIndex(child => child.Type == "ElseBranch");
        if (elseIndex != -1 && elseIndex != node.Children.Count - 1)
        {
            warnings.Add($"路徑 {string.Join(" > ", currentPath)}: ELSE 節點建議放在最後");
        }

        // 修正規則2：檢查同層級的 Assignment 唯一性
        var assignmentCount = node.Children.Count(child => child.Type == "Assignment");
        if (assignmentCount > 0 && node.Children.Count > 1)
        {
            errors.Add($"路徑 {string.Join(" > ", currentPath)}: 同一層級中有 Assignment 節點時，它必須是唯一的節點。此處發現了 {node.Children.Count} 個節點，違反了此規則。");
        }

        // 遞迴檢查子節點
        foreach (var child in node.Children)
        {
            ValidateNode(child, errors, warnings, currentPath);
        }
    }

    private void CloseValidation()
    {
        _showValidation = false;
    }

    #endregion

    #region 公開 API - 供父元件控制樹實例

    /// <summary>
    /// 深度複製樹節點（使用 JSON 序列化）
    /// </summary>
    private TreeNode DeepClone(TreeNode source)
    {
        var json = JsonSerializer.Serialize(source, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        var cloned = JsonSerializer.Deserialize<TreeNode>(json);
        return cloned ?? throw new InvalidOperationException("深度複製失敗：反序列化返回 null");
    }

    /// <summary>
    /// 匯出決策樹的深度複製副本
    /// </summary>
    /// <returns>樹結構的深度複製物件</returns>
    public TreeNode ExportTree()
    {
        return DeepClone(Root);
    }

    /// <summary>
    /// 匯入決策樹結構，覆蓋當前樹狀資料
    /// </summary>
    /// <param name="tree">要匯入的樹結構（會進行深度複製）</param>
    /// <exception cref="ArgumentNullException">tree 為 null</exception>
    /// <exception cref="ArgumentException">tree 結構不正確</exception>
    public void ImportTree(TreeNode tree)
    {
        if (tree == null)
            throw new ArgumentNullException(nameof(tree));

        if (string.IsNullOrEmpty(tree.Id) || tree.Children == null)
            throw new ArgumentException("樹結構不正確：缺少必要欄位", nameof(tree));

        Root = DeepClone(tree);
        SelectedNode = null;
        StateHasChanged();
    }

    #endregion

    #region Export/Import

    private async Task TriggerFileInput()
    {
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("triggerFileInput", "fileInput");
        }
    }

    private async Task ExportToJSON()
    {
        var treeJSON = Root.ToJSON();
        var jsonString = JsonSerializer.Serialize(treeJSON, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
        var filename = $"decision-tree-{timestamp}.json";

        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("downloadFile", filename, jsonString, "application/json");
        }

        await ShowNotificationAsync("✓ JSON 已匯出！", "success");
    }

    private async Task ImportFromJSON(Microsoft.AspNetCore.Components.Forms.InputFileChangeEventArgs e)
    {
        try
        {
            var file = e.File;
            using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024); // 1MB limit
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();

            var jsonData = JsonSerializer.Deserialize<TreeNode>(jsonContent);

            if (jsonData == null || string.IsNullOrEmpty(jsonData.Id) || jsonData.Children == null)
            {
                throw new Exception("JSON 格式不正確：缺少必要欄位 (id, type, children)");
            }

            if (Root.Children.Count > 0)
            {
                // 這裡簡化處理，實際應該用 JavaScript confirm
                // 暫時直接覆蓋
            }

            Root = jsonData;
            SelectedNode = null;
            await ShowNotificationAsync("✓ JSON 匯入成功！建議執行「驗證結構」檢查。", "success");
        }
        catch (Exception ex)
        {
            await ShowNotificationAsync($"匯入失敗：{ex.Message}", "error");
        }
    }

    private async Task ExportToText()
    {
        var textOutput = GenerateTextOutput(Root, 0);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
        var filename = $"decision-tree-{timestamp}.txt";

        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("downloadFile", filename, textOutput, "text/plain");
        }

        await ShowNotificationAsync("✓ TXT 檔案已下載！", "success");
    }

    private string GenerateTextOutput(TreeNode node, int indent)
    {
        var output = new StringBuilder();
        var spaces = new string(' ', indent * 2);

        foreach (var child in node.Children)
        {
            if (child.Type == "IfCondition")
            {
                var conditionText = $"{child.CondField ?? ""} {child.CondOp ?? ""} {child.CondValue ?? ""}".Trim();
                output.AppendLine($"{spaces}IF {conditionText}");
                if (child.Children.Count > 0)
                {
                    output.Append(GenerateTextOutput(child, indent + 1));
                }
            }
            else if (child.Type == "ElseBranch")
            {
                output.AppendLine($"{spaces}ELSE");
                if (child.Children.Count > 0)
                {
                    output.Append(GenerateTextOutput(child, indent + 1));
                }
            }
            else if (child.Type == "Assignment")
            {
                output.AppendLine($"{spaces}{child.Assignment}");
            }
        }

        return output.ToString();
    }

    #endregion

    #region Notification

    private async Task ShowNotificationAsync(string message, string type)
    {
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("showNotification", message, type);
        }
    }

    private void ShowNotification(string message, string type)
    {
        // 同步版本，用於非 async 方法
        _ = ShowNotificationAsync(message, type);
    }

    #endregion

    #region UI Helper Methods

    public string GetNodeLabel(TreeNode node)
    {
        string baseLabel;
        if (node.Type == "IfCondition")
        {
            var condField = node.CondField ?? "?";
            var condOp = node.CondOp ?? "?";
            var condValue = node.CondValue ?? "?";
            var conditionText = string.IsNullOrWhiteSpace(node.CondField) && string.IsNullOrWhiteSpace(node.CondOp) && string.IsNullOrWhiteSpace(node.CondValue)
                ? "(未設定條件)"
                : $"{condField} {condOp} {condValue}";
            baseLabel = $"IF {conditionText}";
        }
        else if (node.Type == "ElseBranch")
        {
            baseLabel = "ELSE";
        }
        else if (node.Type == "Assignment")
        {
            baseLabel = string.IsNullOrWhiteSpace(node.Assignment) ? "(未設定指定值)" : node.Assignment;
        }
        else if (node.Type == "Root")
        {
            baseLabel = "根節點";
        }
        else
        {
            baseLabel = "NONE (未指定類型)";
        }

        if (!string.IsNullOrWhiteSpace(node.Description))
        {
            return $"{baseLabel}, {node.Description}";
        }
        return baseLabel;
    }

    public string GetNodeIcon(string type) => type switch
    {
        "IfCondition" => "?",
        "ElseBranch" => "↩",
        "Assignment" => "=",
        _ => "○"
    };

    public string GetNodeClass(string type) => type switch
    {
        "IfCondition" => "if",
        "ElseBranch" => "else",
        "Assignment" => "assignment",
        _ => "none"
    };

    public string GetNodeTypeName(string type) => type switch
    {
        "IfCondition" => "IF",
        "ElseBranch" => "ELSE",
        "Assignment" => "ASSIGN",
        "None" => "NONE",
        _ => "NONE"
    };

    public bool HasAssignmentError(TreeNode node)
    {
        return node.Type == "Assignment" && node.Children.Count > 0;
    }

    #endregion
}

// TreeNode 資料模型已移至獨立檔案 TreeNode.cs
