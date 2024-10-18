using System.Collections.Immutable;

namespace Vista.Component.Abstractions;

/// <summary>
/// 適用於下拉項目少於２０項的簡單選取。
/// </summary>
public class SelectOptionRepo
{
  private Dictionary<string, string> _options;

  public SelectOptionRepo()
  {
    _options = new Dictionary<string, string>();
  }

  public SelectOptionRepo(Dictionary<string, string> options)
  {
    _options = options;
  }

  public void Add(string code, string name)
  {
    _options.Add(code, name);
  }

  /// <summary>
  /// 轉型成選取清單
  /// </summary>
  public CodeName[] SelectList => _options.Select(c => new CodeName { Code = c.Key, Name = c.Value }).ToArray();

  /// <summary>
  /// map[Value] = Text
  /// </summary>
  public String this[string? value]
  {
    get
    {
      try
      {
        return value is null
          ? string.Empty
          : _options[value];
      }
      catch
      {
        return String.Empty;
      }
    }
  }

  public static implicit operator ICodeName[](SelectOptionRepo repo) => repo.SelectList;
}