namespace PixelEngine.Lang;
public abstract class Visitor<T>() {
  public virtual T Visit(ASTNode node) {
    switch (node) {
      case Program program:
        return VisitProgram(program);
      case Block block:
        return VisitBlock(block);
      case Declaration decl:
        return VisitDeclaration(decl);
      case Assignment assign:
        return VisitAssignment(assign);
      case FuncDecl decl:
        return VisitFuncDecl(decl);
      case AnonObject obj:
        return VisitAnonObject(obj);
      case Error error:
        return VisitError(error);
      case Operand op:
        return VisitOperand(op);
      case CallableStatment stmnt:
        return VisitCallableStatement(stmnt);
      case CallableExpr callable:
        return VisitFnCallExpr(callable);
      case BinExpr binexpr:
        return VisitBinExpr(binexpr);
      case Identifier identifier:
        return VisitIdentifier(identifier);
      case Parameters parameters:
        return VisitParameters(parameters);
    }
    Console.WriteLine($"Failed to visit node {node.GetType()}");
    return default!;
  }
  public abstract T VisitProgram(PixelEngine.Lang.Program program);
  public abstract T VisitBlock(Block block);
  public abstract T VisitDeclaration(Declaration decl);
  public abstract T VisitAssignment(Assignment assign);
  public abstract T VisitFuncDecl(FuncDecl decl);
  public abstract T VisitAnonObject(AnonObject obj);
  public abstract T VisitError(Error error);
  public abstract T VisitOperand(Operand op);
  public abstract T VisitCallableStatement(CallableStatment stmnt);
  public abstract T VisitFnCallExpr(CallableExpr callable);
  public abstract T VisitBinExpr(BinExpr binexpr);
  public abstract T VisitIdentifier(Identifier identifier);
  public abstract T VisitParameters(Parameters parameters);
}