using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Vista.Component.Shared;

namespace Vista.Services;

/// <summary>
/// 補充 IDialogService 不滿足的功能。
/// </summary>
public interface IDialogServiceEx : IDialogService
{
  /// <summary>
  /// 搭配 <Shared\AlertDialog /> 元件顯示訊息。
  /// 將直接進行下一步驟。設計用於最後步驟。
  /// </summary>
  void ShowAlert(string message, Color color = Color.Error, string title = "訊息");

  /// <summary>
  /// 搭配 <Shared\AlertDialog /> 元件顯示訊息。
  /// 等 User 按下確認鈕才進行下一步驟。。
  /// </summary>
  Task ShowAlertAsync(string message, Color color = Color.Info, string title = "訊息");

  Task<bool> ConfirmAsync(string message, Color color = Color.Warning, string title = "再確認一次？");

  /// <summary>
  /// 簡化呼叫 Dialog 步驟
  /// </summary>
  /// <param name="parameters">請使用 anonymous type 傳遞參數。</param>
  /// <example>
  /// var result = await dlgSvc.ShowExAsync<DialogReconfirm>(new { FormData = formData });
  /// if (!result.Canceled && "Yes".Equals(result.Data))
  /// {
  ///   dlgSvc.ShowAlert($"已送出申請，申請單號：{formData.formNo}", type: Severity.Success);
  /// }
  /// else
  /// {
  ///   dlgSvc.ShowAlert($"已取消申請，申請單號：{formData.formNo}", type: Severity.Warning);
  /// }
  /// </example>
  Task<DialogResult> ShowExAsync<TComponent>(object? parameters = null, MaxWidth maxWidth = MaxWidth.Medium, DialogPosition position = DialogPosition.TopCenter)
    where TComponent : ComponentBase;
}

/// <summary>
/// 實作 IDialogServiceEx 介面。
/// </summary>
sealed class CommDialogService(IDialogService _dialog)
  : IDialogServiceEx
{
  /// <summary>
  /// 搭配 <Shared\AlertDialog /> 元件顯示訊息。※ 此 AlertDialog 是非同步的。
  /// </summary>
  public void ShowAlert(string message, Color color = Color.Error, string title = "訊息")
  {
    _ = ShowAlertAsync(message, color, title);
  }

  public async Task ShowAlertAsync(string message, Color color = Color.Info, string title = "訊息")
  {
    var parameters = new DialogParameters();
    parameters.Add("ContentText", message);
    parameters.Add("Title", title);
    parameters.Add("Color", color);

    await _dialog.Show<AlertDialog>(title, parameters).Result;
    // ※ 此 AlertDialog 是非同步的。
  }

  /// <summary>
  /// 搭配 <Shared\ConfirmDialog /> 元件顯示訊息。
  /// </summary>
  public async Task<bool> ConfirmAsync(string message, Color color = Color.Warning, string title = "再確認一次？")
  {
    var parameters = new DialogParameters();
    parameters.Add("ContentText", message);
    parameters.Add("Title", title);
    parameters.Add("Color", color);

    var res = await _dialog.Show<ConfirmDialog>(title, parameters).Result;
    return !res.Canceled;
  }

  public async Task<DialogResult> ShowExAsync<TComponent>(object? parameters, MaxWidth maxWidth, DialogPosition position)
    where TComponent : ComponentBase
  {
    var option = new DialogOptions
    {
      MaxWidth = maxWidth,
      Position = position,
      CloseButton = true,
      FullWidth = true
    };
    /// 把 Dialog 拉到最寬比較容易設計介面，不然會動態縮放又寬又窄的。

    DialogParameters prms = new DialogParameters();
    if (parameters != null)
    {
      if (!parameters.GetType().IsGenericType)
        throw new ApplicationException("ShowExAsync 函式參數 parameters 的型別必需是 anonymous type。");

      foreach (PropertyInfo pi in parameters.GetType().GetProperties())
      {
        prms.Add(pi.Name, pi.GetValue(parameters));
      }
    }

    var dialog = await _dialog.ShowAsync<TComponent>(null, prms, option);
    var result = await dialog.Result;
    return result;
  }

  #region 實作 IDialogService 介面
  event Action<IDialogReference>? IDialogService.OnDialogInstanceAdded
  {
    add => _dialog.OnDialogInstanceAdded += value;
    remove => _dialog.OnDialogInstanceAdded -= value;
  }

  event Action<IDialogReference, DialogResult?>? IDialogService.OnDialogCloseRequested
  {
    add => _dialog.OnDialogCloseRequested += value;
    remove => _dialog.OnDialogCloseRequested -= value;
  }

  IDialogReference IDialogService.Show<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>()
  {
    return _dialog.Show<TComponent>();
  }

  IDialogReference IDialogService.Show<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title)
  {
    return _dialog.Show<TComponent>(title);
  }

  IDialogReference IDialogService.Show<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title, DialogOptions options)
  {
    return _dialog.Show<TComponent>(title, options);
  }

  IDialogReference IDialogService.Show<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title, DialogParameters parameters)
  {
    return _dialog.Show<TComponent>(title, parameters);
  }

  IDialogReference IDialogService.Show<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title, DialogParameters parameters, DialogOptions? options)
  {
    return _dialog.Show<TComponent>(title, parameters, options);
  }

  IDialogReference IDialogService.Show(Type component)
  {
    return _dialog.Show(component);
  }

  IDialogReference IDialogService.Show(Type component, string? title)
  {
    return _dialog.Show(component, title);
  }

  IDialogReference IDialogService.Show(Type component, string? title, DialogOptions options)
  {
    return _dialog.Show(component, title, options);
  }

  IDialogReference IDialogService.Show(Type component, string? title, DialogParameters parameters)
  {
    return _dialog.Show(component, title, parameters);
  }

  IDialogReference IDialogService.Show(Type component, string? title, DialogParameters parameters, DialogOptions options)
  {
    return _dialog.Show(component, title, parameters, options);
  }

  Task<IDialogReference> IDialogService.ShowAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>()
  {
    return _dialog.ShowAsync<TComponent>();
  }

  Task<IDialogReference> IDialogService.ShowAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title)
  {
    return _dialog.ShowAsync<TComponent>(title);
  }

  Task<IDialogReference> IDialogService.ShowAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title, DialogOptions options)
  {
    return _dialog.ShowAsync<TComponent>(title, options);
  }

  Task<IDialogReference> IDialogService.ShowAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title, DialogParameters parameters)
  {
    return _dialog.ShowAsync<TComponent>(title, parameters);
  }

  Task<IDialogReference> IDialogService.ShowAsync<[DynamicallyAccessedMembers((DynamicallyAccessedMemberTypes)(-1))] TComponent>(string? title, DialogParameters parameters, DialogOptions? options)
  {
    return _dialog.ShowAsync<TComponent>(title, parameters, options);
  }

  Task<IDialogReference> IDialogService.ShowAsync(Type component)
  {
    return _dialog.ShowAsync(component);
  }

  Task<IDialogReference> IDialogService.ShowAsync(Type component, string? title)
  {
    return _dialog.ShowAsync(component, title);
  }

  Task<IDialogReference> IDialogService.ShowAsync(Type component, string? title, DialogOptions options)
  {
    return _dialog.ShowAsync(component, title, options);
  }

  Task<IDialogReference> IDialogService.ShowAsync(Type component, string? title, DialogParameters parameters)
  {
    return _dialog.ShowAsync(component, title, parameters);
  }

  Task<IDialogReference> IDialogService.ShowAsync(Type component, string? title, DialogParameters parameters, DialogOptions options)
  {
    return _dialog.ShowAsync(component, title, parameters, options);
  }

  IDialogReference IDialogService.CreateReference()
  {
    return _dialog.CreateReference();
  }

  Task<bool?> IDialogService.ShowMessageBox(string? title, string message, string yesText, string? noText, string? cancelText, DialogOptions? options)
  {
    return _dialog.ShowMessageBox(title, message, yesText, noText, cancelText, options);
  }

  Task<bool?> IDialogService.ShowMessageBox(string? title, MarkupString markupMessage, string yesText, string? noText, string? cancelText, DialogOptions? options)
  {
    return _dialog.ShowMessageBox(title, markupMessage, yesText, noText, cancelText, options);
  }

  Task<bool?> IDialogService.ShowMessageBox(MessageBoxOptions messageBoxOptions, DialogOptions? options)
  {
    return _dialog.ShowMessageBox(messageBoxOptions, options);
  }

  void IDialogService.Close(IDialogReference dialog)
  {
    _dialog.Close(dialog);
  }

  void IDialogService.Close(IDialogReference dialog, DialogResult? result)
  {
    _dialog.Close(dialog, result);
  }
  #endregion
}
