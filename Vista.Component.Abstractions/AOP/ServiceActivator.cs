using Microsoft.Extensions.DependencyInjection;
using System;

namespace Vista.AOP;

/// <summary>
/// Add static service resolver to use when dependencies injection is not available
/// ref → [Resolving instances with ASP.NET Core DI in static classes](https://www.davidezoccarato.cloud/resolving-instances-with-asp-net-core-di-in-static-classes/)
/// 請在 Startup 註冊 IServiceProvider。
/// </summary>
public class ServiceActivator
{
  static IServiceProvider _serviceProvider
  {
    get => (IServiceProvider)AppDomain.CurrentDomain.GetData("AppHost:ServiceProvider")!;
    set => AppDomain.CurrentDomain.SetData("AppHost:ServiceProvider", value);
  }

  /// <summary>
  /// Configure ServiceActivator with full serviceProvider
  /// </summary>
  public static void Configure(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  /// <summary>
  /// Create a scope where use this ServiceActivator
  /// </summary>
  public static IServiceScope CreateScope() => _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
}
