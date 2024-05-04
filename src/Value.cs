using System.Text;

namespace PixelEngine.Lang;

[Flags]
public enum ValueFlags {
  Number = 1 << 0,
  String = 1 << 1,
  Callable = 1 << 2,
  Object = 1 << 3,
  Bool = 1 << 4,
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


  public virtual Value Divide(Value other) {
    return Default;
  }
  public virtual Value Multiply(Value other) {
    return Default;
  }
  public virtual Value Subtract(Value other) {
    return Default;
  }
  public virtual Value Add(Value other) {
    return Default;
  }
  public virtual Bool Or(Value other) {
    return Bool.False;
  }
  public virtual Bool And(Value other) {
    return Bool.False;
  }
  public virtual Bool GreaterThan(Value other) {
    return Bool.False;
  }
  public virtual Bool LessThan(Value other) {
    return Bool.False;
  }
  public virtual Bool GreaterThanOrEqual(Value other) {
    return Bool.False;
  }
  public virtual Bool LessThanOrEqual(Value other) {
    return Bool.False;
  }

  public override bool Equals(object? obj) {
    if (obj is Value value) {
      return value.value == this.value;
    }
    return false;
  }

  public override int GetHashCode() {
    return value?.GetHashCode() ?? 0;
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

  public override bool Equals(object? obj) {
    if (obj is Object o) {
      return o.scope.variables == this.scope.variables;
    }
    return false;
  }
  public override int GetHashCode() {
    return scope.variables.GetHashCode();
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

public class Bool(bool value) : Value(value, ValueFlags.Bool) {
  public static new Bool Default => False;
  public static readonly Bool False = new(false);
  public static readonly Bool True = new(true);
  public override bool Equals(object? obj) {
    if (obj is Bool b) {
      if (b.value is bool b1 && this.value is bool b2) {
        return b1 == b2;
      }
    }
    return false;
  }

  public override int GetHashCode() {
    if (this.value is bool b) {
      return b.GetHashCode();
    }
    return 0;
  }

  public override Bool Or(Value other) {
    if (other is Bool b && b.value is bool bval && this.value is bool bval1) {
      return new(bval || bval1);
    }
    return Bool.False;
  }
  public override Bool And(Value other) {
    if (other is Bool b && b.value is bool bval && this.value is bool bval1) {
      return new(bval && bval1);
    }
    return Bool.False;
  }
}
public class String(string value) : Value(value, ValueFlags.String) {
  public override Value Add(Value other) {
    if (other is String str) {
      return new String($"{base.value ?? ""}{str.value as string}");
    }
    return Value.Default;
  }
  public override bool Equals(object? obj) {
    if (obj is String s) {
      if (s.value is string s1 && this.value is string s2) {
        return s1 == s2;
      }
    }
    return false;
  }

  public override int GetHashCode() {
    if (this.value is string s) {
      return s.GetHashCode();
    }
    return 0;
  }

}

public class Number : Value {
  private Number(object? value) : base(value, ValueFlags.Number) { }
  public static Number FromInt(string value) {
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
  public override Value Divide(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new(Convert.ToSingle(left) / Convert.ToSingle(right));
  }
  
  public override Value Multiply(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new(Convert.ToSingle(left) * Convert.ToSingle(right));
  }
  
  public override Value Subtract(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new(Convert.ToSingle(left) - Convert.ToSingle(right));
  }
  
  public override  Value Add(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;

    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new(Convert.ToSingle(left) + Convert.ToSingle(right));
  }

  public override Bool GreaterThan(Value other) {
    if (other is Number n) {
      if (n.value is float fval && this.value is float fval1) {
        return new(fval > fval1);
      }
      else if (n.value is int ival && this.value is int ival1) {
        return new(ival > ival1);
      }
      else if (n.value is float fval2 && this.value is int ival2) {
        return new(fval2 > ival2);
      }
      else if (n.value is int ival3 && this.value is float fval3) {
        return new(ival3 > fval3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }
  public override Bool LessThan(Value other) {
     if (other is Number n) {
      if (n.value is float fval && this.value is float fval1) {
        return new(fval < fval1);
      }
      else if (n.value is int ival && this.value is int ival1) {
        return new(ival < ival1);
      }
      else if (n.value is float fval2 && this.value is int ival2) {
        return new(fval2 < ival2);
      }
      else if (n.value is int ival3 && this.value is float fval3) {
        return new(ival3 < fval3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }
  public override Bool GreaterThanOrEqual(Value other) {
     if (other is Number n) {
      if (n.value is float fval && this.value is float fval1) {
        return new(fval >= fval1);
      }
      else if (n.value is int ival && this.value is int ival1) {
        return new(ival >= ival1);
      }
      else if (n.value is float fval2 && this.value is int ival2) {
        return new(fval2 >= ival2);
      }
      else if (n.value is int ival3 && this.value is float fval3) {
        return new(ival3 >= fval3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }
  public override Bool LessThanOrEqual(Value other) {
     if (other is Number n) {
      if (n.value is float fval && this.value is float fval1) {
        return new(fval <= fval1);
      }
      else if (n.value is int ival && this.value is int ival1) {
        return new(ival <= ival1);
      }
      else if (n.value is float fval2 && this.value is int ival2) {
        return new(fval2 <= ival2);
      }
      else if (n.value is int ival3 && this.value is float fval3) {
        return new(ival3 <= fval3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }

  public override bool Equals(object? obj) {
    if (obj is Number n) {
      if (n.value is int i1 && this.value is int i2) {
        return i1 == i2;
      }
      else if (n.value is float f1 && this.value is float f2) {
        return f1 == f2;
      }
      else if (n.value is int i3 && this.value is float f3) {
        return i3 == f3;
      }
      else if (n.value is float f4 && this.value is int i4) {
        return f4 == i4;
      }
    }
    return false;
  }

  public override int GetHashCode() {
    if (this.value is int i) {
      return i.GetHashCode();
    }
    else if (this.value is float f) {
      return f.GetHashCode();
    }
    return 0;
  }

}