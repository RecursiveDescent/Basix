using System;
using System.Collections.Generic;
using System.Text;

namespace BasixLexer {
    public class Node {
		public List<Node> Children = new List<Node>();

		public Token Value;

		public string NonTerminal;

		public Node() {

		}

		public Node(Token value) {
			Value = value;
		}
	}
    
    public class Token {
        public string Type;

        public string Value;

        public int Line;

        public int Column;

        public Token(string type, string value, int line, int column) {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }
    }

    public class PatternValue {
        public string Type;

        public bool Optional;

        public PatternValue(string type, bool optional = false) {
            Type = type;
            
            Optional = optional;
        }
    }

    public class Pattern {
        public List<PatternValue> Expected = new List<PatternValue>();

        public void Add(string type, bool optional = false) {
            Expected.Add(new PatternValue(type, optional));
        }

        public void Pop() {
            if (Expected.Count > 0)
                Expected.RemoveAt(0);
        }
    }

    public class Lexer {
        private string Source;

        public int Position = 0;

        private int Line;

        private int Column;

        public char Get() {
            if (Position >= Source.Length) {
                return '\0';
            }

            char c = Source[Position];

            Position++;

            if (c == '\n') {
                Line++;
                Column = 0;
            }
            else {
                Column++;
            }

            return c;
        }

        public char Peek() {
            if (Position >= Source.Length) {
                return '\0';
            }

            return Source[Position];
        }

        public Token GetToken() {
            char c = Get();

            while (char.IsWhiteSpace(c)) {
                c = Get();
            }

            if (c == '\0') {
                return new Token("EOF", "", Line, Column);
            }

            if (char.IsLetter(c)) {
                StringBuilder sb = new StringBuilder();

                sb.Append(c);

                while (char.IsLetterOrDigit(Peek())) {
                    sb.Append(Get());
                }

                return new Token("ID", sb.ToString(), Line, Column);
            }

            if (char.IsDigit(c)) {
                StringBuilder sb = new StringBuilder();

                sb.Append(c);

                while (char.IsDigit(Peek())) {
                    sb.Append(Get());
                }

                return new Token("NUM", sb.ToString(), Line, Column);
            }

            if (c == '"' || c == '\'') {
                string str = "";

                while (Peek() != c) {
                    str += Get();
                }

                Get();

                return new Token("STR", str, Line, Column);
            }

            if (c == ';') {
                return new Token("SEMICOLON", ";", Line, Column);
            }

            if (c == '+') {
                return new Token("PLUS", "+", Line, Column);
            }

            if (c == '-') {
                return new Token("MINUS", "-", Line, Column);
            }

            if (c == '*') {
                return new Token("MUL", "*", Line, Column);
            }

            if (c == '/') {
                return new Token("DIV", "/", Line, Column);
            }

            if (c == '(') {
                return new Token("LPAREN", "(", Line, Column);
            }

            if (c == '|') {
                return new Token("OR", "|", Line, Column);
            }

            if (c == ')') {
                return new Token("RPAREN", ")", Line, Column);
            }

            if (c == '{') {
                return new Token("LBRACE", "{", Line, Column);
            }

            if (c == '}') {
                return new Token("RBRACE", "}", Line, Column);
            }

            if (c == '.') {
                return new Token("DOT", ".", Line, Column);
            }

            if (c == ',') {
                return new Token("COMMA", ",", Line, Column);
            }

            if (c == '>') {
                return new Token("GT", ">", Line, Column);
            }

           return new Token("UNKNOWN", c.ToString(), Line, Column);
        }

        public Token PeekToken(int count = 1) {
            int pos = Position;
            int line = Line;
            int col = Column;

            Token token = GetToken();

            for (int i = 1; i < count; i++) {
                token = GetToken();
            }

            Position = pos;
            Line = line;
            Column = col;

            return token;
        }

        public bool Matches(Pattern pattern) {
            int cur = 1;

            foreach (PatternValue expected in pattern.Expected) {
                if (PeekToken(cur).Type != expected.Type && ! expected.Optional) {
                    return false;
                }

                if (! expected.Optional)
                    cur++;
            }

            return true;
        }

        public Lexer(string source) {
            Source = source;

            Position = 0;

            Line = 1;

            Column = 1;
        }
    }
}