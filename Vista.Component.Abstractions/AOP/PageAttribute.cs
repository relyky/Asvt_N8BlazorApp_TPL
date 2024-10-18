using AspectInjector.Broker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace Vista.AOP;

[AttributeUsage(AttributeTargets.Class)]
[Injection(typeof(PageAspect))]
public class PageAttribute : Attribute
{
  public PageAttribute(string id, string name)
  {
    FuncId = id;
    FuncName = name;
  }

  public string FuncId { get; }
  public string FuncName { get; }
}

[Aspect(Scope.Global)]
public class PageAspect
{
  [DebuggerHidden]
  [Advice(Kind.Before, Targets = Target.Constructor)]
  public void Instance(
      [Argument(Source.Name)] string name,
      [Argument(Source.Triggers)] Attribute[] triggers,
      [Argument(Source.Type)] Type cls)
  {
    var pageAttr = triggers.FirstOrDefault(c => c is PageAttribute) as PageAttribute;

    // 自 DI 取得資源
    using var serviceScope = ServiceActivator.CreateScope();
    var _logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<PageAspect>>();

    //※ log before
    using (LogContext.PushProperty("ClassName", cls.Name))
    using (LogContext.PushProperty("MethodName", name))
    {
      _logger.Log(LogLevel.Information, $"[Page Entry] {pageAttr?.FuncId ?? cls.Name} {pageAttr?.FuncName}。");
    }
  }
}
