namespace PixelEngine.Lang;
public class AnonObject(Block block, Scope scope) : Expression {
  private readonly Block block = block;
  private readonly Scope scope = scope;
  public override Value Evaluate() {
    return new Object(block, scope);
  }
}
public abstract class ASTNode {
  public static readonly Context Context = new();
  
  static ASTNode() {
    Context.PushScope(); // push a root scope.
  }
  
  public abstract object? Evaluate();
}
public class Error(string message) : Statement {
  public string message = message;
  public override object? Evaluate() {
    throw new Exception(message);
  }

  public Exception AsException() {
    return new Exception(message);
  }
}
public class Program : ASTNode {
  public readonly List<Statement> statements = [];
  public override object? Evaluate() {
    foreach (var statement in statements) {
      var result = statement.Evaluate();
      if (result is Error error) {
        error.Evaluate();
      }
    }
    return null;
  }
}
public class Operand(Value value) : Expression {
  public Value value = value;
  public override Value Evaluate() {
    return value;
  }
}
public abstract class Expression : ASTNode {
  public abstract override Value Evaluate();
}
public abstract class Statement : ASTNode {
  public abstract override object? Evaluate();
  
  public static void CatchError(object? v) {
    if (v is Error e) {
      throw e.AsException();
    }
  }
}
public class CallableStatment(Identifier id, List<Expression> args) : Statement {
  public readonly List<Expression> args = args;
  public readonly Identifier id = id;
  public override object? Evaluate() {
    if (Context.TryGet(id, out var value)) {
      return (value as Callable)?.Call(args);
    }
    return new Error($"Failed to call {id}");
  }
}
public class CallableExpr(Identifier id, List<Expression> args) : Expression {
  public readonly List<Expression> args = args;
  public readonly Identifier id = id;
  public override Value Evaluate() {
    if (Context.TryGet(id, out var func)) {
      return (func as Callable)?.Call(args) ?? Value.Default;
    }
    throw new Exception($"Failed to call callable {id}");
  }
}
public class BinExpr(Expression left, Expression right) : Expression {
  public Expression left = left;
  public Expression right = right;
  public TType op;
  public override Value Evaluate() {
    switch (op) {
      case TType.Plus:{
        var left = this.left.Evaluate() as Number;
        left?.Add(right.Evaluate() as Number ?? throw new InvalidOperationException("Invalid arithmetic"));
        return left!;
      }
      case TType.Minus: {
        var left = this.left.Evaluate() as Number;
        left?.Subtract(right.Evaluate() as Number ?? throw new InvalidOperationException("Invalid arithmetic"));
        return left!;
      }
      case TType.Divide: {
        var left = this.left.Evaluate() as Number;
        left?.Divide(right.Evaluate() as Number ?? throw new InvalidOperationException("Invalid arithmetic"));
        return left!;
      }
      case TType.Multiply: {
        var left = this.left.Evaluate() as Number;
        left?.Multiply(right.Evaluate() as Number ?? throw new InvalidOperationException("Invalid arithmetic"));
        return left!;
      }
      case TType.Assign: {
        var left = this.left.Evaluate();
        left.Set(right.Evaluate());
        return left;
      }
      default: return Number.Default;
    }
  }
}
public class Declaration(Identifier id, Expression value) : Statement {
  public Identifier id = id;
  public Expression value = value;
  public override object? Evaluate() {
    Context.TrySet(id, value.Evaluate());
    return null;
  }
}
public class Assignment(Identifier id, Expression value) : Statement {
  public readonly Identifier id = id;
  public readonly Expression value = value;
  public override object? Evaluate() {
    Context.TrySet(id, value.Evaluate());
    return null;
  }
}
public class FuncDecl(Identifier name, Parameters parameters, Block body) : Statement {
  public readonly Identifier name = name;
  public readonly Block body = body;
  public readonly Parameters parameters = parameters;
  
  public override object? Evaluate() {
    if (Context.TryGet(name, out var val)) {
      return val;
    }
    return new Error($"Failed to dereference {name}");
  }
}
public class Block(List<Statement> statements) : Statement{
  public readonly List<Statement> statements = statements;
  
  public override object? Evaluate() {
    foreach (var statement in statements) {
      CatchError(statement.Evaluate());
    }
    return null;
  }
  
  
}
public class Identifier(string name) : Expression {
  public readonly string name = name;
  public string Get() {
    // this will probably get more compilcated as we have lvalues / dot operation
    return name;
  }
  
  public override Value Evaluate() {
    if (Context.TryGet(this, out var val)) {
      return val;
    }
    return Value.Default;
  }
}
public class Parameters(List<Identifier> names) : Statement {
  public List<Identifier> names = names;
  public override object? Evaluate() {
    return null; // this doesnt need to do anything right now
    // maybe it will declare some variables when passed some values
    // but this probably has to be handled externally by teh fn call.
  }
}