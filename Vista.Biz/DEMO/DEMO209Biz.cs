using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.AOP;

namespace Vista.Biz.DEMO;

[CatchAndLog("拖拉編輯展示二Biz")]
class DEMO209Biz
{
}

public class CommentItem
{
  public int Order { get; set; }
  public string Comment { get; set; } = string.Empty;

  public CommentItem Clone()
  {
    return new CommentItem
    {
      Order = this.Order,
      Comment = this.Comment
    };
  }
}

public class CommentItemValidator : AbstractValidator<CommentItem>
{
  public CommentItemValidator()
  {
    //RuleFor(m => m.userId).NotEmpty();
    //RuleFor(m => m.credential).NotEmpty();
    //RuleFor(m => m.vcode).NotEmpty();
  }
}