using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vista.Component.Abstractions;

public class CodeName : ICodeName, IEquatable<CodeName>
{
  public string Code { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string Group { get; set; } = string.Empty;

  bool IEquatable<CodeName>.Equals(CodeName? other) => other == null ? false : this.Code == other.Code;
}
