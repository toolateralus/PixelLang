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
        } else goto default; // err
        case TFamily.Identifier:
          var operand = ParseOperand();
          if (operand is not Identifier id) {
            return new Error("Failed to start identifier statement");
          }
          return ParseIdentifierStatement(id);
        case TFamily.Keyword: {
            Eat(); // consume kw
            return ParseKeyword(token);
          }
        default: return new Error($"Failed to parse statement.. {token}");
      }
    }
    return UnexpectedEOI();
  }
  private Statement ParseKeyword(Token token) {
    switch (token.type) {
      case TType.Func: {
          return ParseFuncDecl();
        }
      default: return new Error($"Failed to parse keyword {token}");
    }
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
    var func = new FuncDecl(id, @params, body);;
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
  private Statement ParseCall(Identifier iden) {
    Expect(TType.LParen);
    List<Expression> args = [];
    while (Peek().type != TType.RParen) {
      args.Add(ParseExpression());
    }
    Expect(TType.RParen);
    
    if (ASTNode.Context.TryGet(iden, out _)) {
      return new CallableStatment(iden, args);
    }
    return new Error($"Use of undeclared identifier {iden}");
  }
  private Statement ParseDeclOrAssign(Identifier iden) {
    Expect(TType.Assign);
    var value = ParseExpression();
    if (ASTNode.Context.TryGet(iden, out _)) {
      return new Assignment(iden, value);
    }
    else {
      ASTNode.Context.TrySet(iden, Value.Default);
      return new Declaration(iden, value);
    }
  }
  private Expression ParseExpression() {
    return ParseTerm();
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
    Expression left = ParseOperand();

    while (Peek().type == TType.Plus || Peek().type == TType.Minus) {
      TType op = Eat().type;
      Expression right = ParseOperand();

      left = new BinExpr(left, right) { op = op };
    }

    return left;
  }
  private Expression ParseOperand() {
    var token = Peek();

    switch (token.type) {
      case TType.LCurly: {
          ASTNode.Context.PushScope();
          var block = ParseBlock();
          return new AnonObject(block, ASTNode.Context.PopScope());
        }

      case TType.Float:
        Eat();
        return new Operand(new Value(float.Parse(token.value)));
      case TType.Int:
        Eat();
        return new Operand(new Value(int.Parse(token.value)));
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
}

