using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor;
using Vista.Component.Services;

namespace Vista.Services;

/// <summary>
/// [實驗性機制]
/// Custom delegating handler for adding Auth headers to outbound requests
/// </summary>
internal sealed class MyIdleEventHandler(
  ILogger<MyIdleEventHandler> _logger,
  ISnackbar _snackSvc,
  NavigationManager _navSvc) : IIdleEventHandler
{

  public Task InvokeAsync(Circuit currentCircuit)
  {
    _logger.LogDebug($"Circuit [{currentCircuit.Id}] is idle at {DateTime.Now:yyyy/MM/dd HH:mm:ss}.");

    //_snackSvc.Add($"Circuit is idle at {DateTime.Now:HH:mm:ss}.", Severity.Info);
    _snackSvc.Add($"偵測到閒置中 {DateTime.Now:HH:mm:ss}", Severity.Normal);

    //_navSvc.NavigateTo(_navSvc.BaseUri, true);

    return Task.CompletedTask;
  }
}




