using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.AOP;
using Vista.Models;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace Vista.Biz.DEMO;

[CatchAndLog("商業邏輯範例")]
class SampleBiz(
  BlazorComponentBus.ComponentBus _busSvc,
  NavigationManager _navSvc,
  IConverter _pdfSvc,
  ILogger<SampleBiz> _logger)
{
  #region ## resource
  static readonly string[] Citys = ["台北市", "新北市", "桃園市", "台中市", "台南市", "高雄市"];
  static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
  #endregion

  [LogTitle("模擬長程運算")]
  public List<WeatherForecast> SimsLongtermProcedure(QryForecastArgs args)
  {
    _busSvc.Publish(new NotifyMessage { Message = "天氣預報２:開始" });
    _logger.LogInformation("天氣預報２:開始");

    // 模擬跑一段時間(1秒)
    SpinWait.SpinUntil(() => false, Random.Shared.Next(1000, 2000));

    if (args.simsFail)
    {
      var ex = new ApplicationException("模擬出現錯誤！");
      _logger.LogError(ex, ex.Message);
      throw ex;
    }

    _busSvc.Publish(new NotifyMessage { Message = "天氣預報２:步驟二" });
    _logger.LogInformation("天氣預報２:步驟二");

    // 模擬跑一段時間(1秒)
    SpinWait.SpinUntil(() => false, Random.Shared.Next(2000, 4000));

    var dataList = Enumerable.Range(1, (int)args.rowCount).Select(index => new WeatherForecast
    {
      Sn = index,
      City = Citys[Random.Shared.Next(Citys.Length)],
      Date = args.startDate.Value.AddDays(index),
      TemperatureC = Random.Shared.Next(-20, 55),
      Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    }).ToList();

    _busSvc.Publish(new NotifyMessage { Message = "天氣預報２:步驟三" });
    _logger.LogInformation("天氣預報２:步驟三");

    // 模擬跑一段時間(1秒)
    SpinWait.SpinUntil(() => false, Random.Shared.Next(2000, 4000));

    _busSvc.Publish(new NotifyMessage { Message = "天氣預報２:步驟四" });
    _logger.LogInformation("天氣預報２:步驟四");

    // 模擬跑一段時間(1秒)
    SpinWait.SpinUntil(() => false, Random.Shared.Next(2000, 4000));

    _busSvc.Publish(new NotifyMessage { Message = "天氣預報２:步驟五" });
    _logger.LogInformation("天氣預報２:步驟五");

    // 模擬跑一段時間(1秒)
    SpinWait.SpinUntil(() => false, Random.Shared.Next(2000, 4000));

    //_logger.ActionLog("FOO", ActionLogEventCode.查詢, "天氣預報成功。", "Y");
    _busSvc.Publish(new NotifyMessage { Message = "天氣預報２:結束", Severity = MudSeverity.Success });
    _logger.LogInformation("天氣預報２:結束");
    return dataList;
  }

  public byte[] MakeHtmlReport()
  {
    //# make html with data.
    string html = DoMakeHtmlPage(/* data model */);

    //# Convert
    byte[] fileBlob = UTF8Encoding.UTF8.GetBytes(html);
    return fileBlob;
  }

  public byte[] MakePdfReport()
  {
    //# make html with data.
    string html = DoMakeHtmlPage(/* data model */);

    //# Convert
    byte[] pdfBlob = HtmlToPdf(html);
    return pdfBlob;
  }

  public string MakeAnzFdlTestHtml()
  {
    //# make html with data.
    FileInfo file = new FileInfo(@"Template/ANZ_FDL_Test.html");

    using var reader1 = File.OpenText(file.FullName);
    StringBuilder htmlTpl = new StringBuilder(reader1.ReadToEnd());

    //htmlTpl.Replace("<!--## BARCODE-STICKER-GROUP ##-->", groupBlock.ToString());

    string html = htmlTpl.ToString();
    return html;
  }

  public string MakeLandscapeHtml()
  {
    //# make html with data.
    FileInfo file = new FileInfo(@"Template/MyLandscapeReport.html");

    using var reader1 = File.OpenText(file.FullName);
    StringBuilder htmlTpl = new StringBuilder(reader1.ReadToEnd());

    //htmlTpl.Replace("<!--## BARCODE-STICKER-GROUP ##-->", groupBlock.ToString());

    string html = htmlTpl.ToString();
    return html;
  }

  #region helper function
  string DoMakeHtmlPage(/* DataModel data */)
  {
    try
    {
      FileInfo file = new FileInfo(@"Template/ReportSampleTpl.html");

      StringBuilder html = new();
      using (var reader = File.OpenText(file.FullName))
      {
        html.Append(reader.ReadToEnd());
      }

      //// 置換參數
      //string barcode1Data = "110502C31";
      //string barcode2Data = "0338584420300001";

      ////// 產生 Barcode Ima,預設:Code128
      //var barcode1 = new Barcode(barcode1Data, false, 280, 40, LabelPosition.BottomCenter, AlignmentPosition.Left); // 原生寬度：268px;
      //var barcode2 = new Barcode(barcode2Data, false, 260, 40, LabelPosition.BottomCenter, AlignmentPosition.Left); // 原生寬度：246px;

      //html.Replace("#barcode_1_code#", barcode1Data);
      //html.Replace("#barcode_1_image#", barcode1.GetBase64Image());
      //html.Replace("#barcode_2_code#", barcode2Data);
      //html.Replace("#barcode_2_image#", barcode2.GetBase64Image());

      return html.ToString();
    }
    catch (Exception ex)
    {
      StringBuilder error = new StringBuilder(); ;
      error.AppendLine("<h3>MakeHtmlPage FAIL!</h3>");
      error.AppendLine($"<p>{ex.GetType().Name}</p>");
      error.AppendLine($"<p>{ex.Message}</p>");
      return error.ToString();
    }
  }


  /// <summary>
  /// A4彩色直印
  /// </summary>
  public byte[] HtmlToPdf(string html)
  {
    //# Define document to convert
    var doc = new HtmlToPdfDocument()
    {
      GlobalSettings = {
          ColorMode = ColorMode.Color,
          Orientation = Orientation.Portrait,
          PaperSize = PaperKind.A4,
          Margins = new MarginSettings() { Top = 26, Left = 10 },
        },
      Objects = {
        // 也有支援封面
        new CoverSettings()
        {
          HtmlContent = @"
<!DOCTYPE html>
<html>
<head>
  <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
  <title>This is conver</title>
  <style>
    body { font-family: '微軟正黑體'; }
    .center { text-align: center; }
  </style>
</head>
<body class='center'>  
  <h1>此頁是封面</h1>
  <h1>This is cover</h1>
  <hr style='margin-top:2mm;margin-bottom:2mm;'>
  <img src='https://www.w3schools.com/html/pic_trulli.jpg' width='500' height='333'>
  <hr style='margin-top:2mm;margin-bottom:2mm;'>
</body>
</html>
"
        },
        // 報表內容
        new ObjectSettings() {
          PagesCount = true,
          HtmlContent = html,
          WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true },
          LoadSettings = new LoadSettings { ZoomFactor = 1.26 }, //1.26
          HeaderSettings = { 
            FontSize = 9, 
            Right = "Date: [date]", 
            Line = false, 
            Spacing = 0, // 1.8, // 2.812,
            HtmlUrl = $"{_navSvc.BaseUri}reportResource/cpaheader.html" // 必需是完整的URL
            //HtmlUrl = "https://localhost:7248/reportResource/cpaheader.html" // 必需是完整的URL
          },
          FooterSettings = { 
            FontSize = 9, 
            Right = "Page [page] of [toPage]", 
            Line = true, 
            Spacing = 2.812,
            HtmlUrl = ""
          },
        }
      }
    };

    //# Convert
    byte[] fileBlob = _pdfSvc.Convert(doc);

    return fileBlob;
  }

  /// <summary>
  /// A4彩色直印
  /// </summary>
  public byte[] HtmlToPdf_NoFooter(string html)
  {
    //# Define document to convert
    var doc = new HtmlToPdfDocument()
    {
      GlobalSettings = {
          ColorMode = ColorMode.Color,
          Orientation = Orientation.Portrait,
          PaperSize = PaperKind.A4,
          Margins = new MarginSettings() { Top = 10, Left = 10 },
        },
      Objects = {
        new ObjectSettings() {
          PagesCount = true,
          HtmlContent = html,
          WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true },
          LoadSettings = new LoadSettings { ZoomFactor = 1.26 }, //1.26
          //HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
          //FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
        }
      }
    };

    //# Convert
    byte[] fileBlob = _pdfSvc.Convert(doc);

    return fileBlob;
  }

  /// <summary>
  /// A4彩色橫印
  /// </summary>
  public byte[] HtmlToPdf_Landscape(string html)
  {
    //# Define document to convert
    var doc = new HtmlToPdfDocument()
    {
      GlobalSettings = {
          ColorMode = ColorMode.Color,
          Orientation = Orientation.Landscape,
          PaperSize = PaperKind.A4,
          Margins = new MarginSettings() { Top = 26, Bottom = 15, Left = 10, Right = 18 },
        },
      Objects = {
        new ObjectSettings() {
          PagesCount = true,
          HtmlContent = html,
          WebSettings = { DefaultEncoding = "utf-8", PrintMediaType = true },
          LoadSettings = new LoadSettings { ZoomFactor = 1.26 }, //1.26
          HeaderSettings = {
            FontSize = 12,
            Right = "Date:[date]",
            Line = true,
            Spacing = 2.812,
            HtmlUrl =  "https://localhost:7248/reportResource/cpaheader.html"
          }, 
          FooterSettings = {
            FontSize = 9,
            Left = "CONFIDENTIAL",
            Center = "附件\r\nAttachment",
            Right = "Page [page] of [toPage]",
            Line = false,
            Spacing = 2.812,
            HtmlUrl = "",
          },
        }
      }
    };

    //# Convert
    byte[] fileBlob = _pdfSvc.Convert(doc);

    return fileBlob;
  }

  #endregion
}
