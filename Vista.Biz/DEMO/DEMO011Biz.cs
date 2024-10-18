using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Vista.AOP;
using Vista.Models;

namespace Vista.Biz.DEMO;

[CatchAndLog("ＣＲＵＤ少欄位樣板")]
class DEMO011Biz(ILogger<DEMO011Biz> _logger, IAuthUser _auth)
{
  [LogTitle("查詢資料清單")]
  public List<DEMO011FormData> QryDataList(int topCount, string keyWord)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# 模擬自DB查詢資料
    var dataList = Enumerable.Range(1, topCount).Select(i => new DEMO011FormData
    {
      formNo = $"SIMS{i:D4}",
      dataFieldA = "dataFieldA",
      dataFieldB = "dataFieldB",
      dataFieldC = "dataFieldC"
    }).ToList();

    //DynamicParameters param = new DynamicParameters(); // Dapper 動態參數
    //StringBuilder sql = new StringBuilder();
    //sql.Append(@"SELECT TOP(@topCount) * FROM DEMO011FormData WHERE 1=1 ");
    //
    //param.Add("topCount", topCount);
    //
    //// 依條件動態加入查詢參數
    //if (!String.IsNullOrWhiteSpace(keyWord))
    //{
    //  sql.Append("AND Message like @msg ");
    //  param.Add("msg", $"%{keyWord}%");
    //}
    //
    //// 排序
    //sql.Append("ORDER BY BgnDate DESC ");
    //
    //// Dapper Query
    //using var conn = DBHelper.CONNDB.Open();
    //var dataList = conn.Query<DEMO011FormData>(sql.ToString(), param).AsList();

    return dataList;
  }

  [LogTitle("新增資料")]
  public (DEMO011FormData, string) AddFormData(DEMO011FormData formData)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# biz checking
    var validator = new DEMO011FormDataValidator();
    var chk = validator.Validate(formData);
    if (!chk.IsValid)
      return (formData, String.Join("；", chk.Errors.Select(c => c.ErrorMessage)));

    //# resource
    var user = _auth.GetCurrentUser();
    var now = DateTime.Now;

    ////# 連接DB啟動交易
    //using var conn = DBHelper.CONNDB.Open();
    //using var txn = conn.BeginTransaction();

    ////...

    //# 自動填入系統欄位
    //formData.AddUser = user.UserId;
    //formData.AddDtm = now;
    //formData.UpdUser = user.UserId;
    //formData.UpdDtm = now;

    ////# Do Access DB
    //conn.InsertEx<DEMO011FormData>(formData, txn);
    //txn.Commit();

    //# 成功
    return (formData, "SUCCESS");
  }

  [LogTitle("更新資料")]
  public (DEMO011FormData, string) UpdFormData(DEMO011FormData formData)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# biz checking
    var validator = new DEMO011FormDataValidator();
    var chk = validator.Validate(formData);
    if (!chk.IsValid)
      return (formData, String.Join("；", chk.Errors.Select(c => c.ErrorMessage)));

    //# resource
    var user = _auth.GetCurrentUser();
    var now = DateTime.Now;

    ////# 連接DB啟動交易
    //using var conn = DBHelper.CONNDB.Open();
    //using var txn = conn.BeginTransaction();

    //...

    ////# 自動填入系統欄位
    //formData.UpdUser = user.UserId;
    //formData.UPdDtm = now;

    ////# Do Access DB
    //conn.UpdateEx<DEMO011FormData>(formData, new { formNo = formData }, txn);
    //txn.Commit();

    //# 成功
    return (formData, "SUCCESS");
  }

  [LogTitle("刪除資料")]
  public int DelFormData(DEMO011FormData aim)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    ////# 連接DB啟動交易
    //using var conn = DBHelper.CONNDB.Open();
    //using var txn = conn.BeginTransaction();

    ////# Do Access DB
    //int ret = conn.DeleteEx<DEMO011FormData>(new { formNo = aim.formNo }, txn);
    //txn.Commit();

    int ret = 1; //# 模擬成功

    return ret;
  }
}

/// <summary>
/// 資料表單
/// </summary>
public class DEMO011FormData
{
  [Display(Name = "表單號碼")]
  public string? formNo { get; set; }

  [Display(Name = "資料欄位A")]
  public string? dataFieldA { get; set; }

  [Display(Name = "資料欄位B")]
  public string? dataFieldB { get; set; }

  [Display(Name = "資料欄位C")]
  public string? dataFieldC { get; set; }

  public DEMO011FormData Clone()
  {
    return new DEMO011FormData
    {
      formNo = this.formNo,
      dataFieldA = this.dataFieldA,
      dataFieldB = this.dataFieldB,
      dataFieldC = this.dataFieldC,
    };
  }
}

/// <summary>
/// 資料表單之Validator
/// </summary>
public class DEMO011FormDataValidator : AbstractValidator<DEMO011FormData>
{
  public DEMO011FormDataValidator()
  {
    RuleFor(m => m.formNo).NotEmpty();
    RuleFor(m => m.dataFieldA).NotEmpty();
  }
}