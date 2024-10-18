using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Vista.Models;

public class LoginArgs
{
  [Display(Name = "帳號")]
  public string userId { get; set; } = string.Empty;

  [Display(Name = "通關密語")]
  public string credential { get; set; } = string.Empty;

  [Display(Name = "驗證碼")]
  public string vcode { get; set; } = string.Empty;

  [Display(Name = "記住我")]
  public bool rememberMe { get; set; } = false;

  [Display(Name = "申請日期")]
  public DateTime? applyDate { get; set; }

  public string returnUrl { get; set; } = string.Empty;
}

public class LoginArgsValidator : AbstractValidator<LoginArgs>
{
  public LoginArgsValidator()
  {
    RuleFor(m => m.userId).NotEmpty();
    RuleFor(m => m.credential).NotEmpty();
    RuleFor(m => m.vcode).NotEmpty();
  }
}
