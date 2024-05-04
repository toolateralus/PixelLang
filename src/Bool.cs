namespace PixelEngine.Lang;

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
  public override Value Not() {
    if (this.value is bool b) {
      return new(!b);
    }
    return Bool.Default;
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
