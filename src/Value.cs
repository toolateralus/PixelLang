namespace PixelEngine.Lang;

[Flags]
public enum ValueFlags {
  Number = 1 << 0,
  String = 1 << 1,
  Callable = 1 << 2,
  Object = 1 << 3,
}

public class Value(object? value, ValueFlags flags = ValueFlags.Number) {
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
}

public class Callable(Block block, Parameters parameters) : Value(null, ValueFlags.Callable) {
  public Block block = block;
  public Parameters parameters = parameters;
  private readonly Dictionary<string, Func<List<Value>, Value>> NativeFunctions = new() {
    ["print"] = (args) => {
      foreach (var val in args) {
        Console.WriteLine(val.value);
      }
      return Number.FromInt("0");
    },
  
  };
  private bool TryCallNativeFunction(string name, List<Value> func, out Value result) {
    if (NativeFunctions.TryGetValue(name, out var fn)) {
      result = fn(func);
      return true;
    }
    result = Default;
    return false;
  }
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
    return new((float)left / (float)right);
  }
  public Number Multiply(Number other) {
    object? left = GetNumber();
    if (left == null) 
      return Default;
      
    object? right = other.GetNumber();
    if (right == null)
      return Default;
    return new((float)left * (float)right);
  }
  public Number Subtract(Number other) {
    object? left = GetNumber();
    if (left == null) 
      return Default;
      
    object? right = other.GetNumber();
    if (right == null)
      return Default;
    return new((float)left - (float)right);
  }
  public Number Add(Number other) {
    object? left = GetNumber();
    if (left == null) 
      return Default;
      
    object? right = other.GetNumber();
    if (right == null)
      return Default;
    return new((float)left+ (float)right);
  }
  
}