"use strict";
import React from 'react'
import ReactDOM from 'react-dom/client'
import RxWebCamera from './RxWebCamera'

//## 註冊雙向繫結 React 元件：RxWebCamera
window.renderRxWebCamera = (dotNetObject, rootElement, screenshotFormat) => {

  function handleCapture(photo) {
    // events up
    //console.trace('onCapture', { length: photo.length, photo })

    if (photo.length > 32000) {
      // ※ SignalR 封包預設限制32KB，已拉大十倍到320KB囉。
      alert(`圖片長度${photo.length}已超過圖片限制長度32KB！`)
      return;
    }

    dotNetObject.invokeMethodAsync('JsInvokeCapture', photo);
  }

  // props down
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <RxWebCamera screenshotFormat={screenshotFormat} onCapture={handleCapture} />
    </React.StrictMode>
  );
}
