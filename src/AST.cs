using System.ComponentModel;

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
    var left = this.left.Evaluate();
    var right = this.right.Evaluate();
    
    if (op == TType.Equal) {
      return new Bool(left?.Equals(right) ?? false);
    } else if (op == TType.NotEqual) {
      return new Bool((!left?.Equals(right)) ?? false);
    }
    
    switch (op) {
      case TType.Plus:
        return left?.Add(right ?? Value.Default) ?? Value.Default;
      case TType.Minus:
        return left?.Subtract(right ?? Value.Default) ?? Value.Default;
      case TType.Divide:
        return left?.Divide(right ?? Value.Default)?? Value.Default;
      case TType.Multiply:
        return left?.Multiply(right ?? Value.Default) ?? Value.Default;
      case TType.LogicalOr:
        return left?.Or(right ?? Value.Default) ?? Value.Default;
      case TType.LogicalAnd:
        return left?.And(right ?? Value.Default) ?? Value.Default;
      case TType.Greater:
        return left?.GreaterThan(right ?? Value.Default) ?? Value.Default;
      case TType.Less:
        return left.LessThan(right);
      case TType.GreaterEq:
        return left.GreaterThanOrEqual(right);
      case TType.LessEq:
        return left?.LessThanOrEqual(right ?? Value.Default) ?? Value.Default;
      case TType.Assign:
        left?.Set(right);
        return left!;
      default:
        return Number.Default;
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
      if (statement is Continue || statement is Break || statement is Return) {
        return statement;
      }
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
public class ExprError(string message) : Expression {
  private readonly string message = message;
  public override Value Evaluate() {
    throw new Exception(message);
  }
}
public class NativeCallableExpr(NativeCallable callable, List<Expression> args) : Expression {
  public readonly NativeCallable callable = callable;
  public readonly List<Expression> args = args;
  public override Value Evaluate() {
     return callable.Call(args);
  }
}
public class Break : Statement {
  public override object? Evaluate() {
    return null;
  }
}
public class Continue : Statement {
  public override object? Evaluate() {
    return null;
  }
}
public class Return(Expression value) : Statement {
  private readonly Expression value = value;
  public override object? Evaluate() {
    return value.Evaluate();
  }
}
public class For(Statement? decl, Expression? condition, Statement? increment, Block block) : Statement {
  private readonly Statement? decl = decl;
  private readonly Expression? condition = condition;
  private readonly Statement? increment = increment;
  private readonly Block block = block;
  public override object? Evaluate() {
    CatchError(decl?.Evaluate());
    if (condition != null) {
      while (true) {
        var conditionResult = condition?.Evaluate();
        if (conditionResult is not Bool b || b.Equals(Bool.False)) {
          return null;
        }
        
        
        var blockResult = block.Evaluate();
        
        if (ASTNode.Context.TryGet(new Identifier("i"), out var v)) {
          Console.WriteLine($"i == {v}");
        }
        
        _ = increment?.Evaluate();
        
        if (ASTNode.Context.TryGet(new Identifier("i"), out var v1)) {
          Console.WriteLine($"i == {v1}");
        }
        CatchError(blockResult);
        if (blockResult is Break) {
          break;
        }
        if (blockResult is Continue) {
          continue;
        }
        if (blockResult is Return ret) {
          return ret.Evaluate();
        }
      }
    } else {
      while (true) {
        _ = increment?.Evaluate();
        var blockResult = block.Evaluate();
        CatchError(blockResult);
        if (blockResult is Break) {
          break;
        }
        if (blockResult is Continue) {
          continue;
        }
        if (blockResult is Return ret) {
          return ret.Evaluate();
        }
        
      }
    }
    return null;
  }
}
public class NativeCallableStatement(NativeCallable callable, List<Expression> args) : Statement {
  public readonly NativeCallable callable = callable;
  public readonly List<Expression> args = args;

  public static NativeCallableStatement FromExpr(NativeCallableExpr nExpr) {
    return new(nExpr.callable, nExpr.args);
  }

  public override object? Evaluate() {
    return callable.Call(args);
  }
}
public class If(Expression condition, Block block) : Statement{
  private readonly Expression condition = condition;
  private readonly Block block = block;
  internal Else? @else;

  public override object? Evaluate() {
    if (condition.Evaluate() is Bool b && b.Equals(Bool.True)) {
      var result = block.Evaluate();
      CatchError(result);
      if (result is Return ret) {
        return ret.Evaluate();
      }
      if (result is Continue || result is Break) {
        return result;
      }
    } else {
      return @else?.Evaluate();
    }
    return null;
  }
}