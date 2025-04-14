using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.AOP;
using Vista.DB;
using Vista.DB.Schema;
using Vista.DbPanda;

namespace Vista.Biz.DEMO;

[CatchAndLog("各項機制測試Biz")]
class DEMO007Biz
{
  [LogTitle("查詢資料清單")]
  public List<MyProduct> QryDataList()
  {
    using var conn = DBHelper.MyLabDB.Open();
    string sql = @"SELECT * FROM MyProduct";
    var dataList = conn.Query<MyProduct>(sql).AsList();
    return dataList;
  }

  [LogTitle("加入一筆數據")]
  public long InsertData()
  {
    using var conn = DBHelper.MyLabDB.Open();
    using var txn = conn.BeginTransaction();

    long sn = conn.InsertEx<MyProduct>(new MyProduct { Title = $"隨意加入一筆@{DateTime.Now:mmss}。", Status = "Testing" }, txn);
    txn.Commit();

    return sn;
  }

  [LogTitle("更新一筆數據")]
  public void UpdateData(long Sn)
  {
    using var conn = DBHelper.MyLabDB.Open();
    using var txn = conn.BeginTransaction();

    var info = conn.GetEx<MyProduct>(new { Sn }, txn);
    info.Title = $"隨意更新@{DateTime.Now:mmss}";

    conn.UpdateEx<MyProduct>(info, new { Sn }, txn);
    txn.Commit();
  }
}
