using System.Text;

namespace PixelEngine.Lang;

[Flags]
public enum ValueFlags {
  Number = 1 << 0,
  String = 1 << 1,
  Callable = 1 << 2,
  Object = 1 << 3,
}

public class Value(object? value, ValueFlags flags = ValueFlags.Number) {
  public override string ToString() {
    return value?.ToString() ?? "null";
  }
  public readonly ValueFlags flags = flags;
  public static readonly Value Default = new(null);
  internal protected object? value = value;
  public T Get<T>() {
    if (value is T t) {
      return t;
    }
    return default!;
  }
  public void Set(object? value) {
    this.value = value;
  }

}

public class Object(Block block, Scope scope) : Value(null, ValueFlags.Object) {
  
  public Block block = block;
  public Scope scope = scope;
  
  public static new readonly Object Default = new(null!, null!);

  public override string ToString() {
    StringBuilder builder = new();
    builder.AppendLine("{");
    foreach (var variable in scope.variables) {
      builder.AppendLine($"\"{variable.Key}\" : {variable.Value.ToString()}");
    }
    builder.AppendLine("}");
    return builder.ToString();
  }

}

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
      Statement.CatchError(statement.Evaluate());
    }
    ASTNode.Context.PopScope();
    return Default;
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
}

public class String(string value) : Value(value, ValueFlags.String);

public class Number : Value {
  private Number(object? value) : base(value, ValueFlags.Number){}
  public static Number FromInt(string value)  {
    return new Number(int.Parse(value));
  }
  public static Number FromFloat(string value) {
    return new Number(float.Parse(value));
  }
  public static new readonly Number Default = FromInt("0");
  public object? GetNumber() {
    object left = Get<int>();
    left ??= Get<float>();
    return left;
  }
  public Number Divide(Number other) {
      object? left = GetNumber();
      if (left == null) 
        return Default;
        
      object? right = other.GetNumber();
      if (right == null)
        return Default;
      return new(Convert.ToSingle(left) / Convert.ToSingle(right));
  }

  public Number Multiply(Number other) {
      object? left = GetNumber();
      if (left == null) 
        return Default;
        
      object? right = other.GetNumber();
      if (right == null)
        return Default;
      return new(Convert.ToSingle(left) * Convert.ToSingle(right));
  }

  public Number Subtract(Number other) {
      object? left = GetNumber();
      if (left == null) 
        return Default;
        
      object? right = other.GetNumber();
      if (right == null)
        return Default;
      return new(Convert.ToSingle(left) - Convert.ToSingle(right));
  }

  public Number Add(Number other) {
      object? left = GetNumber();
      if (left == null) 
        return Default;
        
      object? right = other.GetNumber();
      if (right == null)
        return Default;
      return new(Convert.ToSingle(left) + Convert.ToSingle(right));
  }
  
}