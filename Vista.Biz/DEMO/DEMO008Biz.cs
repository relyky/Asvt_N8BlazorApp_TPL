using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.AOP;
using FluentValidation;
using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;

namespace Vista.Biz.DEMO;

[CatchAndLog("檔案上傳樣板")]
class DEMO008Biz
{
  [LogTitle("上傳Excel檔範例")]
  public List<DEMO008Info> UploadExcelFile(MemoryStream file, DEMO008Args uploadArgs)
  {
    //# uploadArgs 可取得其他上傳參數。
    // 此範例只展示如何傳遞其他上傳參數到 Biz 。

    //# 開始解析Excel
    using var xlsx = new XLWorkbook(file);
    var ws = xlsx.Worksheet(1);

    var dataList = new List<DEMO008Info>();
    foreach (var row in ws.RowsUsed().Skip(1))
    {
      var item = new DEMO008Info
      {
        IdName = row.Cell(1).GetString(),
        Amount = row.Cell(2).GetValue<Decimal>()
      };

      dataList.Add(item);
    }

    return dataList;
  }

}

/// <summary>
/// 上傳參數
/// </summary>
public record DEMO008Args
{
  public bool PickMe1 { get; set; } = true;
  public bool PickMe2 { get; set; } = false;
  public string SelectedOption { get; set; } = string.Empty; // = "選項二";
  public string InputText { get; set; } = "今天天氣真好";
}

public class DEMO008ArgsValidator : AbstractValidator<DEMO008Args>
{
  public DEMO008ArgsValidator()
  {
    RuleFor(m => m.PickMe1).Equal(true).WithMessage("選我選我 必需勾選。");
    RuleFor(m => m.PickMe2).Equal(true).WithMessage("選我才對 必需勾選。");
    RuleFor(m => m.InputText).NotEmpty();
    RuleFor(m => m.SelectedOption).NotEmpty();
  }
}

/// <summary>
/// 上傳資料
/// </summary>
public record DEMO008Info
{
  [Display(Name = "識別名稱")]
  public string IdName { get; set; } = string.Empty;

  [Display(Name = "數值")]
  public decimal Amount { get; set; }
}