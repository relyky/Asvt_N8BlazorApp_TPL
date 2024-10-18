"use strict";
import React from 'react'
import ReactDOM from 'react-dom/client'
import RxPhotoCrop from './RxPhotoCrop'

//## 註冊雙向繫結 React 元件：RxPhotoCrop
window.renderRxPhotoCrop = function (dotNetObject, rootElement, uploadInputId, maxHeight, maxWidth) {
  function handleImageCrop(info /* object */) {
    // events up
    dotNetObject.invokeMethodAsync('JsInvokeImageCrop', info);
    //console.log(`JsInvokeImageCrop`, {info});
  }

  function handleImageCropBefore() {
    // events up
    dotNetObject.invokeMethodAsync('JsInvokeImageCropBefore');
    //console.log(`JsInvokeImageCropBefore`);
  }

  maxHeight = maxHeight || 1280; // 可能直拍或橫拍，故長寬最大值應一樣。
  maxWidth = maxWidth || 1280;   // 可能直拍或橫拍，故長寬最大值應一樣。
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <RxPhotoCrop onImageCrop={handleImageCrop} onImageCropBefore={handleImageCropBefore} uploadInputId={uploadInputId}
        maxHeight={maxHeight} maxWidth={maxWidth} />
    </React.StrictMode>
  );
}
