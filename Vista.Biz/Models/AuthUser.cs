namespace Vista.Models;

/// <summary>
/// 取得現在使用者授權明細
/// </summary>
public interface IAuthUser
{
  internal AuthUser? GetCurrentUser();
  internal Task<AuthUser?> GetCurrentUserAsync();
}

record AuthUser
{
  public string UserId { get; init; } = default!;
  public string UserName { get; init; } = default!;
  public string[] Roles { get; init; } = default!;
  public Guid AuthGuid { get; set; }
  public DateTimeOffset IssuedUtc { get; set; }
  public DateTimeOffset ExpiresUtc { get; set; }
  public bool RememberMe { get; set; }
  public string ClientIp { get; set; } = default!;
  public string ClientHostName { get; set; } = default!;

  /// <summary>
  /// 授權功能選單
  /// </summary>
  public MenuInfo AuthMenu { get; set; } = default!;

  /// <summary>
  /// 授權功能清單
  /// </summary>
  public string[] AuthFuncList() => AuthMenu.GroupList.SelectMany(g => g.FuncList, (g, f) => f.FuncId).ToArray();
}

record MenuInfo
{
  public List<MenuGroup> GroupList { get; set; } = [];

  public MenuGroup AddMenuGroup(MenuGroup menuGroup)
  {
    GroupList.Add(menuGroup);
    return menuGroup;
  }
}

record MenuGroup
{
  public string GroupName { get; set; } = default!;
  public string GroupId { get; set; } = default!;
  public List<MenuItem> FuncList { get; set; } = [];
  public bool IsShow { get; set; } = true;

  public MenuGroup AddMenuItem(MenuItem item)
  {
    FuncList.Add(item);
    return this;
  }
}

record MenuItem
{
  public string FuncId { get; set; } = default!;
  public string FuncName { get; set; } = default!;
  public string Url { get; set; } = default!;
  //public string Tip { get; set; } = default!; // 沒有用
  public bool IsShow { get; set; } = true;
}

///// <summary>
/////  排程不會登入，帳號用"System"替代。
///// </summary>
//internal class SystemAuthUser : IAuthUser
//{
//  public AuthUser GetCurrentUser()
//  {
//    return new AuthUser
//    {
//      UserId = "System",
//      UserName = "系統",
//      Roles = new[] { "System" },
//      AuthMenu = new MenuInfo()
//    };
//  }
//}