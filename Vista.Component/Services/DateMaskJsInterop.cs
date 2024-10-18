using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Vista.Component.Services;

public class DateMaskJsInterop : IAsyncDisposable
{
  private readonly Lazy<Task<IJSObjectReference>> moduleTask;

  public DateMaskJsInterop(IJSRuntime jsRuntime)
  {
    moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
        "import", "./_content/Vista.Component/datemaskJsInterop.js").AsTask());
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
  /// call JS function: setCleaveMask(element:HTMLElement, options)
  /// </summary>
  public async Task SetCleaveMaskAsync(ElementReference element, object options)
  {
    var module = await moduleTask.Value;
    await module.InvokeVoidAsync("setCleaveMask", element, options);
  }
}
