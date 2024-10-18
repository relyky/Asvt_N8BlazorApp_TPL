using Vista.Component.Abstractions;
namespace Vista.DB.BasicData;

/// <summary>
/// 用於 AutoCompleteText 元件
/// </summary>
internal class SampleTextRepo : TextRepoBase
{
  protected override Task<string[]> ConcreteLoadTask()
  {
    string[] simsTextList = ["FOO", "BAR", "BAZ", "今天天氣真好", "吃飽好睡覺"];
    return Task.FromResult(simsTextList);

    ////※ 以非同步語法取回基本資料
    //using var conn = await DBHelper.ASVTAMS.OpenAsync();
    //var result = await conn.QueryAsync<UseDept>(@"SELECT DeptName FROM UseDept (NOLOCK) ORDER BY DeptLevel ASC;");
    //return result.Select(c => c.DeptName).ToArray();
  }
}
