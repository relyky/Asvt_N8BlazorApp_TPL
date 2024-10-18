using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vista.Biz.DEMO;
using Vista.Models;

namespace ZPlaywrightTestProject;

[TestClass]
public abstract class BizTestBase
{
  // singleton
  protected static IConfigurationRoot config = default!;
  protected static ServiceProvider serviceProvider = default!;

  // scope
  protected IServiceScope scope = default!;

  [AssemblyInitialize]
  public static void AssemblyInitialize(TestContext testContext)
  {
    //# 可自 appsettings.json 取測試組態參數
    // 需安裝套件: Microsoft.Extensions.Configuration.Json。
    var configBuilder = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    config = configBuilder.Build();

    //# 註冊 DB 連線
    Vista.DB.DBHelper.CONNSEC = new Vista.DbPanda.ConnProxy(config.GetConnectionString("DefaultConnection")!);

    //# 註冊 DI 服務
    var services = new ServiceCollection();
    services.AddLogging();
    //services.AddSingleton<IConfiguration>(config);
    services.AddScoped<IAuthUser, ATestUser>();
    services.AddScoped<DEMO009Biz>();

    //# 引入 AOP 
    serviceProvider = services.BuildServiceProvider();
    Vista.AOP.ServiceActivator.Configure(serviceProvider);
  }

  [AssemblyCleanup]
  public static void AssemblyCleanup()
  {
    serviceProvider?.Dispose();
    serviceProvider = null!;
  }

  /// <summary>
  /// 每個測試都會跑一次啟用
  /// </summary>
  [TestInitialize()]
  public void TestInitialize()
  {
    scope = serviceProvider.CreateScope();
  }

  /// <summary>
  /// 每個測試都會跑一次清除
  /// </summary>
  [TestCleanup()]
  public void TestCleanup()
  {
    scope?.Dispose();
    scope = null!;
  }
}

