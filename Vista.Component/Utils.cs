using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vista.Component;

class Utils
{
  #region CSS 工具

  /// <summary>
  /// 條件 className
  /// </summary>
  public static string Clsx(string className, bool when)
    => when ? className : string.Empty;

  /// <summary>
  /// baseClassName + 條件 className
  /// </summary>
  public static string Clsx(string baseClassName, string className, bool when)
    => baseClassName + (when ? " " + className : string.Empty);

  /// <summary>
  /// 仿 React 的 clsx 函式。
  /// 註：基於 C#10 語法限制只能有限實作部份能力。
  /// </summary>
  /// <param name="cssClassList">支援 string | ValueTuple(string,bool)</param>
  /// <example>
  /// string cssClsx1 = Utils.Clsx("d-flex pa-6", ("d-none", f_hidden));
  /// string cssClsx2 = Utils.Clsx("d-flex pa-6", f_hidden ? "d-none" : null);
  /// </example>
  public static string Clsx(params object[] cssClassList)
  {
    List<string> clsxList = [];

    foreach (var input in cssClassList)
    {
      if (input is null)
      {
        continue;
      }
      else if (input is string)
      {
        // append className
        clsxList.Add((string)input);
      }
      else if (input is ValueTuple<string, bool>)
      {
        (string className, bool when) = (ValueTuple<string, bool>)input;
        // append className when true
        if (when) clsxList.Add(className);
      }
    }

    return String.Join(" ", clsxList);
  }

  // 舊版暫留
  //public static string Clsx_Old(params object[] cssClassList)
  //{
  //  StringBuilder sb = new();
  //
  //  foreach (var input in cssClassList)
  //  {
  //    if (input is string)
  //    {
  //      string className = ((string)input).Trim();
  //      if (!String.IsNullOrEmpty(className))
  //        sb.Append(className + " ");
  //    }
  //    else if (input is ValueTuple<string, bool>)
  //    {
  //      var classObj = (ValueTuple<string, bool>)input;
  //      if (classObj.Item2)
  //      {
  //        string className = ((string)classObj.Item1).Trim();
  //        if (!String.IsNullOrEmpty(className))
  //          sb.Append(className + " ");
  //      }
  //    }
  //  }
  //
  //  return sb.ToString();
  //}

  #endregion
}
