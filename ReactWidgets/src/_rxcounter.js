"use strict";
import React, { useState } from 'react'
import ReactDOM from 'react-dom/client'
import RxTitle from './RxTitle'
import RxCounter from './RxCounter'
import RxReactiveSampleWrapper from './RxReactiveSample'

//## 註冊單向繫結 React 元件：RxTitle
window.renderRxTitle = function (rootElement, title) {
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <RxTitle title={title} />
    </React.StrictMode>
  );
}

//## 註冊雙向繫結 React 元件：RxCounter
window.renderRxCounter = function (dotNetObject, rootElement, initCount) {
  function handleChange(newCount) {
    // events up
    dotNetObject.invokeMethodAsync('JsInvokeChange', newCount);
    console.trace(`JsInvokeChange => ${newCount}`);
  }

  // props down
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <RxCounter initCount={initCount} onChange={handleChange} />
    </React.StrictMode>
  );
}

//## 註冊雙向繫結可即時互動 React 元件：RxReactiveSample
// 因有使用 hooks 固須再包一層
window.renderRxReactiveSample = function (dotNetObject, rootElement, channel, initValue) {
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <RxReactiveSampleWrapper dotNetObject={dotNetObject} channel={channel} initValue={initValue} />
    </React.StrictMode>
  );
}
