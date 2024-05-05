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
      Statement.CatchError(statement.Evaluate());
    }
    return null;
  }
}
public class Operand(Value value) : Expression {
  public Value value = value;
  public override Value Evaluate() {
    if (value is Array array) {
      array.Init();
    }
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
public class CallableStatment(Expression operand, List<Expression> args) : Statement {
  public readonly List<Expression> args = args;
  public readonly Expression operand = operand;
  public override object? Evaluate() {
    if (operand is Identifier id && Context.TryGet(id, out var value)) {
      return (value as Callable)?.Call(args);
    } else  {
      var lvalue = operand.Evaluate();
      return (lvalue as Callable)?.Call(args);
    }
  }
}
public class CallableExpr(Expression operand, List<Expression> args) : Expression {
  public readonly List<Expression> args = args;
  public readonly Expression operand = operand;
  public override Value Evaluate() {
    if (operand is Identifier id && Context.TryGet(id, out var func)) {
      return (func as Callable)?.Call(args) ?? Value.Default;
    } else if (operand.Evaluate() is Callable callable) {
      return callable.Call(args);
    }
    throw new Exception($"Failed to call callable {operand}. This is likely an undefined function or the type is not callable");
  }
}
public class Else : Statement {
  private readonly If? @if;
  private readonly Block? block;

  public Else(If @if) {
    this.@if = @if;
  }
  public Else(Block block) {
    this.block = block;
  }

  public override object? Evaluate() {
    if (@if != null) {
      return @if.Evaluate();
    }
    else {
      return block?.Evaluate();
    }
  }
}

public class DotExpr(Expression left, Expression right) : Expression {
  public readonly Expression left = left;
  public readonly Expression right = right;
  public override Value Evaluate() {
    var leftValue = left.Evaluate();
    if (leftValue is not Object obj) {
      throw new Exception("Left-hand side of dot operator must be an object");
    }
    Context.PushScope(obj.scope);
    var result = right switch {
      DotExpr dotExpr => dotExpr.Evaluate(),
      Identifier identifier => identifier.Evaluate(),
      CallableExpr call => call.Evaluate(),
      SubscriptExpr expr => expr.Evaluate(),
      _ => throw new Exception($"Invalid Right hand side type: {right}")
    };
    Context.PopScope();
    return result;
  }
  public void Assign(Value value) {
    var leftValue = left.Evaluate();
    if (leftValue is not Object obj) {
      throw new Exception("Left-hand side of dot operator must be an object");
    }

    if (right is DotExpr dotExpr) {
      Context.PushScope(obj.scope);
      dotExpr.Assign(value);
      Context.PopScope();
    }

    if (right is Identifier identifier) {
      obj.SetMember(identifier, value);
    }
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
      return new Bool(left.Equals(right));
    }
    else if (op == TType.NotEqual) {
      return new Bool(!left.Equals(right));
    }

    var result = op switch {
      TType.Plus => left.Add(right),
      TType.Minus => left.Subtract(right),
      TType.Divide => left.Divide(right),
      TType.Multiply => left.Multiply(right),
      TType.LogicalOr => left.Or(right),
      TType.LogicalAnd => left.And(right),
      TType.Greater => left.GreaterThan(right),
      TType.Less => left.LessThan(right),
      TType.GreaterEq => left.GreaterThanOrEqual(right),
      TType.LessEq => left.LessThanOrEqual(right),
      TType.Assign => left.Set(right),
      _ =>
         Number.Default,
    };

    //Console.WriteLine($"Result : {result}");

    return result;
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
public class Block(List<Statement> statements) : Statement {
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
    if (NativeFunctions.TryCreateCallable(this.name, out var callable)) {
      return callable;
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
        _ = increment?.Evaluate();

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
    else {
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
public class If(Expression condition, Block block) : Statement {
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
    }
    else {
      return @else?.Evaluate();
    }
    return null;
  }
}
public class UnaryExpr(TType type, Expression operand) : Expression {
  public readonly TType op = type;
  public readonly Expression operand = operand;

  public override Value Evaluate() {
    if (op == TType.Minus) {
      return operand.Evaluate().Negate();
    }
    else if (op == TType.Not) {
      return operand.Evaluate().Not();
    }
    return Value.Default;
  }
}
public class DotAssignStmnt(DotExpr dot, Expression value) : Statement {
  private readonly DotExpr dot = dot;
  private readonly Expression value = value;
  public override object? Evaluate() {
    dot.Assign(value.Evaluate());
    return null;
  }
}
public class DotCallStmnt(DotExpr dot) : Statement {
  private readonly DotExpr dot = dot;
  public override object? Evaluate() {
    return dot.Evaluate();
  }
}

public class SubscriptExpr(Expression left, Expression index) : Expression {
  public readonly Expression left = left;
  public readonly Expression index = index;
  public override Value Evaluate() {
    var lvalue = left.Evaluate();
    if (lvalue is not Array array) {
      throw new Exception("Cannot use subscript on anything but an array/map type.");
    }
    var idx = index.Evaluate();
    if (idx is not Number number) {
      throw new Exception("Subscript index must be a number.");
    }
    return array.At(number);
  }
}


internal class SubscriptAssignStmnt(SubscriptExpr subscript, Expression value) : Statement {
  private readonly SubscriptExpr subscript = subscript;
  private readonly Expression value = value;
  public override object? Evaluate() {
    var lvalue = subscript.left.Evaluate();
    var idx = subscript.index.Evaluate();
    if (lvalue is not Array array || idx is not Number number) {
      throw new Exception("Cannot subscript on non-array types or non-numeric indices");
    }
    array.Assign(number, value.Evaluate());
    return Value.Default;
  }
}

internal class CompoundAssignStmnt : Statement {
  private CompoundAssignExpr expr;
  public CompoundAssignStmnt(CompoundAssignExpr expr) {
    this.expr = expr;
  }
  public override object? Evaluate() {
    expr.Evaluate();
    return null;
  }
}

internal class CompoundAssignExpr(Expression left, Expression right) : Expression {
  private Expression left = left;
  private Expression right = right;
  public TType op { get; set; }
  public override Value Evaluate() {
    var left = this.left.Evaluate();
    var right = this.right.Evaluate();
    
    switch (op) {
      case TType.AssignDiv:
        left.Set(left.Divide(right));
        break;
      case TType.AssignMul:
        left.Set(left.Multiply(right));
        break;
      case TType.AssignPlus: 
        left.Set(left.Add(right));
        break;
      case TType.AssignMinus:
        left.Set(left.Subtract(right));
        break;
    }
    return left;
  }
}

public class NoopStatement : Statement {
  public override object? Evaluate() {
    // do nothing
    return null;
  }
}