using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.Component.Abstractions;

namespace Vista.Biz.DataPicking;

public class CustomCodeInfo : ICodeName, IEquatable<CustomCodeInfo>
{
  public string Code { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Group { get; set; } = string.Empty;
  public string FieldA { get; set; } = string.Empty;
  public string FieldB { get; set; } = string.Empty;
  public string FieldC { get; set; } = string.Empty;

  bool IEquatable<CustomCodeInfo>.Equals(CustomCodeInfo? other) => other == null ? false : this.Code == other.Code;
}

internal class CustomCodeRepo : CodeNameRepoBase<CustomCodeInfo>, ICodeNameRepo
{
  protected override Task<List<CustomCodeInfo>> ConcreteLoadTask()
  {
    return Task.Run(async () =>
    {
      // 模擬查詢DB時間超過２秒。正式版請移除。
      await Task.Delay(2000);

      // 模擬查詢資料庫
      var codeList = Enumerable.Range(1, 300).Select(i => new CustomCodeInfo
      {
        Code = $"{i:D3}",
        Name = $"我是{i:D3}的名稱-群組{i / 15:D3}",
        Group = $"群組{i / 15:D3}",
        FieldA = $"FieldA{i:D3}",
        FieldB = $"FieldB{i:D3}",
        FieldC = $"FieldC{i:D3}",
      }).ToList();

      return codeList;

      ////※ 以非同步語法取回基本資料
      //using var conn = await DBHelper.MyLabDB.OpenAsync();
      //var result = await conn.QueryAsync<CodeName>(@"SELECT Code = TRIM(STR([Sn])), Name = Title, Group = null FROM MyProduct (NOLOCK)");
      //return result.AsList(); // codeList
    });
  }
}
