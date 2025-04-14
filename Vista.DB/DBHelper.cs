using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Immutable;
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
  /// 用來測試連線，不必初始化。
  /// </summary>
  public static ConnProxy CONNTEST = default!;

  /// 其他需要的資料庫...

  /// <summary>
  /// 登記所有 DB 連線組態。
  /// </summary>
  public static void Register(IConfiguration config)
  {
    //※ 假設 CONNSEC/CONNSSO 已經取得。
    DBHelper.CONNSEC = new ConnProxy("CONNSSO", config);

    //## ※ 再依此取得並註冊其他連線組態。
    using var conn = DBHelper.CONNSEC.Open();
    var conns = conn.Query(@"SELECT ConnID, ConnStr FROM SecConnectionPool (NOLOCK)")
                    .ToImmutableDictionary(c => (string)c.ConnID, c => (string)c.ConnStr);

    //# 註冊其他連線組態。
    DBHelper.MyLabDB = new ConnProxy(conns["MyLabDB"]);
    //DBHelper.CHB_EXTEND = new ConnProxy(conns["CHB_EXTEND"]);
    //DBHelper.CHB_MPIS = new ConnProxy(conns["CHB_MPIS"]);
  }
}
