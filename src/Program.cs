
using PixelEngine.Lang;

var lexer = new Lexer();




const string TEST_CODE = @"
object = {
  field_1 = 10 * 2 * 300 * (20 + 1)
  field_2 = 2 + 2
}
print(object)
field = 10

func inc(value) {
  value = value + 1
  print(value)
}

inc(1)

value = ""string"" == ""string""

value = object == object

print(value)

for {
  print(""Loop"")
  break
}

if 1 == 0 {
  print(""was true"")
} else if 1 == 2 {
  print(""was true x2"")
} else {
  print(""none true"")
}

i = 0
i = i + 2
i = i + 2

for {
  break  
}
i = 0
for i < 1000000 {
  i = i + 1
}

print(i == 1000000)

";

const string TEST_2 = @"
  v = 1 == 1
  
  v = !v
  
  print(v)
  print(""Expected false"")
  
  
  v = -1
  
  print(v)
  print(""Expected -1"")
";

var tokens = lexer.Lex(TEST_2);

tokens.Reverse();

var parser = new Parser(tokens);

var program = parser.ParseProgram();

Statement.CatchError(program.Evaluate());

// var interpreter = new Interpreter();

// interpreter.VisitProgram(program);



