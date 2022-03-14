using System;
using System.IO;
using BasixLexer;
using Basix.Grammar;

namespace Basix {
	public class Generator {
		public void Generate(GrammarSpec grammar, string output, string lang) {
			StreamWriter writer = new StreamWriter(output);

			FormatState state = new FormatState();

			state.IndentLevel++;

			CodeEmitter emit = lang == "js" ? new JSCodeEmitter() : new CodeEmitter();

			emit.Imports();

			emit.DefineClass("GeneratedParser");

			emit.DefineFunction("", "constructor");
			
			emit.Assign(emit.Access("this", "Lex"), emit.Null);

			emit.EndBlock();

			foreach (NonTerminal nonterm in grammar.NonTerminals) {
				(emit as JSCodeEmitter)?.DefinedFunctions?.Add(nonterm.Name);
			}
			
			foreach (NonTerminal nonterm in grammar.NonTerminals) {
				nonterm.Init(emit);

				nonterm.Produce(new JSCollection(emit));
			}

			emit.EndBlock();

			writer.Write(emit.Source);

			writer.Close();
		}

		[Obsolete]
		public string GenerateString(GrammarSpec grammar) {
			string output = "";

			FormatState state = new FormatState();

			state.IndentLevel++;

			output += "using System;\n";
			output += "using System.Collections.Generic;\n\npublic class GeneratedParser {\n";
			
			foreach (NonTerminal nonterm in grammar.NonTerminals) {
				nonterm.Produce(new Collection(new CodeEmitter()));

				output += nonterm.Output;
			}

			output += ';';

			return output;
		}
	}

	public class Test {
		public static void Main(string[] args) {
			Console.WriteLine("Generating parser...");

			Generator gen = new Generator();

			// Basic argument parsing for now

			string grammarfile = null;

			string outputfile = "output.cs";

			string lang = "js";

			for (int i = 0; i < args.Length; i++) {
				if (args[i] == "-o") {
					outputfile = args[i + 1];

					i += 2;

					continue;
				}

				if (args[i].StartsWith("-l")) {
					lang = args[i].Substring(2);

					continue;
				}

				if (args[i] == "-lang") {
					lang = args[i + 1];

					i += 2;

					continue;
				}

				if (grammarfile == null) {
					grammarfile = args[i];
				}
			}

			lang = lang.ToLower();

			if (lang != "js" && lang != "cpp") {
				Console.WriteLine("Only JS or CPP are supported at this time.");

				return;
			}

			GrammarSpec grammar = GrammarSpec.FromFile(grammarfile);

			gen.Generate(grammar, outputfile, lang);
		}
	}
}