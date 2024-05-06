using System.Text;

namespace PixelEngine.Lang;

public class Array : Value {
  public List<Value> values = [];
  private bool initialized;
  private readonly List<Expression>? initializer;
  public Array(List<Expression> init) {
    initializer = init;
  }
  public Array() {
    
  }

  public override string ToString() {
    StringBuilder sb = new();
    sb.Append('[');
    foreach (var val in values) {
      sb.Append(val);
      if (values.Last() != val) {
        sb.Append(", ");
      }
    }
    sb.Append(']');
    return sb.ToString();
  }
  public override bool Equals(object? obj) {
    if (obj is Array other) {
      return other.values == this.values;
    }
    return false;
  }
  public override int GetHashCode() {
    return values?.GetHashCode() ?? 0;
  }

  public override Value Add(Value other) {
    Push(other);
    return this;
  }
  
  public override Value Subtract(Value other) {
    if (!values.Remove(other)) {
      return Undefined;
    }
    return this;
  }
  
  public void Push(Value value) {
    values.Add(value);
  }
  public Value Pop() {
    var val = values.Last();
    values.Remove(val);
    return val;
  }
  public Value At(Number index) {
    var idx = index.GetNumber();
    if (idx is float fidx && (int)fidx < values.Count) {
      return values[(int)fidx];
    }
    if (idx is int iidx && iidx < values.Count) {
      return values[iidx];
    }
    return Undefined;
  }

  internal void Init() {
    if (initializer != null && !initialized) {
      foreach (var expr in initializer) {
        Push(expr.Evaluate());
      }
      initialized = true;
    }
  }

  internal void Assign(Number index, Value value) {
    var idx = index.GetNumber();
    if (idx is float fidx && (int)fidx < values.Count) {
      values[(int)fidx] = value;
    }
    if (idx is int iidx && iidx < values.Count) {
      values[iidx] = value;
    }
  }
}