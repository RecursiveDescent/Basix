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
            writer.WriteLine("using System.Collections.Generic;\n\npublic class GeneratedParser {\nLexer Lex; public GeneratedParser(string source) { Lex = new Lexer(source); }\n");
            
            foreach (NonTerminal nonterm in grammar.NonTerminals) {
                nonterm.Produce();

                writer.Write(nonterm.Output);
            }

            writer.WriteLine("}");

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

            GrammarSpec grammar = GrammarSpec.FromFile("test.gr");

            gen.Generate(grammar, "test.txt");
        }
    }
}