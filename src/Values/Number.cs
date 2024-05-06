namespace PixelEngine.Lang;

public class Number : Value {
  private Number(object? value) : base(value, ValueFlags.Number) { }
  public static Number ParseInt(string value) {
    return new Number(int.Parse(value));
  }
  public static Number ParseFloat(string value) {
    return new Number(float.Parse(value));
  }
  public static Number From(float value) {
    return new Number(value);
  }
  public static Number FromInt(int value) {
    return new Number(value);
  }
  public static new readonly Number Default = ParseInt("0");
  
  public object? GetNumber() {
    if (Get<int>(out var left)) {
      return left;  
    }
    if (Get<float>(out var f)) {
      return f;
    }
    return null;
  }
  public override Value Negate() {
    var val = GetNumber();
    if (val is int i) {
      return new(-i);
    } else if (val is float f) {
      return new(-f);
    }
    return Default;
  }
  public override Value Divide(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new Number(Convert.ToSingle(left) / Convert.ToSingle(right));
  }
  
  public override Value Multiply(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new Number(Convert.ToSingle(left) * Convert.ToSingle(right));
  }
  
  public override Value Subtract(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new Number(Convert.ToSingle(left) - Convert.ToSingle(right));
  }
  
  public override  Value Add(Value other) {
    object? left = GetNumber();
    if (left == null)
      return Default;
    
    object? right = (other as Number)?.GetNumber();
    if (right == null)
      return Default;
    return new Number(Convert.ToSingle(left) + Convert.ToSingle(right));
  }
  public override Bool GreaterThan(Value other) {
    if (other is Number n) {
      if (this.value is float fval && n.value is float fval1) {
        return new(fval > fval1);
      }
      else if (this.value is int ival && n.value is int ival1) {
        return new(ival > ival1);
      }
      else if (this.value is int ival2 && n.value is float fval2) {
        return new(ival2 > fval2);
      }
      else if (this.value is float fval3 && n.value is int ival3) {
        return new(fval3 > ival3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }
  public override Bool LessThan(Value other) {
     if (other is Number n) {
      if (this.value is float fval && n.value is float fval1) {
        return new(fval < fval1);
      }
      else if (this.value is int ival && n.value is int ival1) {
        return new(ival < ival1);
      }
      else if (this.value is int ival2 && n.value is float fval2) {
        return new(ival2 < fval2);
      }
      else if (this.value is float fval3 && n.value is int ival3) {
        return new(fval3 < ival3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }
  public override Bool GreaterThanOrEqual(Value other) {
     if (other is Number n) {
      if (this.value is float fval && n.value is float fval1) {
        return new(fval >= fval1);
      }
      else if (this.value is int ival && n.value is int ival1) {
        return new(ival >= ival1);
      }
      else if (this.value is int ival2 && n.value is float fval2) {
        return new(ival2 >= fval2);
      }
      else if (this.value is float fval3 && n.value is int ival3) {
        return new(fval3 >= ival3);
      }
      else {
        return Bool.False;
      }
    }
    return Bool.False;
  }
  public override Bool LessThanOrEqual(Value other) {
     if (other is Number n) {
      if (this.value is float fval && n.value is float fval1) {
        return new(fval <= fval1);
      }
      else if (this.value is int ival && n.value is int ival1) {
        return new(ival <= ival1);
      }
      else if (this.value is int ival2 && n.value is float fval2) {
        return new(ival2 <= fval2);
      }
      else if (this.value is float fval3 && n.value is int ival3) {
        return new(fval3 <= ival3);
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

  public override Bool Or(Value other) {
    return Bool.False;
  }

  public override Bool And(Value other) {
    return Bool.False;
  }
}
