using System.Text;
using Microsoft.VisualBasic;

namespace PixelEngine.Lang;

public class Object(Block block, Scope scope) : Value(null, ValueFlags.Object) {
  
  public Block block = block;
  public Scope scope = scope;

  public static new readonly Object Default = new(null!, null!);
  
  public override string ToString() {
    StringBuilder builder = new();
    if (scope == null) {
      builder.Append("NULL");
      return builder.ToString();
    }
    builder.AppendLine("{");
    foreach (var variable in scope.variables) {
        builder.AppendLine($"   \"{variable.Key}\" : {variable.Value.ToString()}");
      }
    builder.AppendLine("}");
    return builder.ToString();
  }
  
  public override bool Equals(object? obj) {
    if (obj is Object o) {
      return o.scope.variables == this.scope.variables;
    }
    return false;
  }

  
  public override int GetHashCode() {
    return scope.variables.GetHashCode();
  }

  internal Value GetMember(Identifier right) {
    if (scope.variables.TryGetValue(right.name, out var v)) {
      return v;
    }
    return Default;
  }

  internal void SetMember(Identifier right, Value value) {
    if (!scope.variables.TryAdd(right.name, value)) {
      scope.variables[right.name] = value;
    }
  }
}
