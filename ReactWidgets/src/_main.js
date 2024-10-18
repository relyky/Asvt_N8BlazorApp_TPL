"use strict";
import r2wc from '@r2wc/react-to-web-component'
import WebSample from './WebSample'
import WebBlackScreen from './WebBlackScreen'
import WebIdleWidget from './WebIdleWidget'
import WebFilePlayer from './WebFilePlayer'
import WebYouTubePlayer from './WebYouTubePlayer'
import WebQRCode from './WebQRCode'
import WebQRCodeSVG from './WebQRCodeSVG'

//#region 註冊 Web Components
///※註1：web-component 的命名需全小寫。
///※註2：因為 web-component 是純前端元件，經由 function 向上傳遞訊息只能送到前端。

customElements.define("web-sample", r2wc(WebSample, {
  props: {
    dot_net_object: "object", // 透過 dotNetObject 送訊息回去，可以直達 Blazor 後端。
    foo: "string",
    on_bar: "function", // 只作用在前端！
  },
}));

customElements.define("web-black-screen", r2wc(WebBlackScreen, {
  props: {
    idname: "string",
  },
}));

customElements.define("web-idle-widget", r2wc(WebIdleWidget, {
  props: {
    timeout: "number",
    signout: "string",
  },
}));

customElements.define("web-file-player", r2wc(WebFilePlayer, {
  props: {
    url: "string",
    playing: "boolean",
    loop: "boolean",
    controls: "boolean",
  },
}));

customElements.define("web-youtube-player", r2wc(WebYouTubePlayer, {
  props: {
    url: "string",
    playing: "boolean",
    loop: "boolean",
    controls: "boolean",
  },
}));

customElements.define("web-qrcode", r2wc(WebQRCode, {
  props: {
    value: "string", // required
    level: "string", // L,M*,Q,H
    size: "number",  // option
    image_url: "string", // option
    fg_color: "string", // option
    bg_color: "string", // option
  },
}));

customElements.define("web-qrcode-svg", r2wc(WebQRCodeSVG, {
  props: {
    value: "string", // required
    level: "string", // L,M*,Q,H
    size: "number",  // option
    image_url: "string", // option
    fg_color: "string", // option
    bg_color: "string", // option
  },
}));

//#endregion
