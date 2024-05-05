


using System.Runtime.Serialization;
using System.Text;

namespace PixelEngine.Lang;


public enum TFamily {
  Operator,
  Literal,
  Identifier,
  Keyword,
}
public enum TType {
  Int,
  String,
  Bool,
  Float,
  Identifier,

  Plus,
  Minus,
  Divide,
  Multiply,
  Assign,

  LParen,
  RParen,
  LCurly,
  RCurly,
  Newline,
  Func,
  LogicalOr,
  LogicalAnd,
  Equal,
  NotEqual,
  Not,
  LessEq,
  GreaterEq,
  Less,
  Greater,
  If,
  Else,
  For,
  Continue,
  Return,
  Break,
  Comma,
  Dot,
  SubscriptLeft,
  SubscriptRight,
  Import,
  AssignDiv,
  AssignMul,
  AssignMinus,
  AssignPlus,
  Start,
  EOF,
}

public class Token(int loc, int col, string val, TFamily fam, TType type) {
  public readonly int loc = loc, col = col;
  public readonly string value = val;
  public readonly TFamily family = fam;
  public readonly TType type = type;
  public static readonly Token EOF = new(0, 0, "", TFamily.Operator, TType.EOF);
  public override string ToString() {
    return $"Token({value})::{type}::{family}\nl:{loc} c:{col}";
  }
}

public class Lexer {
  public readonly Dictionary<string, TType> Keywords = new() {
    ["func"] = TType.Func,
    ["if"] = TType.If,
    ["else"] = TType.Else,
    ["for"] = TType.For,
    ["continue"] = TType.Continue,
    ["return"] = TType.Return,
    ["import"] = TType.Import,
    ["break"] = TType.Break,
    ["start"] = TType.Start,
  };
  
  public readonly Dictionary<string, TType> Operators = new() {
    ["+"] = TType.Plus,
    ["-"] = TType.Minus,
    ["*"] = TType.Multiply,
    ["/"] = TType.Divide,
    [","] = TType.Comma,
    ["||"] = TType.LogicalOr,
    ["&&"] = TType.LogicalAnd,
    ["."] = TType.Dot,
    ["!"] = TType.Not,
    
    ["["] = TType.SubscriptLeft,
    ["]"] = TType.SubscriptRight,
    
    ["=="] = TType.Equal,
    ["!="] = TType.NotEqual,
    
    [">"] = TType.Greater,
    ["<"] = TType.Less,
    [">="] = TType.GreaterEq,
    ["<="] = TType.LessEq,
    
    ["+="] = TType.AssignPlus,
    ["-="] = TType.AssignMinus,
    ["*="] = TType.AssignMul,
    ["/="] = TType.AssignDiv,
    
    ["("] = TType.LParen,
    [")"] = TType.RParen,
    ["="] = TType.Assign,
    
    ["{"] = TType.LCurly,
    ["}"] = TType.RCurly,
  };
  private int col = 0, loc = 0;
  public List<Token> Lex(string input) {
    int pos = 0;
    List<Token> tokens = [];
    while (pos < input.Length) {
      char cur = input[pos];
      
      // Comments (// single line)
      if (pos < input.Length - 1 && cur == '/' && input[pos + 1] == '/') {
        col = 0;
        loc++;
        while (pos < input.Length && cur != '\n') {
          pos++;
          cur = input[pos];
        }
        continue;
      }
      
      if (cur == '\t') {
        pos++;
        continue;
      }
      
      // Newlines
      if (cur == '\n') {
        col = 0;
        loc++;
        pos++;
        tokens.Add(new(loc, col, "\n", TFamily.Operator, TType.Newline));
        continue;
      }
      
      // Spaces.
      if (cur == ' ') {
        pos++;
        col++;
        continue;
      }
      
      if (cur == '\"') {
        ParseString(input, ref pos, tokens);
        continue;
      }

      if (char.IsDigit(cur)) {
        LexNumber(input, ref pos, tokens, ref cur);
      } else if (char.IsLetter(cur)) {
        LexIdentifier(input, ref pos, tokens, ref cur);
      } else if (IsOperator(cur)) {
        LexOperator(input, ref pos, tokens, ref cur);
      } else {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Unexpected character (at {loc}:{col})::({cur})");
        Console.ResetColor();
        pos++;
      }
    
    }
    
    return tokens;
  }

  private void ParseString(string input, ref int pos, List<Token> tokens) {
    char cur;
    pos++;
    StringBuilder value = new StringBuilder();
    cur = input[pos];
    while (pos < input.Length && cur != '\"') {
      value.Append(cur);
      pos++;
      cur = input[pos];
    }
    pos++;
    tokens.Add(new Token(loc, col, value.ToString(), TFamily.Literal, TType.String));
  }

  private bool IsOperator(char c) {
    return Operators.Keys.Any(op => op.StartsWith(c.ToString()));
}
  
  private void LexOperator(string input, ref int pos, List<Token> tokens, ref char cur) {
    TFamily family = TFamily.Operator;
    
    foreach (var op in Operators.Keys.OrderByDescending(o => o.Length)) {
      if (input[pos..].StartsWith(op)) {
        tokens.Add(new Token(loc, col, op, family, Operators[op]));
        pos += op.Length;
        return;
      }
    }
  }
  
  private void LexIdentifier(string input, ref int pos, List<Token> tokens, ref char cur) {
    string value = string.Empty;
    TType type = TType.Identifier;
    TFamily family = TFamily.Identifier;
    
    while (pos < input.Length) {
      cur = input[pos];
      if (!char.IsLetterOrDigit(cur) && cur != '_') {
        break;
      }
      value += cur;
      pos++;
    }
    
    if (Keywords.TryGetValue(value, out var kwType)) {
      type = kwType;
      family = TFamily.Keyword;
    }
    
    tokens.Add(new(loc, col, value, family, type));
  }
  
  private void LexNumber(string input, ref int pos, List<Token> tokens, ref char cur) {
    string value = string.Empty;
    TType type = TType.Int;

    while (pos < input.Length) {
      cur = input[pos];

      if (char.IsDigit(cur)) {
        value += cur;
        pos++;
      } else if (type == TType.Int && cur == '.') {
        value += cur;
        pos++;
        type = TType.Float;
      } else {
        break;
      }
    }

    tokens.Add(new Token(loc, col, value, TFamily.Literal, type));
  }
}

[Serializable]
public class LexerException : Exception {
  public LexerException() {
  }

  public LexerException(string? message) : base(message) {
  }

  public LexerException(string? message, Exception? innerException) : base(message, innerException) {
  }
}