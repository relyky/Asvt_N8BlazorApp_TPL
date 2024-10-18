/*
 * on 2023-11-2 試著轉換成 TypeScript 來與 Blazor 互動 => 失敗！
 *    接下來以官方的解為主。不要再考慮這件事。
 * on 2005-5-27 重構。只放入有用到的部份。
*/

/// 伺服器下載檔案 by stream
// ref→[ASP.NET Core Blazor file downloads](https://docs.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-6.0)
export function triggerFileDownload(fileName, url) {
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
}

/// 伺服器下載檔案 by URL
// ref→[ASP.NET Core Blazor file downloads](https://docs.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-6.0)
export async function downloadFileFromStream(fileName, contentStreamReference) {
  const arrayBuffer = await contentStreamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  const anchorElement = document.createElement('a');
  anchorElement.href = url;
  anchorElement.download = fileName ?? '';
  anchorElement.click();
  anchorElement.remove();
  URL.revokeObjectURL(url);
}

/// 讓瀏覽器發出嗶嗶聲
///All arguments are optional:
///duration of the tone in milliseconds. Default is 500
///frequency of the tone in hertz. default is 440
///volume of the tone. Default is 1, off is 0.
///type of tone. Possible values are sine, square, sawtooth, triangle, and custom. Default is sine.
///callback to use on end of tone
/// ref→[How to make beep sound in JavaScript](https://stackoverflow.com/questions/879152/how-to-make-beep-sound-in-javascript)
export function beep(duration, frequency, volume, type, callback) {
  //if you have another AudioContext class use that one, as some browsers have a limit
  const audioCtx = new (window.AudioContext || window.webkitAudioContext || window.audioContext);

  const oscillator = audioCtx.createOscillator();
  const gainNode = audioCtx.createGain();

  oscillator.connect(gainNode);
  gainNode.connect(audioCtx.destination);

  if (volume) { gainNode.gain.value = volume; }
  if (frequency) { oscillator.frequency.value = frequency; }
  if (type) { oscillator.type = type; }
  if (callback) { oscillator.onended = callback; }

  oscillator.start(audioCtx.currentTime);
  oscillator.stop(audioCtx.currentTime + ((duration || 500) / 1000));
}

/// <summary>
/// 捲動至指定元素。
/// ref→[Element.scrollIntoView()](https://developer.mozilla.org/en-US/docs/Web/API/Element/scrollIntoView)
/// </summary>
export function scrollIntoView(element, scrollIntoViewOptions) {
  element.scrollIntoView(scrollIntoViewOptions);
}

/// <summary>
/// 取得公開網址。(即以 browser/client 角度看主機網址)
/// </summary>
export function getLocationOrigin() {
  return window.location.origin;
}
