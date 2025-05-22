namespace Vista.Biz.DataPicking;

/// <summary>
/// 客戶聯絡人資訊
/// </summary>
public class CustomCodeInfo : IEquatable<CustomCodeInfo>
{
  public string Code { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public string AcctId { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string Tel { get; init; } = string.Empty;
  public string Fax { get; init; } = string.Empty;

  bool IEquatable<CustomCodeInfo>.Equals(CustomCodeInfo? other) => other == null ? false : this.Code == other.Code;
}

class CustomCodeSeeker
{
  public async Task<List<CustomCodeInfo>> LoadAsync(string AcctId)
  {
    // 模擬查詢DB時間超過２秒。正式版請移除。
    await Task.Delay(2000);

    // 模擬查詢資料庫
    var codeList = Enumerable.Range(1, 200).Select(i => new CustomCodeInfo
    {
      Code = $"c{i:D3}",
      Name = $"我是{i:D3}的名稱",
      AcctId = $"客戶{i / 15:D3}",
      Email = $"c{i:D3}@notemail.server",
      Tel = $"02-1234-8888",
      Fax = $"02-1234-5678",
    }).ToList();

    return codeList;


    //    //※ 以非同步語法取回基本資料
    //    using var conn = await DBHelper.ASVTQUO.OpenAsync();
    //    var result = await conn.QueryAsync<AcctContact>(@"SELECT [Ssn]
    //,[AcctId]
    //,[EName]
    //,[CName]
    //,[Email]
    //,[Tel]
    //,[Fax]
    //FROM [AcctUserCont] (NOLOCK)
    //WHERE [Enable] = 'Y'
    // AND AcctId = @AcctId; ", new { AcctId });

    //    return result.AsList(); // codeList
  }
}

//public interface CodeSeekerBase<TFields, TArgs>
//  where TFields : class
//  where TArgs : class?
//{
//  public TArgs Args { get; set; }

//  public abstract Task<List<TFields>> LoadAsync();
//}
