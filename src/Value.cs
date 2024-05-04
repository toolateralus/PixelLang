namespace PixelEngine.Lang;

[Flags]
public enum ValueFlags {
  Number = 1 << 0,
  String = 1 << 1,
  Callable = 1 << 2,
  Object = 1 << 3,
  Bool = 1 << 4,
}
public class Value(object? value = null, ValueFlags flags = ValueFlags.Number) {
  public override string ToString() {
    return value?.ToString() ?? "null";
  }
  public readonly ValueFlags flags = flags;
  internal protected object? value = value;
  
  public static readonly Value Default = new(null);
  public bool Get<T>(out T val) {
    if (value is T t) {
      val = t;
      return true;
    }
    val = default!;
    return false;
  }
  public Value Set(object? value) {
    this.value = value;
    return this;
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
  public virtual Value Not() {
    return Default;
  }
  public virtual Value Add(Value other) {
    return Default;
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
