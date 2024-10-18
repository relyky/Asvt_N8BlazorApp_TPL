using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Vista.AOP;
using Vista.Models;

namespace Vista.Biz.DEMO;

[CatchAndLog("案件審查樣板")]
class DEMO014Biz(ILogger<DEMO014Biz> _logger, IAuthUser _auth)
{
  #region 模擬資料(正式版請移除)
  static List<DEMO014FormData> _simsDataList = MakeSimsDataList();

  static List<DEMO014FormData> MakeSimsDataList()
  {
      Random r = new Random();
      List<DEMO014FormData> metaList = new List<DEMO014FormData>();
      for (int i = 1; i <= 49; i++)
      {
        metaList.Add(new DEMO014FormData
        {
          FormNo = $"F{i:D3}",
          SpentAmount = r.Next(1000, 90000), //  i % 17 * 10000 + (100 - i) * 29,
          SpentDate = DateTime.Today.AddDays(i % 17 - i * 10 % 11).ToString("yyyyMMdd"),
          CreditNo = "1111222233334444",
          IsSkiProject = (i % 17 > 5) ? "Y" : "N",
          Gender = (i % 2 == 0) ? "F" : "M",
          Field1 = "field1",
          Field2 = "field2",
          Field3 = "field3",
          Field4 = "field4",
          Field5 = "field5",
          Field6 = "field6",
        });
      }

      return metaList;
  }

  #endregion

  [LogTitle("查詢資料")]
  public List<DEMO014Profile> QryDataList(int topCount, DEMO014QryArgs args)
  {
    // 模擬長時間查詢，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 800);

    // 模擬查詢資料。
    var dataList = _simsDataList.Take(topCount).Select(fromData => new DEMO014Profile(fromData)).ToList();
    return dataList;
  }


  [LogTitle("取得表單資料")]
  public DEMO014FormData? GetFormData(DEMO014Profile aim)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 800);

    // 模擬結果
    var formData = _simsDataList.Find(c => c.FormNo == aim.FormNo);

    //# 成功
    return formData;
  }


  [LogTitle("檢核資料")]
  public DEMO014FormData ApproveCase(DEMO014FormData formData)
  {
    // 模擬長時間運算，正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 800);

    var validator = new DEMO004FormDataValidator();
    var result = validator.Validate(formData);

    if (!result.IsValid)
    {
      throw new ApplicationException("資料格式不對。");
    }

    // 進一步檢核…
    //throw new ApplicationException("模擬邏輯上的錯誤訊息。");

    // 隨機模擬成功或失敗
    if (formData.FormNo.EndsWith("2") || formData.FormNo.EndsWith("4"))
    {
      // 模擬失敗
      formData.CheckResult = "檢核不通過";
      formData.CaseRemark = "模擬檢核不通過原因說明。";
    }
    else
    {
      // 模擬成功
      formData.CheckResult = "檢核通過";
      formData.CaseRemark = "商務卡及VISA金融卡不可參加";
    }

    return formData;
  }
}

public record DEMO014QryArgs
{
  [Display(Name = "信用卡號")]
  public string? CreditNo { get; set; }

  [Display(Name = "新壽派員件專案")]
  public string? IsSkiProject { get; set; }

  [Display(Name = "消費日期(起)")]
  public string? SpentDateBgn { get; set; }

  [Display(Name = "消費日期(迄)")]
  public string? SpentDateEnd { get; set; }
}

public class DEMO014QryArgsValidator : AbstractValidator<DEMO014QryArgs>
{
  public DEMO014QryArgsValidator()
  {
    RuleFor(m => m.CreditNo).NotEmpty();
  }
}

/// <summary>
/// 資料表單
/// </summary>
public record DEMO014FormData
{
  [Display(Name = "表單號碼")]
  public string FormNo { get; set; } = string.Empty;

  [Display(Name = "消費金額")]
  public decimal SpentAmount { get; set; }

  [Display(Name = "消費日期")]
  public string? SpentDate { get; set; }

  [Display(Name = "信用卡號")]
  public string? CreditNo { get; set; }

  [Display(Name = "新壽派員件專案")]
  public string? IsSkiProject { get; set; }

  [Display(Name = "性別")]
  public string? Gender { get; set; }

  [Display(Name = "檢核結果")]
  public string? CheckResult { get; set; }

  [Display(Name = "專案備註")]
  public string? CaseRemark { get; set; }

  public string? Field1 { get; set; }
  public string? Field2 { get; set; }
  public string? Field3 { get; set; }
  public string? Field4 { get; set; }
  public string? Field5 { get; set; }
  public string? Field6 { get; set; }

  public List<DEMO014FormItem> ItemList { get; set; } = [];
}

public class DEMO014FormItem
{
  public string? DetailA { get; set; }
  public string? DetailB { get; set; }
  public string? DetailC { get; set; }
  public string? DetailD { get; set; }
  public string? DetailE { get; set; }
  public string? DetailF { get; set; }
  public string? DetailG { get; set; }
}

public class DEMO004FormDataValidator : AbstractValidator<DEMO014FormData>
{
  public DEMO004FormDataValidator()
  {
    RuleFor(m => m.FormNo).NotEmpty();
    RuleFor(m => m.SpentAmount).NotEmpty();
    RuleFor(m => m.SpentDate).NotEmpty();
    RuleFor(m => m.CreditNo).CreditCard(); // .NotEmpty().Length(16);
    RuleFor(m => m.IsSkiProject).NotEmpty();
    RuleFor(m => m.Gender).NotEmpty().Length(1);
  }
}

/// <summary>
/// 資料表單之簡要欄位
/// </summary>
public record DEMO014Profile
{
  public DEMO014Profile() { } 

  /// <summary>
  /// trasnform
  /// </summary>
  public DEMO014Profile(DEMO014FormData src)
  {
    this.FormNo = src.FormNo;
    this.SpentAmount = src.SpentAmount;
    this.Field1 = src.Field1;
    this.Field2 = src.Field2;
    this.Field3 = src.Field3;
    this.Field4 = src.Field4;
    this.Field5 = src.Field5;
    this.Field6 = src.Field6;
  }

  [Display(Name = "表單號碼")]
  public string FormNo { get; set; } = string.Empty;

  [Display(Name = "消費金額")]
  public decimal SpentAmount { get; set; }

  public string? Field1 { get; set; }
  public string? Field2 { get; set; }
  public string? Field3 { get; set; }
  public string? Field4 { get; set; }
  public string? Field5 { get; set; }
  public string? Field6 { get; set; }
}