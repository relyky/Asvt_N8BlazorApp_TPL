using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Vista.DB;

/// <summary>
/// Fisher-Yates Shuffle 是一個經典的洗牌演算法，特別適合用於抽獎系統，因為它能產生完全隨機且公平的排列。
/// </summary>
internal class FisherYatesLottery : IDisposable
{
  private readonly Random _random;
  private readonly RandomNumberGenerator _cryptoRandom;
  private bool _disposed = false;

  public FisherYatesLottery()
  {
    // 使用系統時間作為種子
    _random = new Random();
    // 使用密碼學安全的隨機數產生器（更適合重要抽獎）
    _cryptoRandom = RandomNumberGenerator.Create();
  }

  /// <summary>
  /// Fisher-Yates Shuffle 標準實作
  /// </summary>
  /// <typeparam name="T">陣列元素類型</typeparam>
  /// <param name="array">要洗牌的陣列</param>
  public void Shuffle<T>(T[] array)
  {
    if (array == null || array.Length <= 1)
      return;

    // 從最後一個元素開始，向前遍歷
    for (int i = array.Length - 1; i > 0; i--)
    {
      // 在 0 到 i（包含）之間選擇隨機索引
      int j = _random.Next(i + 1);

      // 交換元素
      (array[i], array[j]) = (array[j], array[i]);
    }
  }

  /// <summary>
  /// 使用密碼學安全隨機數的 Fisher-Yates Shuffle
  /// 適用於需要高安全性的抽獎
  /// </summary>
  /// <typeparam name="T">陣列元素類型</typeparam>
  /// <param name="array">要洗牌的陣列</param>
  public void SecureShuffle<T>(T[] array)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    if (array == null || array.Length <= 1)
      return;

    for (int i = array.Length - 1; i > 0; i--)
    {
      int j = GetSecureRandomInt(i + 1);
      (array[i], array[j]) = (array[j], array[i]);
    }
  }

  /// <summary>
  /// 產生密碼學安全的隨機整數
  /// </summary>
  /// <param name="maxValue">最大值（不包含）</param>
  /// <returns>0 到 maxValue-1 之間的隨機整數</returns>
  private int GetSecureRandomInt(int maxValue)
  {
    ObjectDisposedException.ThrowIf(_disposed, this);

    if (maxValue <= 1) return 0;

    byte[] bytes = new byte[4];
    int result;

    do
    {
      _cryptoRandom.GetBytes(bytes);
      result = Math.Abs(BitConverter.ToInt32(bytes, 0));
    } while (result >= int.MaxValue - (int.MaxValue % maxValue));

    return result % maxValue;
  }

  /// <summary>
  /// 進行抽獎，返回指定數量的中獎者
  /// </summary>
  /// <typeparam name="T">參與者類型</typeparam>
  /// <param name="participants">參與者列表</param>
  /// <param name="winnerCount">中獎人數</param>
  /// <param name="useSecureRandom">是否使用密碼學安全隨機數</param>
  /// <returns>中獎者陣列</returns>
  public T[] DrawLottery<T>(IEnumerable<T> participants, int winnerCount, bool useSecureRandom = false)
  {
    var participantArray = participants.ToArray();

    if (participantArray.Length == 0)
      return Array.Empty<T>();

    if (winnerCount >= participantArray.Length)
      return participantArray;

    // 洗牌
    if (useSecureRandom)
      SecureShuffle(participantArray);
    else
      Shuffle(participantArray);

    // 取前 winnerCount 個作為中獎者
    var winners = new T[winnerCount];
    Array.Copy(participantArray, winners, winnerCount);

    return winners;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposed && disposing)
    {
      _cryptoRandom?.Dispose();
      _disposed = true;
    }
  }

  /// <summary>
  /// 檢查物件是否已釋放
  /// </summary>
  protected void ThrowIfDisposed()
  {
    ObjectDisposedException.ThrowIf(_disposed, this);
  }
}

