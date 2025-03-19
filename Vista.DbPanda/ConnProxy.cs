using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Security;
using System.Security.Cryptography;
using System.Text;

/***************************************************************************
第三版 DBHelper.v３.0 on 2023-1-18
***************************************************************************/

namespace Vista.DbPanda;

public class ConnProxy
{
  /// <summary>
  /// 連線字串，平常保持在加密狀態。
  /// </summary>
  private SecureString _connStr;

#if DEBUG
  /// <summary>
  /// for DEBUG: 檢查連線字串內容
  /// </summary>
  public String DebugConnString => AsString(_connStr);
#endif

  public ConnProxy(string connString)
  {
    /// 連線字串只有建構時可設定。
    _connStr = AsSecureString(connString);
  }

  public ConnProxy(SecureString connString)
  {
    /// 連線字串只有建構時可設定。
    _connStr = connString;
  }

  /// <summary>
  /// 依 IConfiguration 取得連線字串。
  /// </summary>
  public ConnProxy(string connName, IConfiguration config)
  {
    /// 連線字串只有建構時可設定。
    _connStr = DoCreateConnProxyWithIConfig(connName, config);
  }

  private SecureString DoCreateConnProxyWithIConfig(string connName, IConfiguration config)
  {
    try
    {
      bool enablePlainText = "EnablePlainText".Equals(config["ConnStringParser"]);
      if (enablePlainText)
        return AsSecureString(config.GetConnectionString(connName)!);

      // 解密連線字串
      return AsSecureString(Decrypt(config.GetConnectionString(connName)!));
    }
    catch (Exception ex)
    {
      throw new ApplicationException($"取得連線字串[{connName}]失敗！", ex);
    }
  }

  public virtual SqlConnection Open()
  {
    try
    {
      var conn = CreateSqlConnection(_connStr);
      conn.Open();
      //OnOpenSuccess?.Invoke(this, new EventArgs());
      return conn;
    }
    catch (Exception ex)
    {
      //OnOpenFail?.Invoke(this, new ErrMsgEventArgs($"DB連線失敗！", ErrSeverity.Exception, ex));
      throw;
    }
  }

  public async Task<SqlConnection> OpenAsync()
  {
    try
    {
      var conn = CreateSqlConnection(_connStr);
      await conn.OpenAsync();
      //OnOpenSuccess?.Invoke(this, new EventArgs());
      return conn;
    }
    catch (Exception ex)
    {
      //OnOpenFail?.Invoke(this, new ErrMsgEventArgs($"DB連線失敗！", ErrSeverity.Exception, ex));
      throw;
    }
  }

  private static SecureString AsSecureString(String str)
  {
    SecureString secstr = new();
    str.ToList().ForEach(secstr.AppendChar);
    secstr.MakeReadOnly();
    return secstr;
  }

  private static SqlConnection CreateSqlConnection(SecureString ss)
  {
    try
    {
      IntPtr ssAsIntPtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(ss);
      //string connStr = System.Runtime.InteropServices.Marshal.PtrToStringUni(ssAsIntPtr);
      StringBuilder connStr = new();
      for (Int32 i = 0; i < ss.Length; i++)
      {
        // multiply 2 because Unicode chars are 2 bytes
        Char ch = (Char)System.Runtime.InteropServices.Marshal.ReadInt16(ssAsIntPtr, i * 2);
        // do something with each char
        connStr.Append(ch);
      }
      // don't forget to free it at the end
      System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(ssAsIntPtr);

      return new SqlConnection(connStr.ToString());
    }
    catch
    {
      return null;
    }
  }

  private static String AsString(SecureString secstr)
  {
    IntPtr valuePtr = IntPtr.Zero;
    try
    {
      valuePtr = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secstr);
      return System.Runtime.InteropServices.Marshal.PtrToStringUni(valuePtr);
    }
    catch
    {
      return null;
    }
    finally
    {
      System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
    }
  }

  // 移除，因為未達預期作用。
  //public event EventHandler OnOpenSuccess;
  //public event EventHandler<ErrMsgEventArgs> OnOpenFail;

  #region SecureString helepr

  private string Decrypt(string encryptedText)
  {
    // Remove all new-lines
    encryptedText = encryptedText.Replace(Environment.NewLine, "");

    // 解開 Protected SecureString
    string decryptedText = DoUpprotectText(encryptedText);
    return decryptedText;
  }

  private string Encrypt(string encryptedText)
  {
    string cypherText = DoProtectText(encryptedText);
    return cypherText;
  }

  /// <summary>
  /// 說明：SecureString 的基底是 ProtectedData 類別。
  /// </summary>
  private static string DoUpprotectText(string cypherText)
  {
    // Convert the hex dump to byte array
    int length = cypherText.Length / 2;
    byte[] encryptedData = new byte[length];
    for (int index = 0; index < length; ++index)
    {
      var chunk = cypherText.Substring(2 * index, 2);
      encryptedData[index] = byte.Parse(chunk, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    byte[] data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
    string unprotectedText = Encoding.Unicode.GetString(data);
    return unprotectedText;
  }

  /// <summary>
  /// 說明：SecureString 的基底是 ProtectedData 類別。
  /// </summary>
  private static string DoProtectText(string plainText)
  {
    byte[] plainBlob = Encoding.Unicode.GetBytes(plainText);
    byte[] cypherBlob = ProtectedData.Protect(plainBlob, null, DataProtectionScope.LocalMachine);
    string cypherText = Convert.ToHexString(cypherBlob);
    return cypherText;
  }

  #endregion
}
