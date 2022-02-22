using System;
using System.IO;
using System.Collections.Generic;
using BasixLexer;
using Basix.Grammar;

namespace Basix.Grammar {
	public class GrammarSpec {
		public List<NonTerminal> NonTerminals = new List<NonTerminal>();

		public static GrammarPattern GetRulePattern(string parentname, Lexer Lex, Dictionary<string, NonTerminal> nonterms) {
			GrammarPattern ptrn = new GrammarPattern();

			while (Lex.PeekToken().Value != ";") {
				if (Lex.PeekToken().Type == "EOF") {
					throw new Exception("Unexpected end of file [Expected ';']");
				}

				Token rule = Lex.GetToken();
				
				if (nonterms.ContainsKey(rule.Value)) {
					if (rule.Value == "\\")
						rule = Lex.GetToken();
					
					ptrn.Add(nonterms[rule.Value]);

					if (Lex.PeekToken().Value == "*") {
						Lex.GetToken();

						nonterms[rule.Value].Repeat = true;
					}

					if (Lex.PeekToken().Value == "?") {
						Lex.GetToken();

						nonterms[rule.Value].Optional = true;
					}

					if (Lex.PeekToken().Value == "|") {
						Lex.GetToken();

						ptrn.Alternate = GetRulePattern(parentname, Lex, nonterms);
					}

					continue;
				}

				if (rule.Value == ".") {
					 NonTerminal r = new NonTerminal(new Rule(Lex.GetToken().Value, null));

					 r.ParentRule = parentname;

					if (Lex.PeekToken().Value == "*") {
						Lex.GetToken();

						r.Repeat = true;
					}

					if (Lex.PeekToken().Value == "?") {
						Lex.GetToken();

						r.Optional = true;
					}

					ptrn.Add(r);

					if (Lex.PeekToken().Value == "|") {
						Lex.GetToken();

						ptrn.Alternate = GetRulePattern(parentname, Lex, nonterms);
					}

					continue;
				}

				if (rule.Value == "\\")
					rule = Lex.GetToken();

				NonTerminal added = new NonTerminal(new Rule(null, rule.Value));

				added.ParentRule = parentname;

				if (Lex.PeekToken().Value == "*") {
					Lex.GetToken();

					added.Repeat = true;
				}

				if (Lex.PeekToken().Value == "?") {
					Lex.GetToken();

					added.Optional = true;
				}
				
				ptrn.Add(added);

				if (Lex.PeekToken().Value == "|") {
					Lex.GetToken();

					ptrn.Alternate = GetRulePattern(parentname, Lex, nonterms);
				}
			}

			return ptrn;
		}

		public static GrammarSpec FromFile(string path) {
			GrammarSpec spec = new GrammarSpec();

			string src = File.ReadAllText(path);

			Lexer Lex = new Lexer(src);

			Dictionary<string, NonTerminal> nonterms = new Dictionary<string, NonTerminal>();
				
			while (true) {
				if (Lex.PeekToken().Value == "=") {
					Lex.GetToken();

					nonterms.Add(Lex.PeekToken().Value, new NonTerminal(Lex.GetToken().Value));

					Lex.GetToken();

					continue;
				}

				Token name = Lex.GetToken();

				if (name.Type == "EOF")
					break;

				if (! nonterms.ContainsKey(name.Value))
					nonterms.Add(name.Value, new NonTerminal(name.Value));


				if (Lex.GetToken().Value != ">")
					throw new Exception("Expected '>'");

				GrammarPattern ptrn = GetRulePattern(name.Value, Lex, nonterms);

				nonterms[name.Value].Add(new Rule(ptrn));

				spec.NonTerminals.Add(nonterms[name.Value]);

				Lex.GetToken();
			}

			return spec;
		}
	}
}