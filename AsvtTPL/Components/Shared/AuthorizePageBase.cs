//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Authorization;
//using Vista.Services;
//
//namespace AsvtTPL.Components.Shared;
//
/////※ 因為重新啟用 @attribute [Authorize("AuthPage")]，已失去了價值。
//
///// <summary>
///// 客製化授權驗證 Page。
///// 注意１：必需搭配 PageAttribute 才有作用，例：@attribute [Page("DEMO012", "ＣＲＵＤ多欄位樣板")]。
///// 注意２：若 override OnInitializedAsync 時一定要執行 base.OnInitializedAsync() 才會生效。
///// </summary>
///// <remarks>
///// 取代 @attribute [Authorize("AuthPage")]。因為 F5 刷新會 => 404！
///// 查原 CustomAuthenticationStateProvider 在頁面初始戴入驗證時未啟用。
///// 在頁面初始戴入驗證時，驗的是 HttpContext，而不是驗 AuthenticationStateProvider。
///// </remarks>
//public class AuthorizePageBase : ComponentBase
//{
//  [Inject]
//  protected NavigationManager navSvc { get; set; } = default!;
//
//  [Inject]
//  protected IAuthorizationService authSvc { get; set; } = default!;
//
//  [Inject]
//  ILogger<AuthorizePageBase> logger { get; set; } = default!;
//
//  [CascadingParameter]
//  protected Task<AuthenticationState> AuthStateTask { get; set; } = default!;
//
//  /// <summary>
//  /// 授權處理中
//  /// </summary>
//  protected bool Authing { get; private set; } = true;
//
//  protected override async Task OnInitializedAsync()
//  {
//    try
//    {
//      await base.OnInitializedAsync();
//
//      Authing = true;
//      Type pageType = this.GetType();
//
//      //# RedirectToLogin when not IsAuthenticated
//      var authState = await AuthStateTask;
//      if (!(authState.User.Identity?.IsAuthenticated ?? false))
//      {
//        /// 401 未登入
//        navSvc.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(navSvc.Uri)}", forceLoad: true);
//        return;
//      }
//
//      ////# [Role-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-8.0)
//      //PageAttribute? pageAttr = pageType.GetCustomAttribute<PageAttribute>();
//      //string? funcCode = pageAttr?.FuncId; // 功能代碼
//      //if (funcCode is null || !authState.User.IsInRole(funcCode))
//      //{
//      //  /// 403 授權不足 禁止使用此功能
//      //  navSvc.NavigateTo($"/ErrorStatus/{403}", forceLoad: false);
//      //  return;
//      //}
//
//      //# [Resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?view=aspnetcore-8.0)
//      var result = await authSvc.AuthorizeAsync(authState.User, pageType, new AuthPageRequirement());
//      if (!result.Succeeded)
//      {
//        /// 403 授權不足 禁止使用此功能
//        navSvc.NavigateTo($"/ErrorStatus/{403}", forceLoad: false);
//        return;
//      }
//
//      //# SUCCESS
//      logger.LogDebug($"User [{authState.User.Identity.Name}] has passed AuthorizeAsync。");
//    }
//    catch (Exception ex)
//    {
//      /// 401 出現不可預期的例外
//      logger.LogError("401 出現不可預期的例外！" + ex.Message);
//      navSvc.NavigateTo($"/ErrorStatus/{401}", forceLoad: false);
//    }
//    finally
//    {
//      Authing = false;
//    }
//  }
//}
