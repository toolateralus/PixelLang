namespace PixelEngine.Lang;

[Flags]
public enum ValueFlags {
  Number = 1 << 0,
  String = 1 << 1,
  Callable = 1 << 2,
  Object = 1 << 3,
  Bool = 1 << 4,
  Undefined = 1 << 5,
}

public class Undefined() : Value(new(), ValueFlags.Undefined) {
  public override bool Equals(object? obj) {
    return obj == Undefined;
  }
  
  public override int GetHashCode() {
    return -1;
  }
  public override string ToString() {
    return "undefined";
  }
}
public class Value(object? value = null, ValueFlags flags = ValueFlags.Number) {
  public override string ToString() {
    return value?.ToString() ?? "null";
  }
  public readonly ValueFlags flags = flags;
  internal protected object? value = value;
  public static readonly Undefined Undefined = new();
  public static readonly Value Null = new(null);
  public bool Get<T>(out T val) {
    if (value is T t) {
      val = t;
      return true;
    }
    val = default!;
    return false;
  }
  public Value Set(object? value) {
    if (value is Array a && this is Array v) {
      v.values = a.values;
      return this;
    } else if (value is Object o && this is Object o1) {
      o.scope = o1.scope;
      o.block = o1.block;
      return this;
    }
    this.value = value switch {
      Number n => n.value,
      String s => s.value,
      Bool b => b.value,
      float f => f,
      int i => i,
      string s => s,
      bool b => b,
      _ => value,
    };
    
    return this;
  }
  
  
  public virtual Value Divide(Value other) {
    return Undefined;
  }
  public virtual Value Multiply(Value other) {
    return Undefined;
  }
  public virtual Value Subtract(Value other) {
    return Undefined;
  }
  public virtual Value Not() {
    return Undefined;
  }
  public virtual Value Add(Value other) {
    return Undefined;
  }
  public virtual Bool Or(Value other) {
    return Bool.Default;
  }
  public virtual Bool And(Value other) {
    return Bool.Default;
  }
  public virtual Bool GreaterThan(Value other) {
    return Bool.Default;
  }
  public virtual Bool LessThan(Value other) {
    return Bool.Default;
  }
  public virtual Bool GreaterThanOrEqual(Value other) {
    return Bool.Default;
  }
  public virtual Bool LessThanOrEqual(Value other) {
    return Bool.Default;
  }
  public virtual Value Negate() {
    return Number.Default;
  }
  
}
