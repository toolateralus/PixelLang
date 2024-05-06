using System.Text;

namespace PixelEngine.Lang;

public class Callable(Block block, Parameters parameters) : Value(null, ValueFlags.Callable) {
  public Block block = block;
  public Parameters parameters = parameters;
  public virtual Value Call(List<Expression> args) {
    List<Value> values = GetArgsValueList(args);
    ASTNode.Context.PushScope();
    for (int i = 0; i < values.Count; i++) {
      Value? value = values[i];
      var name = parameters.names[i];

      // No shadowing.
      ASTNode.Context.Current.variables.TryAdd(name.name, value);
    }

    foreach (Statement? statement in block.statements) {
      var result = statement.Evaluate();
      Statement.CatchError(result);
      
      if (result is Value value) {
        return value;
      }
    }
    ASTNode.Context.PopScope();
    return Undefined;
  }

  public static List<Value> GetArgsValueList(List<Expression> args) {
    var values = new List<Value>();
    for (int i = 0; i < args.Count; i++) {
      Expression? arg = args[i];
      var value = arg.Evaluate();
      values.Add(value);
    }

    return values;
  }
  public override string ToString() {
    StringBuilder builder = new();
    builder.Append("callable(");
    foreach (var p in parameters.names) {
      builder.Append(p.name);
    }
    builder.Append(')');
    return builder.ToString();
  }
  
  public override bool Equals(object? obj) {
    return obj is Callable c && c == this;
  }
  
  public override int GetHashCode() {
    return this.block.GetHashCode();
  }
}
