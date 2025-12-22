using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Vista.AOP;
using Vista.Models;

namespace Vista.Biz.DEMO;

[CatchAndLog("ＣＲＵＤ多欄位樣板")]
class DEMO012Biz(ILogger<DEMO012Biz> _logger, IAuthUser _auth)
{
  [LogTitle("查詢資料清單")]
  public List<DEMO012Profile> QryDataList(int topCount, DEMO012QryArgs args)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# 模擬自DB查詢資料
    var dataList = Enumerable.Range(1, topCount).Select(i => new DEMO012Profile
    {
      formNo = $"SIMS{i:D4}",
      dataFieldA = String.IsNullOrEmpty(args.dataFieldA) ? "dataFieldA" : args.dataFieldA,
      dataFieldB = "dataFieldB",
      dataFieldC = "dataFieldC",
      dataFieldD = "dataFieldD",
      dataFieldE = "dataFieldE",
      dataFieldF = "dataFieldF"
    }).ToList();

    //# 查詢範例
    //    DynamicParameters param = new DynamicParameters(); // Dapper 動態參數
    //    StringBuilder sql = new StringBuilder();
    //    sql.Append(@"SELECT TOP (@topCount)
    //  formNo
    // ,dataFieldA
    // ,dataFieldB
    // ,...
    // FROM DataTableName WITH(NOLOCK)
    // WHERE 1=1 ");
    // 
    //    param.Add("topCount", topCount);
    //
    //    //# 依條件動態加入查詢參數
    //    if (!String.IsNullOrWhiteSpace(args.dataFieldA))
    //    {
    //      sql.Append("AND dataFieldA = @dataFieldA ");
    //      param.Add("@dataFieldA", args.dataFieldA);
    //    }
    //
    //    // 排序方式
    //    sql.AppendLine("ORDER BY dataFieldA DESC ");
    //
    //    // 執行查詢
    //    using var conn = DBHelper.CONNSEC.Open();
    //    var dataList = conn.Query<DEMO012Profile>(sql.ToString(), param).AsList();

    return dataList;
  }

  [LogTitle("新增資料")]
  public (DEMO012FormData,string) AddFormData(DEMO012FormData formData)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# biz checking
    var validator = new DEMO012FormDataValidator();
    var chk = validator.Validate(formData);
    if (!chk.IsValid)
      return (formData, String.Join("；", chk.Errors.Select(c => c.ErrorMessage)));

    //# resource
    var user = _auth.GetCurrentUser();
    var now = DateTime.Now;

    //# 連接DB啟動交易
    //using var conn = DBHelper.CONNNST.Open();
    //using var txn = conn.BeginTransaction();

    //# 自動填入系統欄位
    //formData.AddUser = user.UserId;
    //formData.AddDtm = now;
    //formData.UpdUser = user.UserId;
    //formData.UpdDtm = now;


    //# Do Access DB
    //conn.InsertEx<DEMO012FormData>(formData, txn);
    //txn.Commit();

    //# 成功
    return (formData, "SUCCESS");
  }

  [LogTitle("取得資料")]
  public DEMO012FormData GetFormData(DEMO012Profile aim)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# 連接DB取得資料
    //using var conn = DBHelper.CONNNST.Open();
    //var formData = conn.GetEx<DEMO012FormData>(new { formNo });

    // 模擬結果
    var formData = new DEMO012FormData { formNo = aim.formNo, dataFieldA = "這是模擬資料" };

    //# 成功
    return formData;
  }

  [LogTitle("更新資料")]
  public (DEMO012FormData, string) UpdFormData(DEMO012FormData formData)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# biz checking
    var validator = new DEMO012FormDataValidator();
    var chk = validator.Validate(formData);
    if (!chk.IsValid)
      return (formData, String.Join("；", chk.Errors.Select(c => c.ErrorMessage)));

    //# resource
    var user = _auth.GetCurrentUser();
    var now = DateTime.Now;

    //# 連接DB啟動交易
    //using var conn = DBHelper.CONNNST.Open();
    //using var txn = conn.BeginTransaction();

    //...

    ////# 自動填入系統欄位
    //formData.UpdUser = user.UserId;
    //formData.UpdDtm = now;

    ////# Do Access DB
    //conn.Update<DEMO012FormData>(updItem, txn);
    //txn.Commit();

    //# 成功
    return (formData, "SUCCESS");
  }

  [LogTitle("刪除資料")]
  public int DelFormData(DEMO012Profile item)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 1000);

    //# 連接DB啟動交易
    //using var conn = DBHelper.CONNNST.Open();
    //using var txn = conn.BeginTransaction();

    ////# Do Access DB
    //int ret = conn.DeleteEx<IWIM003>(new { item.Appl_No, item.Idno }, txn);
    //txn.Commit();

    // 模擬結果
    int ret = 1;

    return ret;
  }
}

public class DEMO012QryArgs
{
  [Display(Name = "表單號碼")]
  public string? formNo { get; set; }

  [Display(Name = "資料欄位A")]
  public string? dataFieldA { get; set; }

  [Display(Name = "查詢日期(起)")]
  public string? dateBgn { get; set; }

  [Display(Name = "查詢日期(訖)")]
  public string? dateEnd { get; set; }
}

public class DEMO012QryArgsValidator : AbstractValidator<DEMO012QryArgs>
{
  public DEMO012QryArgsValidator()
  {
    //RuleFor(m => m.dateBgn).NotEmpty();
    //RuleFor(m => m.dateEnd).NotEmpty();

    RuleFor(m => m.dateBgn).Must((args, _) =>
        String.IsNullOrWhiteSpace(args.dateBgn) ||
        String.IsNullOrWhiteSpace(args.dateEnd) ||
        args.dateBgn.CompareTo(args.dateEnd) <= 0
    ).WithMessage("起日 不可大於訖日！");

    RuleFor(m => m.dateEnd).Must((args, _) =>
        String.IsNullOrWhiteSpace(args.dateBgn) ||
        String.IsNullOrWhiteSpace(args.dateEnd) ||
        args.dateBgn.CompareTo(args.dateEnd) <= 0
    ).WithMessage("訖日 不可小於起日！");
  }
}

/// <summary>
/// 資料表單
/// </summary>
public class DEMO012FormData
{
  [Display(Name = "表單號碼")]
  public string? formNo { get; set; }

  [Display(Name = "資料欄位A")]
  public string? dataFieldA { get; set; }

  [Display(Name = "資料欄位B")]
  public string? dataFieldB { get; set; }

  [Display(Name = "資料欄位C")]
  public string? dataFieldC { get; set; }

  [Display(Name = "資料欄位D")]
  public string? dataFieldD { get; set; }

  [Display(Name = "資料欄位E")]
  public string? dataFieldE { get; set; }

  [Display(Name = "資料欄位F")]
  public string? dataFieldF { get; set; }

  [Display(Name = "建檔人員")]
  public string AddUser { get; set; } = default!;
  [Display(Name = "建檔時間")]
  public DateTime? AddDtm { get; set; }
  [Display(Name = "異動人員")]
  public string UpdUser { get; set; } = default!;
  [Display(Name = "異動時間")]
  public DateTime? UpdDtm { get; set; }
}

/// <summary>
/// 資料表單之Validator
/// </summary>
public class DEMO012FormDataValidator : AbstractValidator<DEMO012FormData>
{
  public DEMO012FormDataValidator()
  {
    RuleFor(m => m.formNo).NotEmpty();
    RuleFor(m => m.dataFieldA).NotEmpty();
  }
}

/// <summary>
/// 資料表單之簡要欄位
/// </summary>
public class DEMO012Profile
{
  public DEMO012Profile() { }

  /// <summary>
  /// trasnform
  /// </summary>
  public DEMO012Profile(DEMO012FormData src)
  {
    this.formNo = src.formNo;
    this.dataFieldA = src.dataFieldA;
    this.dataFieldB = src.dataFieldB;
    this.dataFieldC = src.dataFieldC;
    this.dataFieldD = src.dataFieldD;
    this.dataFieldE = src.dataFieldE;
    this.dataFieldF = src.dataFieldF;
  }

  [Display(Name = "表單號碼")]
  public string? formNo { get; set; }

  [Display(Name = "資料欄位A")]
  public string? dataFieldA { get; set; }

  [Display(Name = "資料欄位B")]
  public string? dataFieldB { get; set; }

  [Display(Name = "資料欄位C")]
  public string? dataFieldC { get; set; }

  [Display(Name = "資料欄位D")]
  public string? dataFieldD { get; set; }

  [Display(Name = "資料欄位E")]
  public string? dataFieldE { get; set; }

  [Display(Name = "資料欄位F")]
  public string? dataFieldF { get; set; }
}
