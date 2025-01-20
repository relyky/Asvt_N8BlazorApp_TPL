using Vista.DbPanda;

namespace Vista.DB;

/// <summary>
/// 資料庫連線標的
/// 一個資料庫一個 ConnProxy 依需求加入。
/// </summary>
class DBHelper
{
  /// <summary>
  /// 安控資料庫：授權資料、連線資料、系統級紀錄。
  /// </summary>
  public static ConnProxy CONNSEC = default!;

  /// <summary>
  /// 主資料庫：與資料庫名稱相同就好。
  /// </summary>
  public static ConnProxy MyLabDB = default!;

  /// <summary>
  /// 測試資料庫。
  /// </summary>
  public static ConnProxy CHB_EXTEND = default!;

  /// <summary>
  /// 測試資料庫。
  /// </summary>
  public static ConnProxy CHB_MPIS = default!;


  /// <summary>
  /// 用來測試連線，不必初始化。
  /// </summary>
  public static ConnProxy CONNTEST = default!;

  /// 其他需要的資料庫...
}
