import React, { useState } from 'react'
import cx from "./WebSample.module.css"

//§ 機制：WebComponent 與 Blazor 通訊 
//※ web-component 的 props 名稱需全小寫，否則通訊會失敗。
export default function WebSample({ dot_net_object /* object */, foo /* string */, on_bar /* (payload) => void */ }) {
  const [cnt, setCnt] = useState(1)

  return (
    <div className={cx.wall}>
      <h5>來自外面：{foo || 'nil'}</h5>
      <button className={cx.button} onClick={handleClick}>送出訊息到 Blazor 後端</button>
    </div>
  )

  function handleClick() {
    console.log('WebSample.handleClick', { dot_net_object, cnt })

    setCnt(c => c + 1)

    //## events up
    //※ 透過 dotNetObject 送訊息回去，可以直達 Blazor 後端。
    const message = `[${cnt}] 透過 dotNetObject 送訊息到 Blazor 後端。 - ${foo}`;
    dot_net_object.invokeMethodAsync('JsInvokeBarEvent', message);

    //// 自己送訊息回去，只在前端有作用。
    //const payload = {
    //  uuid: uuid,
    //  message: `[${cnt}]show me the money.今天天氣真好。`
    //}
    //on_bar(payload) // 自己送訊息回去，只在前端有作用。
  }
}