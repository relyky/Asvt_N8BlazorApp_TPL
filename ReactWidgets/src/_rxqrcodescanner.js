"use strict";
import React from 'react'
import ReactDOM from 'react-dom/client'
import RxQRCodeScanner from './RxQRCodeScanner'

//## 註冊雙向繫結 React 元件：RxWebCamera
window.renderRxQRCodeScanner = (dotNetObject, rootElement) => {

  function handleScanSuccess(decodedText, decodedResult) {
    // To handle the scanned code as you like
    console.log(`Code matched => ${decodedText}`, decodedResult);
    dotNetObject.invokeMethodAsync('JsInvokeScanSuccess', decodedText);
  }

  function handleScanFailure(error) {
    // To handle scan failure, usually better to ignore and keep scanning.
    const errMsg = `Code scan error => ${error}`;
    console.warn(errMsg);
    dotNetObject.invokeMethodAsync('JsInvokeScanFailure', errMsg);
  }

  // props down
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <RxQRCodeScanner onScanSuccess={handleScanSuccess} onScanFailure={handleScanFailure} />
    </React.StrictMode>
  );
}
