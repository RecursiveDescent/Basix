class Token {
	constructor(type, value, line, column) {
		this.Type = type;

		this.Value = value;

		this.Line = line;

		this.Column = column;
	}
}

class Lexer {
	constructor(source) {
		this.src = source;

		this.Position = 0;

		this.line = 1;

		this.column = 1;

		this.length = source.length;
	}

	get() {
		if (this.Position > this.length)
			return '\0';
		
		let c = this.src[this.Position++];

		this.column++;

		if (c == '\n') {
			this.line++;

			this.column = 0;
		}
		else {
			this.column++;
		}

		return c;
	}

	peek() {
		if (this.Position >= this.length) {
			return '\0';
		}

		return this.src[this.Position];
	}

	GetToken() {
		let c = this.get();

		while (/\s/.test(c)) {
			c = this.get();
		}

		if (/[a-zA-Z_]/.test(c)) {
			let value = "";

			while (/[a-zA-Z_0-9]/.test(c)) {
				value += this.get();

				c = this.peek();
			}

			return new Token("ID", value, this.line, this.column);
		}

		if (/[0-9]/.test(c)) {
			let value = "";

			while (/[0-9]/.test(c)) {
				value += this.get();

				c = this.peek();
			}

			return new Token("NUM", value, this.line, this.column);
		}

		if (c == '"' || c == '\'') {
			let qoute = c;

			c = this.get();

			let value = "";

			while (c != qoute) {
				value += c;

				c = this.get();
			}

			return new Token("STR", value, this.line, this.column);
		}

		if (c == ';') {
			return new Token("SEMICOLON", ";", this.line, this.column);
		}

		if (c == '+') {
			return new Token("PLUS", "+", this.line, this.column);
		}

		if (c == '-') {
			return new Token("MINUS", "-", this.line, this.column);
		}

		if (c == '*') {
			return new Token("MUL", "*", this.line, this.column);
		}

		if (c == '/') {
			return new Token("DIV", "/", this.line, this.column);
		}

		if (c == '(') {
			return new Token("LPAREN", "(", this.line, this.column);
		}

		if (c == '|') {
			return new Token("OR", "|", this.line, this.column);
		}

		if (c == ')') {
			return new Token("RPAREN", ")", this.line, this.column);
		}

		if (c == '{') {
			return new Token("LBRACE", "{", this.line, this.column);
		}

		if (c == '}') {
			return new Token("RBRACE", "}", this.line, this.column);
		}

		if (c == '^') {
			return new Token("POW", "^", this.line, this.column);
		}

		if (c == '?') {
			return new Token("QMARK", "?", this.line, this.column);
		}

		if (c == '.') {
			return new Token("DOT", ".", this.line, this.column);
		}

		if (c == ',') {
			return new Token("COMMA", ",", this.line, this.column);
		}

		if (c == '>') {
			return new Token("GT", ">", this.line, this.column);
		}

	   return new Token("UNKNOWN", c, this.line, this.column);
	}

	PeekToken(count) {
		let pos = this.Position;
		let line = this.line;
		let col = this.column;

		let token = this.GetToken();

		for (let i = 1; i < (count || 1); i++) {
			token = this.GetToken();
		}

		this.Position = pos;
		this.line = line;
		this.column = col;

		return token;
	}
}

class Node {
	constructor(nonterm, value) {
		this.Children = [];

		if (value) {
			this.NonTerminal = nonterm;

			this.Value = value;

			return;
		}

		this.Value = nonterm;
	}
}

module.exports = {
	Lexer,
	Node
}