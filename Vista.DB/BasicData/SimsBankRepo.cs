using Dapper;
using Vista.Component.Abstractions;
namespace Vista.DB.BasicData;

internal class SimsBankRepo : CodeNameRepoBase<CodeName>, ICodeNameRepo
{
  protected override Task<List<CodeName>> ConcreteLoadTask()
  {
    return Task.Run(async () =>
    {
      // 模擬查詢DB時間超過２秒。正式版請移除。
      await Task.Delay(2000);

      // 模擬查詢資料庫
      var codeList = Enumerable.Range(1, 200).Select(i => new CodeName
      {
        Code = $"B{i:D3}",
        Name = $"我是銀行B{i:D3}",
      }).AsList();

      return codeList;

      ////※ 以非同步語法取回基本資料
      //using var conn = await DBHelper.MyLabDB.OpenAsync();
      //var codeList = await conn.QueryAsync<CodeName>(@"SELECT Code = TRIM(STR([Sn])), Name = Title, Gropp = null FROM MyProduct");
      //return codeList.ToArray();
    });
  }
}

