
using PixelEngine.Lang;

var lexer = new Lexer();

const string TEST_CODE = @"
object = {
  field_1 = 10 * 2 * 300 * (20 + 1)
  field_2 = 10
}
field = 10

func print (value) {
  
}

print(object)
";

var tokens = lexer.Lex(TEST_CODE);

tokens.Reverse();

var parser = new Parser(tokens);

var program = parser.ParseProgram();

Statement.CatchError(program.Evaluate());

// var interpreter = new Interpreter();

// interpreter.VisitProgram(program);



