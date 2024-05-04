namespace PixelEngine.Lang;

public class Interpreter : Visitor<Value> {
  public override Value VisitAnonObject(AnonObject node) {
    return node.Evaluate() ?? Value.Default;
  }
  
  public override Value VisitAssignment(Assignment node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitBinExpr(BinExpr node) {
    return node.Evaluate() ?? Value.Default;
  }
  
  public override Value VisitBlock(Block node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitCallableStatement(CallableStatment node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitDeclaration(Declaration node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitError(Error node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitFnCallExpr(CallableExpr node) {
    return node.Evaluate() ?? Value.Default;
  }
  
  public override Value VisitFuncDecl(FuncDecl node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitIdentifier(Identifier node) {
    return node.Evaluate() ?? Value.Default;
  }
  
  public override Value VisitOperand(Operand node) {
    return node.Evaluate() ?? Value.Default;
  }
  
  public override Value VisitParameters(Parameters node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
  
  public override Value VisitProgram(PixelEngine.Lang.Program node) {
    return node.Evaluate() as Value ?? Value.Default;
  }
}