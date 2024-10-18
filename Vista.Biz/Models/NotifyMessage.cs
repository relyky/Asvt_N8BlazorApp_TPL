using System.ComponentModel;

namespace Vista.Models;

public class NotifyMessage
{
  public DateTime MsgTime { get; set; } = DateTime.Now;
  public string Message { get; set; } = string.Empty;
  public MudSeverity Severity { get; set; } = MudSeverity.Info;
}

/// <summary>
/// 與 MudBlazor.Severity 一致
/// </summary>
public enum MudSeverity
{
  [Description("normal")]
  Normal,
  [Description("info")]
  Info,
  [Description("success")]
  Success,
  [Description("warning")]
  Warning,
  [Description("error")]
  Error
}