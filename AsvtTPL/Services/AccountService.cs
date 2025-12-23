using AsvtTPL;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Net;
using Vista.Models;

namespace Vista.Services;

/// <summary>
/// 帳號登入操作與授權資料緩存。
/// 帳號是否登入依 Auth-Cookie 或與 SSO 登入狀態判斷。
/// 注意：現在登入使用者用 IAuthUser 介面注入取得。
/// </summary>
public class AccountService(
  IConfiguration _config,
  IHttpContextAccessor _http,
  IMemoryCache _cache,
  ILogger<AccountService> _logger)
{
  #region resource

  internal record AuthTicket
  {
    public Guid ticketId;
    public string userId = String.Empty;
    public string returnUrl = String.Empty;
    public bool rememberMe = false;
    public DateTime expires;
  }

  #endregion
  #region State

  /// <summary>
  /// 門票緩衝池
  /// </summary>
  //readonly ConcurrentDictionary<Guid, AuthTicket> _ticketPool = new();

  readonly object _lockObj = new object();

  #endregion
  #region helper

  /// <summary>
  /// AuthUser: 一位用戶只能有一份登入狀態。時效之內都有效。
  /// </summary>
  void AuthUserPool_PutIn(AuthUser auth)
  {
    lock (_lockObj)
    {
      string key = $"AuthData:{auth.UserId}";
      TimeSpan expires = auth.ExpiresUtc - DateTimeOffset.UtcNow;
      if (expires > TimeSpan.Zero)
        _cache.Set<AuthUser>(key, auth, expires);
    }
  }

  /// <summary>
  /// AuthTicket: 只能用一次。
  /// </summary>
  void AuthTicketPool_PutIn(AuthTicket ticket)
  {
    lock (_lockObj)
    {
      string key = $"AuthTicket:{ticket.ticketId}";
      TimeSpan expires = ticket.expires - DateTime.Now;
      if (expires > TimeSpan.Zero)
        _cache.Set<AuthTicket>(key, ticket, expires);
    }
  }

  #endregion

  /// <summary>
  /// 認證檢查
  /// ※需確保在後端執行。
  /// </summary>
  internal AuthTicket? Authenticate(LoginArgs ln)
  {
    // 取登入者來源IP
    ThisProjectHelper.GetLoginClientIp(_http.HttpContext, out string clientIp, out string hostName);

    try
    {
      if (String.IsNullOrWhiteSpace(ln.userId))
        throw new ApplicationException("登入認證失敗！");

      if (String.IsNullOrWhiteSpace(ln.credential))
        throw new ApplicationException("登入認證失敗！");

      //## verify vcode;
      if (!"0222210505".Equals(ln.vcode))
        throw new ApplicationException("登入認證失敗！");

      //※ 模擬登入時間，正式版移除
      SpinWait.SpinUntil(() => false, 800);

      //## 驗證帳號與密碼
      ////# 先確定帳號已註冊
      //if (!AuthModule.TryGetUserRegistered(ln.userId, out UserInfo regUser))
      //{
      //  _logger.LogError($"登入認證失敗因為未註冊，帳號：{ln.userId}。");
      //  throw new ApplicationException("登入認證失敗！");
      //}

      ////## 再AD驗證
      //ADAuthResult? ad = null;
      //if (!"ByPassAD".Equals(_config["ADByPass"]))
      //{
      //  string ldapHost = _config["ADHost"]!;
      //  ad = ADAuthenticate(ldapHost, ln.userId, ln.credential);
      //  if (ad == null)
      //    throw new ApplicationException("登入認證失敗！");
      //
      //  //## 若不存在 AdUser 資料，新增它。
      //  bool isEnabled = AuthModule.CheckAndRegisterUser(new UserInfo
      //  {
      //    UserID = ad.SamAccountName,
      //    CName = ad.Name,
      //    IsEnabled = "Y",
      //  });
      //  if (!isEnabled)
      //    throw new ApplicationException("登入認證失敗！");
      //}
      //else
      //{
      //  _logger.LogWarning($"登入認證時跳過AD驗證，帳號：{ln.userId}。");
      //
      //  //※ 不驗帳密就需先註冊
      //  if (!AuthModule.TryGetUserRegistered(ln.userId, out UserInfo regUser))
      //  {
      //    _logger.LogError($"登入認證失敗因為未註冊，帳號：{ln.userId}。");
      //    throw new ApplicationException("登入認證失敗！");
      //  }
      //
      //  //# 仿AD驗證結果
      //  ad = new ADAuthResult
      //  {
      //    Path = "ByPassAD",
      //    SamAccountName = regUser.UserID,
      //    Name = regUser.CName,
      //  };
      //}

      //## 帳號特例：內定系統管理員
      //if (ln.userId != "管理員" || ln.credential != @"asvt")
      //    return null;

      //## 製作 ticket
      var ticket = new AuthTicket
      {
        ticketId = Guid.NewGuid(),
        userId = ln.userId, // ad.SamAccountName,
        returnUrl = ln.returnUrl,
        rememberMe = ln.rememberMe,
        expires = DateTime.Now.AddSeconds(20)
      };

      //# success
      _logger.LogDebug($"User [{ticket.userId}] authenticate SUCCESS, from {hostName}({clientIp}).");
      return ticket;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"User [{ln.userId}] authenticate FAIL, from {hostName}({clientIp}).");
      return null;
    }
  }

  /// <summary>
  /// 取得授權資料，並存入授權資料緩存區。
  /// ※需確保在後端執行。
  /// </summary>
  internal string? Authorize(AuthTicket ticket)
  {
    // 取登入者來源IP
    ThisProjectHelper.GetLoginClientIp(_http.HttpContext, out string clientIp, out string hostName);

    try
    {
      double expiresMinutes = _config.GetValue<double>("ExpiresMinutes");
      AuthUser? auth = null;

      ///
      ///※ 可以進來執行表示身份驗證已成功。這裡只處理取得授權能力。
      ///

      #region ## 帳號特例：內定系統管理員
      // boss,beauty,smart
      if ((new string[] { "boss", "beauty", "smart" }).Contains(ticket.userId))
      {
        if (ticket.userId == "boss")
        {
          auth = new AuthUser
          {
            UserId = "boss",
            UserName = "祝海邊",
            Roles = ["Boss", "Staff"],
          };
        }
        else if (ticket.userId == "beauty")
        {
          auth = new AuthUser
          {
            UserId = "beauty",
            UserName = "甄美麗",
            Roles = ["Admin", "Staff"],
          };
        }
        else //if(ticket.userId == "smart")
        {
          auth = new AuthUser
          {
            UserId = "smart",
            UserName = "郝聰明",
            Roles = ["Staff"],
          };
        }

        auth.AuthMenu = new();
        auth.AuthMenu.AddMenuGroup(new MenuGroup { GroupId = "TEST", GroupName = "功能測試" })
            .AddMenuItem(new MenuItem { FuncId = "TEST001", FuncName = "Auth", Url = "auth" })
            .AddMenuItem(new MenuItem { FuncId = "TEST002", FuncName = "Counter", Url = "counter" })
            .AddMenuItem(new MenuItem { FuncId = "TEST003", FuncName = "Weather", Url = "weather" })
            .AddMenuItem(new MenuItem { FuncId = "TEST004", FuncName = "My Form", Url = "myform" })
            .AddMenuItem(new MenuItem { FuncId = "TEST005", FuncName = "My Form2", Url = "myform2" });

        auth.AuthMenu.AddMenuGroup(new MenuGroup { GroupId = "DEMO", GroupName = "功能展示" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO002", FuncName = "佈景主題", Url = "DEMO002" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO003", FuncName = "基本操作", Url = "DEMO003" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO004", FuncName = "零件展示", Url = "DEMO004" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO005", FuncName = "表單部局", Url = "DEMO005" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO006", FuncName = "基本表單驗證", Url = "DEMO006" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO007", FuncName = "各項機制測試", Url = "DEMO007" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO008", FuncName = "檔案上傳樣板", Url = "DEMO008" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO009", FuncName = "查詢作業樣板", Url = "DEMO009" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO011", FuncName = "ＣＲＵＤ少欄位樣板", Url = "DEMO011" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO012", FuncName = "ＣＲＵＤ多欄位樣板", Url = "DEMO012" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO013", FuncName = "步步申請樣板", Url = "DEMO013" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO014", FuncName = "案件審查樣板", Url = "DEMO014" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO017", FuncName = "基本表單驗證Ｙ", Url = "DEMO017" });

        auth.AuthMenu.AddMenuGroup(new MenuGroup { GroupId = "DEMO2", GroupName = "應用展示" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO201", FuncName = "上傳圖檔", Url = "DEMO201" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO202", FuncName = "Render React Component", Url = "DEMO202" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO203", FuncName = "產生與掃描 QR Code", Url = "DEMO203" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO205", FuncName = "播放影片測試", Url = "DEMO205" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO206", FuncName = "Dynamic Expression", Url = "DEMO206" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO207", FuncName = "QR Code Scanner Sample", Url = "DEMO207" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO208", FuncName = "拖拉編輯展示", Url = "DEMO208" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO209", FuncName = "拖拉編輯展示二", Url = "DEMO209" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO210", FuncName = "畫面分割範例", Url = "DEMO210" })
            .AddMenuItem(new MenuItem { FuncId = "DEMO211", FuncName = "畫面分割範例２", Url = "DEMO211" });
      }

      #endregion

      ////## 取得授權(未實作)
      //AuthUser? auth = AuthModule.GetUserAuthz(ticket.userId, _config["SystemID"]);

      if (auth == null)
        throw new ArgumentNullException(nameof(auth));

      // 補充:登入來源資訊、時效等
      auth.ClientIp = clientIp;
      auth.ClientHostName = hostName;
      auth.IssuedUtc = DateTimeOffset.UtcNow;
      auth.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(expiresMinutes);
      auth.AuthGuid = Guid.NewGuid();
      auth.RememberMe = ticket.rememberMe;

      //# 用 JWT 包裝 ticketId
      string ticketToken = Utils.JwtHostingEncode<Guid>(ticket.ticketId);
      //string ticketToken = ticket.ticketId.ToString();

      AuthTicketPool_PutIn(ticket);
      AuthUserPool_PutIn(auth);

      // success
      //※ 正常來說授權不會失敗！
      _logger.LogDebug($"User [{auth.UserId}] authorize SUCCESS, from {hostName}({clientIp}).");
      return ticketToken;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"User [{ticket.userId}] authorize FAIL, from {hostName}({clientIp}).");
      return null;
    }
  }

  internal bool IsUserLoggedIn(string userId)
  {
    lock (_lockObj)
    {
      // isLogin
      return _cache.TryGetValue($"AuthData:{userId}", out object? _);
    }
  }



  /// <summary>
  /// AuthTicketPool_TakeOut。
  /// 只能用一次。
  /// </summary>
  internal AuthTicket? TakeOutTicket(Guid ticketId)
  {
    lock (_lockObj)
    {
      string key = $"AuthTicket:{ticketId}";
      var ticket = _cache.Get<AuthTicket>(key);
      _cache.Remove(key); // 只能用一次，故取出就移除。
      return ticket;
    }
  }

  /// <summary>
  /// AuthUserPool_TakeOut。
  /// 一位用戶只能有一份登入狀態。時效之內都有效。
  /// </summary>
  internal AuthUser? GetAuthDataFromPool(string userId)
  {
    lock (_lockObj)
    {
      string key = $"AuthData:{userId}";
      var auth = _cache.Get<AuthUser>(key);

      // 若已過時則清除。
      if (auth != null && DateTime.UtcNow > auth.ExpiresUtc)
      {
        _cache.Remove(key);
        return null;
      }

      return auth;
    }
  }
}
