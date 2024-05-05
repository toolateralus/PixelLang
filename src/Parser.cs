namespace PixelEngine.Lang;
public class Parser(IEnumerable<Token> tokens) {
  public Stack<Token> tokens = new(tokens);

  public Token Eat() {
    return tokens.Pop();
  }
  public Token Peek() {
    if (tokens.Count == 0) {
      return Token.EOF;
    }
    return tokens.Peek();
  }
  /// <summary>
  /// This eats the expected token or throws an exception
  /// </summary>
  /// <param name="type"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  public Token Expect(TType type) {
    if (Peek()?.type == type) {
      return Eat();
    }
    throw new Exception($"Expected {type}.. got {Peek()?.type}");
  }
  public Program ParseProgram() {
    Program program = new();
    while (tokens.Count > 0) {
      if (Peek().type == TType.Newline) {
        Eat();
        continue;
      }
      var statement = ParseStatement();
      Statement.CatchError(statement);
      program.statements.Add(statement);
    }
    return program;
  }
  public Statement ParseStatement() {
    while (tokens.Count > 0) {
      var token = Peek();
      switch (token.family) {
        case TFamily.Operator:
          if (token.type == TType.Newline) {
            Eat();
            continue;
          }
          else goto default; // err
        case TFamily.Identifier:
          var operand = ParsePostfix();
          if (operand is Identifier id) {
            return ParseIdentifierStatement(id);
          }
          else if (operand is SubscriptExpr) {
            return ParseLValuePostFix(operand);
          }
          else if (operand is DotExpr dot) {
            return ParseLValuePostFix(dot);
          }
          else if (operand is CallableExpr expr) {
            return new CallableStatment(expr.operand, expr.args);
          }
          else if (operand is NativeCallableExpr nExpr) {
            return NativeCallableStatement.FromExpr(nExpr);
          } else if (operand is CompoundAssignExpr cExpr) {
            return new CompoundAssignStmnt(cExpr);
          }
          return new Error($"Failed to start identifier statement {operand}");
        case TFamily.Keyword: {
            Eat(); // consume kw
            return ParseKeyword(token);
          }
        default: return new Error($"Failed to parse statement.. {token}");
      }
    }
    return UnexpectedEOI();
  }
  private Statement ParseLValuePostFix(Expression expr) {
    if (expr is DotExpr dot) {
      if (Peek().type == TType.Assign) {
        Eat();
        var value = ParseExpression();
        return new DotAssignStmnt(dot, value);
      }
      else if (dot.right is CallableExpr) {
        return new DotCallStmnt(dot);
      }
    }
    else if (expr is SubscriptExpr subscript) {
      if (Peek().type == TType.Assign) {
        Eat();
        var value = ParseExpression();
        return new SubscriptAssignStmnt(subscript, value);
      }
    }

    return new Error("Failed to parse LValue postfix statement");
  }
  private Statement ParseKeyword(Token token) {
    switch (token.type) {
      case TType.Func: {
          return ParseFuncDecl();
        }
      case TType.If: {
          return ParseIf();
        }
      case TType.Else: {
          return new Error("An else must be preceeded by an if statement.");
        }
      case TType.For: {
          return ParseFor();
        }
      case TType.Continue: {
          return ParseContinue();
        }
      case TType.Return: {
          return ParseReturn();
        }
      case TType.Start: {
        return new Coroutine(ParseStatement());
      }
      case TType.Import: {
          Expect(TType.String);
          return new NoopStatement();
        }
      case TType.Break: {
          return ParseBreak();
        }

      default: return new Error($"Failed to parse keyword {token}");
    }
  }
  private Statement ParseBreak() {
    return new Break();
  }
  private Statement ParseReturn() {
    var expr = ParseExpression();
    return new Return(expr);
  }
  private Statement ParseContinue() {
    return new Continue();
  }
  private Statement ParseFor() {
    if (Peek().type == TType.LParen) {
      Eat();
    }
    Statement? decl = null;
    Expression? condition = null;
    Statement? inc = null;
    
    if (Peek().type == TType.LCurly) {
      return new For(null, null, null, ParseBlock());
    }
    
    if (Peek().type == TType.Identifier) {
      var idTok = Peek();
      var iden = (ParseOperand() as Identifier)!;

      if (Peek().type == TType.Assign) {
        decl = ParseDeclOrAssign(iden);
      }
      else {
        tokens.Push(idTok);
        condition = ParseExpression();
      }
    }
    
    if (Peek().type == TType.Comma) {
      Eat();
      condition = ParseExpression();
    }
    
    if (Peek().type == TType.Comma) {
      Eat();
      inc = ParseStatement();
    }
    
    return new For(decl, condition, inc, ParseBlock());
  }
  private Else ParseElse() {
    Eat(); // consume else.
    if (Peek().type == TType.If) {
      Eat();
      var @if = ParseIf();
      return new Else(@if);
    }
    else {
      return new Else(ParseBlock());
    }

  }
  private If ParseIf() {
    var condition = ParseExpression();
    var block = ParseBlock();
    var ifStmnt = new If(condition, block);
    if (Peek().type == TType.Else) {
      var @else = ParseElse();
      ifStmnt.@else = @else;
    }
    return ifStmnt;
  }
  private Statement ParseIdentifierStatement(Identifier identifier) {
    switch (Peek().type) {
      case TType.Assign: {
          return ParseDeclOrAssign(identifier);
        }
      case TType.LParen: {
          return ParseCall(identifier);
        }
      default: return new Error($"Failed to parse identifier {Peek()}");
    }
  }
  private FuncDecl ParseFuncDecl() {
    // we parse a single token here and make an iden to prevent parsing issues.
    // otherwise iden() is recognized as a fn call, wheras we want that to be treated as
    // a parameterless fn call.
    var name = Expect(TType.Identifier);
    var parameters = ParseParameters();
    Statement.CatchError(parameters);
    var body = ParseBlock();
    var id = new Identifier(name.value);
    var @params = (parameters as Parameters)!;
    var func = new FuncDecl(id, @params, body); ;
    ASTNode.Context.TrySet(id, new Callable(func.body, @params));
    return func;
  }
  private Block ParseBlock() {
    Expect(TType.LCurly);
    List<Statement> statements = [];
    var next = Peek();
    while (next.type != TType.RCurly) {

      if (next.type == TType.Newline) {
        Eat();
        next = Peek();
        continue;
      }

      var statement = ParseStatement();
      Statement.CatchError(statement);
      statements.Add(statement);
      next = Peek();
    }
    Expect(TType.RCurly);
    return new Block(statements);
  }
  private Statement ParseParameters() {
    Expect(TType.LParen);
    List<Identifier> paramNames = [];
    while (tokens.Count > 0) {
      var next = Peek();
      if (next.type == TType.RParen) {
        break;
      }
      else if (paramNames.Count > 0) {
        Expect(TType.Comma);
      }
      var iden = ParseOperand();
      if (iden is not Identifier id) {
        return new Error("Cannot use a non-identifer as a parameter declaration");
      }
      paramNames.Add(id);
    }
    Expect(TType.RParen);
    return new Parameters(paramNames);
  }
  private static Error UnexpectedEOI() {
    return new Error("Unexpceted end of input");
  }
  private Expression ParseCallExpr(Expression operand) {
    List<Expression> args = ParseArguments();
    if (operand is Identifier id && NativeFunctions.TryCreateCallable(id.name, out var callable)) {
      return new NativeCallableExpr(callable, args);
    }
    return new CallableExpr(operand, args);
  }
  private Statement ParseCall(Expression operand) {
    List<Expression> args = ParseArguments();
    if (operand is Identifier id && NativeFunctions.TryCreateCallable(id.name, out var callable)) {
      return new NativeCallableStatement(callable, args);
    }
    return new CallableStatment(operand, args);
  }
  private List<Expression> ParseArguments() {
    Expect(TType.LParen);
    List<Expression> args = [];
    while (tokens.Count > 0) {
      var next = Peek();
      if (next.type == TType.RParen) {
        break;
      }
      else if (args.Count > 0) {
        Expect(TType.Comma);
      }
      args.Add(ParseExpression());
    }
    Expect(TType.RParen);
    return args;
  }
  private Statement ParseDeclOrAssign(Identifier iden) {
    Expect(TType.Assign);
    var value = ParseExpression();
    if (ASTNode.Context.TryGet(iden, out _)) {
      return new Assignment(iden, value);
    }
    else {
      ASTNode.Context.TrySet(iden, value.Evaluate() ?? Value.Default);
      return new Declaration(iden, value);
    }
  }
  #region Expressions
  private Expression ParseExpression() {
    return ParseLogicalOr();
  }

  private Expression ParseLogicalOr() {
    Expression left = ParseLogicalAnd();

    while (Peek().type == TType.LogicalOr) {
      TType op = Eat().type;
      Expression right = ParseLogicalAnd();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }

  private Expression ParseLogicalAnd() {
    Expression left = ParseEquality();

    while (Peek().type == TType.LogicalAnd) {
      TType op = Eat().type;
      Expression right = ParseEquality();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }

  private Expression ParseEquality() {
    Expression left = ParseComparison();

    while (Peek().type == TType.Equal || Peek().type == TType.NotEqual) {
      TType op = Eat().type;
      Expression right = ParseComparison();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }

  private Expression ParseComparison() {
    Expression left = ParseTerm();

    while (Peek().type == TType.Greater || Peek().type == TType.Less ||
         Peek().type == TType.GreaterEq || Peek().type == TType.LessEq) {
      TType op = Eat().type;
      Expression right = ParseTerm();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }

  private Expression ParseTerm() {
    Expression left = ParseFactor();

    while (Peek().type == TType.Multiply || Peek().type == TType.Divide) {
      TType op = Eat().type;
      Expression right = ParseFactor();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }

  private Expression ParseFactor() {
    Expression left = ParsePostfix();

    while (Peek().type == TType.Plus || Peek().type == TType.Minus) {
      TType op = Eat().type;
      Expression right = ParsePostfix();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }

  private Expression ParsePostfix() {
    Expression left = ParseOperand();

    while (tokens.Count > 0) {
      if (Peek().type == TType.SubscriptLeft) {
        Eat();
        Expression index = ParseExpression();
        Expect(TType.SubscriptRight);
        left = new SubscriptExpr(left, index);
      }
      else if (Peek().type == TType.Dot) {
        Eat();
        var iden = ParsePostfix();
        left = new DotExpr(left, iden);
      }
      else if (Peek().type == TType.LParen) {
        left = ParseCallExpr(left);
      }
      else if (Peek().type == TType.AssignPlus || Peek().type == TType.AssignMinus ||
             Peek().type == TType.AssignMul || Peek().type == TType.AssignDiv) {
        TType op = Eat().type;
        Expression right = ParseExpression();
        left = new CompoundAssignExpr(left, right) { op = op };
      }
      else {
        break;
      }
    }

    return left;
  }


  #endregion
  private Expression ParseOperand() {
    var token = Peek();
    if (token.type == TType.Minus || token.type == TType.Not) {
      Eat();
      var operand = ParseOperand();
      return new UnaryExpr(token.type, operand);
    }

    switch (token.type) {
      case TType.SubscriptLeft: {
          return ParseArrayInitializer();
        }
      case TType.LCurly: {
          var scope = ASTNode.Context.PushScope();
          var block = ParseBlock();
          ASTNode.Context.PopScope();
          return new AnonObject(block, scope);
        }
      case TType.String: {
          Eat();
          return new Operand(new String(token.value));
        }
      case TType.True:
        Eat();
        return new Operand(new Bool(true));
      case TType.False:
        Eat();
        return new Operand(new Bool(false));
      case TType.Null:
        Eat();
        return new Operand(Value.Default);
      case TType.Float:
        Eat();
        return new Operand(Number.FromFloat(token.value));
      case TType.Int:
        Eat();
        return new Operand(Number.FromInt(token.value));
      case TType.Identifier:
        Eat();
        return new Identifier(token.value);
      case TType.LParen:
        Eat();
        Expression expr = ParseExpression();
        Expect(TType.RParen);
        return expr;
      default:
        throw new Exception($"Unexpected token: {token.type}");
    }
  }

  private Operand ParseArrayInitializer() {
    Eat();
    if (Peek().type == TType.SubscriptRight) {
      Eat();
      return new Operand(new Array());
    }
    else {
      List<Expression> values = [];
      while (Peek().type != TType.SubscriptRight) {
        var val = ParseExpression();
        values.Add(val);
        if (Peek().type == TType.Comma) {
          Eat();
        }
      }
      Expect(TType.SubscriptRight);
      return new Operand(new Array(values));
    }
  }
}

internal class Coroutine(Statement statement) : Statement {
  private Statement statement = statement;
  
  public override object? Evaluate() {
    Task.Run(()=> {
      statement.Evaluate();
    });
    return null;
  }
}