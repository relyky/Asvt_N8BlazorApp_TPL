using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace Vista.Component.Services;

/// <summary>
/// Monitor server-side circuit activity
/// 參考：[監視伺服器端線路活動](https://learn.microsoft.com/zh-tw/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-8.0#monitor-server-side-circuit-activity)
/// </summary>
public class IdleCircuitHandler : CircuitHandler, IDisposable
{
  // jnject
  readonly ILogger<IdleCircuitHandler> _logger;
  readonly Timer _timer;
  readonly IServiceProvider _provider;
  readonly Type _handlerType;

  // state
  Circuit? currentCircuit;

  public void Dispose() => _timer?.Dispose();

  public IdleCircuitHandler(Type handlerType, ILogger<IdleCircuitHandler> logger, IOptions<IdleCircuitOptions> options, IServiceProvider provider)
  {
    _timer = new Timer
    {
      Interval = options.Value.IdleTimeout.TotalMilliseconds,
      AutoReset = false
    };

    _timer.Elapsed += OnCircuitIdle;

    _handlerType = handlerType;
    _logger = logger;
    _provider = provider;
  }

  public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
  {
    currentCircuit = circuit;
    return Task.CompletedTask;
  }

  public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(Func<CircuitInboundActivityContext, Task> next)
  {
    return context =>
    {
      _timer.Stop();
      _timer.Start();

      return next(context);
    };
  }

  /// <summary>
  /// timer event handler
  /// </summary>
  void OnCircuitIdle(object? sender, System.Timers.ElapsedEventArgs e)
  {
    IIdleEventHandler handler = (IIdleEventHandler)_provider.GetRequiredService(_handlerType);
    handler.InvokeAsync(currentCircuit!);
  }
}

/// <summary>
/// IdleCircuitHandler 參數
/// </summary>
public class IdleCircuitOptions
{
  public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

public interface IIdleEventHandler
{
  public Task InvokeAsync(Circuit currentCircuit);
}

/// <summary>
/// 用於註冊：IdleCircuitHandler
/// builder.Services.Configure<IdleCircuitOptions>((options) => options.IdleTimeout = TimeSpan.FromSeconds(1)); // 指定參數。
/// builder.Services.AddScoped<CircuitHandler, IdleCircuitHandler>();
/// </summary>
public static class IdleCircuitHandlerServiceCollectionExtensions
{
  public static IServiceCollection AddIdleCircuitHandler<THandler>(
      this IServiceCollection services,
      Action<IdleCircuitOptions> configureOptions
    ) where THandler : IIdleEventHandler
  {
    services.Configure(configureOptions);
    services.AddIdleCircuitHandler<THandler>();
    return services;
  }

  public static IServiceCollection AddIdleCircuitHandler<THandler>(
      this IServiceCollection services) where THandler : IIdleEventHandler
  {
    services.AddScoped<CircuitHandler, IdleCircuitHandler>(provider => new IdleCircuitHandler(
        typeof(THandler),
        provider.GetRequiredService<ILogger<IdleCircuitHandler>>(),
        provider.GetRequiredService<IOptions<IdleCircuitOptions>>(),
        provider));

    return services;
  }
}
