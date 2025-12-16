§§ 此 ReactWidgets.njsproj 專案目的只為統一收管 react 原始碼。
專案建置不用預設的建置指令，而是另外開啟 Terimal 手動執行 `npm run build` 呼叫 webpack 執行 bundle。

包函了二種實作方法:
1. JSInterOpService 與 Blazor 元件包裝層互通
2. Web Components
