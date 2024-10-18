using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vista.Component.Abstractions;

namespace Vista.Models;

static class OptionRepos
{
  /// <summary>
  /// 姓別
  /// </summary>
  public static readonly SelectOptionRepo Gender = new(new()
  {
    { "M", "男性" },
    { "F", "女性" },
    { "X", "第三性" },
  });

  /// <summary>
  /// 興趣
  /// </summary>
  public static readonly SelectOptionRepo Interesting = new(new()
  {
    { "animation", "動畫" },
    { "aquascape", "水族造景" },
    { "art", "藝術" },
    { "astrology", "占星術" },
    { "badminton", "羽球" },
    { "bake", "烘烤" },
    { "basketball", "籃球" },
    { "billiards", "桌球" },
    { "camping", "露營" },
    { "climbing", "爬山" },
    { "planting", "植裁" },
  });

  /// <summary>
  /// 職稱
  /// </summary>
  public static readonly SelectOptionRepo PositionTitle = new(new()
  {
    {"GM", "總經理-General Manager" },
    {"DGM","副總經理-Deputy General Manager" },
    {"DM", "部門經理-Department Manager" },
    {"PM", "專案經理-Project Manager" },
    {"HRM","人力資源經理-Human Resources Manager" },
    {"MKM","行銷經理-Marketing Manager" },
    {"FM", "財務經理-Finance Manager" },
    {"AM", "行政經理-Administration Manager" },
    {"SM", "業務經理-Sales Manager" },
    {"ENG","工程師-Engineer" },
    {"PGR","程式設計師-Programmer" },
    {"DES","設計師-Designer" },
    {"ACC","會計師-Accountant" },
    {"AST","助理-Assistant" },
    {"SEC","秘書-Secretary" },
    {"TRS","翻譯-Translator" },
    {"TCH","老師-Teacher" },
    {"DOC","醫生-Doctor" },
    {"LWY","律師-Lawyer" },
  });
}
