using Microsoft.AspNetCore.Components.Authorization;
using Vista.Models;

namespace Vista.Services;

/// <summary>
/// 謹用於在後台取得登入者資訊
/// 參考：[用來為自訂服務擷取使用者的線路處理常式](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/security/server/additional-scenarios?view=aspnetcore-8.0#circuit-handler-to-capture-users-for-custom-services)
/// </summary>
class AuthUserService(AccountService _accSvc, AuthenticationStateProvider _authProvider) : IAuthUser
{
  public AuthUser? GetCurrentUser()
  {
    var user = GetCurrentUserAsync().GetAwaiter().GetResult();
    return user;
  }

  /// <remarks>
  /// 現在使用者。※ 正常來說 Blazor 拿不到 HTTP 資訊，因為 Blazor 用 WebSocket 通訊。
  /// 故 Blazor 規定需從 AuthenticationStateProvider 間接取得 HttpContext。
  /// </remarks>
  public async Task<AuthUser?> GetCurrentUserAsync()
  {
    var authState = await _authProvider.GetAuthenticationStateAsync();
    var user = authState.User;
    if (user?.Identity?.IsAuthenticated ?? false)
      return _accSvc.GetAuthDataFromPool(user.Identity.Name!);

    ////# 取授權資料
    //var authUser = _accSvc.GetAuthDataFromPool(userIdentity.Name!);
    //if (authUser == null)
    //  authUser = _accSvc.RefillAuthData(userIdentity); // Cookie 仍是登入狀態的話！

    return null;
  }
}
