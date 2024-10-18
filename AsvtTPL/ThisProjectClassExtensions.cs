using System.Net;

namespace AsvtTPL;

//internal static class ThisProjectClassExtensions
//{
//}

internal static class ThisProjectHelper
{
  /// <summary>
  /// 取登入者來源IP
  /// </summary>
  public static void GetLoginClientIp(HttpContext? httpCtx, out string clientIp, out string hostName)
  {
    //## 取登入者來源IP
    clientIp = "無法取得來源IP";
    hostName = "無法識別或失敗";
    try
    {
      IPAddress? remoteIp = httpCtx?.Connection.RemoteIpAddress;
      if (remoteIp != null)
      {
        clientIp = remoteIp.ToString();
        IPHostEntry host = Dns.GetHostEntry(remoteIp);
        hostName = host.HostName;
      }
    }
    catch
    {
      // 預防取不到IP/HostName當掉。
    }
  }
}