using System.Text.RegularExpressions;

namespace Vista.Component.Abstractions;

public interface ICodeName
{
  public string Code { get;set; }
  public string Name { get; set; }
  public string Group { get; set; }
}

public interface ICodeNameRepo
{
  /// <summary>
  /// 註冊代碼名稱的實體類別
  /// </summary>
  Type EntityType { get; }

  /// <summary>
  /// 代碼名稱清單
  /// </summary>
  List<ICodeName> CodeList { get; }

  /// <summary>
  /// map code to name
  /// </summary>
  ICodeName? this[string key] { get; }

  /// <summary>
  /// map code to name
  /// </summary>
  ICodeName? this[string key, string group] { get; }

  /// <summary>
  /// AutoCompoete:SearchFunc:專案代碼
  /// </summary>
  Task<IEnumerable<ICodeName>> SearchFunc(string keyword, CancellationToken token);

  /// <summary>
  /// 是否已完成載入基本資料。
  /// </summary>
  bool IsLoaded { get; }

  /// <summary>
  /// 完成載入的通知動作
  /// </summary>
  Action? LoadedAct { get; set; }

  /// <summary>
  /// [option]由外部指定分群條件，用於SearchFunc。
  /// </summary>
  string? GroupBy { get; set; }
}

public interface ITextRepo
{
  /// <summary>
  /// 代碼名稱清單
  /// </summary>
  String[] TextList { get; }

  /// <summary>
  /// AutoCompoete:SearchFunc:專案代碼
  /// </summary>
  Task<IEnumerable<String>> SearchFunc(string keyword, CancellationToken token);

  /// <summary>
  /// 是否已完成載入基本資料。
  /// </summary>
  bool IsLoaded { get; }

  /// <summary>
  /// 完成載入的通知動作
  /// </summary>
  Action? LoadedAct { get; set; }
}
