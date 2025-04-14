namespace Vista.DB.Schema
{
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// 測試產品
/// </summary>
[Table("MyProduct")]
public class MyProduct 
{
  /// <summary>
  /// 序號
  /// </summary>
  [Display(Name = "序號")]
  [Key]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public int Sn { get; set; }
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public Byte[] RowVersion { get; set; }
  /// <summary>
  /// Computed Definition: (CONVERT([bigint],[RowVersion]))
  /// </summary>
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public Int64? RowVersionLong { get; set; }
  /// <summary>
  /// 抬頭
  /// </summary>
  [Display(Name = "抬頭")]
  public string Title { get; set; } = default!;
  /// <summary>
  /// 狀態: Enable | Disable
  /// </summary>
  [Display(Name = "狀態")]
  public string Status { get; set; } = default!;

  public void Copy(MyProduct src)
  {
    this.Sn = src.Sn;
    this.RowVersion = src.RowVersion;
    this.RowVersionLong = src.RowVersionLong;
    this.Title = src.Title;
    this.Status = src.Status;
  }

  public MyProduct Clone()
  {
    return new MyProduct {
      Sn = this.Sn,
      RowVersion = this.RowVersion,
      RowVersionLong = this.RowVersionLong,
      Title = this.Title,
      Status = this.Status,
    };
  }
}
}

