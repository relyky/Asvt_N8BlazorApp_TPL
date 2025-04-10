using AsvtTPL.Components;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MudBlazor.Services;
using Serilog;
using System.Reflection;
using Vista.Component;
using Vista.Component.Services;
using Vista.Models;
using Vista.Services;
using WkHtmlToPdfDotNet;

try
{
  // 使用 Console 紀錄初始化過程，這樣才能讓作業系統抓到錯誤訊息。以讓【事件檢視器】查看錯誤訊息。 
  Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} Web host init.");

  var builder = WebApplication.CreateBuilder(args);
  IConfiguration config = builder.Configuration;

  #region §§ Prefix system initialization -------------------------------------
  //## 取得系統名稱，系統基本參數
  string SystemId = config["SystemID"] ?? builder.Environment.ApplicationName;
  string LogFolder = config["LogFolder"] ?? "Log";

  //## 設定 FluentValidation 取得欄位名稱方法
  FluentValidation.ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) =>
  {
    var withDisplay = member?.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
    if (withDisplay != null) return withDisplay.GetName();

    // 應該沒在用 LabelAttribute 先remark。
    //var withLabel = member?.GetCustomAttributes(typeof(MudBlazor.LabelAttribute), false).FirstOrDefault() as MudBlazor.LabelAttribute;
    //if (withLabel != null) return withLabel.Name;

    return null;
  };

  //## 資料庫連線初始化
  //var conns = AsvtSecureModule.TakeSecDbConnString("CONNSEC", "CONNLAB", "CONNGRAPH");
  //Vista.DB.DBHelper.CONNSEC = new Vista.DB.ConnProxy(conns["CONNSEC"]);
  //Vista.DB.DBHelper.MyLabDB = new Vista.DB.ConnProxy(conns["MyLabDB"]);

  //※ 或取得全部資料庫連線組態
  //Vista.DB.DBHelper.Register(config);

  //※ 或用本機保護模組解開
  Vista.DB.DBHelper.CHB_EXTEND = new Vista.DbPanda.ConnProxy("CHB_EXTEND", config);
  Vista.DB.DBHelper.CHB_MPIS = new Vista.DbPanda.ConnProxy("CHB_MPIS", config);

  //※ 或 bypass secure module
  Vista.DB.DBHelper.MyLabDB = new Vista.DbPanda.ConnProxy("Data Source=relynb2;Initial Catalog=MyLabDB;Integrated Security=True;Encrypt=False");

  #endregion

  #region §§ Serilog configuration. -------------------------------------------
  Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(builder.Configuration)
      .Enrich.WithProperty("MachineName", Environment.MachineName)
      .Enrich.WithProperty("DomainUserName", $"{Environment.UserDomainName}\\{Environment.UserName}")
      .Enrich.WithProperty("SysName", SystemId)
      .Enrich.FromLogContext()
      .WriteTo.Async(cfg =>
      {
        //# 文字檔
        cfg.File($"{LogFolder}/{SystemId}Log.txt",
              rollingInterval: RollingInterval.Day);
        //# JSON 檔
        cfg.File(new Serilog.Formatting.Json.JsonFormatter(), $"{LogFolder}/{SystemId}Log.json",
              rollingInterval: RollingInterval.Day);
      })
      .CreateLogger();
  #endregion

  builder.Host.UseSerilog();

  Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} Web host start.");
  Log.Information("Web host start.");

  #region §§ Add services to the container. -----------------------------------

  ////## 變更預設編碼成中日韓語系 --- 沒有問題就不用打開。
  ////  參考：[如何變更 ASP.NET Core MVC 的預設 HtmlEncoder 的編碼範圍](https://blog.miniasp.com/post/2023/09/01/How-to-change-HtmlEncoder-UnicodeRanges-in-ASPNET-Core?full=1&fbclid=IwAR2ljLeSiAJWozDbXMVJEFVoeVW5wUOjWK65p5xNC0Qr4CQptyLCdnE9viM)
  //builder.Services.AddSingleton(System.Text.Encodings.Web.HtmlEncoder.Create(allowedRanges: new[] {
  //    System.Text.Unicode.UnicodeRanges.BasicLatin,
  //    System.Text.Unicode.UnicodeRanges.CjkUnifiedIdeographs
  //}));

  //## for 多國語系(指定預設語系)
  /// NET8 多國語系改變了，ref→[ASP.NET Core Blazor 全球化和當地語系化-8.0](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/globalization-localization?view=aspnetcore-8.0)
  /// 預設抓取作業系統語系。
  //builder.Services.AddLocalization();

  builder.Services.AddControllers(); // for enable WebApi
  //builder.Services.AddEndpointsApiExplorer();

  // 註冊：DI服務
  builder.Services.AddHealthChecks();
  builder.Services.AddMemoryCache();         // IMemoryCache
  builder.Services.AddHttpContextAccessor(); // IHttpContextAccessor
  //builder.Services.AddHttpClient();        // IHttpClientFactory

  // 註冊：message bus between components (註冊成 Singleton 會更好用。)
  builder.Services.AddSingleton<BlazorComponentBus.ComponentBus>();

  // 註冊：WkHtmlToPdfDotNet, 必需是Singleton。
  builder.Services.AddSingleton(typeof(WkHtmlToPdfDotNet.Contracts.IConverter), new SynchronizedConverter(new PdfTools()));

  // 註冊：客製服務
  builder.Services.AddSingleton<AccountService>();
  builder.Services.AddTransient<MyIdleEventHandler>(); // [實驗性機制] for AddIdleCircuitHandler 

  // 註冊 Vista.Biz 中名稱結尾為 "Biz" 的服務
  foreach (var bizType in (Assembly.GetAssembly(typeof(Vista.Biz.DEMO.SampleBiz))!.GetTypes())
    .Where(t => t.Name.EndsWith("Biz")))
  {
    builder.Services.AddScoped(bizType);
  }

  // 註冊：MudBlazor 與延伸功能延伸
  builder.Services.AddMudServices()
    .AddVistaComponentServices()
    .AddIdleCircuitHandler<MyIdleEventHandler>(options => options.IdleTimeout = TimeSpan.FromSeconds(7)); // [實驗性機制]

  // for Interactive mode
  // 並拉大 SignalR 上傳限制到 640KB 以上傳圖片，預設為32KB。
  // ref → [Size limits on JavaScript interop calls](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-8.0)
  builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents();
      //.AddHubOptions(options => options.MaximumReceiveMessageSize = 640000);

  //# for BLAZOR COOKIE Auth
  /// 參考：[針對錯誤進行疑難排解](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/security/?view=aspnetcore-8.0#troubleshoot-errors)
  /// 在.NET 8 或更新版本中，請勿使用 <CascadingAuthenticationState /> 元件；改用 builder.Services.SAddCascadingAuthenticationState();
  builder.Services.AddCascadingAuthenticationState();
  //builder.Services.AddScoped<AuthenticationStateProvider, Custom2AuthenticationStateProvider>(); 
  builder.Services.AddScoped<IAuthUser, AuthUserService>();
  builder.Services.AddHttpContextAccessor();
  /// ※ 不要在前端元件使用 IHttpContextAccesser 因為 HttpContext 的狀態並非即時反應的，
  /// 成因是 SignalR 與 HTTP 是不同的通訊協定且原則上不能共存。
  /// 但 Blazor Server App 又支援 Cookie Authentication。
  /// 改用 AuthenticationStateProvider 機制取登入狀態。

  //## for BLAZOR COOKIE Auth
  /// ref → https://blazorhelpwebsite.com/ViewBlogPost/36
  builder.Services.Configure<CookiePolicyOptions>(options =>
  {
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
    options.CheckConsentNeeded = context => true;
    options.ConsentCookie.Name = "GDPRConsent";
  });
  builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
      .AddCookie(options =>
      {
        options.LoginPath = "/login"; // default: /Accout/Login
        options.Cookie.Name = ".AsvtTPL.Cookies"; //default:.AspNetCore.Cookies
      });

  // 註冊 客製 Policy 與滿足它所要的需求 Authorization Requirement
  builder.Services.AddAuthorization(options =>
  {
    options.AddPolicy("AuthPage", policy => policy.Requirements.Add(new AuthPageRequirement()));
  });

  // 註冊 客製化授權需求 Authorization Requirement。
  // ※ 將自動觸發對應的檢驗程序 
  builder.Services.AddSingleton<IAuthorizationHandler, AuthPageHandler>();

  builder.Services.AddAntiforgery();

  #endregion

  var app = builder.Build();

  #region §§ Configure the HTTP request pipeline. -----------------------------
  /// ref → [中介軟體順序](https://learn.microsoft.com/zh-tw/aspnet/core/fundamentals/middleware/?view=aspnetcore-8.0#middleware-order)

  //## 註冊 IServiceProvider 備用
  Vista.AOP.ServiceActivator.Configure(app.Services);

  if (!app.Environment.IsDevelopment())
  {
    //# [停用熱重新載入的回應壓縮](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/fundamentals/signalr?preserve-view=true&view=aspnetcore-8.0#disable-response-compression-for-hot-reload)
    //app.UseResponseCompression(); // 正式版發行後會啟動失敗的樣子？

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
  }

  app.UseHttpsRedirection();
  app.UseStaticFiles();
  app.UseAntiforgery(); // for SSG form 

  //## for 多國語系(指定預設語系)
  /// NET8 多國語系改變了，ref→[ASP.NET Core Blazor 全球化和當地語系化-8.0](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/globalization-localization?view=aspnetcore-8.0)
  //app.UseRequestLocalization("zh-TW"); // 指定預設語系。

  //# for BLAZOR COOKIE Auth
  app.UseCookiePolicy();
  app.UseAuthentication();
  app.UseAuthorization();

  //## HTTP 400-599 錯誤處理 
  /// 根據預設，ASP.NET Core 應用程式不會提供 HTTP 錯誤狀態碼 (例如「404 - 找不到」) 的狀態碼頁面。 
  /// 當應用程式設定沒有本文的 HTTP 400-599 錯誤狀態碼時，它會傳回該狀態碼和一個空白的回應本文。 
  /// ref→https://learn.microsoft.com/zh-tw/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#usestatuscodepages
  app.UseStatusCodePagesWithRedirects("/ErrorStatus/{0}");

  //## Endpoints
  app.MapHealthChecks("/healthz");
  app.MapControllers(); // for enable WebApi

  // for Interactive mode
  app.MapRazorComponents<App>()
      .AddInteractiveServerRenderMode();

  #endregion

  app.Run();

  Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} Web host exit.");
  Log.Information("Web host exit.");
}
catch (Exception ex)
{
  // 使用 Console 紀錄初始化過程，這樣才能讓作業系統抓到錯誤訊息。以讓【事件檢視器】查看錯誤訊息。
  Console.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss} Web host terminated unexpectedly!.\r\n{ex}");
  Log.Fatal(ex, "Web host terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}
