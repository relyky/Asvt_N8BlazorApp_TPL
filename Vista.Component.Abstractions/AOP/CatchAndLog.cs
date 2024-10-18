using AspectInjector.Broker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace Vista.AOP;

/// <summary>
/// 掛在類別上的AOP。將針對類別內的 Method 加掛 Catch-And-Log。建議用在 Biz 層。
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[Injection(typeof(CatchAndLogAspect))]
public class CatchAndLogAttribute : Attribute
{
  public string Title { get; init; }

  public CatchAndLogAttribute(string Title)
  {
    this.Title = Title;
  }
}

/// <summary>
/// 抬頭屬性，將被用於 CatchAndLogAspect。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LogTitleAttribute : Attribute
{
  public string Title { get; init; }

  public LogTitleAttribute(string Title)
  {
    this.Title = Title;
  }
}

/// <summary>
/// 忽略屬性，將被用於 CatchAndLogAspect。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LogIgnoreAttribute : Attribute { }

[Aspect(Scope.Global)]
public class CatchAndLogAspect
{
  [DebuggerHidden]
  [Advice(Kind.Around, Targets = Target.Method)]
  public object Around(
      [Argument(Source.Name)] string name,
      [Argument(Source.Triggers)] Attribute[] triggers,
      [Argument(Source.Type)] Type cls,
      [Argument(Source.Metadata)] System.Reflection.MethodBase meta,
      [Argument(Source.Target)] Func<object[], object> func,
      [Argument(Source.Instance)] object instance,
      [Argument(Source.Arguments)] object[] args,
      [Argument(Source.ReturnType)] Type retType)
  {
    var catchAttr = triggers.FirstOrDefault(c => c is CatchAndLogAttribute) as CatchAndLogAttribute;
    var titleAttr = meta.GetCustomAttributes(typeof(LogTitleAttribute), false).FirstOrDefault() as LogTitleAttribute;
    var ignoreAttr = meta.GetCustomAttributes(typeof(LogIgnoreAttribute), false).FirstOrDefault() as LogIgnoreAttribute;

    #region 特例：若是忽略模式則直接離開。
    if (ignoreAttr != null)
      return func.Invoke(args);
    #endregion

    // 自 DI 取得資源
    using var serviceScope = ServiceActivator.CreateScope();
    var _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<CatchAndLogAspect>>();

    // GO
    using (LogContext.PushProperty("ClassName", cls.Name))
    using (LogContext.PushProperty("MethodName", name))
    {
      /// logTitle := {className}.{methodName}
      string logTitle = $"{catchAttr?.Title ?? cls.Name}.{titleAttr?.Title ?? name}";
      try
      {
        ///※ 可以在這裡 catch, retry, authenticate, .... 

        //※ around begin
        /// Log → [BEFORE] {method-name}
        _logger.Log(LogLevel.Debug, $"[Method BEGIN] {logTitle}");

        //var sw = Stopwatch.StartNew(); // 開始計時(官方建議的計時指令)
        var ret = func.Invoke(args);
        //sw.Stop(); // 計時結束
        //sw.ElapsedMilliseconds // 花費時間(保留備用)

        //※ around end
        /// Log → [After] {method-name}
        _logger.Log(LogLevel.Information, $"[Method DONE] {logTitle}");

        return ret; //※ 可以改變處理結果。
      }
      catch (Exception ex)
      {
        //※ catch exception
        _logger.Log(LogLevel.Error, ex, $"[Method CATCH] {logTitle} => Exception:{ex.Message}");
        throw;
      }
    }
  }
}
