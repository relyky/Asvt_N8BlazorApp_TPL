using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.AOP;

namespace Vista.Biz.DEMO;

[CatchAndLog("拖拉編輯展示Biz")]
class DEMO208Biz
{

}

public class DropItem
{
  /// <summary>
  /// UI控制欄位: 拖拉區內索引, IndexInZone
  /// </summary>
  public int Order { get; set; }

  /// <summary>
  /// UI控制欄位: 階層:1,2。
  /// </summary>
  public int Level { get; set; }

  /// <summary>
  /// UI控制欄位: 是否為項目分群。
  /// </summary>
  public bool IsGroup { get; set; }

  /// <summary>
  /// 產品名稱/型號及說明
  /// </summary>
  public required string Description { get; set; }

  /// <summary>
  /// 單價
  /// </summary>
  public decimal UnitPrice { get; set; }

  /// <summary>
  /// 數量
  /// </summary>
  public decimal Quantity { get; set; }

  /// <summary>
  /// 複價 := 單價 x 數量
  /// </summary>
  public decimal ExtendedPrice => UnitPrice * Quantity;

  /// <summary>
  /// 項次 L1
  /// </summary>
  public int L1Num { get; set; }

  /// <summary>
  /// 項次 L2
  /// </summary>
  public int L2Num { get; set; }
}

public class DropItemValidator : AbstractValidator<DropItem>
{
  public DropItemValidator()
  {
    //RuleFor(m => m.userId).NotEmpty();
    //RuleFor(m => m.credential).NotEmpty();
    //RuleFor(m => m.vcode).NotEmpty();
  }
}