# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 專案概述

這是一個將 React 元件打包成 Web Components 的專案,用於整合到 Blazor 應用程式中。使用 `@r2wc/react-to-web-component` 將 React 元件轉換為 Custom Elements,讓 Blazor 可以直接使用這些元件。

## 建置指令

```bash
npm run build
```

使用 Webpack 編譯所有元件,輸出到 `../Vista.Component/wwwroot` 目錄。

## 架構設計

### 1. 雙模式元件系統

專案支援兩種不同的整合模式:

#### **Web Components 模式** (在 `_main.js` 中註冊)
- 使用 `customElements.define()` 註冊為標準 Web Components
- 可在任何 HTML 頁面直接使用
- 範例元件: `WebSample`, `WebBlackScreen`, `WebQRCode` 等
- 使用方式: `<web-qrcode value="..." level="M"></web-qrcode>`

#### **React Render 模式** (在 `_rx*.js` 檔案中註冊)
- 透過全域 `window.render*()` 函數直接渲染 React 元件
- 用於需要複雜雙向溝通的場景
- 範例元件: `RxCounter`, `RxWebCamera`, `RxPhotoCrop` 等
- 使用方式: `window.renderRxCounter(dotNetObject, rootElement, initCount)`

### 2. Blazor 與 React 的通訊機制

#### **Web Components → Blazor**
- 透過 `dot_net_object` prop 傳入 DotNetObjectReference
- 使用 `dotNetObject.invokeMethodAsync('MethodName', args)` 呼叫 Blazor 方法
- 範例: `WebSample` 元件的 `dot_net_object` prop

#### **React Render → Blazor**
- 直接透過 callback 函數傳遞 dotNetObject
- 範例: `RxCounter` 的 `JsInvokeChange` 呼叫 (見 `_rxcounter.js:22`)

**重要提醒**: Web Components 的 `on_*` function props 只能作用在前端,無法直達 Blazor 後端。需要後端通訊時必須使用 `dot_net_object` 方式。

### 3. Webpack 配置關鍵點

**多入口點配置** (`webpack.config.js`):
- `main`: 註冊所有 Web Components
- `rxcounter`, `rxwebcamera`, `rxphotocrop`, `rxqrcodescanner`: 各自獨立的 React Render 模式入口
- `shared`: 共用依賴 (lodash)
- `runtimeChunk: 'single'`: 所有 bundle 共用一個 runtime

**輸出路徑**: `../Vista.Component/wwwroot` - 直接輸出到 Blazor 專案的靜態資源目錄

## 主要元件功能

### RxPhotoCrop
照像裁剪元件,支援手機照像 → 裁剪 → 縮圖 → 自動上傳流程:
- 使用 `react-image-crop` 進行裁剪
- 使用 Canvas API 進行縮圖處理
- 透過 DataTransfer API 模擬檔案上傳
- 支援手機與桌面兩種裁剪界面模式

### RxQRCodeScanner
QR Code 掃描元件:
- 使用 `html5-qrcode` 套件
- 預設使用環境鏡頭 (後置鏡頭)
- 限定只掃描 QR Code 格式

### RxWebCamera
網路攝影機拍照元件:
- 使用 `react-webcam`
- 支援截圖並轉為 Base64 格式
- 優先選取環境鏡頭

### WebFilePlayer / WebYouTubePlayer
媒體播放器元件:
- 使用 `react-player`
- 支援本地檔案與 YouTube 影片播放
- 可控制播放狀態、循環、控制列顯示

## 開發注意事項

### 檔案編碼
- 所有程式碼檔案使用 UTF-8 編碼

### Web Components 命名規範
- Web Component 的標籤名稱必須全小寫 (例如: `web-sample`, 不可使用 `WebSample`)
- 使用連字號分隔單字 (例如: `web-qrcode-svg`)

### 手機相機處理
- `RxWebCamera` 的註解明確指出「WebCam 比較適合用在 Web,不能將手機照像特性全開」
- 需要完整手機相機功能時,應使用 `<input type="file" capture="environment">` 方式 (參考 `RxPhotoCrop`)

### iOS 相容性
- 避免使用 `OffscreenCanvas`,iOS 尚未支援 (見 `RxPhotoCrop.js:132`)
- 使用一般 `<canvas>` 元素進行圖像處理
