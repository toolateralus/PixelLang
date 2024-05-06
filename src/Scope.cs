namespace PixelEngine.Lang;
public class Context {
  public readonly Stack<Scope> Scopes = [];
  public Scope PopScope() {
    return Scopes.Pop();
  }
  public Scope Current => Scopes.Peek();
  public Scope PushScope(Scope? scope = null) {
    scope ??= new();
    Scopes.Push(scope);
    return scope;
  }
  
  /// <summary>
  /// Try get a variable's value from any scope.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="val"></param>
  /// <returns></returns>
  public bool TryGet(Identifier id, out Value val) {
    foreach (var scope in Scopes) {
      if (scope.variables.TryGetValue(id.name, out val!)) {
        return true;
      }
    }
    val = Value.Undefined;
    return false;
  }  
  /// <summary>
  /// Try set or add a variable
  /// </summary>
  /// <param name="id"></param>
  /// <param name="value"></param>
  /// <returns>Returns true if the variable is set in a parent scope, returns false if a new variable was created in the current scope.</returns>
  public bool TrySet(Identifier id, Value value) {
    foreach (var scope in Scopes) {
      if (scope.variables.ContainsKey(id.name)) {
        scope.variables[id.name] = value; 
        return true;
      }
    }
    var vars = Current.variables;
    if (!vars.TryAdd(id.name, value)) {
      vars[id.name] = value;
    }

    return false;
  }
}
public class Scope {
  public readonly Dictionary<string, Value> variables = [];
}