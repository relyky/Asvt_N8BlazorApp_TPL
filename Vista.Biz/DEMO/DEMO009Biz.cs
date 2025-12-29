using ClosedXML.Excel;
using Dapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Vista.AOP;
using Vista.Models;

namespace Vista.Biz.DEMO;

[CatchAndLog("查詢作業樣板")]
class DEMO009Biz(ILogger<DEMO009Biz> _logger, IAuthUser _auth)
{
  [LogTitle("查詢資料清單")]
  public List<DEMO009Data> QryDataList(DEMO009QryArgs args)
  {
    //# 模擬長時間執行。※正式版請移除。
    System.Threading.SpinWait.SpinUntil(() => false, 2000); // 等2秒

    //# 模擬自DB查詢資料
    string[] titleRepo = new string[] { "王聖麟", "李百勇", "林育俐", "賴惠君", "吳祐誠", "張廷航", "李琇延", "馮俊良", "陳秀男", "汪君豪", "蔡隆愛", "吳建富", "杜智強" };
    DateTime now = DateTime.Now;

    var dataList = Enumerable.Range(1, 800).Select(i => new DEMO009Data
    {
      SN = i,
      Date = now.AddDays(i),
      Title = titleRepo[Random.Shared.Next(titleRepo.Length)] + new string('Ａ', Random.Shared.Next(0, 10)),
      Code = $"{i:0000}", // 模擬編碼
      A = 100 + Random.Shared.Next(3000),
      B = 90000000 + Random.Shared.Next(90000000)
    }).ToList();

    var resultList = dataList.Where(c => c.Title is not null && args.Arg1 is not null && c.Title.Contains(args.Arg1)).AsList();

    return resultList; // dataList;
  }

  [LogTitle("轉成Excel檔")]
  public MemoryStream ExporyExcel(List<DEMO009Data> dataList)
  {
    // take sheet name
    string sheetName = "模擬匯出資料";

    //# 開啟 Excel 範本檔
    using (var workbook = new XLWorkbook())
    {
      var ws = workbook.Worksheets.Add(sheetName);

      ws.Column(9).Width = 20; // 為多行備註指定欄寬

      //# header
      var header = ws.FirstRow();
      header.Cell(1).Value = "序號";
      header.Cell(2).Value = "Title";
      header.Cell(3).Value = "編碼";
      header.Cell(4).Value = "日期";
      header.Cell(5).Value = "欄位A";
      header.Cell(6).Value = "欄位B";
      header.Cell(7).Value = "A+B";
      header.Cell(8).Value = "長數值編碼";
      header.Cell(9).Value = "備註";

      //# data
      // copy data row to excel
      int rowIdx = 2;
      foreach (var c in dataList)
      {
        var row = ws.Row(rowIdx);
        row.Cell(1).Value = c.SN;
        row.Cell(2).Value = c.Title;
        row.Cell(4).Value = c.Date;
        row.Cell(5).Value = c.A;
        row.Cell(6).Value = c.B;

        //※用SetValue()這樣文字格式才會生效。
        row.Cell(3).SetValue(c.Code);                  // 模擬純數字(文字)編碼
        row.Cell(8).SetValue($"{DateTime.Now.Ticks}"); // 模擬長數值(文字)編碼，如：卡號、銀行帳號

        // 公式：依列計算
        row.Cell(7).SetFormulaA1($"=SUM(E{rowIdx}:F{rowIdx})");

        //# Cell(7) 設定顯示樣式：數值格式化,對齊,上底色,畫格子
        var cellA = row.Cell(7).Style;
        cellA.NumberFormat.Format = "#,0.00";
        cellA.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cellA.Fill.BackgroundColor = XLColor.Yellow;
        cellA.Border.BottomBorder = XLBorderStyleValues.Medium;

        //# Cell(9) 多行備註
        int lineCount = rowIdx % 5 + 1;
        StringBuilder remark = new();
        for (int i = 1; i < lineCount; i++) remark.AppendLine($"第{i}行備註");
        remark.Append($"第{lineCount}行備註"); // 最後一行備註
        var cell9 = row.Cell(9);
        cell9.Value = remark.ToString();
        cell9.Style.Alignment.WrapText = true; // 自動換行

        // next row
        rowIdx++;
      }

      //# footer
      // 公式：加總欄位
      ws.Cell(rowIdx, 5).SetFormulaA1($"=SUM(D2:D{rowIdx - 1})");
      ws.Cell(rowIdx, 6).SetFormulaA1($"=SUM(E2:E{rowIdx - 1})");

      //# 自適寬度
      //※ 新版的自適寬度計算已變得不直覺了。已知寬度計算與字型有關。
      //※ 舊版 0.95.4 版的自適寬度與直覺相同。
      ws.Columns("D").AdjustToContents();

      // return
      var ms = new MemoryStream();
      workbook.SaveAs(ms);
      return ms;
    }
  }
}

public record DEMO009QryArgs
{
  [Display(Name = "查詢條件１")]
  public string? Arg1 { get; set; }

  [Display(Name = "查詢條件２")]
  public string? Arg2 { get; set; }
}

public class DEMO009QryArgsValidator : AbstractValidator<DEMO009QryArgs>
{
  public DEMO009QryArgsValidator()
  {
    //RuleFor(m => m.dateBgn).NotEmpty();
    //RuleFor(m => m.dateEnd).NotEmpty();
  }
}

public record DEMO009Data
{
  public int SN { get; set; }
  public DateTime Date { get; set; }
  public string? Title { get; set; }
  public decimal A { get; set; }
  public decimal B { get; set; }
  public string? Code { get; set; } // 模擬純數字之文字編碼 0000~9999
}
