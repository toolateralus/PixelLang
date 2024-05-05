using System.Text;

namespace PixelEngine.Lang;

public delegate Value NativeFunction(List<Value> args);

public class NativeCallable(string name) : Callable(null!, null!) {
  public string name = name;
  public override Value Call(List<Expression> args) {
    if (NativeFunctions.functions.TryGetValue(name, out var fn)) {
      return fn(GetArgsValueList(args));
    }
    // Don't return a default callable.
    return Value.Default;
  }

}

public static class NativeFunctions {
  public static readonly Dictionary<string, NativeFunction> functions = new() {
    ["print"] = (args) => {
      foreach (var arg in args) {
        Console.WriteLine(arg);
      }
      return Value.Default;
    },
    ["to_string"] = (args) => {
      StringBuilder builder = new();
      foreach (var arg in args) {
        builder.Append(arg.ToString());
      }
      return new String(builder.ToString());
    },
    
  };
  public static bool TryCreateCallable(string name, out NativeCallable callable) {
    callable = null!;
    if (functions.TryGetValue(name, out _)) {
      callable = new NativeCallable(name);
      return true;
    }
    return false;
  }
}
