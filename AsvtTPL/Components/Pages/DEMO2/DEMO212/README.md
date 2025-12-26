# DecisionTreeEditor 決策樹編輯器

## 專案概述

DecisionTreeEditor 是一個互動式決策樹編輯器，從單一 HTML 檔案改寫為標準的 Blazor 元件架構。此元件提供視覺化的決策樹建立、編輯、驗證與匯出功能。

**改寫日期**: 2025-12-26
**原始檔案**: `html_template/decision-tree-editor.html` (1415 行)
**目標框架**: .NET 8 Blazor Server-Side Rendering

---

## 檔案結構

改寫後的元件分為 5 個檔案，遵循 Blazor 元件標準組織方式：

```
DEMO212/
├── DecisionTreeEditor.razor          # 主元件標記 (237 行)
├── DecisionTreeEditor.razor.cs       # C# 邏輯與狀態管理 (580 行)
├── DecisionTreeEditor.razor.css      # 主元件樣式 (257 行)
├── DecisionTreeEditor.razor.js       # JavaScript Interop (309 行)
├── TreeNodeWidget.razor              # 遞迴子元件 (96 行)
├── TreeNodeWidget.razor.css          # 子元件樣式 (190 行)
└── README.md                         # 本設計說明文件
```

### 檔案職責說明

| 檔案 | 職責 | 關鍵功能 |
|------|------|---------|
| **DecisionTreeEditor.razor** | UI 標記與版面配置 | 雙面板佈局、工具列、表單渲染 |
| **DecisionTreeEditor.razor.cs** | 狀態管理與業務邏輯 | TreeNode 模型、CRUD 操作、驗證、匯入匯出 |
| **DecisionTreeEditor.razor.css** | 主元件樣式 | 佈局、工具列、表單、驗證、輸出區 |
| **DecisionTreeEditor.razor.js** | 瀏覽器互動功能 | 拖拉排序、檔案下載、剪貼簿操作 |
| **TreeNodeWidget.razor** | 樹狀節點遞迴渲染 | 子節點視覺化、展開/收合 |
| **TreeNodeWidget.razor.css** | 節點樣式隔離 | 節點結構、拖拉狀態、圖示、標籤 |

---

## 技術架構

### 資料模型

```csharp
public class TreeNode
{
    public string Id { get; set; }              // 唯一識別碼
    public string Type { get; set; }            // None | IfCondition | ElseBranch | Assignment
    public string Description { get; set; }     // 節點說明

    // 條件屬性 (Type = IfCondition)
    public string CondField { get; set; }       // 比較欄位
    public string CondOp { get; set; }          // 比較運算子 (=, !=, >, <, >=, <=)
    public string CondValue { get; set; }       // 比較數值

    // 指定值 (Type = Assignment)
    public string Assignment { get; set; }

    // 樹狀結構
    public List<TreeNode> Children { get; set; }
    public bool Collapsed { get; set; }         // UI 狀態：是否收合
}
```

### 核心功能模組

#### 1. 節點管理 (CRUD)
- **AddRootNode()**: 新增根節點
- **AddChildNode()**: 新增子節點（掛在當前節點下方）
- **AddSiblingNode()**: 新增兄弟節點（掛在同一父節點下）
- **DeleteNode()**: 刪除節點及所有子節點
- **FindNodeById()**: 遞迴搜尋節點
- **FindParentNode()**: 找出父節點

#### 2. 驗證機制
- **ValidateTree()**: 完整樹狀結構驗證
- **ValidateNode()**: 單一節點驗證
- **關鍵規則**:
  - Assignment 節點不得有子節點（終止點規則）
  - IfCondition 必須填寫完整條件（欄位、運算子、數值）

#### 3. 拖拉排序 (Drag & Drop)
- **JavaScript 事件處理**: dragstart, dragover, drop, dragend
- **C# 回調**: OnDragStart(), OnDrop()
- **兩種放置模式**:
  - **above**: 作為兄弟節點插入（掛在同一父節點）
  - **below**: 作為第一個子節點插入

#### 4. 匯入匯出
- **ExportToJSON()**: 序列化整棵樹為 JSON 檔案
- **ExportToText()**: 生成類似程式碼的文字規則
- **ImportFromJSON()**: 從 JSON 檔案還原樹狀結構
- **CopyNodeJSON()**: 複製單一節點 JSON 到剪貼簿

---

## 改寫過程關鍵問題與解決方案

### 問題 1: InputFile 元件無 Click() 方法

**問題描述**:
Blazor 的 `InputFile` 元件沒有 `Click()` 方法，無法程式化觸發檔案選擇對話框。

**解決方案**:
```csharp
// C# 端
private async Task TriggerFileInput()
{
    await _jsModule.InvokeVoidAsync("triggerFileInput", "fileInput");
}

// JavaScript 端 (DecisionTreeEditor.razor.js)
export function triggerFileInput(elementId) {
    const fileInput = document.getElementById(elementId);
    if (fileInput) {
        fileInput.click();
    }
}
```

**檔案位置**: DecisionTreeEditor.razor.cs:99, DecisionTreeEditor.razor.js:204

---

### 問題 2: CSS 類別名稱衝突

**問題描述**:
原始 HTML 使用 Bootstrap 風格的 class 名稱（`.container`, `.btn`, `.form-group` 等），但專案已移除 Bootstrap CSS，導致樣式完全失效。

**解決方案**:
所有 CSS class 統一加上 `dt-` (decision-tree) 前綴，確保命名唯一性。

**重新命名對照表**:

| 原始 class | 新 class | 說明 |
|-----------|---------|------|
| `.container` | `.dt-container` | 主容器 |
| `.btn` | `.dt-btn` | 按鈕基底樣式 |
| `.btn-primary` | `.dt-btn-primary` | 主要按鈕 |
| `.form-group` | `.dt-form-group` | 表單群組 |
| `.validation-message` | `.dt-validation-message` | 驗證訊息 |
| `.node-content` | `.dt-node-content` | 節點內容區 |
| `.node-icon` | `.dt-node-icon-{type}` | 節點圖示（重大修改） |

**影響範圍**:
- DecisionTreeEditor.razor.css: 約 80 個 class 選擇器
- DecisionTreeEditor.razor: 約 30 處 HTML class 屬性
- TreeNodeView.razor: 約 10 處 HTML class 屬性
- DecisionTreeEditor.razor.js: 約 6 處 querySelector

---

### 問題 3: Blazor CSS Isolation 導致子元件樣式失效

**問題描述**:
節點圖示（彩色方塊）完全不顯示，檢查 DOM 發現 HTML class 正確（`dt-node-icon-if`），但 CSS 樣式沒有套用。

**根本原因**:
Blazor CSS Isolation 機制會限制 `.razor.css` 檔案中的樣式只作用於該元件的直接 DOM 元素。由於節點圖示渲染在子元件 `TreeNodeView.razor` 中，父元件的樣式無法穿透。

**解決方案**:
在 `DecisionTreeEditor.razor.css` 中對所有需要作用到子元件的樣式使用 `::deep` 選擇器。

**修改前**:
```css
.dt-node-icon-if {
    background: #3498db;
    color: white;
}
```

**修改後**:
```css
::deep .dt-node-icon-if {
    background: #3498db;
    color: white;
}
```

**受影響的 CSS 規則**:
所有 `.dt-node-*`, `.dt-badge-*` 相關樣式（約 30 個選擇器）

**檔案位置**: DecisionTreeEditor.razor.css:83-272

---

### 問題 4: 節點圖示 CSS 架構調整

**問題描述**:
原始設計使用複合 class 選擇器（`.node-icon.if`），在 Blazor CSS Isolation 環境下不穩定。

**架構變更**:

**舊架構（HTML + CSS）**:
```html
<!-- TreeNodeView.razor -->
<span class="node-icon @GetNodeClass(Node.Type)">?</span>

<!-- CSS -->
.node-icon.if { background: #3498db; }
```

**新架構（HTML + CSS）**:
```html
<!-- TreeNodeView.razor -->
<span class="dt-node-icon-@GetNodeClass(Node.Type)">?</span>

<!-- CSS -->
::deep .dt-node-icon-if { background: #3498db; }
::deep .dt-node-icon-else { background: #e67e22; }
::deep .dt-node-icon-assignment { background: #27ae60; }
::deep .dt-node-icon-none { background: #95a5a6; }
```

**C# 方法不變**:
```csharp
public string GetNodeClass(string type) => type switch
{
    "IfCondition" => "if",
    "ElseBranch" => "else",
    "Assignment" => "assignment",
    _ => "none"
};
```

**優點**: 單一 class 選擇器更穩定，避免 CSS specificity 問題。

**檔案位置**: TreeNodeView.razor:20, DecisionTreeEditor.razor.css:163-213

---

### 問題 5: JavaScript 拖拉功能失效

**問題描述**:
CSS class 重新命名後，JavaScript 中的 `querySelector()` 仍使用舊 class 名稱，導致拖拉功能完全無法運作。

**解決方案**:
同步修改 JavaScript 中所有 class 選擇器。

**修改清單**:
```javascript
// 修改前 → 修改後
'.node-content'      → '.dt-node-content'
'.node-label'        → '.dt-node-label'
'.drag-hint'         → '.dt-drag-hint'
'validation-message' → 'dt-validation-${type}'
```

**影響的函式**:
- handleDragStart() (L31, L47)
- handleDragOver() (L57)
- handleDragEnter() (L79)
- handleDragLeave() (L89)
- handleDrop() (L103)
- showDragHint() (L170)
- showNotification() (L239)

**檔案位置**: DecisionTreeEditor.razor.js

---

## 關鍵設計決策

### 1. 元件分離策略

**為何將 TreeNodeWidget 獨立為子元件？**

- **遞迴渲染需求**: 決策樹本質是遞迴結構，子元件可以自我引用
- **職責分離**: 主元件負責整體狀態管理，子元件專注於視覺渲染
- **效能考量**: 降低重複渲染範圍，只更新變更的子樹

**替代方案（未採用）**:
使用 RenderFragment 遞迴渲染，但可讀性較差且難以維護。

### 2. CSS 命名規範

**為何選擇 `dt-` 前綴？**

- **dt = Decision Tree**: 語意明確
- **避免全域污染**: 確保不與 MudBlazor、專案既有 CSS 衝突
- **一致性**: 所有 class 統一前綴，易於搜尋與維護

### 3. 拖拉實作方式

**為何不使用 Blazor 套件（如 Blazor.DragDrop）？**

- **輕量化原則**: 原始 HTML 已有完整實作，直接移植即可
- **客製化需求**: 需要精確控制拖拉位置（above/below 判定）
- **學習曲線**: 團隊熟悉原生 Drag & Drop API

### 4. 狀態管理

**為何使用元件內部狀態而非集中式狀態管理？**

- **獨立元件**: 此元件不需與其他頁面共享狀態
- **簡化架構**: 避免過度工程（符合專案 CLAUDE.md 的 MVP 原則）
- **序列化支援**: 所有狀態可直接匯出為 JSON

---

## 使用說明

### 基本操作

1. **新增節點**
   - 點擊「新增根節點」建立第一個節點
   - 選擇節點後點擊「新增子節點」或「新增兄弟節點」

2. **編輯節點**
   - 在左側樹狀圖點擊節點
   - 右側編輯器會顯示節點詳細資訊
   - 修改類型、說明、條件或指定值

3. **拖拉排序**
   - 拖拉節點到目標節點上方 → 插入為兄弟節點
   - 拖拉節點到目標節點下方 → 插入為第一個子節點

4. **驗證與匯出**
   - 點擊「驗證結構」檢查樹狀結構是否合法
   - 點擊「匯出 JSON」下載完整資料
   - 點擊「匯出規則」生成文字格式規則

### 節點類型說明

| 類型 | 圖示顏色 | 說明 | 可有子節點 |
|------|---------|------|-----------|
| **NONE** | 灰色 | 未指定類型 | ✓ |
| **IF** | 藍色 | 條件判斷 | ✓ |
| **ELSE** | 橘色 | 其它分支 | ✓ |
| **Assignment** | 綠色 | 指定數值（終止點） | ✗ |

### 驗證規則

- **錯誤**: Assignment 節點存在子節點
- **警告**: IfCondition 未填寫完整條件
- **警告**: 樹狀結構為空

---

## 技術參考

### JavaScript Interop 註冊

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _jsModule = await JS.InvokeAsync<IJSObjectReference>(
            "import",
            "./Components/Pages/DEMO2/DEMO212/DecisionTreeEditor.razor.js"
        );

        _dotNetRef = DotNetObjectReference.Create(this);
        await _jsModule.InvokeVoidAsync("initializeDragDrop", _dotNetRef);
    }
}
```

### JSON 匯出範例

```json
{
  "id": "node_1766719402741_a13aad3c3",
  "type": "IfCondition",
  "description": "這是一個範例條件。",
  "condField": "A",
  "condOp": "=",
  "condValue": "3",
  "children": [
    {
      "id": "node_1766719402741_2a9b8c4d5",
      "type": "Assignment",
      "assignment": "'3A'",
      "children": []
    }
  ]
}
```

### 文字規則匯出範例

```
▼ ? IF A = 3, 這是一個範例條件。 IF
  = '3A' ASSIGN
▼ ? IF B > 4 IF
  = 'Bar' ASSIGN
▼ ? IF A < 10 IF
  = '少數' ASSIGN
▼ ↩ ELSE ELSE
  = 'otherwise' ASSIGN
```

---

## CSS 架構重構說明

### 樣式分離原則

**2025-12-26 重構**：將子元件樣式從主 CSS 移至 TreeNodeWidget.razor.css

#### 移動的樣式（21 個規則）
- 節點結構：`.dt-tree-node`, `.dt-node-content`, `.dt-node-toggle`
- 拖拉狀態：`.drag-over`, `.drop-above::before`, `.drop-below::after`
- 節點圖示：`.dt-node-icon-if/else/assignment/none`
- 類型標籤：`.dt-badge-*`
- 子節點容器：`.dt-node-children`

#### 保留在主 CSS 的樣式
- 佈局容器、工具列、表單、驗證、輸出區
- **全域動畫**：`@keyframes pulse`

#### 關鍵技術點
- **移除 ::deep 前綴**：子元件 CSS 直接作用於自己的 DOM
- **拖拉 CSS 歸屬**：樣式作用於子元件 DOM，因此放在子元件 CSS
- **動畫跨檔案引用**：`.dt-badge-error` 引用主 CSS 的 `@keyframes pulse`

---

## 已知限制

1. **拖拉動畫**: 目前無平滑過渡動畫（原始 HTML 也無）
2. **復原/重做**: 不支援 Undo/Redo 功能
3. **多選操作**: 一次只能操作一個節點
4. **匯入驗證**: 匯入 JSON 時無完整格式驗證

---

## 維護注意事項

### 修改 CSS 樣式時
- 確保所有需作用於 TreeNodeView 的樣式使用 `::deep` 前綴
- 新增的 class 必須以 `dt-` 開頭

### 修改 JavaScript 時
- 所有 DOM 查詢必須使用 `dt-` 前綴的 class 名稱
- 記得同步更新 C# 端的 JSInterop 呼叫簽章

### 修改資料模型時
- 更新 TreeNode.ToJSON() 方法確保序列化正確
- 同步更新匯入 JSON 的解析邏輯
- 檢查驗證規則是否需要調整

---

## 測試檢查清單

- [ ] 新增根節點功能
- [ ] 新增子節點/兄弟節點功能
- [ ] 刪除節點功能
- [ ] 節點類型切換
- [ ] 條件編輯（欄位、運算子、數值）
- [ ] Assignment 編輯
- [ ] 拖拉排序（above 模式）
- [ ] 拖拉排序（below 模式）
- [ ] 展開/收合節點
- [ ] 驗證結構功能
- [ ] 匯出 JSON 功能
- [ ] 匯入 JSON 功能
- [ ] 匯出文字規則功能
- [ ] 複製節點 JSON 到剪貼簿
- [ ] 彩色節點圖示正確顯示
- [ ] Assignment 子節點錯誤警告顯示
- [ ] TreeNodeWidget.razor.css 樣式生效
  - [ ] 節點結構樣式（邊框、間距、hover 效果）
  - [ ] 拖拉狀態視覺回饋（drag-over, drop-above, drop-below）
  - [ ] 四種節點圖示顏色正確（藍/橘/綠/灰）
  - [ ] 類型標籤顏色正確
- [ ] CSS Isolation 無衝突
  - [ ] 主元件與子元件樣式各自獨立
  - [ ] @keyframes pulse 動畫跨檔案引用成功

---

**最後更新**: 2025-12-26
**狀態**: ✅ 初測通過
**相容性**: .NET 8 Blazor Server, Chrome/Edge/Safari
