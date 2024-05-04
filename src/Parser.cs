namespace PixelEngine.Lang;
public class Parser(IEnumerable<Token> tokens) {
  public Stack<Token> tokens = new(tokens);

  public Token Eat() {
    return tokens.Pop();
  }
  public Token Peek() {
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
          var operand = ParseDot();
          
          if (operand is Identifier id) {
            return ParseIdentifierStatement(id);
          }
          else if (operand is DotExpr dot) {
            return ParseLValueStatement(dot);
          }
          else if (operand is CallableExpr expr) {
            return new CallableStatment(expr.id, expr.args);
          }
          else if (operand is NativeCallableExpr nExpr) {
            return NativeCallableStatement.FromExpr(nExpr);
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

  private Statement ParseLValueStatement(DotExpr dot) {
    Expect(TType.Assign);
    var value = ParseExpression();
    return new LValue(dot, value);
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
        } else {
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
        var iden = (ParseOperand() as Identifier)!;
        inc = ParseDeclOrAssign(iden) as Assignment;
    }
    
    return new For(decl, condition, inc, ParseBlock());
  }
  private Else ParseElse() {
    Eat(); // consume else.
    if (Peek().type == TType.If) {
      Eat();
      var @if = ParseIf();
      return new Else(@if);
    } else {
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
    while (tokens.Count > 0 && Peek().type != TType.RParen) {
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

  private Expression ParseCallExpr(Token token) {
    List<Expression> args = ParseArguments();
    Identifier iden = new(token.value);
    if (NativeFunctions.TryCreateCallable(iden.name, out var callable)) {
      return new NativeCallableExpr(callable, args);
    }
    if (ASTNode.Context.TryGet(iden, out var call)) {
      return new CallableExpr(iden, args);
    }
    return new ExprError($"Use of undeclared identifier {iden}");
  }
  private Statement ParseCall(Identifier iden) {
    List<Expression> args = ParseArguments();

    if (NativeFunctions.TryCreateCallable(iden.name, out var callable)) {
      return new NativeCallableStatement(callable, args);
    }

    if (ASTNode.Context.TryGet(iden, out _)) {
      return new CallableStatment(iden, args);
    }
    return new Error($"Use of undeclared identifier {iden}");
  }

  private List<Expression> ParseArguments() {
    Expect(TType.LParen);
    List<Expression> args = [];
    while (Peek().type != TType.RParen) {
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
      var val = value.Evaluate();
      ASTNode.Context.TrySet(iden, value.Evaluate() ?? Value.Default);
      return new Declaration(iden, value);
    }
  }
  
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
    Expression left = ParseDot();
    
    while (Peek().type == TType.Plus || Peek().type == TType.Minus) {
      TType op = Eat().type;
      Expression right = ParseDot();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }
  
  private Expression ParseDot() {
    Expression left = ParseOperand();

    while (Peek().type == TType.Dot) {
      Eat();
      Expression right = ParseOperand();

      left = new DotExpr(left, right);
    }

    return left;
  }
  
  private Expression ParseOperand() {
    var token = Peek();
    if (token.type == TType.Minus || token.type == TType.Not) {
      Eat();
      var operand = ParseOperand();
      return new UnaryExpr(token.type, operand);
    }
    
    switch (token.type) {
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
      case TType.Float:
        Eat();
        return new Operand(Number.FromFloat(token.value));
      case TType.Int:
        Eat();
        return new Operand(Number.FromInt(token.value));
      case TType.Identifier:
        Eat();
        if (Peek().type == TType.LParen) {
          return ParseCallExpr(token);
        }
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


}
