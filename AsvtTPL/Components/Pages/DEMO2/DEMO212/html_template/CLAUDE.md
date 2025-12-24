# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## 專案概述

這是一個獨立的決策樹編輯器（Decision Tree Editor），使用單一 HTML 檔案實現，包含完整的 JavaScript 邏輯。專案提供視覺化的決策樹建立、編輯、驗證與匯入/匯出功能。

**核心特性：**
- 純前端實現（無需伺服器或建置工具）
- 拖拉式節點重組
- 即時視覺化回饋
- JSON 格式匯入/匯出
- 嚴格的結構驗證規則

---

## 如何使用

### 開啟編輯器
```bash
# 方法 1：直接在瀏覽器開啟
start decision-tree-editor.html  # Windows
open decision-tree-editor.html   # macOS

# 方法 2：透過本機伺服器（建議）
python -m http.server 8000       # Python 3
# 然後開啟 http://localhost:8000/decision-tree-editor.html
```

**注意：** 不需要任何建置或編譯步驟，這是一個自包含的 HTML 檔案。

### 測試範例檔案
- `sample-tree.json` - 基本結構（正確）
- `sample-tree-complex.json` - 複雜巢狀結構（正確）
- `sample-tree-with-errors.json` - 包含錯誤的範例（用於測試驗證功能）

---

## 核心架構

### 資料結構（TreeNode 類別）

```javascript
class TreeNode {
  id: string              // 唯一識別碼 (例如: 'node_1735012345_abc')
  type: string            // 節點類型: 'IfCondition' | 'ElseBranch' | 'Assignment' | 'None' | 'Root'
  condition: string       // IF 條件表達式（僅 IfCondition 使用）
  assignment: string      // 指定值（僅 Assignment 使用）
  children: TreeNode[]    // 子節點陣列
  collapsed: boolean      // UI 展開/收合狀態
}
```

### 節點類型說明

| 類型 | 用途 | 可否有子節點 | 特殊規則 |
|------|------|-------------|---------|
| `Root` | 樹根節點 | ✅ 是 | 唯一，ID 固定為 'root' |
| `IfCondition` | 條件判斷 | ✅ 是 | 必須有 condition 屬性 |
| `ElseBranch` | 其它分支 | ✅ 是 | 同層級只能有一個 |
| `Assignment` | 指定結果值 | ❌ **否** | **終止點規則** |
| `None` | 未設定類型 | ✅ 是 | 佔位符，建議設定為其他類型 |

---

## 關鍵設計規則：Assignment 終止點（葉節點）

這是整個決策樹的核心約束，有**兩個層面**的規則：

### 規則 1：Assignment 不能有子節點 ⭐

**原因：** Assignment 代表決策的最終結果（葉節點），是邏輯終止點。

**強制執行位置：**
- `decision-tree-editor.html:683-698` - `addChildNode()` 函數會阻止添加
- `decision-tree-editor.html:948-968` - 拖拉操作會被阻止
- `decision-tree-editor.html:1084-1092` - 驗證時會報錯

**UI 提示：**
- 編輯面板中「新增子節點」按鈕會被禁用
- 節點旁邊會顯示紅色閃爍的「⚠️ 錯誤」標記

### 規則 2：同層級唯一性 ⭐⭐ （v1.2 新增）

**規則：** 同一層級中，如果有 Assignment 節點，則不能有其他**任何類型**的節點（包括其他 Assignment）。

**錯誤範例：**
```
IF age < 18
  Assignment: result = 'Minor'     ← ❌
  IF needParent = true             ← ❌ 與 Assignment 同層級
```

**正確範例：**
```
IF age < 18
  IF needParent = true
    Assignment: guardian = required  ← ✓ 該層級只有這一個節點
  ELSE
    Assignment: guardian = optional  ← ✓ 該層級只有這一個節點
```

**強制執行位置：**
- `decision-tree-editor.html:686-692` - `addChildNode()` 檢查
- `decision-tree-editor.html:707-719` - `addSiblingNode()` 檢查
- `decision-tree-editor.html:816-854` - `updateNodeType()` 類型切換檢查
- `decision-tree-editor.html:1107-1115` - 驗證函數

---

## 主要函數對照表

### 節點操作
- `addRootNode()` (658) - 新增根層級節點
- `addChildNode(parent)` (680) - 新增子節點（含 Assignment 規則檢查）
- `addSiblingNode(current)` (700) - 新增兄弟節點（含同層級檢查）
- `deleteNode(node, parent)` (731) - 刪除節點及其子樹

### 渲染與 UI
- `renderTree()` (507) - 重新渲染整個樹狀視圖
- `renderNode(node, parent)` (521) - 遞迴渲染單一節點
- `renderEditPanel(node)` (742) - 渲染右側編輯面板

### 拖拉功能
- `handleDragStart(e, node, parent)` (908) - 開始拖拉
- `handleDrop(e, targetNode)` (941) - 放下節點（含 Assignment 規則檢查）

### 驗證與匯出
- `validateTree()` (1026) - 執行完整的結構驗證
- `validateNode(node, errors, warnings, path)` (1060) - 遞迴驗證節點
- `exportToJSON()` (1248) - 匯出為 JSON 檔案
- `importFromJSON(event)` (1266) - 從 JSON 檔案匯入
- `generateTextOutput(node, indent)` (1221) - 產生 IF/ELSE 文字規則

### 輔助函數
- `findNodeById(node, id)` (1006) - 以 ID 查找節點
- `findParentNode(parent, childId)` (1015) - 查找節點的父節點
- `showNotification(message, type)` (1323) - 顯示通知訊息

---

## 驗證規則層級

### 🔴 錯誤（必須修正）
1. Assignment 節點有子節點
2. **同層級有 Assignment 且有其他節點**（v1.2 新增）
3. IF 節點缺少條件
4. Assignment 節點缺少指定值
5. 同層級有多個 ELSE

### ⚠️ 警告（建議修正）
1. IF/ELSE 節點沒有子節點
2. ELSE 不在同層級最後
3. 節點類型為 None（未設定）

詳細說明請參閱 `VALIDATION-RULES.md`。

---

## 檔案結構

```
html_template/
├── decision-tree-editor.html          # 主編輯器（自包含）
├── JSON-IMPORT-EXPORT-GUIDE.md        # 匯入/匯出功能說明
├── VALIDATION-RULES.md                # 完整驗證規則文件
├── sample-tree.json                   # 簡單範例（正確結構）
├── sample-tree-complex.json           # 複雜範例（多層巢狀）
└── sample-tree-with-errors.json       # 錯誤範例（測試驗證）
```

---

## 開發注意事項

### 修改驗證規則時
1. 同時更新三個地方的邏輯：
   - `addChildNode()` / `addSiblingNode()` - UI 操作限制
   - `handleDrop()` - 拖拉操作限制
   - `validateNode()` - 驗證函數
2. 更新 `VALIDATION-RULES.md` 文件
3. 考慮是否需要更新 `updateNodeType()` 的類型切換檢查

### 修改節點類型時
1. 更新 `TreeNode` 類別的 constructor
2. 更新 `getNodeIcon()`, `getNodeClass()`, `getNodeTypeName()` 函數
3. 更新 CSS 中的對應樣式類別
4. 更新編輯面板的類型選單

### 測試新功能
```bash
# 建議測試流程
1. 開啟 decision-tree-editor.html
2. 匯入 sample-tree-with-errors.json
3. 執行「驗證結構」功能
4. 確認錯誤和警告訊息正確顯示
5. 測試拖拉功能是否正確阻止非法操作
```

---

## JSON 資料格式範例

```json
{
  "id": "root",
  "type": "Root",
  "condition": "",
  "assignment": "",
  "children": [
    {
      "id": "node_001",
      "type": "IfCondition",
      "condition": "age >= 18",
      "assignment": "",
      "children": [
        {
          "id": "node_002",
          "type": "Assignment",
          "condition": "",
          "assignment": "category = 'Adult'",
          "children": []  // ← Assignment 必須是空陣列
        }
      ]
    },
    {
      "id": "node_003",
      "type": "ElseBranch",
      "assignment": "",
      "children": [
        {
          "id": "node_004",
          "type": "Assignment",
          "assignment": "category = 'Minor'",
          "children": []  // ← Assignment 必須是空陣列
        }
      ]
    }
  ]
}
```

---

## 技術債與改進機會

1. **自動儲存** - 目前無本機儲存，關閉視窗會遺失資料
2. **撤銷/重做** - 無歷史記錄功能
3. **鍵盤快捷鍵** - 目前僅支援滑鼠操作
4. **即時驗證** - 當前需手動觸發驗證，可改為即時提示

---

## 版本歷史

### v1.2 (當前版本)
- ⭐⭐ 新增 Assignment 同層級唯一性規則
- 強化節點類型切換時的驗證
- 改進拖拉操作的限制檢查

### v1.1
- ⭐ 新增 Assignment 終止點規則（子節點檢查）
- 新增視覺錯誤標記
- 禁用非法操作按鈕

### v1.0
- 初始版本
- 基本 CRUD 功能
- JSON 匯入/匯出
- 拖拉重組功能
