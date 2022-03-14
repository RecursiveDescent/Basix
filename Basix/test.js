let refstack = [];

let { Lexer, Node } = require("./lexer.js");

class GeneratedParser {
	constructor() {
		this.Lex = null;

	}
	Literal(outnode) {
		let node = new Node();

		node.NonTerminal = "Literal";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Lex.PeekToken().Type == "NUM") {
			node.Children.push(new Node("Literal", this.Lex.GetToken()));

		}
		else {
			node = new Node();

			node.NonTerminal = "Literal";

			this.Lex.Position = pos;

			if (this.Lex.PeekToken().Type == "ID") {
				node.Children.push(new Node("Literal", this.Lex.GetToken()));

			}
			else {
				node = new Node();

				node.NonTerminal = "Literal";

				this.Lex.Position = pos;

				if (this.Lex.PeekToken().Type == "STR") {
					node.Children.push(new Node("Literal", this.Lex.GetToken()));

				}
				else {
					this.Lex.Position = pos;

					outnode = null;

										refstack.push(null);

					return false;
				}
				outnode = node;

								refstack.push(node);

				return true;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	DivQ(outnode) {
		let node = new Node();

		node.NonTerminal = "DivQ";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Lex.PeekToken().Value == "/") {
			node.Children.push(new Node("DivQ", this.Lex.GetToken()));

		}
		else {
			this.Lex.Position = pos;

			outnode = null;

						refstack.push(null);

			return false;
		}
		if (this.Literal(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			this.Lex.Position = pos;

			return false;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	Div(outnode) {
		let node = new Node();

		node.NonTerminal = "Div";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Literal(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Div";

			this.Lex.Position = pos;

			if (this.Literal(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		if (this.DivQ(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Div";

			this.Lex.Position = pos;

			if (this.Literal(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	MulQ(outnode) {
		let node = new Node();

		node.NonTerminal = "MulQ";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Lex.PeekToken().Value == "*") {
			node.Children.push(new Node("MulQ", this.Lex.GetToken()));

		}
		else {
			this.Lex.Position = pos;

			outnode = null;

						refstack.push(null);

			return false;
		}
		if (this.Mul(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			this.Lex.Position = pos;

			return false;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	Mul(outnode) {
		let node = new Node();

		node.NonTerminal = "Mul";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Div(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Mul";

			this.Lex.Position = pos;

			if (this.Div(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		if (this.MulQ(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Mul";

			this.Lex.Position = pos;

			if (this.Div(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	SubQ(outnode) {
		let node = new Node();

		node.NonTerminal = "SubQ";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Lex.PeekToken().Value == "-") {
			node.Children.push(new Node("SubQ", this.Lex.GetToken()));

		}
		else {
			this.Lex.Position = pos;

			outnode = null;

						refstack.push(null);

			return false;
		}
		if (this.Sub(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			this.Lex.Position = pos;

			return false;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	Sub(outnode) {
		let node = new Node();

		node.NonTerminal = "Sub";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Mul(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Sub";

			this.Lex.Position = pos;

			if (this.Mul(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		if (this.SubQ(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Sub";

			this.Lex.Position = pos;

			if (this.Mul(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	AddQ(outnode) {
		let node = new Node();

		node.NonTerminal = "AddQ";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Lex.PeekToken().Value == "+") {
			node.Children.push(new Node("AddQ", this.Lex.GetToken()));

		}
		else {
			this.Lex.Position = pos;

			outnode = null;

						refstack.push(null);

			return false;
		}
		if (this.Add(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			this.Lex.Position = pos;

			return false;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
	Add(outnode) {
		let node = new Node();

		node.NonTerminal = "Add";

		let rule = null;

		let pos = this.Lex.Position;

		let next = this.Lex.PeekToken();

		if (this.Sub(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Add";

			this.Lex.Position = pos;

			if (this.Sub(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		if (this.AddQ(rule)) {
			rule = refstack.pop();

			node.Children.push(rule);

		}
		else {
			node = new Node();

			node.NonTerminal = "Add";

			this.Lex.Position = pos;

			if (this.Sub(rule)) {
				rule = refstack.pop();

				node.Children.push(rule);

			}
			else {
				this.Lex.Position = pos;

				return false;
			}
			outnode = node;

						refstack.push(node);

			return true;
		}
		outnode = node;

				refstack.push(node);

		return true;
	}
}
