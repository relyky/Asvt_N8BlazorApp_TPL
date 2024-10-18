using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Vista.Component.Services;
using Vista.Services;

namespace Vista.Component;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddVistaComponentServices(this IServiceCollection services)
  {
    //註冊：客製服務
    services.AddScoped<ExampleJsInterop>();
    services.AddScoped<IDialogServiceEx, CommDialogService>();
    services.AddScoped<DateMaskJsInterop>();
    services.AddScoped<JSInterOpService>();
    return services;
  }
}
