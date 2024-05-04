
using PixelEngine.Lang;

var lexer = new Lexer();

const string TEST_CODE = @"
object = {
  field_1 = 10 * 2 * 300 * (20 + 1)
  field_2 = 2 + 2
}
field = 10


func add(value) {
  value = value + 1
}

print(object)
print(field + 2)
";

var tokens = lexer.Lex(TEST_CODE);

tokens.Reverse();

var parser = new Parser(tokens);

var program = parser.ParseProgram();

Statement.CatchError(program.Evaluate());

// var interpreter = new Interpreter();

// interpreter.VisitProgram(program);



