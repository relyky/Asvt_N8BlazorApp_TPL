# 決策樹驗證規則說明

## 驗證規則總覽

決策樹編輯器會自動檢查以下規則，確保結構的邏輯正確性和完整性。

---

## 🔴 錯誤級別（必須修正）

### 1. Assignment 終止點規則（子節點）⭐
**規則：** Assignment 節點不能有子節點

**說明：**
- Assignment 節點代表決策的最終結果（葉節點）
- 是結構的終止點，其下不應該再有任何節點
- 這是決策樹的基本邏輯：賦值後即終止該分支

**錯誤訊息：**
```
Assignment 是結構終止點，不應該有子節點（發現 X 個子節點）
```

**錯誤範例：**
```
IF age >= 18
  Assignment: category = 'Adult'
    IF hasJob = true              ← ❌ 錯誤！Assignment 下不能有子節點
      Assignment: status = 'Working'
```

**正確範例：**
```
IF age >= 18
  IF hasJob = true
    Assignment: category = 'Working Adult'    ← ✓ 正確
  ELSE
    Assignment: category = 'Unemployed Adult' ← ✓ 正確
```

---

### 2. Assignment 終止點規則（同層級）⭐⭐ 新增
**規則：** 同一層級中，如果有 Assignment 節點，則不能有其他任何類型的節點

**說明：**
- Assignment 代表該分支的最終結果
- 同層級不應該再有其他邏輯判斷（IF/ELSE）或其他 Assignment
- 這確保了決策邏輯的清晰性和唯一性

**錯誤訊息：**
```
同一層級中有 Assignment 節點時，不能有其他節點。
發現 X 個 Assignment 和 Y 個其他節點（IF, ELSE）。
Assignment 是終止點，該層級應只有 Assignment 節點。
```

**錯誤範例 1：Assignment 與 IF 同層級**
```
IF age < 18
  Assignment: result = 'Minor'     ← ❌ 錯誤！
  IF needParent = true             ← ❌ 與 Assignment 同層級
    Assignment: guardian = required
```

**錯誤範例 2：Assignment 與 ELSE 同層級**
```
IF age = null
  Assignment: result = 'Unknown'   ← ❌ 錯誤！
  ELSE                             ← ❌ 與 Assignment 同層級
    Assignment: error = true
```

**錯誤範例 3：多個 Assignment 與其他類型混合**
```
IF condition
  Assignment: value1               ← ❌ 錯誤！
  Assignment: value2               ← ❌ 與其他 Assignment 同層級
  IF otherCondition                ← ❌ 與 Assignment 同層級
    Assignment: value3
```

**正確範例：**
```
IF age < 18
  IF needParent = true
    Assignment: guardian = required  ← ✓ 正確：該層級只有這一個節點
  ELSE
    Assignment: guardian = optional  ← ✓ 正確：ELSE 層級也只有一個節點
```

**為什麼需要這個規則？**
1. **邏輯清晰**：確保每個分支的結果是唯一且明確的
2. **避免混淆**：防止同時有結果賦值和條件判斷
3. **結構一致**：維持決策樹的標準結構

**如何修正：**
1. 將 Assignment 移到更深的層級
2. 將其他邏輯（IF/ELSE）提到 Assignment 之前
3. 重新設計決策樹結構，確保邏輯先判斷，最後才賦值

---

### 3. IF 節點缺少條件
**規則：** IfCondition 類型的節點必須有條件表達式

**錯誤訊息：**
```
IF 節點缺少條件
```

**如何修正：**
在編輯面板的「條件 (Condition)」欄位填入條件表達式

---

### 3. Assignment 節點缺少指定值
**規則：** Assignment 類型的節點必須有指定值

**錯誤訊息：**
```
Assignment 節點缺少指定值
```

**如何修正：**
在編輯面板的「指定值 (Assignment)」欄位填入值

---

### 4. 同層級多個 ELSE
**規則：** 同一層級只能有一個 ELSE 節點

**說明：**
ELSE 是 fallback 邏輯，同層級只能有一個

**錯誤訊息：**
```
同一層級有 X 個 ELSE 節點，只能有一個
```

**如何修正：**
刪除多餘的 ELSE 節點，或將其改為 IF 節點

---

## ⚠️ 警告級別（建議修正）

### 1. IF 節點沒有子節點
**規則：** IF 節點應該有子節點來表示結果

**警告訊息：**
```
IF 節點沒有子節點（結果）
```

**如何修正：**
為 IF 節點添加子節點（通常是 Assignment 或更多的 IF/ELSE）

---

### 2. ELSE 節點沒有子節點
**規則：** ELSE 節點應該有子節點來表示結果

**警告訊息：**
```
ELSE 節點沒有子節點（結果）
```

**如何修正：**
為 ELSE 節點添加子節點

---

### 3. ELSE 位置建議
**規則：** ELSE 節點建議放在同層級的最後

**警告訊息：**
```
ELSE 節點建議放在最後
```

**說明：**
ELSE 是 fallback 邏輯，放在最後更符合邏輯順序

**如何修正：**
拖拉 ELSE 節點到該層級的最後位置

---

### 4. 節點類型未設定
**規則：** 節點應該設定類型（IF/ELSE/Assignment）

**警告訊息：**
```
節點類型未設定
```

**如何修正：**
在編輯面板選擇適當的節點類型

---

## 驗證流程

### 自動驗證
- 無（目前需手動觸發）

### 手動驗證
1. 點擊工具列的「✓ 驗證結構」按鈕
2. 系統遍歷整個樹狀結構
3. 顯示所有錯誤和警告
4. 提供錯誤路徑以便定位

### 驗證結果
```
✓ 決策樹結構驗證通過！
```
或
```
❌ 發現錯誤:
- 路徑 根節點 > IF age >= 18 > 'Adult': Assignment 是結構終止點...

⚠️ 警告訊息:
- 路徑 根節點: ELSE 節點建議放在最後
```

---

## UI 視覺提示

### 錯誤標記
- **紅色閃爍標記**：Assignment 有子節點
- **禁用按鈕**：Assignment 節點的「新增子節點」按鈕
- **警告訊息**：編輯面板顯示詳細錯誤說明

### 拖拉限制
- 無法拖拉節點到 Assignment 節點下
- 拖拉時會顯示錯誤通知

### 通知系統
- 🔴 錯誤通知（紅色）
- ⚠️ 警告通知（黃色）
- ✅ 成功通知（綠色）

---

## 最佳實踐

### 1. 結構設計
```
正確的決策樹結構：

IF condition1
  IF condition1.1
    Assignment: result1       ← 終止點
  ELSE
    Assignment: result2       ← 終止點
IF condition2
  Assignment: result3         ← 終止點
ELSE
  Assignment: default_result  ← 終止點
```

### 2. 定期驗證
- 編輯過程中定期點擊「驗證結構」
- 發現問題立即修正
- 避免累積錯誤

### 3. 儲存前驗證
- 匯出 JSON 前先驗證
- 確保結構正確
- 避免匯出錯誤資料

### 4. 匯入後驗證
- 匯入 JSON 後立即驗證
- 檢查是否有結構問題
- 特別注意 Assignment 終止點規則

---

## 常見問題

### Q1: 為什麼 Assignment 不能有子節點？
**A:** Assignment 代表決策的最終結果，是邏輯的終止點。如果還需要更多條件判斷，應該在 Assignment 之前處理，而不是在其下方。

### Q2: 如果我需要在 Assignment 後執行其他邏輯怎麼辦？
**A:** Assignment 代表決策的最終結果，不存在在其之後執行其他邏輯的情境。

### Q3: 警告訊息需要立即修正嗎？
**A:** 警告不會阻止功能運作，但建議修正以保持結構清晰和邏輯正確。

### Q4: 驗證會自動執行嗎？
**A:** 目前需要手動點擊「驗證結構」按鈕。部分規則（如 Assignment 子節點）在 UI 操作時會即時檢查。

### Q5: 錯誤的 JSON 可以匯入嗎？
**A:** 可以匯入，但建議匯入後立即驗證並修正錯誤。

---

## 技術細節

### 驗證函數
```javascript
function validateNode(node, errors, warnings, path)
```

### 檢查時機
- 手動驗證：點擊「驗證結構」按鈕
- 操作限制：添加子節點、拖拉節點時
- 視覺提示：渲染節點時

### 驗證順序
1. 節點類型檢查
2. IF 條件檢查
3. ELSE 分支檢查
4. Assignment 終止點檢查 ⭐
5. Assignment 指定值檢查
6. 同層級 ELSE 數量檢查
7. ELSE 位置檢查
8. Assignment 順序檢查 ⭐
9. 遞迴檢查子節點

---

## 更新歷史

### v1.2 (最新)
- ⭐⭐ 新增：Assignment 同層級唯一性規則（錯誤級別）
- 當同層級有 Assignment 節點時，不允許有其他任何類型的節點
- 新增：節點類型切換時的驗證
- 新增：添加子節點/兄弟節點時的驗證
- 新增：拖拉操作的更嚴格限制
- 移除：Assignment 順序建議（已被更嚴格的規則涵蓋）

### v1.1
- ⭐ 新增：Assignment 終止點規則 - 子節點檢查（錯誤級別）
- 新增：Assignment 順序建議（警告級別）
- 新增：UI 視覺錯誤標記
- 新增：拖拉操作限制
- 新增：按鈕禁用功能

### v1.0
- 基本驗證規則
- IF 條件檢查
- ELSE 規則檢查
- Assignment 值檢查
