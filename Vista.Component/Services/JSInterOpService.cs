using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace Vista.Component.Services;

public class JSInterOpService : IAsyncDisposable
{
  //# Injection Member
  //readonly IJSRuntime jsr;
  readonly IHttpContextAccessor httpCtx;

  //# resource
  readonly Lazy<Task<IJSObjectReference>> moduleTask;

  public JSInterOpService(IJSRuntime _jsr, IHttpContextAccessor _httpCtx)
  {
    // injection
    //jsr = _jsr;
    httpCtx = _httpCtx;

    // load module
    moduleTask = new(() => _jsr.InvokeAsync<IJSObjectReference>(
        "import", "./_content/Vista.Component/interop-module.js").AsTask());
  }

  public async ValueTask DisposeAsync()
  {
    if (moduleTask.IsValueCreated)
    {
      var module = await moduleTask.Value;
      await module.DisposeAsync();
    }
  }

  /// <summary>
  /// 伺服器下載檔案 by URL.
  /// </summary>
  public async Task DownloadFileFromUrlAsync(string fileName, Uri fileURL)
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("triggerFileDownload", fileName, fileURL.AbsoluteUri);
    // 未測試
  }

  /// <summary>
  /// 伺服器下載檔案 by binary blob.
  /// </summary>
  public async Task DownloadFileAsync(string fileName, byte[] fileBlob, string contentType = System.Net.Mime.MediaTypeNames.Application.Octet)
  {
    var jsModule = await moduleTask.Value;
    string base64Url = $"data:{contentType};base64," + Convert.ToBase64String(fileBlob);
    await jsModule.InvokeVoidAsync("triggerFileDownload", fileName, base64Url);
  }

  /// <summary>
  /// 伺服器下載檔案 by MemoryStream.
  /// </summary>
  public async Task DownloadFileAsync(string fileName, System.IO.MemoryStream ms, string contentType = System.Net.Mime.MediaTypeNames.Application.Octet)
  {
    var jsModule = await moduleTask.Value;
    string base64Url = $"data:{contentType};base64," + Convert.ToBase64String(ms.ToArray());
    await jsModule.InvokeVoidAsync("triggerFileDownload", fileName, base64Url);
  }

  /// <summary>
  /// 伺服器下載檔案 by stream。※注意：在某些安全性考量下時會無效！
  /// </summary>
  public async Task DownloadStreamAsync(string fileName, System.IO.Stream fileStream)
  {
    var jsModule = await moduleTask.Value;

    //※ 從頭開始讀檔，因為前面步驟可能已讀到檔尾。
    if (fileStream.CanSeek) fileStream.Seek(0, System.IO.SeekOrigin.Begin);

    using var streamRef = new DotNetStreamReference(fileStream);
    await jsModule.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
  }

  /// <summary>
  /// 讓瀏覽器發出嗶嗶聲。
  /// </summary>
  public async Task BeepAsync()
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("beep");
  }

  /// <summary>
  /// 捲動至指定元素。
  /// ref→[Element.scrollIntoView()](https://developer.mozilla.org/en-US/docs/Web/API/Element/scrollIntoView)
  /// </summary>
  /// <param name="element">The target element.</param>
  /// <param name="block">Defines vertical alignment. One of start, center, end, or nearest. Defaults to start.</param>
  /// <param name="inline">Defines horizontal alignment. One of start, center, end, or nearest. Defaults to nearest.</param>
  /// <param name="behavior">Determines whether scrolling is instant or animates smoothly. One of smooth, instant, auto.</param>
  public async Task ScrollIntoViewAsync(ElementReference element, string block = "start", string inline = "nearest", string behavior = "smooth")
  {
    var jsModule = await moduleTask.Value;
    await jsModule.InvokeVoidAsync("scrollIntoView", element, new { behavior, block, inline });
  }

  /// <summary>
  /// 取得公開網址。(即以 browser/client 角度看主機網址)
  /// </summary>
  public async Task<string> GetBaseUrlAsync()
  {
    var jsModule = await moduleTask.Value;

    var pathBase = httpCtx.HttpContext!.Request.PathBase; // path base 只能在 server 端取得。
    string originUrl = await jsModule.InvokeAsync<string>("getLocationOrigin"); // 取得公開網址。即以 browser/client 角度看主機網址。
    return originUrl + pathBase;
  }
}
