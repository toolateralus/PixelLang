using System.Reflection;
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
        Console.Write(arg);
      }
      return Value.Default;
    },
    ["println"] = (args) => {
      foreach (var arg in args) {
        Console.WriteLine(arg);
      }
      return Value.Default;
    },
    ["readkey"] = (args) => {
      var value = Console.ReadKey();
      return new String(value.KeyChar.ToString());
    },
    ["readkeycode"] = (args) => {
      var value = Console.ReadKey();
      return Number.FromInt((int)value.Key);
    },
    ["readln"] = (args) => {
      if (args.Count > 0) {
        foreach (var arg in args) {
          Console.WriteLine(arg);
        }
      }
      var value = Console.ReadLine() ?? "";
      return new String(value);
    },
    ["to_string"] = (args) => {
      StringBuilder builder = new();
      foreach (var arg in args) {
        builder.Append(arg.ToString());
      }
      return new String(builder.ToString());
    },

  };

  public static bool RegisterFunction(string name, NativeFunction func) {
    return functions.TryAdd(name, func);
  }
  public static bool TryCreateCallable(string name, out NativeCallable callable) {
    callable = null!;
    if (functions.TryGetValue(name, out _)) {
      callable = new NativeCallable(name);
      return true;
    }
    return false;
  }
  public static void LoadModule(string dllPath) {
    var assembly = Assembly.LoadFrom(dllPath);
    foreach (var type in assembly.GetTypes()) {
      if (typeof(INativeModule).IsAssignableFrom(type)) {
        var module = Activator.CreateInstance(type) as INativeModule ?? throw new NullReferenceException($"Failed to load module. {dllPath}");
        foreach (var function in module.GetFunctions()) {
          functions[function.Key] = function.Value;
        }
      }
    }
  }
}

public interface INativeModule {
  public Dictionary<string, NativeFunction> GetFunctions();
  public static void ThrowInvalidType<T>(Value value) {
    throw new Exception($"{value} is the incorrect type, {typeof(T).Name} was expected.");
  }
  public static int GetInt(Value value) {
    if (value is Number n && n.Get<int>(out var result)) {
      return result;
    }
    if (value is Number fn && fn.Get<float>(out var fresult)) {
      return (int)fresult;
    }
    throw new TypeAccessException($"Expected a type of (int), got {value.value ?? "null"}");
  }
  public static float GetFloat(Value value) {
    if (value is Number n && n.Get<float>(out var result)) {
      return result;
    }
    if (value is Number fn && fn.Get<int>(out var fresult)) {
      return (float)fresult;
    }
    throw new TypeAccessException($"Expected a type of (float), got {value.value ?? "null"}");
  }
  public static bool GetBool(Value value) {
    if (value is Number n && n.Get<bool>(out var result)) {
      return result;
    }
    throw new TypeAccessException($"Expected a type of (bool), got {value.value ?? "null"}");
  }
  public static string GetString(Value value) {
    if (value is Number n && n.Get<string>(out var result)) {
      return result;
    }
    throw new TypeAccessException($"Expected a type of (string), got {value.value ?? "null"}");
  }
}