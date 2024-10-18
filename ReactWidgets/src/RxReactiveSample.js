import React, { useState, useEffect } from 'react'

/// 元件外層包裝：使可即時互動 DidUpdate。
/// ※需搭配 Blazor 端包裝程式碼。
export default function RxReactiveSampleWrapper({ dotNetObject, channel, initValue }) {
  const [shellValue, setShellValue] = useState(initValue)

  useEffect(() => {
    // 註冊通訊。※需預先設定 js-event-bus 套件實作 mediator。
    window.eventBus.on(channel, (payload) => {
      //## update props --- 實現 DidUpate
      const { value: newValue } = payload
      console.trace('RxReactiveSampleWrapper.DidUpate', { payload, newValue });
      setShellValue(newValue)
    });
    return () => {
      // 解除註冊通訊
      window.eventBus.detach(channel)
    }
  }, [])

  function handleChange(newValue) {
    // events up
    dotNetObject.invokeMethodAsync('JsInvokeChange', newValue);
    console.trace(`RxReactiveSampleWrapper.JsInvokeChange`, { newValue });
  }

  return (
    <RxReactiveSample value={shellValue} onChange={handleChange} />
  )
}

/// 元件本體
function RxReactiveSample({ value /* string */, onChange /* event */ }) {
  const [innerValue, setInnerValue] = useState(value || '');

  useEffect(() => {
    setInnerValue(value)
  }, [value])

  function handleChange(e) {
    setInnerValue(e.target.value)
  }

  const handleKeyPress = (e) => {
    if (e.key === 'Enter') {
      onChange(innerValue)
    }
  }

  function handleBlur() {
    onChange(innerValue)
  }

  return (
    <div className="pa-2 my-2" style={{ border: 'solid 2px red', borderRadius: 8 }}>
      <h3>RxJS Reactive Sample</h3>
      <p>value down: {value}</p>
      <input value={innerValue} onChange={handleChange} onKeyDown={handleKeyPress} onBlur={handleBlur} style={{ width: '100%', border: 'solid 1px black' }} />
    </div>
  )
}
