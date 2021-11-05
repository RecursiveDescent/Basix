using System;
using System.IO;
using ParseLexer;
using ParseGen.Grammar;

namespace ParseGen {
    public class Generator {
        public void Generate(GrammarSpec grammar, string output) {
            StreamWriter writer = new StreamWriter(output);

            FormatState state = new FormatState();

            state.IndentLevel++;

            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;\nusing ParseLexer\n\nnamespace BasixParser {\npublic class GeneratedParser {\nLexer Lex; public GeneratedParser(string source) { Lex = new Lexer(source); }\n");
            
            foreach (NonTerminal nonterm in grammar.NonTerminals) {
                nonterm.Produce();

                writer.Write(nonterm.Output);
            }

            writer.WriteLine("}\n}");

            writer.Close();
        }

        public string GenerateString(GrammarSpec grammar) {
            string output = "";

            FormatState state = new FormatState();

            state.IndentLevel++;

            output += "using System;\n";
            output += "using System.Collections.Generic;\n\npublic class GeneratedParser {\n";
            
            foreach (NonTerminal nonterm in grammar.NonTerminals) {
                nonterm.Produce();

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

            for (int i = 0; i < args.Length; i++) {
                if (args[i] == "-o") {
                    outputfile = args[i + 1];
                    i += 2;
                }

                if (grammarfile == null) {
                    grammarfile = args[i];
                }
            }

            GrammarSpec grammar = GrammarSpec.FromFile(grammarfile);

            gen.Generate(grammar, outputfile);
        }
    }
}