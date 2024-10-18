using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Vista.Biz.DEMO;

public record SampleFormData
{
  [Display(Name = "表單編號")]
  public string FormNo { get; set; } = string.Empty;

  [Display(Name = "姓名")]
  public string Name { get; set; } = string.Empty;

  [Display(Name = "姓別")]
  public string Gender { get; set; } = string.Empty;

  [Display(Name = "期望薪資")]
  public decimal? ExpectedSalary { get; set; }

  [Display(Name = "期望職稱")]
  public string? ExpectedJobTitle { get; set; }

  [Display(Name = "出身日期")]
  public string Birthday { get; set; } = string.Empty;

  [Display(Name = "出身年月")]
  public string BirthYearMonth { get; set; } = string.Empty;

  /// <summary>
  /// 汽車駕照:Y/N
  /// </summary>
  [Display(Name = "汽車駕照")]
  public string HasDriverLicense { get; set; } = "N";

  [Display(Name = "基本代碼")]
  public string? SampleCode { get; set; }

  [Display(Name = "基本代碼２")]
  public string? SampleCode2 { get; set; }

  [Display(Name = "基本名稱")]
  public string SampleName { get; set; } = string.Empty;

  [Display(Name = "銀行代碼")]
  public string BankCd { get; set; } = string.Empty;

  [Display(Name = "分行代碼")]
  public string BranchCd { get; set; } = string.Empty;

  [Display(Name = "全客製欄位")]
  public string CustomCode { get; set; } = string.Empty;

  [Display(Name = "是否有")]
  public string YesIam { get; set; } = "N";

  [Display(Name = "興趣")]
  public List<string> InterestingList { get; set; } = [];

  [Display(Name = "愛好")]
  public string? Favorite { get; set; }

  [Display(Name = "自我介紹")]
  public string? SelfIntroduction { get; set; }

  [Display(Name = "聯絡方式")]
  public ContactInfo Contact { get; set; } = new();

  [Display(Name = "學歷")]
  public List<EducationInfo> EduList { get; set; } = [];

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
/// 聯絡方式
/// </summary>
public record ContactInfo
{
  [Display(Name = "電郵地址")]
  public string Email { get; set; } = string.Empty;
  [Display(Name = "行動電話")]
  public string MobilePhone { get; set; } = string.Empty;
  [Display(Name = "聯絡地址")]
  public string Address { get; set; } = string.Empty;
}

public class ContactInfoValidator : AbstractValidator<ContactInfo>
{
  public ContactInfoValidator()
  {
    RuleFor(c => c.Email).NotEmpty();
    RuleFor(c => c.MobilePhone).NotEmpty().Length(8, 10);
    RuleFor(c => c.Address).NotEmpty();
  }
}

/// <summary>
/// 學歷
/// </summary>
public record EducationInfo
{
  /// <summary>
  /// 教育階段:博士班、研究所、大學、高中等。
  /// </summary>
  [Display(Name = "教育階段")]
  public string Level { get; set; } = string.Empty;
  [Display(Name = "學校名稱")]
  public string SchoolName { get; set; } = string.Empty;
}

public class EducationInfoValidator : AbstractValidator<EducationInfo>
{
  public EducationInfoValidator()
  {
    RuleFor(c => c.Level).NotEmpty().MinimumLength(2);
    RuleFor(c => c.SchoolName).NotEmpty().MinimumLength(2);
  }
}

public class SampleFormDataValidator : AbstractValidator<SampleFormData>
{
  public SampleFormDataValidator()
  {
    RuleFor(c => c.Name).NotEmpty();
    RuleFor(c => c.Gender).NotEmpty();
    RuleFor(c => c.BirthYearMonth).NotEmpty();
    RuleFor(c => c.Birthday).NotEmpty();
    RuleFor(c => c.SampleCode).NotEmpty();
    RuleFor(c => c.SampleCode2).NotEmpty();
    RuleFor(c => c.SelfIntroduction).NotEmpty().MinimumLength(100);
    RuleFor(c => c.Favorite).NotEmpty();

    RuleFor(c => c.HasDriverLicense).Must(v => "Y".Equals(v)).WithMessage("'汽車駕照' 為必需。");
    //RuleFor(c => c.HasDriverLicense).NotEqual("N");

    RuleFor(c => c.BankCd).NotEmpty();
    RuleFor(c => c.BranchCd).NotEmpty();

    RuleFor(c => c.ExpectedSalary).NotEmpty().InclusiveBetween(28000, 50000);
    RuleFor(c => c.ExpectedJobTitle).NotEmpty();

    RuleFor(c => c.YesIam).NotEmpty().NotEqual("N");

    RuleFor(c => c.InterestingList)
      .NotEmpty()
      .Must(c => c.Count >= 3).WithMessage("'興趣' 項目需3項以上。");

    // 檢查群組物件
    RuleFor(m => m.Contact)
      .SetValidator(new ContactInfoValidator());

    // 檢查群組物件，法二
    //RuleFor(c => c.Contact.Email).NotEmpty();
    //RuleFor(c => c.Contact.MobilePhone).NotEmpty();
    //RuleFor(c => c.Contact.Address).NotEmpty();

    // 檢查明細清單
    RuleForEach(m => m.EduList)
      .SetValidator(new EducationInfoValidator());
  }
}
