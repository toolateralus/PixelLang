namespace PixelEngine.Lang;

public class Array : Value {
  public List<Value> values = [];
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
      return Default;
    }
    return other;
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
    return Default;
  }
  
}