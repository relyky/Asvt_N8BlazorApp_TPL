using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Vista.Services;

/// <summary>
/// 因 NET8 變更授權行為，使得加值授權在某些間隙狀況下會無效！
/// 不再加值授權；只進行授權過渡，由 HttpContext 過渡至 AuthenticationState。
/// 導入 Resource-based authorization 進行搭配。 
/// </summary>
/// <remarks>
/// NET8 授權文章。
/// [不透過 ASP.NET Core Identity 使用 cookie 驗證 - 回應後端變更](https://learn.microsoft.com/zh-tw/aspnet/core/security/authentication/cookie?view=aspnetcore-8.0#react-to-back-end-changes)
/// [伺服器端 ASP.NET Core Blazor 其他安全性情節 - 用來為自訂服務擷取使用者的線路處理常式](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/security/server/additional-scenarios?view=aspnetcore-8.0#circuit-handler-to-capture-users-for-custom-services)
/// [ASP.NET Core Blazor 驗證與授權 - 針對錯誤進行疑難排解](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/security/?view=aspnetcore-8.0)
/// [ASP.NET Core Blazor 驗證與授權 - 資源授權](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/security/?view=aspnetcore-8.0#resource-authorization)
/// [ASP.NET Core BlazorSignalR 指引 - 監視伺服器端線路活動](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/fundamentals/signalr?preserve-view=true&view=aspnetcore-8.0#monitor-server-side-circuit-activity)
/// [Access HttpContext in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-6.0#access-httpcontext-from-middleware)
/// [Resource-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-8.0)
/// [ASP.NET Core 中的原則型授權](https://learn.microsoft.com/zh-tw/aspnet/core/security/authorization/policies?view=aspnetcore-8.0)
/// 以下為 NET6 時期參考文章。
/// AuthenticationStateProvider vs AuthenticationState
/// 請參考： https://www.eugenechiang.com/2020/12/26/authenticationstateprovider-vs-authenticationstate/
/// Blazor Tutorial : Authentication | Custom AuthenticationStateProvider - EP12
/// 請參考： https://www.youtube.com/watch?v=BmAnSNfFGsc&list=PL4WEkbdagHIR0RBe_P4bai64UDqZEbQap&index=12&ab_channel=CuriousDrive
/// Blazor Server Custom Authentication [Blazor Tutorial C# - Part 11]
/// 請參考：https://www.youtube.com/watch?v=iq2btD9WufI&list=PLzewa6pjbr3IQEUfNiK2SROQC1NuKl6PV&index=12&ab_channel=CodingDroplets
/// Webassembly Custom Authentication [Blazor Tutorial C# - Part 12]
/// 請參考：https://www.youtube.com/watch?v=7P_eyz4mEmA&ab_channel=CodingDroplets
/// Blazor Server and the Logout Problem
/// 請參考：https://auth0.com/blog/amp/blazor-server-and-the-logout-problem/
/// </remarks>
internal class Custom2AuthenticationStateProvider(
  ILogger<Custom2AuthenticationStateProvider> _logger,
  IHttpContextAccessor _http
  ) : AuthenticationStateProvider
{
  readonly ClaimsPrincipal anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

  public override Task<AuthenticationState> GetAuthenticationStateAsync()
  {
    _logger.LogDebug(">>>>>> Custom2-GetAuthenticationStateAsync");
    return Task.FromResult(new AuthenticationState(_http.HttpContext?.User ?? anonymousUser));
  }
}

