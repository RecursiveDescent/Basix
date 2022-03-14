using System;
using System.IO;
using System.Collections.Generic;

namespace Basix.Grammar {
	public class FormatState {
		public int IndentLevel;

		public string Indent() {
			string indent = "";

			for (int i = 0; i < IndentLevel; i++) {
				indent += "\t";
			}

			return indent;
		}
	}

	public class NonTerminal {
		public string Name;

		public string ParentRule = "";

		public List<Rule> Rules = new List<Rule>();

		public string Output;

		private CodeEmitter Emit = new CodeEmitter();

		private bool Produced = false;

		private FormatState State = new FormatState();

		public List<Rule> Terminals = new List<Rule>();

		public bool Repeat = false;

		public bool Optional = false;

		public void Add(Rule rule) {
			Rules.Add(rule);

			if (rule.Terminal) {
				Terminals.Add(rule);
			}
			else {
				if (rule.NonTerminal != null) {
					foreach (Rule r in rule.NonTerminal.Terminals) {
						Add(r);
					}

					return;
				}

				foreach (Rule r in rule.Terminals) {
					Add(r);
				}
			}
		}

		public void Produce(Collection collection) {
			if (Produced)
				return;

			collection.Subject = Emit.Access("node", "Children");
			
			foreach (var rule in Rules) {
				rule.Init(Emit);

				Output += rule.Produce(collection, Name) + '\n';
			}

			State.IndentLevel--;

			Emit.Assign(Emit.Deref("outnode"), "node");

			if (! Emit.SupportsPassByRef)
				Emit.PushRef("node");

			Emit.Return("true");

			Emit.EndBlock();

			Produced = true;
		}

		public void Init(CodeEmitter emit) {
			Emit = emit;

			Emit.DefineFunction("bool", Name, new KeyValuePair<string, string>(Emit.RefType(Emit.RefType("Node")), "outnode"));

			Emit.Define(Emit.RefType("Node"), "node", Emit.Construct("Node"));

			Emit.Assign(Emit.Access("node", "NonTerminal"), $"\"{Name}\"");

			Emit.Define(Emit.RefType("Node"), "rule", Emit.Null);

			Emit.Define("int", "pos", Emit.Access("Lex", "Position"));

			Emit.Define(Emit.RefType("Token"), "next", Emit.CallExpr(Emit.Access("Lex", "PeekToken")));
		}

		public NonTerminal(CodeEmitter emitter, string name) {
			Emit = emitter;

			Name = name;
		}

		public NonTerminal(string name) {
			Name = name;
			
			Emit.DefineFunction("bool", name, new KeyValuePair<string, string>(Emit.RefType("Node"), "outnode"));

			Emit.Define(Emit.RefType("Node"), "node", Emit.Construct("Node"));

			Emit.Assign(Emit.Access("node", "NonTerminal"), $"\"{name}\"");

			Emit.Define(Emit.RefType("Node"), "rule", Emit.Null);

			Emit.Define("int", "pos", Emit.Access("Lex", "Position"));

			Emit.Define(Emit.RefType("Token"), "next", Emit.CallExpr(Emit.Access("Lex", "PeekToken")));
		}

		public NonTerminal(Rule rule) {
			Name = null;

			Rules.Add(rule);

			Terminals.Add(rule);
		}
	}

	public class Rule {
		public string TerminalType;

		public GrammarPattern Recognizer;

		public bool Terminal = true;

		public string Value;

		public NonTerminal NonTerminal;

		private FormatState State = new FormatState();

		public List<Rule> Terminals = new List<Rule>();

		public CodeEmitter Emit;

		public void Add(Rule rule) {
			if (rule.Terminal) {
				Terminals.Add(rule);
			}
			else {
				if (rule.NonTerminal != null) {
					foreach (Rule r in rule.NonTerminal.Terminals) {
						Add(r);
					}

					return;
				}
				
				foreach (Rule r in rule.Terminals) {
					Add(r);
				}
			}
		}

		public string Produce(Collection collection, string name = null) {
			string output = "";

			Collection nodechildren = collection;

			if (Terminal) {
				if (Recognizer != null) {

					foreach (NonTerminal rule in Recognizer.Expected) {
						string conditional = rule.Repeat ? "while" : "if";

						if (rule.Name != null) {
							if (rule.Repeat) {
								Emit.While(Emit.CallExpr(rule.Name, Emit.Ref("rule")));
								
								if (! Emit.SupportsPassByRef)
									Emit.Assign(Emit.Ref("rule"), Emit.PopRef());
							}
							else {
								Emit.If(Emit.CallExpr(rule.Name, Emit.Ref("rule")));

								if (! Emit.SupportsPassByRef)
									Emit.Assign(Emit.Ref("rule"), Emit.PopRef());
							}
							
							// output += state.Indent() + $"{conditional} ({ rule.Name }(out rule)) {{\n";


							// output += state.Indent() + "\tnode.Children.Add(rule);\n";

							// Emit.Call(Emit.Access(Emit.Access("node", "Children"), "Add"), "rule");

							nodechildren.Add("rule");

							Emit.EndBlock();

							// state.IndentLevel--;

							// output += state.Indent() + "}\n";

							if (Recognizer.Alternate != null && ! rule.Repeat && ! rule.Optional) {
								// output += state.Indent() + "else {\n";

								Emit.Else();

								// state.IndentLevel++;

								// output += state.Indent() + $"\tnode = new Node(); node.NonTerminal = \"{name ?? rule.ParentRule}\";\n";

								Emit.Assign("node", Emit.Construct("Node"));
								
								Emit.Assign(Emit.Access("node", "NonTerminal"), $"\"{name ?? rule.ParentRule}\"");

								// output += state.Indent() + "\tLex.Position = pos;\n";

								Emit.Assign(Emit.Access("Lex", "Position"), "pos");

								// output += new Rule(Recognizer.Alternate).Produce(state);

								Rule altrule = new Rule(Recognizer.Alternate);

								altrule.Init(Emit);

								altrule.Produce(collection);

								// output += state.Indent() + "outnode = node; return true;\n";

								Emit.Assign(Emit.Deref("outnode"), "node");

								if (! Emit.SupportsPassByRef)
									Emit.PushRef("node");

								Emit.Return("true");

								Emit.EndBlock();
							}
							else if (! rule.Repeat && ! rule.Optional) {
								// output += state.Indent() + "else {\n";

								Emit.Else();

								// state.IndentLevel++;

								// output += state.Indent() + "\tLex.Position = pos;\n";

								Emit.Assign(Emit.Access("Lex", "Position"), "pos");

								// output += state.Indent() + "\toutnode = null;\n";

								// Emit.Assign("outnode", "nullptr");

								// output += state.Indent() + "\treturn false;\n";

								Emit.Return("false");

								Emit.EndBlock();
							}
						}
						else {
							if (rule.Rules[0].TerminalType == null) {
								// output += state.Indent() + $"{conditional} (Lex.PeekToken().Value == \"{rule.Rules[0].Value}\") {{\n";

								string cond = Emit.Op(Emit.Access(Emit.CallExpr(Emit.Access("Lex", "PeekToken")), "Value"), "==", $"\"{rule.Rules[0].Value}\"");

								if (rule.Repeat)
									Emit.While(cond);
								else
									Emit.If(cond);
							}
							else {
								if (rule.Rules[0].Value == null) {
									// output += state.Indent() + $"{conditional} (Lex.PeekToken().Type == \"{rule.Rules[0].TerminalType}\") {{\n";

									string cond = Emit.Op(Emit.Access(Emit.CallExpr(Emit.Access("Lex", "PeekToken")), "Type"), "==", $"\"{rule.Rules[0].TerminalType}\"");

									if (rule.Repeat)
										Emit.While(cond);
									else
										Emit.If(cond);
								}
								else {
									// output += state.Indent() + $"{conditional} (Lex.PeekToken().Type == \"{rule.Rules[0].TerminalType}\" && Lex.PeekToken().Value == \"{rule.Rules[0].Value}\") {{\n";

									string cond1 = Emit.Op(Emit.Access(Emit.CallExpr(Emit.Access("Lex", "PeekToken")), "Type"), "==", $"\"{rule.Rules[0].TerminalType}\"");
									string cond2 = Emit.Op(Emit.Access(Emit.CallExpr(Emit.Access("Lex", "PeekToken")), "Value"), "==", $"\"{rule.Rules[0].Value}\"");

									string cond = Emit.Op(cond1, "&&", cond2);

									if (rule.Repeat)
										Emit.While(cond);
									else
										Emit.If(cond);
								}
							}

							// state.IndentLevel++;
							// output += state.Indent() + $"\tnode.Children.Add(new Node(\"{name ?? rule.ParentRule}\", Lex.GetToken()));\n";

							// Emit.Call(Emit.Access(Emit.Access("node", "Children"), "Add"), Emit.Construct("Node", $"\"{name ?? rule.ParentRule}\"", Emit.CallExpr(Emit.Access("Lex", "GetToken"))));

							nodechildren.Add(Emit.Construct("Node", $"\"{name ?? rule.ParentRule}\"", Emit.CallExpr(Emit.Access("Lex", "GetToken"))));

							// state.IndentLevel--;

							// output += state.Indent() + "}\n";

							Emit.EndBlock();

							if (rule.Repeat)
								continue;

							// output += state.Indent() + "else {\n";

							Emit.Else();

							if (Recognizer.Alternate != null) {
								// output += state.Indent() + $"\tnode = new Node(); node.NonTerminal = \"{name ?? rule.ParentRule}\";\n";

								Emit.Assign("node", Emit.Construct("Node"));

								Emit.Assign(Emit.Access("node", "NonTerminal"), $"\"{name ?? rule.ParentRule}\"");

								// output += state.Indent() + "\tLex.Position = pos;\n";

								Emit.Assign(Emit.Access("Lex", "Position"), "pos");

								// output += new Rule(Recognizer.Alternate).Produce(state);

								Rule altrule = new Rule(Recognizer.Alternate);

								altrule.Init(Emit);

								altrule.Produce(collection);

								// output += state.Indent() + "outnode = node; return true;\n";

								Emit.Assign(Emit.Deref("outnode"), "node");

								if (! Emit.SupportsPassByRef)
									Emit.PushRef("node");

								Emit.Return("true");
							}
							else {
								// output += state.Indent() + "\tLex.Position = pos;\n";

								Emit.Assign(Emit.Access("Lex", "Position"), "pos");

								// output += state.Indent() + "\toutnode = null;\n";

								Emit.Assign(Emit.Deref("outnode"), Emit.Null);

								if (! Emit.SupportsPassByRef)
									Emit.PushRef(Emit.Null);

								// output += state.Indent() + "\treturn false;\n";

								Emit.Return("false");
							}

							// state.IndentLevel--;

							// output += state.Indent() + "}\n";

							Emit.EndBlock();
						}
					}

					/*output += '\t' + Recognizer.Compile(State) + "\n";

					output += state.Indent() + $"if (Lex.Matches(match)) {{\n";

					state.IndentLevel++;
					output += state.Indent() + "while (Lex.Matches(match) && match.Expected.Count > 0) {\n";

					if (Recognizer.Expected[0].Terminal == false) {
						output += state.Indent() + "\tvalue = " + Recognizer.Expected[0].Name + "();";
					}
					else {
						output += state.Indent() + "\tvalue = new Node(Lex.GetToken());";
					}
					
					output += state.Indent() + "\tnode.Add(value); match.Pop();\n";

					output += state.Indent() + "}\n\n";

					state.IndentLevel--;

					output += state.Indent() + $"}}\n\n";*/
				}
				else {
					if (TerminalType == null) {
						// output += state.Indent() + $"if (next.Value == \"{Value}\") {{\n";

						Emit.If(Emit.Op(Emit.Access("next", "Value"), "==", $"\"{Value}\""));
					}
					else {
						if (Value == null) {
							// output += state.Indent() + $"if (next.Type == \"{TerminalType}\") {{\n";

							Emit.If(Emit.Op(Emit.Access("next", "Type"), "==", $"\"{TerminalType}\""));
						}
						else {
							// output += state.Indent() + $"if (next.Type == \"{TerminalType}\" && next.Value == \"{Value}\") {{\n";

							string cond1 = Emit.Op(Emit.Access("next", "Type"), "==", $"\"{TerminalType}\"");
							
							string cond2 = Emit.Op(Emit.Access("next", "Value"), "==", $"\"{Value}\"");

							string cond = Emit.Op(cond1, "&&", cond2);

							Emit.If(cond);
						}
					}

					// state.IndentLevel++;

					// output += state.Indent() + $"node.Children.Add(new Node(\"{name}\", Lex.GetToken()));\n";

					// Emit.Call(Emit.Access(Emit.Access("node", "Children"), "Add"), Emit.Construct("Node", $"\"{name}\"", Emit.CallExpr(Emit.Access("Lex", "GetToken"))));

					nodechildren.Add(Emit.Construct("Node", $"\"{name}\"", Emit.CallExpr(Emit.Access("Lex", "GetToken"))));

					// state.IndentLevel--;

					// output += state.Indent() + $"}}\n\n";

					Emit.EndBlock();
				}
			}
			else {
				// output += state.Indent() + $"{NonTerminal.Name}(rule); outnode = rule; return true;";

				Emit.Call(NonTerminal.Name, Emit.Ref("rule"));

				/*if (! Emit.SupportsPassByRef) {
					Emit.Assign("rule", Emit.PopRef());

					Emit.PushRef("rule");
				}*/

				Emit.Assign(Emit.Deref("outnode"), "rule");

				if (! Emit.SupportsPassByRef)
					Emit.PushRef("rule");

				Emit.Return("true");
			}

			return output;
		}

		public void Init(CodeEmitter emit) {
			Emit = emit;
		}

		public Rule(string type, string value) {
			Terminal = true;

			TerminalType = type;

			Value = value;
		}

		public Rule(GrammarPattern recognizer) {
			Recognizer = recognizer;

			foreach (NonTerminal nonterm in Recognizer.Expected) {
				foreach (Rule term in nonterm.Terminals) {
					Add(term);
				}
			}

			// Value = value;

			Terminal = true;
		}

		public Rule(NonTerminal nonterm) {
			Terminal = false;

			NonTerminal = nonterm;
		}
	}

	public class GrammarPattern {
		public List<NonTerminal> Expected = new List<NonTerminal>();

		public GrammarPattern Alternate;

		public void Add(string type) {
			Expected.Add(new NonTerminal(new Rule(type, null)));
		}

		public void Add(string type, string value) {
			Expected.Add(new NonTerminal(new Rule(type, value)));
		}

		public void Add(NonTerminal nonterm) {
			Expected.Add(nonterm);
		}

		public string Compile(FormatState state, bool recurse = false) {
			string output = recurse ? "" : state.Indent() + $"match = new Pattern();\n";

			foreach (NonTerminal nonterm in Expected) {
				foreach (Rule term in nonterm.Terminals) {
					if (term.Recognizer != null) {
						output += term.Recognizer.Compile(state, true);

						continue;
					}

					output += state.Indent() + $"match.Add(\"{term.TerminalType}\");\n";
				}
			}

			return output + '\n';
		}
	}
}