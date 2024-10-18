//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.Server;
//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text.Json;
//using System.Threading.Tasks;
//using Vista.Models;

//namespace Vista.Services;

///// <summary>
///// 加值預設的 AuthenticationStateProvider。
///// </summary>
///// <remarks>
///// AuthenticationStateProvider vs AuthenticationState
///// 請參考： https://www.eugenechiang.com/2020/12/26/authenticationstateprovider-vs-authenticationstate/
///// Blazor Tutorial : Authentication | Custom AuthenticationStateProvider - EP12
///// 請參考： https://www.youtube.com/watch?v=BmAnSNfFGsc&list=PL4WEkbdagHIR0RBe_P4bai64UDqZEbQap&index=12&ab_channel=CuriousDrive
///// Blazor Server Custom Authentication [Blazor Tutorial C# - Part 11]
///// 請參考：https://www.youtube.com/watch?v=iq2btD9WufI&list=PLzewa6pjbr3IQEUfNiK2SROQC1NuKl6PV&index=12&ab_channel=CodingDroplets
///// Webassembly Custom Authentication [Blazor Tutorial C# - Part 12]
///// 請參考：https://www.youtube.com/watch?v=7P_eyz4mEmA&ab_channel=CodingDroplets
///// Blazor Server and the Logout Problem
///// 請參考：https://auth0.com/blog/amp/blazor-server-and-the-logout-problem/
///// </remarks>
//internal class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
//{
//  // inject 
//  readonly IHttpContextAccessor _http; //
//  readonly AccountService _accSvc;
//  readonly ILogger<Custom2AuthenticationStateProvider> _logger;

//  // resource
//  readonly AuthenticationState anonymousUser;

//  public CustomAuthenticationStateProvider(
//    ILoggerFactory loggerFactory,
//    ILogger<Custom2AuthenticationStateProvider> logger,
//    IHttpContextAccessor http, AccountService accSvc) : base(loggerFactory)
//  {
//    _logger = logger;
//    _http = http;
//    _accSvc = accSvc;
//    anonymousUser = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
//  }

//  /// <summary>
//  /// 每隔幾秒查看是否還在登入狀態。
//  /// 因為 HttpContext 的 Cookie 並非即時反應的。這會導致在多畫面應用情境時，若一畫面登出其他畫面卻仍在登入狀態的不同步冏狀。
//  /// </summary>
//  protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(7);

//  public override Task<AuthenticationState> GetAuthenticationStateAsync()
//  {
//    _logger.LogDebug(">>>>>> Custom-GetAuthenticationStateAsync");

//    var userIdentity = _http.HttpContext?.User.Identity as ClaimsIdentity;
//    //# 沒有 AuthCookie，回傳anonymousUser。
//    if (userIdentity == null || !userIdentity.IsAuthenticated)
//      return Task.FromResult(anonymousUser);

//    //# 取授權資料
//    var authUser = _accSvc.GetAuthDataFromPool(userIdentity.Name!);
//    //if (authUser == null)
//    //  authUser = _accSvc.RefillAuthData(userIdentity); // Cookie 仍是登入狀態的話！

//    //# 授權資料不完整，回傳anonymousUser。
//    if (authUser == null)
//      return Task.FromResult(anonymousUser);

//    ///※ 加值 UserData 依完整的登入者資料(AuthUser) 
//    userIdentity.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(authUser)));

//    //## 加值授權資料
//    var authFuncList = authUser.AuthFuncList();
//    if (authFuncList != null)
//    {
//      /// FuncId 授權資料也併入Role清單。
//      /// 此處的 Roles 數量可以上百筆，不會影響 Auth Cookie。
//      /// 對 <AuthorizeView />, Task<AuthenticationState> 會多出這裡加入的角色。

//      ///※ 加值 Role 數量
//      foreach (var funcId in authFuncList)
//        userIdentity.AddClaim(new Claim(ClaimTypes.Role, funcId));
//    }

//    //## 登入完成
//    var userAuthState = new AuthenticationState(new ClaimsPrincipal(userIdentity));
//    // NotifyAuthenticationStateChanged(Task.FromResult(userAuthState)); //※不該在此函式通知登入狀態已改變。應該在外部才對。
//    return Task.FromResult(userAuthState);
//  }

//  /// <summary>
//  /// 查看是否還在登入狀態。與 RevalidationInterval 搭配。
//  /// </summary>
//  protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authState, CancellationToken cancellationToken)
//  {
//    string userId = authState.User.Identity?.Name ?? string.Empty;
//    bool isLogged = _accSvc.IsUserLoggedIn(userId);
//    return Task.FromResult(isLogged);
//    ///※ 確認使用者仍是登入，若不是後續將會強制登出。
//  }
//}

//internal static class AuthenticationStateClassExtensions
//{
//  /// <summary>
//  /// 依據 AuthenticationState 取出完整的登入者資料。
//  /// </summary>
//  public static async Task<AuthUser?> UnpackAuthDataAsync(this AuthenticationState authState)
//  {
//    if (authState.User.Identity?.IsAuthenticated ?? false)
//    {
//      var claimsIdentity = (ClaimsIdentity)authState.User.Identity;
//      string? authUserJson = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
//      if (authUserJson == null) return null;
//      using var authUserStream = GenerateStreamFromString(authUserJson);
//      var result = await JsonSerializer.DeserializeAsync<AuthUser>(authUserStream);
//      return result!;
//    }

//    return null;
//  }

//  private static MemoryStream GenerateStreamFromString(string str)
//  {
//    var stream = new MemoryStream();
//    var writer = new StreamWriter(stream);
//    writer.Write(str);
//    writer.Flush();
//    stream.Position = 0;
//    return stream;
//  }
//}
