using System;
using System.IO;
using System.Collections.Generic;
using BasixLexer;
using Basix.Grammar;

namespace Basix {
	public class Collection {
		private CodeEmitter Emit;

		public string Subject;

		public virtual void Add(string value) {
			Emit.Call(Emit.Access(Subject, "push_back"), value);
		}

		public Collection(CodeEmitter emit) {
			Emit = emit;
		}

		public Collection(CodeEmitter emit, string subject) {
			Emit = emit;

			Subject = subject;
		}
	}

	// JS Generation doesn't work yet.
	public class JSCollection : Collection {
		private CodeEmitter Emit;

		public override void Add(string value) {
			Emit.Call(Emit.Access(Subject, "push"), value);
		}

		public string Pop() {
			return Emit.CallExpr(Emit.Access(Subject, "pop"));
		}

		public JSCollection(CodeEmitter emit) : base(emit) {
			Emit = emit;
		}

		public JSCollection(CodeEmitter emit, string subject) : base(emit, subject) {
			Emit = emit;

			Subject = subject;
		}
	}

	// Default C++
	public class CodeEmitter {
		public bool SupportsPassByRef = true;

		public string Source = "";

		protected int IndentLevel = 0;

		protected Stack<string> IndentStack = new Stack<string>();

		public string Null = "nullptr";

		public void Indent() {
			for (int i = 0; i < IndentLevel; i++) {
				Source += '\t';
			}
		}

		public virtual void Imports() {
			Source += "#include <iostream>\n";
			Source += "#include <vector>\n\n";
		}

		public virtual void Call(string subject, params string[] args) {
			Indent();

			Source += subject;

			Source += '(';

			foreach (string arg in args) {
				Source += $"{arg}, ";
			}

			Source = Source.Trim(new char[] { ',', ' ' });

			Source += ");\n\n";
		}

		public virtual string CallExpr(string subject, params string[] args) {
			string str = "";

			str += subject;

			str += '(';

			foreach (string arg in args) {
				str += $"{arg}, ";
			}

			str = str.Trim(',', ' ');

			str += ')';

			return str;
		}

		public virtual void DefineClass(string name) {
			Indent();

			Source += $"class {name} {{\n";

			IndentLevel++;

			Indent();

			Source += "public:\n\n";

			IndentStack.Push(name);
		}

		public virtual void DefineFunction(string type, string name, params KeyValuePair<string, string>[] args) {
			Indent();

			Source += $"{type} {name}(";

			foreach (KeyValuePair<string, string> arg in args) {
				Source += $"{arg.Key} {arg.Value}, ";
			}

			Source = Source.Trim(',', ' ');

			Source += ") {\n";

			IndentLevel++;

			IndentStack.Push(name);
		}

		public virtual void EndBlock() {
			IndentLevel--;

			IndentStack.Pop();

			Indent();

			Source += "}\n";
		}

		public virtual void If(string condition) {
			Indent();

			Source += $"if ({condition}) {{\n";

			IndentLevel++;

			IndentStack.Push("if");
		}

		public virtual void Else() {
			IndentStack.Push("else");

			Indent();

			IndentLevel++;

			Source += "else {\n";
		}

		public virtual void While(string condition) {
			Indent();

			Source += $"while ({condition}) {{\n";

			IndentLevel++;

			IndentStack.Push("while");
		}

		public virtual void Define(string type, string name, string value = null) {
			Indent();

			if (value == null) {
				Source += $"{type} {name};\n\n";

				return;
			}

			Source += $"{type} {name} = {value};\n\n";
		} 

		public virtual string Construct(string name, params string[] args) {
			string str = $"new {name}(";

			foreach (string arg in args) {
				str += $"{arg}, ";
			}

			str = str.Trim(',', ' ');

			str += ")";

			return str;
		}

		public virtual void Assign(string name, string value) {
			Indent();

			Source += $"{name} = {value};\n\n";
		}

		public virtual void Return(string value) {
			Indent();

			Source += $"return {value};\n";
		}

		public virtual string Op(string left, string op, string right) {
			return $"{left} {op} {right}";
		}

		public virtual string RefType(string type) {
			return $"{type}*";
		}

		public virtual string Ref(string name) {
			return $"&{name}";
		}

		public virtual string Access(string subject, string member) {
			return $"{subject}->{member}";
		}

		public virtual string Deref(string subject) {
			return $"*{subject}";
		}

		public virtual void Free(string subject) {
			Indent();

			Source += $"delete {subject};\n";
		}

		// For targets that don't support pass by reference
		public virtual void PushRef(string reference) {
			return;
		}

		public virtual string PopRef() {
			return null;
		}
	}

	public class JSCodeEmitter : CodeEmitter {
		public override void Imports() {
			Source += "let refstack = [];\n\nlet { Lexer, Node } = require(\"./lexer.js\");\n\n";
		}

		private JSCollection RefStack;

		public List<string> DefinedFunctions = new List<string>();

		public override void Call(string subject, params string[] args) {
			Indent();

			if (DefinedFunctions.Contains(subject))
				Source += "this." + subject;
			else
				Source += subject;

			Source += '(';

			foreach (string arg in args) {
				Source += $"{arg}, ";
			}

			Source = Source.Trim(',', ' ');

			Source += ");\n\n";
		}

		public override string CallExpr(string subject, params string[] args) {
			if (DefinedFunctions.Contains(subject))
				subject = "this." + subject;

			string str = "";

			str += subject;

			str += '(';

			foreach (string arg in args) {
				str += $"{arg}, ";
			}

			str = str.Trim(',', ' ');

			str += ')';

			return str;
		}

		public override void Define(string type, string name, string value = null)
		{
			Indent();

			if (value == null) {
				Source += $"let {name} = null;\n\n";

				return;
			}

			// JS has no type checking

			Source += $"{(IndentStack.Peek() != "class" ? "let " : " ")}{name} = {value};\n\n";
		}

		public override void DefineClass(string name) {
			Indent();

			Source += $"class {name} {{\n";

			IndentLevel++;

			IndentStack.Push("class");

			DefinedFunctions = new List<string>();
		}

		public override void DefineFunction(string type, string name, params KeyValuePair<string, string>[] args) {
			Indent();

			DefinedFunctions.Add(name);

			Source += $"{name}(";

			foreach (KeyValuePair<string, string> arg in args) {
				Source += $"{arg.Value}, ";
			}

			Source = Source.Trim(',', ' ');

			Source += ") {\n";

			IndentLevel++;

			IndentStack.Push("function");
		}

		public override string Ref(string name) {
			return name;
		}

		public override string Deref(string subject)
		{
			return subject;
		}

		public override string Access(string subject, string member)
		{
			return $"{(subject == "Lex" ? "this.Lex" : subject)}.{member}";
		}

		public override void PushRef(string jsref) {
			Indent();
			
			RefStack.Add(jsref);
		}

		public override string PopRef() {
			return RefStack.Pop();
		}

		public JSCodeEmitter() : base() {
			Null = "null";

			RefStack = new JSCollection(this, "refstack");

			SupportsPassByRef = false;
		}
	}
}