using Jose;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Vista.Models;

static class Utils
{
  /// <summary>
  /// JsonSerializer Extension
  /// </summary>
  public static string JsonSerialize<TValue>(TValue value, bool UnsafeRelaxedJsonEscaping = true, bool WriteIndented = true)
  {
    var options = new JsonSerializerOptions()
    {
      WriteIndented = WriteIndented
    };

    if (UnsafeRelaxedJsonEscaping)
      options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    return JsonSerializer.Serialize(value, options);
  }

  public static bool IsEmailAddress(string email)
  {
    try
    {
      return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
          RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
    }
    catch (RegexMatchTimeoutException)
    {
      return false;
    }
  }

  /// <summary>
  /// 檢查是否為多筆 Email Address。以分號(;)分隔。
  /// </summary>
  public static bool IsMultiEmailAddress(string multiEmail, bool allowEmpty = true)
  {
    // 允許空值
    if (allowEmpty && string.IsNullOrWhiteSpace(multiEmail))
      return true;

    // 否則需符合 Email address 格式
    return multiEmail.Split(';').Select(e => e.Trim()).Where(s => s != string.Empty).All(email => IsEmailAddress(email));
  }

  /// <summary>
  /// Catch-Wait-Retry
  /// </summary>
  /// <remarks>
  /// 範例：CatchAndRetry<Exception>(3, 3000, n => {...});
  /// 範例：CatchAndRetry<Exception>(3, 3000, n => {...}, logger);
  /// </remarks>
  public static void CatchAndRetry<TException>(int retryCount, int retryDuration, Action<int> act, Action<TException, int>? loggerAct = null)
      where TException : Exception
  {
    for (; ; )
    {
      try
      {
        act.Invoke(retryCount);
        break;
      }
      catch (TException ex)
      {
        if (retryCount-- > 0) // decide to retry or not.
        {
          // used for logging the exception
          loggerAct?.Invoke(ex, retryCount);

          // waiting short time and then re-exec.
          System.Threading.SpinWait.SpinUntil(() => false, retryDuration);
          continue; // retry
        }
        throw; // fail & leave
      }
    }
  }

  /// <summary>
  /// Catch-Wait-Retry
  /// </summary>
  /// <remarks>
  /// 範例：CatchAndRetry<Exception>(new int[] {1000,2000,3000}, n => {...});
  /// 範例：CatchAndRetry<Exception>(new int[] {1000,2000,3000}, n => {...},logger);
  /// </remarks>
  public static void CatchAndRetry<TException>(int[] retryDuration, Action<int> act, Action<TException, int>? loggerAct = null)
      where TException : Exception
  {
    int idx = 0;
    for (; ; )
    {
      try
      {
        act.Invoke(idx);
        break;
      }
      catch (TException ex)
      {
        if (retryDuration.Length > idx) // decide to retry or not.
        {
          // used for logging the exception
          loggerAct?.Invoke(ex, idx);

          // waiting short time and then re-exec.
          System.Threading.SpinWait.SpinUntil(() => false, retryDuration[idx++]);
          continue; // retry
        }
        throw; // fail & leave
      }
    }
  }

  #region UrlCombine

  /// <summary>
  /// UrlCombine - 主程式
  /// 參考 → https://github.com/jean-lourenco/UrlCombine/blob/master/UrlCombineLib/UrlCombine.cs
  /// </summary>
  public static string UrlCombine(string baseUrl, params string[] relativePaths)
  {
    if (string.IsNullOrWhiteSpace(baseUrl))
      throw new ArgumentNullException(nameof(baseUrl));

    return relativePaths.Aggregate(baseUrl, (accUrl, relativePath) =>
      string.IsNullOrWhiteSpace(relativePath)
      ? accUrl
      : String.Concat(accUrl.TrimEnd('/'), "/", relativePath.TrimStart('/')));
  }

  #endregion

  #region Protect sensitive text and interoperate with PowerShell

  public static string UnprotectText(string encryptedText)
  {
    try
    {
      // Remove all new-lines
      encryptedText = encryptedText.Replace(Environment.NewLine, "");

      // 解開 Protected SecureString
      string decryptedText = DoUpprotectText(encryptedText);
      return decryptedText;
    }
    catch (Exception ex)
    {
      throw new ApplicationException("UnprotectText failed.", ex);
    }
  }

  /// <summary>
  /// 若解開失敗則回傳原始字串。
  /// </summary>
  /// <param name="enablePlainText">是否啟用明文模式</param>
  public static string UnprotectText(string encryptedText, bool enablePlainText)
  {
    try
    {
      return UnprotectText(encryptedText);
    }
    catch
    {
      if (enablePlainText)
        return encryptedText;
      else
        throw;
    }
  }

  public static string ProtectText(string encryptedText)
  {
    string cypherText = DoProtectText(encryptedText);
    return cypherText;
  }

  /// <summary>
  /// 說明：SecureString 的基底是 ProtectedData 類別。
  /// 只能處理英數字串，不支援中文。
  /// </summary>
  static string DoUpprotectText(string cypherText)
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
  /// 只能處理英數字串，不支援中文。
  /// </summary>
  static string DoProtectText(string plainText)
  {
    byte[] plainBlob = Encoding.Unicode.GetBytes(plainText);
    byte[] cypherBlob = ProtectedData.Protect(plainBlob, null, DataProtectionScope.LocalMachine);
    string cypherText = Convert.ToHexString(cypherBlob);
    return cypherText;
  }

  static string DoProtectTextB64(string plainText)
  {
    byte[] plainBlob = Encoding.Unicode.GetBytes(plainText);
    byte[] cypherBlob = ProtectedData.Protect(plainBlob, null, DataProtectionScope.LocalMachine);
    string cypherText = Convert.ToBase64String(cypherBlob);
    return cypherText;
  }

  static string DoUpprotectTextB64(string cypherText)
  {
    byte[] encryptedData = Convert.FromBase64String(cypherText);
    byte[] data = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
    string unprotectedText = Encoding.Unicode.GetString(data);
    return unprotectedText;
  }

  #endregion

  #region Simple Encryption

  /// <summary>        
  /// JwtHelper:只能用於短期且同APP內的交換訊息。
  /// </summary>
  public static string JwtHostingEncode<TObject>(TObject payload)
  {
    using var sha = new HMACSHA256(Encoding.ASCII.GetBytes(Environment.ProcessPath!));
    string envprops = $"{Environment.ProcessId}{Environment.MachineName}{Environment.Version}{Environment.UserName}{Environment.OSVersion}{Environment.UserDomainName}{Environment.ProcessorCount}{DateTime.Today.DayOfYear}0okmNJIx.y(8uhbVGY&";
    byte[] key256 = sha.ComputeHash(Encoding.ASCII.GetBytes(envprops));
    string json = JsonSerializer.Serialize<TObject>(payload);
    byte[] blob = Encoding.UTF8.GetBytes(json);
    byte[] blobR = blob.Select(b => (byte)~b).ToArray();
    string token = JWT.EncodeBytes(blobR, key256, JweAlgorithm.A256GCMKW, JweEncryption.A256GCM);
    string tokenR = String.Join('.', token.Split('.').Reverse());
    return tokenR;
  }

  /// <summary>
  /// JwtHelper:只能用於短期且同APP內的交換訊息。
  /// </summary>
  public static TObject? JwtHostingDecode<TObject>(string token)
  {
    using var sha = new HMACSHA256(Encoding.ASCII.GetBytes(Environment.ProcessPath!));
    string envprops = $"{Environment.ProcessId}{Environment.MachineName}{Environment.Version}{Environment.UserName}{Environment.OSVersion}{Environment.UserDomainName}{Environment.ProcessorCount}{DateTime.Today.DayOfYear}0okmNJIx.y(8uhbVGY&";
    byte[] key256 = sha.ComputeHash(Encoding.ASCII.GetBytes(envprops));
    string tokenR = String.Join('.', token.Split('.').Reverse());
    byte[] blobR = JWT.DecodeBytes(tokenR, key256, JweAlgorithm.A256GCMKW, JweEncryption.A256GCM);
    byte[] blob = blobR.Select(b => (byte)~b).ToArray();
    string json = Encoding.UTF8.GetString(blob, 0, blob.Length);
    TObject? payload = JsonSerializer.Deserialize<TObject?>(json);
    return payload;
  }

  /// <summary>
  /// JwtHelper:只能用於短期且同台電腦主機交換訊息。
  /// </summary>
  public static string JwtSimpleEncode<TObject>(TObject payload)
  {
    using var sha = new HMACSHA256(seed);
    string envprops = $"{Environment.MachineName}{Environment.OSVersion}{Environment.ProcessorCount}{DateTime.Today.DayOfYear}9ijnBGT%rdX0Ox.y";
    byte[] key256 = sha.ComputeHash(Encoding.ASCII.GetBytes(envprops));
    string json = JsonSerializer.Serialize<TObject>(payload);
    byte[] blob = Encoding.UTF8.GetBytes(json);
    byte[] blobR = blob.Select(b => (byte)~b).ToArray();
    string token = JWT.EncodeBytes(blobR, key256, JweAlgorithm.A256GCMKW, JweEncryption.A256GCM);
    string tokenR = String.Join('.', token.Split('.').Reverse());
    return tokenR;
  }

  /// <summary>
  /// JwtHelper:只能用於短期且同台電腦主機交換訊息。
  /// </summary>
  public static TObject? JwtSimpleDecode<TObject>(string token)
  {
    using var sha = new HMACSHA256(seed);
    string envprops = $"{Environment.MachineName}{Environment.OSVersion}{Environment.ProcessorCount}{DateTime.Today.DayOfYear}9ijnBGT%rdX0Ox.y";
    byte[] key256 = sha.ComputeHash(Encoding.ASCII.GetBytes(envprops));
    string tokenR = String.Join('.', token.Split('.').Reverse());
    byte[] blobR = JWT.DecodeBytes(tokenR, key256, JweAlgorithm.A256GCMKW, JweEncryption.A256GCM);
    byte[] blob = blobR.Select(b => (byte)~b).ToArray();
    string json = Encoding.UTF8.GetString(blob, 0, blob.Length);
    TObject? payload = JsonSerializer.Deserialize<TObject?>(json);
    return payload;
  }

  /// <summary>
  /// AesHelper:只能用於短期交換訊息。
  /// </summary>
  public static string AesSimpleEncrypt<TObject>(TObject payload, Int32 tag = 0)
  {
    using var aes = GenAesKey(tag);
    return Convert.ToBase64String(DoEncryptData(JsonSerializer.Serialize<TObject>(payload), aes));
  }

  /// <summary>
  /// AesHelper:只能用於短期交換訊息。
  /// </summary>
  public static TObject? AesSimpleDecrypt<TObject>(string cipher, Int32 tag = 0)
  {
    using var aes = GenAesKey(tag);
    return JsonSerializer.Deserialize<TObject>(DoDecryptData(Convert.FromBase64String(cipher), aes));
  }

  /// <summary>
  /// only for [AesSimpleEncrypt]
  /// </summary>
  static byte[] DoEncryptData(string plainText, Aes aesAlg)
  {
    // Create an encryptor to perform the stream transform.
    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

    // Create the streams used for encryption.
    using var msEncrypt = new MemoryStream();
    using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
    using var swEncrypt = new StreamWriter(csEncrypt);

    //Write all data to the stream.
    swEncrypt.Write(RandomString(17) + plainText);
    swEncrypt.Close();

    return msEncrypt.ToArray();
  }

  /// <summary>
  /// only for [AesSimpleDecrypt]
  /// </summary>
  static string DoDecryptData(byte[] cipherText, Aes aesAlg)
  {
    // Create a decryptor to perform the stream transform.
    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

    // Create the streams used for decryption.
    using var msDecrypt = new MemoryStream(cipherText);
    using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
    using var srDecrypt = new StreamReader(csDecrypt);

    // Read the decrypted bytes from the decrypting stream
    return srDecrypt.ReadToEnd()[17..];
  }

  /// <summary>
  /// only for [AesSimpleEncrypt,AesSimpleDecrypt]
  /// </summary>
  static Aes GenAesKey(Int32 salt = 0)
  {
    /// 產生只有當日有效的加密金鑰
    using var sha = new HMACSHA384(seed);
    DateTime d = DateTime.Today;
    string envprops = $"{11 - d.Day}{d:MMddyy}oBx39i{23 - d.DayOfWeek}x8ek{d.Day}a{43 - d.Day}7b*$X{d.DayOfWeek}3{10 - d.DayOfWeek}eo{salt}jk{37 - d.Month}erAbx{d.Year}4e6d{d.DayOfYear}3et2b5s%t{d.Month}&^y1d0O)a";
    var key48 = sha.ComputeHash(Encoding.ASCII.GetBytes(envprops));
    Aes aesAlg = Aes.Create();
    aesAlg.Key = key48[..32];
    aesAlg.IV = key48[32..];
    aesAlg.Mode = CipherMode.CFB;
    return aesAlg;
  }

  /// <summary>
  /// only for [AesSimpleEncrypt]
  /// </summary>
  static string RandomString(int length)
  {
    Random r = new Random((int)DateTime.Now.Ticks);
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+<>?";
    return new string(Enumerable.Repeat(chars, length).Select(s => s[r.Next(chars.Length)]).ToArray());
  }

  /// <summary>
  /// only for [JwtSimpleEncode] & [JwtSimpleDecode]
  /// </summary>
  static readonly byte[] seed = { 131, 117, 87, 14, 21, 150, 19, 75, 24, 10, 159, 78, 90, 51, 71, 159, 214, 186, 251, 20, 207, 246, 142, 127, 13, 29, 37, 43, 59, 30, 234, 13 };

  /// <summary>
  /// 計算檔案 SHA256 雜湊值。
  /// 傳回 Base64 字串。
  /// </summary>
  public static string ComputeSha256(byte[] fileBlob)
  {
    using SHA256 sha256 = SHA256.Create();
    byte[] hashBytes = sha256.ComputeHash(fileBlob);
    return Convert.ToBase64String(hashBytes);
  }

  #endregion

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
