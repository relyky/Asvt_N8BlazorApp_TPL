using System.Runtime.CompilerServices;
using Vista.Component.Abstractions;

public abstract class CodeNameRepoBase<T> : ICodeNameRepo
  where T : ICodeName, new()
{
  public Type EntityType => typeof(T);

  /// <summary>
  /// ※ 必需繼承並實作 LoadTask 實體。
  /// </summary>
  protected abstract Task<List<T>> ConcreteLoadTask();

  /// <summary>
  /// Task：載入代碼資料。一建構就立刻（非同步）執行。
  /// </summary>
  protected Task<List<T>> LoadTask { get; init; } = default!;

  //## Resource
  protected TaskAwaiter<List<T>> _waiter;

  public Action? LoadedAct { get; set; }

  public CodeNameRepoBase()
  {
    LoadTask = ConcreteLoadTask();

    _waiter = LoadTask.GetAwaiter();
    _waiter.OnCompleted(() =>
    {
      if (LoadTask.IsFaulted)
      {
        // 把 LoadTask 例外丟出去不然看不到。
        throw LoadTask.Exception!;
      }
      else
      {
        // 載入完成。
        _codeList = LoadTask.Result;
        // 通知載入已完成。
        LoadedAct?.Invoke();
      }
    });
  }

  /// <summary>
  /// 代碼清單存放庫
  /// </summary>
  public List<ICodeName> CodeList
  {
    get
    {
      if (_codeList == null) return []; // 當載入未完成，先暫給空值。
      return _codeList.Select(c => (ICodeName)c).ToList();
    }
  }
  List<T>? _codeList = null;

  /// <summary>
  /// [option]由外部指定分群條件，用於SearchFunc。
  /// </summary>
  public string? GroupBy { get; set; }

  public async Task<IEnumerable<ICodeName>> SearchFunc(string keyword, CancellationToken token)
  {
    // 等 LoadTask 跑完並取值。會叫用此函式應資料已載入完成了。
    if (_codeList == null)
      _codeList = await LoadTask;

    return String.IsNullOrWhiteSpace(GroupBy)
      ? (IEnumerable<ICodeName>)_codeList
          .Where(c => keyword == null
                   || c.Code.StartsWith(keyword, StringComparison.CurrentCultureIgnoreCase)
                   || c.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
      : (IEnumerable<ICodeName>)_codeList
          .Where(c => GroupBy.Equals(c.Group))
          .Where(c => keyword == null
                   || c.Code.StartsWith(keyword, StringComparison.CurrentCultureIgnoreCase)
                   || c.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
  }

  /// <summary>
  /// ※使用前需先確定資料已載入。
  /// </summary>
  public ICodeName? this[string key] => CodeList.FirstOrDefault(c => c.Code == key);

  /// <summary>
  /// ※使用前需先確定資料已載入。
  /// </summary>
  public ICodeName? this[string key, string group] => CodeList.FirstOrDefault(c => c.Group.Equals(group) && c.Code == key);

  public bool IsLoaded => LoadTask.IsCompletedSuccessfully;
}
