# DecisionTreeEditor 決策樹編輯器

## 專案概述

DecisionTreeEditor 是一個互動式決策樹編輯器，從單一 HTML 檔案改寫為標準的 Blazor 元件架構。此元件提供視覺化的決策樹建立、編輯、驗證與匯出功能。

**改寫日期**: 2025-12-26
**原始檔案**: `../html_template/decision-tree-editor.html` (1415 行)
**目標框架**: .NET 8 Blazor Server-Side Rendering

---

## 檔案結構

### 目錄組織

```
DEMO212/
├── _DEMO212.razor                    # 路由定義頁面
├── DecisionTreeView.razor            # 頁面入口元件
├── html_template/                    # 原始 HTML 範本（保留參考）
└── widgets/                          # 決策樹編輯器模組
    ├── DecisionTreeEditor.razor          # 主元件 UI
    ├── DecisionTreeEditor.razor.cs       # 業務邏輯
    ├── DecisionTreeEditor.razor.css      # 主元件樣式
    ├── DecisionTreeEditor.razor.js       # JavaScript Interop
    ├── TreeNode.cs                       # 決策樹節點資料模型
    ├── TreeNodeWidget.razor              # 樹節點子元件
    ├── TreeNodeWidget.razor.css          # 節點樣式
    └── README.md                         # 本文件
```

### 檔案職責

| 檔案 | 職責 |
|------|------|
| **DecisionTreeView.razor** | 頁面入口，引用 widgets 命名空間 |
| **DecisionTreeEditor.razor** | 雙面板 UI、工具列、表單渲染 |
| **DecisionTreeEditor.razor.cs** | 業務邏輯、CRUD、驗證、拖拉、匯入匯出 |
| **DecisionTreeEditor.razor.css** | 容器佈局、工具列、表單、驗證訊息樣式 |
| **DecisionTreeEditor.razor.js** | 拖拉事件處理、檔案下載、剪貼簿操作 |
| **TreeNode.cs** | 決策樹節點資料模型（包含 JSON 序列化） |
| **TreeNodeWidget.razor** | 遞迴渲染樹狀節點、展開/收合 |
| **TreeNodeWidget.razor.css** | 節點結構、拖拉狀態、圖示、標籤樣式 |

**命名空間**: `AsvtTPL.Components.Pages.DEMO2.DEMO212.widgets`

---

## 核心架構

### 1. 資料模型 (TreeNode)

**檔案位置**: `TreeNode.cs`

```csharp
public class TreeNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; }                  // 唯一識別碼 (時間戳+GUID)

    [JsonPropertyName("type")]
    public string Type { get; set; }                // "None" | "IfCondition" | "ElseBranch" | "Assignment"

    [JsonPropertyName("description")]
    public string Description { get; set; }         // 節點說明

    // IF 條件欄位
    [JsonPropertyName("condField")]
    public string? CondField { get; set; }          // 比較欄位 (如: age, score)

    [JsonPropertyName("condOp")]
    public string? CondOp { get; set; }             // 運算子: =, !=, >, <, >=, <=, in

    [JsonPropertyName("condValue")]
    public string? CondValue { get; set; }          // 比較值

    // Assignment 欄位
    [JsonPropertyName("assignment")]
    public string Assignment { get; set; }          // 指定值 (終止節點)

    // 樹狀結構
    [JsonPropertyName("children")]
    public List<TreeNode> Children { get; set; }    // 子節點集合

    [JsonIgnore]
    public bool Collapsed { get; set; }             // UI 狀態：是否收合
}
```

**ID 生成規則**: `node_{timestamp}_{guid9位}`
**範例**: `node_1766724433853_b636d5773`

### 2. 元件架構

#### 主元件 (DecisionTreeEditor)

**職責劃分**:
- **狀態管理**: Root 根節點、SelectedNode 選中節點、拖拉狀態
- **業務邏輯**: CRUD 操作、驗證、匯入匯出、拖拉排序
- **JavaScript Interop**: 初始化拖拉事件、檔案操作、通知系統

**關鍵屬性**:
```csharp
private TreeNode Root { get; set; }              // 根節點（Type="root"）
private TreeNode? SelectedNode { get; set; }     // 當前選中的節點
private string? _draggedNodeId { get; set; }     // 拖拉中的節點 ID
private string? _draggedNodeParentId { get; set; } // 拖拉節點的父節點 ID
```

#### 子元件 (TreeNodeWidget)

**遞迴結構**: 子元件透過自我引用實現無限層級樹狀顯示

**參數傳遞** (9 個必要參數):
```csharp
[Parameter] public TreeNode Node { get; set; }                           // 當前節點
[Parameter] public TreeNode Parent { get; set; }                         // 父節點
[Parameter] public TreeNode? SelectedNode { get; set; }                  // 選中節點（高亮顯示用）
[Parameter] public Action<TreeNode> OnSelectNode { get; set; }           // 選中回調
[Parameter] public Action<string> OnToggleNode { get; set; }             // 展開/收合回調
[Parameter] public Func<TreeNode, string> GetNodeLabel { get; set; }     // 節點標籤邏輯
[Parameter] public Func<string, string> GetNodeIcon { get; set; }        // 圖示字元
[Parameter] public Func<string, string> GetNodeClass { get; set; }       // CSS class 映射
[Parameter] public Func<string, string> GetNodeTypeName { get; set; }    // 類型顯示名稱
[Parameter] public Func<TreeNode, bool> HasAssignmentError { get; set; } // 錯誤檢查
```

### 3. JavaScript Interop

**模組載入** (`OnAfterRenderAsync`):
```csharp
_jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
    "import", "./Components/Pages/DEMO2/DEMO212/widgets/DecisionTreeEditor.razor.js");

_dotNetRef = DotNetObjectReference.Create(this);
await _jsModule.InvokeVoidAsync("initializeDragDrop", _dotNetRef);
```

**C# → JS 呼叫**:
- `triggerFileInput(elementId)`: 觸發檔案選擇對話框
- `downloadFile(filename, content)`: 下載 JSON/文字檔案
- `copyToClipboard(text)`: 複製到剪貼簿
- `showNotification(message, type)`: 顯示通知訊息

**JS → C# 回調** (`[JSInvokable]`):
- `OnDragStart(nodeId, parentId)`: 拖拉開始，記錄來源節點
- `OnDrop(targetNodeId, dropPosition)`: 拖拉放下，執行節點移動

---

## 功能說明

### 1. 節點管理 (CRUD Operations)

#### 新增節點
```csharp
void AddRootNode()              // 新增根節點（掛在 Root 下）
void AddChildNode(TreeNode)     // 新增子節點（掛在當前節點下）
void AddSiblingNode(TreeNode)   // 新增兄弟節點（插入在當前節點後）
```

**預設節點**: Type = "None"，自動選中新建節點

#### 刪除節點
```csharp
void DeleteNode(TreeNode)       // 遞迴刪除節點及所有子節點
void ClearTree()                // 清空整棵樹（重設 Root）
```

**保護機制**: 根節點無法被刪除

#### 查詢節點
```csharp
TreeNode? FindNodeById(TreeNode, string id)      // 遞迴搜尋節點
TreeNode? FindParentNode(TreeNode, string childId) // 尋找父節點
```

#### 節點選中與展開/收合
```csharp
void SelectNode(TreeNode)       // 選中節點，觸發右側編輯區更新
void ToggleNode(string nodeId)  // 切換節點展開/收合狀態
```

### 2. 拖拉排序 (Drag & Drop)

#### 拖拉模式

**above 模式**: 作為兄弟節點插入（掛在目標節點的同一父節點下）
```
Parent
├── Target Node       ← 拖拉到這裡的上半部
├── [Dragged Node]    ← 插入在這裡
└── ...
```

**below 模式**: 作為第一個子節點插入（掛在目標節點下）
```
Target Node           ← 拖拉到這裡的下半部
├── [Dragged Node]    ← 插入為第一個子節點
└── Original Children
```

#### 安全性檢查

```csharp
bool IsDescendant(TreeNode ancestor, TreeNode node)
```

**防止循環依賴**: 不允許將節點移動到其自身的子節點下

#### JavaScript 拖拉事件

| 事件 | 處理函式 | 職責 |
|------|---------|------|
| `dragstart` | `handleDragStart` | 記錄拖拉節點資訊、呼叫 C# `OnDragStart` |
| `dragover` | `handleDragOver` | 判定 above/below、顯示視覺指示 |
| `dragenter` | `handleDragEnter` | 添加 `drag-over` CSS class |
| `dragleave` | `handleDragLeave` | 移除視覺指示 |
| `drop` | `handleDrop` | 呼叫 C# `OnDrop`、執行節點移動 |
| `dragend` | `handleDragEnd` | 清理 CSS class、重置狀態 |

**視覺回饋 CSS**:
- `.drag-over`: 拖拉經過時背景變色 (#d5f4e6)
- `.drop-above::before`: 綠色插入線（上方）
- `.drop-below::after`: 藍色插入線（下方）

### 3. 驗證機制

#### 驗證規則

**錯誤 (Errors)**:
- Assignment 節點存在子節點（違反終止點規則）

**警告 (Warnings)**:
- IfCondition 未填寫完整條件（CondField、CondOp、CondValue 任一為空）
- 樹狀結構為空（Root.Children.Count == 0）

#### 驗證流程

```csharp
void ValidateTree()             // 遞迴驗證所有節點
void ValidateNode(TreeNode)     // 單一節點驗證
```

**UI 顯示**:
- 驗證訊息顯示在右側編輯區
- 錯誤節點標記 `.dt-badge-error` + 閃爍動畫 (pulse)

### 4. 匯入匯出

#### 匯出 JSON

```csharp
async Task ExportToJSON()
```

**格式**: 標準 JSON，使用 `System.Text.Json` 序列化
```json
{
  "id": "node_1766724433853_d45dea104",
  "type": "IfCondition",
  "description": "這是一個範例條件。",
  "condField": "A",
  "condOp": "=",
  "condValue": "3",
  "children": [
    {
      "id": "node_1766724433853_b636d5773",
      "type": "Assignment",
      "assignment": "'3A'",
      "children": []
    }
  ]
}
```

**檔名**: `decision-tree-{timestamp}.json`

#### 匯出文字規則

```csharp
async Task ExportToText()
```

**格式**: 樹狀文字表示，使用符號顯示結構
```
▼ ? IF A = 3, 這是一個範例條件。 IF
  = '3A' ASSIGN
▼ ? IF B > 4 IF
  = 'Bar' ASSIGN
▼ ↩ ELSE ELSE
  = 'otherwise' ASSIGN
```

**符號說明**:
- `▼`: 有子節點且已展開
- `▶`: 有子節點但已收合
- `?`: IF 條件節點
- `↩`: ELSE 分支節點
- `=`: Assignment 節點

**檔名**: `decision-tree-rules-{timestamp}.txt`

#### 匯入 JSON

```csharp
async Task ImportFromJSON(InputFileChangeEventArgs e)
```

**流程**:
1. 讀取 JSON 檔案內容（限制 5MB）
2. 使用 `JsonSerializer.Deserialize<TreeNode>` 解析
3. 重設 Root.Children 為匯入的節點
4. 清空 SelectedNode 選中狀態

**支援格式**: 單一節點或節點陣列

---

## 技術細節

### 1. CSS 命名規範

**前綴**: 所有 class 統一使用 `dt-` (decision-tree) 前綴

**主要 CSS class**:

| Class | 作用範圍 | 檔案 |
|-------|---------|------|
| `.dt-container` | 主容器（flex 雙面板） | DecisionTreeEditor.razor.css |
| `.dt-tree-panel` | 左側樹狀面板 | DecisionTreeEditor.razor.css |
| `.dt-edit-panel` | 右側編輯面板 | DecisionTreeEditor.razor.css |
| `.dt-toolbar` | 工具列容器 | DecisionTreeEditor.razor.css |
| `.dt-btn` | 按鈕基底樣式 | DecisionTreeEditor.razor.css |
| `.dt-form-group` | 表單群組 | DecisionTreeEditor.razor.css |
| `.dt-tree-node` | 節點容器 | TreeNodeWidget.razor.css |
| `.dt-node-content` | 節點內容區（可點選） | TreeNodeWidget.razor.css |
| `.dt-node-icon-{type}` | 節點圖示 (if/else/assignment/none) | TreeNodeWidget.razor.css |
| `.dt-badge-{type}` | 類型標籤 | TreeNodeWidget.razor.css |
| `.dt-validation-{type}` | 驗證訊息 (error/warning/success) | DecisionTreeEditor.razor.css |

### 2. CSS Isolation 架構

**主元件樣式** (DecisionTreeEditor.razor.css):
- 佈局容器、工具列、按鈕
- 表單元素、輸入控制項
- 驗證訊息區塊、輸出區域
- `@keyframes pulse` 動畫（全域）

**子元件樣式** (TreeNodeWidget.razor.css):
- 節點結構 (.dt-tree-node, .dt-node-content, .dt-node-toggle)
- 拖拉狀態 (.drag-over, .drop-above, .drop-below)
- 節點圖示 (.dt-node-icon-if/else/assignment/none)
- 類型標籤 (.dt-badge-*)
- 子節點容器 (.dt-node-children)

**關鍵技術**: 子元件 CSS 不需使用 `::deep` 前綴，直接作用於自己的 DOM 元素

### 3. 節點類型與視覺設計

| Type | 中文名稱 | 圖示顏色 | 圖示符號 | 可有子節點 |
|------|---------|---------|---------|-----------|
| **None** | 未指定 | 灰色 (#95a5a6) | ⊙ | ✓ |
| **IfCondition** | 條件判斷 | 藍色 (#3498db) | ? | ✓ |
| **ElseBranch** | 其它分支 | 橘色 (#e67e22) | ↩ | ✓ |
| **Assignment** | 指定數值 | 綠色 (#27ae60) | = | ✗ (終止點) |

**圖示實作**:
```csharp
public string GetNodeIcon(string type) => type switch
{
    "IfCondition" => "?",
    "ElseBranch" => "↩",
    "Assignment" => "=",
    _ => "⊙"
};

public string GetNodeClass(string type) => type switch
{
    "IfCondition" => "if",
    "ElseBranch" => "else",
    "Assignment" => "assignment",
    _ => "none"
};
```

### 4. 元件通訊模式

**父 → 子**:
- 透過 `[Parameter]` 傳遞資料與回調函式
- TreeNodeWidget 接收 9 個參數

**子 → 父**:
- 透過 `Action` 和 `Func` 回調
- `OnSelectNode(TreeNode)`: 節點被點選
- `OnToggleNode(string)`: 展開/收合切換

**JavaScript ↔ C#**:
- `IJSRuntime.InvokeAsync`: C# 呼叫 JS
- `[JSInvokable]`: JS 呼叫 C# 標記

---

## 公開 API

DecisionTreeEditor 提供以下公開方法供父元件控制樹實例：

### ExportTree()

```csharp
public TreeNode ExportTree()
```

**功能**: 匯出整顆決策樹的深度複製副本

**回傳值**: `TreeNode` - 樹結構的深度複製物件

**使用場景**:
- 父元件需要保存當前編輯狀態
- 實作復原/重做功能
- 將樹結構傳遞給其他元件處理

**注意事項**:
- 回傳的是深度複製，修改不會影響元件內部狀態
- `Collapsed` 屬性不會被複製（有 `[JsonIgnore]`）

---

### ImportTree(TreeNode tree)

```csharp
public void ImportTree(TreeNode tree)
```

**功能**: 匯入整顆決策樹結構，覆蓋當前編輯內容

**參數**:
- `tree` - 要匯入的樹結構

**行為**:
1. 驗證參數有效性（null 檢查、必要欄位檢查）
2. 深度複製輸入的樹結構
3. 覆蓋 `Root` 狀態
4. 清空 `SelectedNode`
5. 觸發 UI 重新渲染

**異常**:
- `ArgumentNullException` - tree 為 null
- `ArgumentException` - tree 結構缺少必要欄位

**使用場景**:
- 從外部來源載入樹結構
- 復原先前保存的狀態
- 實作多個樹實例之間的切換

---

### LoadSampleData()

```csharp
public void LoadSampleData()
```

**功能**: 載入預設的範例決策樹資料

**行為**:
- 建立預定義的範例樹結構
- 覆蓋當前 `Root`
- 觸發 UI 重新渲染

**使用場景**:
- 父元件需要提供「載入範例」功能
- 初始化元件時載入預設資料
- 重置為範例狀態

**注意**:
- 元件不會在 `OnInitialized` 自動載入範例資料
- 父元件需主動呼叫此方法或使用 `ImportTree` 進行初始化

---

### 父元件使用範例

```razor
@page "/demo212/decision-tree-editor"
@using AsvtTPL.Components.Pages.DEMO2.DEMO212.widgets

<h1>決策樹管理</h1>

<div>
    <button @onclick="LoadSample">載入範例</button>
    <button @onclick="SaveTree">保存樹</button>
    <button @onclick="RestoreTree">復原樹</button>
</div>

<DecisionTreeEditor @ref="_editor" />

@code {
    private DecisionTreeEditor? _editor;
    private TreeNode? _savedTree;

    private void LoadSample()
    {
        _editor?.LoadSampleData();
    }

    private void SaveTree()
    {
        if (_editor != null)
        {
            _savedTree = _editor.ExportTree();
            // 可選：序列化到檔案或資料庫
        }
    }

    private void RestoreTree()
    {
        if (_editor != null && _savedTree != null)
        {
            _editor.ImportTree(_savedTree);
        }
    }
}
```

---

## 使用說明

### 基本操作流程

1. **新增節點**
   - 點擊「新增根節點」建立第一層節點
   - 選中節點後點擊「新增子節點」或「新增兄弟節點」

2. **編輯節點**
   - 左側樹狀圖點選節點
   - 右側編輯區顯示節點詳細資訊
   - 修改類型、說明、條件或指定值

3. **設定條件** (Type = IfCondition)
   - 比較欄位：變數名稱 (如: `age`, `score`)
   - 比較運算子：`=`, `!=`, `>`, `<`, `>=`, `<=`, `in`
   - 比較數值：數值或字串 (如: `18`, `'Admin'`)

4. **設定指定值** (Type = Assignment)
   - 輸入終止節點的返回值
   - 範例：`'3A'`, `'Bar'`, `result`

5. **拖拉排序**
   - 拖拉節點到目標節點**上半部** → 作為兄弟節點插入
   - 拖拉節點到目標節點**下半部** → 作為第一個子節點插入

6. **驗證與匯出**
   - 點擊「驗證結構」檢查樹狀結構合法性
   - 點擊「匯出 JSON」下載結構化資料
   - 點擊「匯出規則」生成文字格式規則
   - 點擊「匯入 JSON」從檔案還原樹狀結構

### 快捷操作

- **展開/收合**: 點擊節點前的 ▼/▶ 符號
- **複製節點 JSON**: 右側編輯區底部「複製節點 JSON」按鈕
- **切換顯示格式**: 節點資訊區可選擇「基本資訊」或「JSON 格式」

---

## 測試檢查清單

### 節點操作
- [ ] 新增根節點
- [ ] 新增子節點
- [ ] 新增兄弟節點
- [ ] 刪除節點（含子節點遞迴刪除）
- [ ] 清空樹狀結構
- [ ] 選中節點（右側編輯區更新）
- [ ] 展開/收合節點

### 節點編輯
- [ ] 修改節點類型 (None/IF/ELSE/Assignment)
- [ ] 編輯節點說明
- [ ] 設定 IF 條件（欄位、運算子、數值）
- [ ] 設定 Assignment 指定值
- [ ] 切換節點資訊顯示格式（文字/JSON）

### 拖拉排序
- [ ] 拖拉節點作為兄弟節點（above 模式）
- [ ] 拖拉節點作為子節點（below 模式）
- [ ] 拖拉視覺回饋（綠線/藍線/背景色）
- [ ] 循環依賴檢查（無法拖到子節點下）

### 驗證機制
- [ ] Assignment 子節點錯誤檢測
- [ ] IF 條件未填寫警告
- [ ] 空樹警告
- [ ] 錯誤標籤閃爍動畫

### 匯入匯出
- [ ] 匯出 JSON 檔案
- [ ] 匯出文字規則檔案
- [ ] 匯入 JSON 檔案還原結構
- [ ] 複製節點 JSON 到剪貼簿

### CSS 與樣式
- [ ] 節點圖示顏色正確（IF藍/ELSE橘/ASSIGN綠/NONE灰）
- [ ] 節點 hover 效果
- [ ] 節點選中高亮顯示
- [ ] 拖拉狀態樣式 (.drag-over, .drop-above, .drop-below)
- [ ] CSS Isolation 作用域正確
- [ ] @keyframes pulse 動畫跨檔案引用

### JavaScript Interop
- [ ] 拖拉事件處理正常
- [ ] 檔案下載功能
- [ ] 剪貼簿複製功能
- [ ] 通知訊息顯示

---

## 重構歷史

### 2025-12-26 公開 API 新增

**目標**: 提供父元件控制樹實例的能力

**新增內容**:
- `ExportTree()`: 匯出樹結構的深度複製
- `ImportTree(TreeNode tree)`: 匯入樹結構並覆蓋當前資料
- `LoadSampleData()`: 從私有改為公開

**修改內容**:
- `OnInitialized()`: 移除自動載入範例資料

**深度複製實作**:
- 使用 JSON 序列化/反序列化實現
- 確保狀態獨立，避免引用共享問題

**檔案變更**:
- DecisionTreeEditor.razor.cs: 605 行 → 657 行 (+52 行)

### 2025-12-26 資料模型抽離
- 將 TreeNode 類別從 DecisionTreeEditor.razor.cs 抽出為獨立檔案 `TreeNode.cs`
- 提升程式碼可維護性與重用性
- DecisionTreeEditor.razor.cs 從 670 行減少至 605 行

### 2025-12-26 模組化重構
- 將所有元件檔案移至 `widgets/` 子目錄
- 建立 `DecisionTreeView.razor` 作為頁面入口
- 更新命名空間為 `AsvtTPL.Components.Pages.DEMO2.DEMO212.widgets`
- TreeNodeView.razor → TreeNodeWidget.razor 重新命名
- 建立獨立的 TreeNodeWidget.razor.css（移除 21 個 `::deep` 規則）

### 2025-12-26 HTML → Blazor 改寫
- 從 `html_template/decision-tree-editor.html` (1415 行) 改寫
- 拆分為 6 個檔案：.razor, .razor.cs, .razor.css, .razor.js + 子元件
- 解決 CSS Isolation、InputFile、拖拉功能問題
- 統一 CSS 命名規範（`dt-` 前綴）

---

**最後更新**: 2025-12-26
**狀態**: ✅ 資料模型抽離完成，功能驗證通過
**相容性**: .NET 8 Blazor Server, Chrome/Edge/Safari
**命名空間**: `AsvtTPL.Components.Pages.DEMO2.DEMO212.widgets`
**檔案總數**: 7 個程式碼檔案
**總行數**: 1813 行（不含 README.md）
