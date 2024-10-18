using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Vista.Component.Abstractions;

public abstract class TextRepoBase : ITextRepo
{
  /// <summary>
  /// ※ 必需繼承並實作 LoadTask 實體。
  /// </summary>
  protected abstract Task<string[]> ConcreteLoadTask();

  /// <summary>
  /// Task：載入代碼資料。一建構就立刻（非同步）執行。
  /// </summary>
  protected Task<string[]> LoadTask { get; init; } = default!;

  //## Resource
  protected TaskAwaiter<string[]> _waiter;

  public Action? LoadedAct { get; set; }

  public TextRepoBase()
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
        _textList = LoadTask.Result;
        // 通知載入已完成。
        LoadedAct?.Invoke();
      }
    });
  }

  /// <summary>
  /// 代碼清單存放庫
  /// </summary>
  public string[] TextList
  {
    get
    {
      if (_textList == null) return []; // 當載入未完成，先暫給空值。
      return _textList;
    }
  }
  string[]? _textList = null;

  public async Task<IEnumerable<string>> SearchFunc(string keyword, CancellationToken token)
  {
    // 等 LoadTask 跑完並取值。會叫用此函式應資料已載入完成了。
    if (_textList == null)
      _textList = await LoadTask;

    return String.IsNullOrEmpty(keyword)
      ? _textList
      : _textList.Where(c => c.Contains(keyword, StringComparison.CurrentCultureIgnoreCase));
  }

  public bool IsLoaded => LoadTask.IsCompletedSuccessfully;
}