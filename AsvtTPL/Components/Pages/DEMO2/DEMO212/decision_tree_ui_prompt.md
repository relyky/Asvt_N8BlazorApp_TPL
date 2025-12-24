# 可拖拉決策樹 UI 實作需求

## 專案概述

實作一個基於 NET8 Blazor Web App (interactive server reder mode) 的可拖拉決策樹編輯器，用於視覺化建立和編輯二元決策邏輯。

---

## 核心功能需求

### 1. 決策樹邏輯結構

#### 基本語法規則
- 採用 **IF-THEN-ELSE** 二元分支結構
- 每個條件分支產生兩個結果：**成立 (THEN)** 或 **不成立 (ELSE)**
- 同一層級可包含多個 IF 條件判斷

#### 節點類型
1. **條件節點 (Condition Node)**
   - 包含比較條件表達式
   - 例如：`A = 3`、`B = 4`、`A < 10`
   
2. **動作節點 (Action Node)**
   - THEN 分支的執行結果
   - ELSE 分支的執行結果
   - 例如：`'3A'`、`'Bar'`、`'少數'`、`'otherwise'`

3. **容器節點 (Container Node)**
   - THEN 或 ELSE 分支下可嵌套新的條件判斷
   - 支援多層巢狀結構

---

## UI 設計規範

### 畫面配置

```
┌─────────────────────────────────────────────────┐
│  決策樹編輯器                                    │
├────────────────┬────────────────────────────────┤
│                │                                │
│   Tree View    │    屬性編輯面板                │
│   (左側區域)    │    (右側區域)                  │
│                │                                │
│   - 樹狀結構    │    - 節點屬性表單              │
│   - 拖拉操作    │    - 條件編輯                  │
│   - 節點選取    │    - 動作編輯                  │
│                │    - 類型切換                  │
│                │                                │
└────────────────┴────────────────────────────────┘
```

### 左側區域：Tree View

**功能要求：**
- 以樹狀結構視覺化顯示完整決策邏輯
- 支援節點拖拉重新排序
- 支援節點拖拉變更層級（移入/移出分支）
- 點擊節點進行選取，觸發右側屬性編輯
- 清楚標示 THEN/ELSE 分支
- 顯示節點摺疊/展開控制

**視覺設計建議：**
- 使用縮排表示層級關係
- IF 條件以不同顏色/圖示標示
- THEN 分支與 ELSE 分支視覺區隔
- 拖拉時顯示可放置位置提示

### 右側區域：屬性編輯面板

**功能要求：**
- 表單式編輯界面
- 根據節點類型顯示對應欄位：
  - **條件節點**：條件表達式輸入框
  - **動作節點**：動作/返回值輸入框
  - **容器節點**：新增子條件按鈕

**表單欄位：**
1. 節點類型選擇（條件/動作）
2. 條件表達式編輯區
3. 動作/返回值編輯區
4. 新增/刪除子節點按鈕
5. 儲存/取消按鈕

---

## 語法範例解析

### 範例 1：同層多條件 (扁平結構)

```
IF condition THEN action
IF condition THEN action
IF condition THEN action
IF condition THEN action
ELSE action
```

**特徵：**
- 所有 IF 條件位於同一層級
- 最後一個 ELSE 作為所有條件不成立時的預設動作
- 按順序逐一檢查條件

**實際案例：**
```
IF A = 3 THEN '3A'
IF B = 4 THEN 'Bar'
IF A < 10 THEN '少數'
ELSE 'otherwise'
```

### 範例 2：雙層巢狀 (對稱結構)

```
IF condition THEN
  IF condition THEN action
  ELSE action
ELSE
  IF condition THEN action
  ELSE action
```

**特徵：**
- THEN 和 ELSE 分支各包含一組完整的 IF-ELSE 判斷
- 形成對稱的二元決策樹
- 總共 4 個可能的執行結果

### 範例 3：混合結構 (多層不對稱)

```
IF condition THEN action
IF condition THEN action
IF condition THEN
  IF condition THEN action
  IF condition THEN action
  ELSE action
ELSE
  IF condition THEN action
  IF condition THEN action
  ELSE action
```

**特徵：**
- 同時包含同層多條件與巢狀結構
- THEN 分支與 ELSE 分支的複雜度可不對稱
- 靈活組合不同決策邏輯

---

## 技術實作要求

### 必要功能

1. **拖拉操作 (Drag & Drop)**
   - 使用 HTML5 Drag and Drop API
   - 或使用第三方拖拉函式庫（如 Sortable.js）
   - 支援跨層級拖拉
   - 即時視覺化反饋

2. **樹狀結構資料管理**
   - 使用 JSON 或類似格式儲存樹狀結構
   - 支援節點新增、刪除、修改、移動
   - 資料結構範例：
     ```json
     {
       "type": "condition",
       "expression": "A = 3",
       "then": {
         "type": "action",
         "value": "3A"
       },
       "else": {
         "type": "condition",
         "expression": "B = 4",
         "then": {...},
         "else": {...}
       }
     }
     ```

3. **狀態管理**
   - 當前選取節點
   - 節點展開/摺疊狀態
   - 編輯歷史紀錄（可選）

4. **表單驗證**
   - 條件表達式格式檢查
   - 必填欄位驗證
   - 即時錯誤提示

### 選用進階功能

- [ ] 節點搜尋/篩選
- [ ] 匯出/匯入 JSON 格式
- [ ] Undo/Redo 操作歷史
- [ ] 節點複製/貼上
- [ ] 快捷鍵支援
- [ ] 暗色模式切換
- [ ] 樹狀圖視覺化（流程圖模式）

---

## 輸出要求

- **檔案格式**：單一 HTML 檔案（包含內嵌 CSS 和 JavaScript）
- **瀏覽器相容性**：現代瀏覽器（Chrome、Firefox、Edge、Safari）
- **響應式設計**：支援桌面端操作（拖拉功能主要針對桌面設計）
- **程式碼規範**：清晰註解、模組化結構、易於維護

---

## 參考 UI 元件

建議使用以下技術或函式庫（可選）：

- **拖拉功能**：Sortable.js、dragula
- **樹狀元件**：jstree、Fancy Tree
- **UI 框架**：Bootstrap、Tailwind CSS
- **圖示庫**：Font Awesome、Material Icons

---

## 交付清單

- [x] 完整的 HTML 檔案
- [x] 實作所有核心功能
- [x] 包含測試用的範例決策樹資料
- [x] 簡要的使用說明（註解或 README）
- [x] 符合上述三個語法範例的邏輯結構

---

## 額外說明

- 此決策樹編輯器主要用於**業務邏輯設計**，非程式碼產生器
- 條件表達式採用**自然語言描述**或**簡單運算式**
- 需考慮**易用性**，讓非技術人員也能理解和操作
- UI 應直觀反映決策流程的邏輯關係

---

*文件版本：1.0*  
*最後更新：2024-12-24*
