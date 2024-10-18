using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Vista.Component;

public class AsvtFieldBase<T> : ComponentBase
{
  /// <summary>
  /// 支援 EditForm 表單
  /// </summary>
  [CascadingParameter] protected EditContext EditContext { get; set; } = default!;
  [CascadingParameter(Name = "AsvtFormRowOption")] protected AsvtFormRowOption RowOption { get; set; } = default!;

  [Parameter, EditorRequired] public Expression<Func<T>> For { get; set; } = default!;
  [Parameter] public bool Immediate { get; set; }
  /// <summary>
  /// ※ Field 的 Label 屬性一般參考自 DisplayAttribute.Name 屬性，不過自己的 Label 屬性優先權較高。 
  /// </summary>
  [Parameter] public string? Label { get; set; }
  /// <summary>
  /// ※ Field 的 ReadOnly 屬性一般參考自 AsvtFormRow.ReadOnly 屬性，不過自己的 ReadOnly 屬性優先權較高。 
  /// </summary>
  [Parameter] public bool? ReadOnly { get; set; }
  [Parameter] public string? HelperText { get; set; }
  [Parameter] public bool Required { get; set; }
  [Parameter] public string? Class { get; set; }

  /// <summary>
  /// 自訂訊息。對外通知 FieldValue 已改變。
  /// </summary>
  [Parameter] public EventCallback<T> OnChange { get; set; }

  /// <summary>
  /// 欄位數值
  /// </summary>
  protected virtual T FieldValue
  {
    get => (T)_propInfo.GetValue(_fieldIdentifier.Model)!;
    set
    {
      _propInfo.SetValue(_fieldIdentifier.Model, value);
      EditContext.NotifyFieldChanged(_fieldIdentifier);
      OnChange.InvokeAsync(value);
    }
  }

  #region MudGrid
  /// <summary>
  /// RWD Grid Cell 屬性：預設四欄。[12, 6, 4, 3]。
  /// 依 xs,sm,md,lg 指定數值
  /// </summary>
  [Parameter] public int[]? Grid { get; set; } = null;

  protected int _xs = 12;
  protected int _sm = 6;
  protected int _md = 4;
  protected int _lg = 3;
  protected int _xl = 3;
  protected int _xxl = 3;

  #endregion end of: MudGrid

  protected FieldIdentifier _fieldIdentifier;  //※ 此屬性應該一開始就存在
  protected PropertyInfo _propInfo = default!; //※ 此屬性應該一開始就存在
  protected string? _label;
  protected bool _readOnly;

  protected override void OnParametersSet()
  {
    base.OnParametersSet();
    _fieldIdentifier = FieldIdentifier.Create(For);
    _propInfo = _fieldIdentifier.Model.GetType().GetProperty(_fieldIdentifier.FieldName)!;
    if (_propInfo == null) throw new ApplicationException($"無法取得欄位屬性{_fieldIdentifier.FieldName}！");

    _label = !String.IsNullOrWhiteSpace(Label) ? Label : _propInfo != null ? DisplayName(_propInfo) : null;
    _readOnly = ReadOnly.HasValue ? ReadOnly.Value : RowOption.ReadOnly;

    //for MudGrid
    int[] grid = this.Grid ?? RowOption.Grid;
    int len = grid.Length;
    _xs = len > 0 ? grid[0] : 12;
    _sm = len > 1 ? grid[1] : _xs;
    _md = len > 2 ? grid[2] : _sm;
    _lg = len > 3 ? grid[3] : _md;
    _xl = len > 4 ? grid[4] : _lg;
    _xxl = len > 5 ? grid[5] : _xl;
  }

  #region 用來替代 For。因為外部輸入型別與元件輸入型別對不上。
  /// <summary>
  /// 取得(Validation)錯誤訊息, --- GetErrorText()
  /// </summary>
  public virtual string? ValidationErrorText => String.Join(null, EditContext.GetValidationMessages(_fieldIdentifier));

  /// <summary>
  /// 是否有(Validation)錯誤訊息, --- HasErrors
  /// </summary>
  public virtual bool ValidationError => !String.IsNullOrEmpty(ValidationErrorText);

  #endregion end of: 用來替代 For。

  #region Inner Action / Helper

  /// <summary>
  /// 自 Property 的 DisplayAttribute 屬性取 Title。
  /// get DisplayAttribute Name of a property of a model. 
  /// </summary>
  static string DisplayName(PropertyInfo property)
  {
    var attr = property.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
    return attr?.GetName() ?? property.Name;
  }

  ///// <summary>
  ///// 自 Property 的 DisplayAttribute 屬性取 Title。
  ///// get DisplayAttribute Name of a property of a model. 
  ///// </summary>
  //static string Display<TProperty>(Expression<Func<TProperty>> f)
  //{
  //  MemberExpression? exp = f.Body switch
  //  {
  //    MemberExpression member => member,
  //    UnaryExpression unary => unary.Operand as MemberExpression,
  //    _ => null
  //  };
  //
  //  var property = exp?.Expression?.Type.GetProperty(exp.Member.Name);
  //  var attr = property?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
  //  return attr?.GetName() ?? exp?.Member.Name ?? "Unknow";
  //}

  #endregion
}

public class AsvtFormRowOption
{
  /// <summary>
  /// RWD Grid Cell 屬性：預設四欄。[12, 6, 4, 3]。
  /// 依 xs,sm,md,lg 指定數值
  /// </summary>
  public int[] Grid { get; set; } = [12, 6, 4, 3];
  public bool ReadOnly { get; set; } = false;
}