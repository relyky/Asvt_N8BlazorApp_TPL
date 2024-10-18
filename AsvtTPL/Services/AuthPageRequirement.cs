using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Vista.AOP;

namespace Vista.Services;

// 宣告 Authorization Requirement
// ※ 不必實作內容，只做為需求的宣示。
class AuthPageRequirement : IAuthorizationRequirement { }

// 實作 handler --- Authorization Requirement 檢驗程序
internal class AuthPageHandler(
  ILogger<AuthPageHandler> _logger,
  AccountService _accSvc)
  : AuthorizationHandler<AuthPageRequirement>
{
  // 檢驗程序實作
  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthPageRequirement requirement)
  {
    bool f_success = false;
    string funcCode = string.Empty;
    try
    {
      //# 未登入離開。
      if (!context.User.Identity?.IsAuthenticated ?? false)
        return Task.CompletedTask;

      //# 取得需要資源。
      PageAttribute? pageAttr = context.Resource switch
      {
        Type pageType => pageType.GetCustomAttribute<PageAttribute>(),
        Microsoft.AspNetCore.Components.RouteData rd => rd.PageType.GetCustomAttribute<PageAttribute>(),
        DefaultHttpContext http => http.GetEndpoint()?.Metadata.GetMetadata<PageAttribute>(),
        _ => null
      };

      //# 若資源取得不足直接離開。
      if (pageAttr == null) return Task.CompletedTask;

      // 功能代碼
      funcCode = pageAttr.FuncId;

      //# 開始驗證
      // 是否登入者有授權的功能清單。
      //if (!context.User.IsInRole(funcCode)) return Task.CompletedTask;
      var authUser = _accSvc.GetAuthDataFromPool(context.User.Identity!.Name!);
      if (authUser is null) return Task.CompletedTask;
      if (!authUser.AuthFuncList().Contains(funcCode)) return Task.CompletedTask;

      // OK
      context.Succeed(requirement); // 滿足此項授權需求
      f_success = true;
      return Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "出現例外！" + ex.Message);
      return Task.CompletedTask;
    }
    finally
    {
      // 只是為了寫一筆 LOG。
      if (f_success)
        _logger.LogInformation($"User [{context.User.Identity!.Name}] has passed [{funcCode}] AuthPageRequirement.");
      else
        _logger.LogWarning($"User [{context.User.Identity!.Name}] has NOT passed [{funcCode}] AuthPageRequirement.");
    }
  }
}