namespace PixelEngine.Lang;

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
