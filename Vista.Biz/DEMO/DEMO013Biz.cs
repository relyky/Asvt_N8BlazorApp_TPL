using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vista.AOP;

namespace Vista.Biz.DEMO;

[CatchAndLog("步步申請樣板")]
internal class DEMO013Biz
{
}

public class DEMO013FormStep1
{
  [Display(Name = "客戶姓名")]
  public string? ClientName { get; set; } = "櫻木花道";

  [Display(Name = "連絡電話")]
  public string? ContactPhone { get; set; } = "0900123456";

  [Display(Name = "通過步驟")]
  public bool HasPassed { get; set; }
}

public class DEMO013FormStep1Validator : AbstractValidator<DEMO013FormStep1>
{
  public DEMO013FormStep1Validator()
  {
    RuleFor(m => m.ClientName).NotEmpty().MinimumLength(2);
    RuleFor(m => m.ContactPhone).NotEmpty().Length(8, 10);
  }
}

public class DEMO013FormStep2
{

  [Display(Name = "申請資助")]
  public string? ApplyForSubsidy { get; set; } = "免死金牌";

  [Display(Name = "援助資金")]
  public decimal ApplyForFunding { get; set; } = 9876543210;

  [Display(Name = "通過步驟")]
  public bool HasPassed { get; set; }
}

public class DEMO013FormStep2Validator : AbstractValidator<DEMO013FormStep2>
{
  public DEMO013FormStep2Validator()
  {
    RuleFor(m => m.ApplyForSubsidy).NotEmpty();
    RuleFor(m => m.ApplyForFunding).NotEmpty();
  }
}

public class DEMO013FormStep3
{
  [Display(Name = "上傳檔案")]
  public string? UploadFile { get; set; } = "假裝這是檔案存放位置資訊";

  [Display(Name = "通過步驟")]
  public bool HasPassed { get; set; }
}

public class DEMO013FormStep3Validator : AbstractValidator<DEMO013FormStep3>
{
  public DEMO013FormStep3Validator()
  {
    RuleFor(m => m.UploadFile).NotEmpty();
  }
}

public class DEMO013FormStep4
{
  [Display(Name = "我已閱讀並同意")]
  public string ReadAndAgree { get; set; } = "N";

  [Display(Name = "通過步驟")]
  public bool HasPassed { get; set; }
}

public class DEMO013FormStep4Validator : AbstractValidator<DEMO013FormStep4>
{
  public DEMO013FormStep4Validator()
  {
    RuleFor(m => m.ReadAndAgree).Must(v => "Y".Equals(v)).WithMessage("'我已閱讀並同意' 必需同意才能繼續。");
  }
}

/// <summary>
/// 資料表單
/// </summary>
public class DEMO013FormData
{
  public DEMO013FormStep1 Step1 { get; set; } = new();
  public DEMO013FormStep2 Step2 { get; set; } = new();
  public DEMO013FormStep3 Step3 { get; set; } = new();
  public DEMO013FormStep4 Step4 { get; set; } = new();
  public bool IsSubmitedSuccess { get; set; }
}

public class DEMO013FormDataValidator : AbstractValidator<DEMO013FormData>
{
  public DEMO013FormDataValidator()
  {
    // 檢查群組物件
    RuleFor(m => m.Step1)
      .SetValidator(new DEMO013FormStep1Validator());

    RuleFor(m => m.Step2)
      .SetValidator(new DEMO013FormStep2Validator());

    RuleFor(m => m.Step3)
      .SetValidator(new DEMO013FormStep3Validator());

    RuleFor(m => m.Step4)
      .SetValidator(new DEMO013FormStep4Validator());
  }
}