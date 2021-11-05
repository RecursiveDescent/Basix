using System;
using System.IO;
using System.Collections.Generic;
using ParseLexer;
using ParseGen.Grammar;

namespace ParseGen {
    public class GrammarSpec {
        public List<NonTerminal> NonTerminals = new List<NonTerminal>();

        public static GrammarPattern GetRulePattern(Lexer Lex, Dictionary<string, NonTerminal> nonterms) {
            GrammarPattern ptrn = new GrammarPattern();

            while (Lex.PeekToken().Value != ";") {
                Token rule = Lex.GetToken();
                
                if (nonterms.ContainsKey(rule.Value)) {
                    ptrn.Add(nonterms[rule.Value]);

                    if (Lex.PeekToken().Value == "*") {
                        Lex.GetToken();

                        Console.WriteLine("Repeat");

                        nonterms[rule.Value].Repeat = true;
                    }

                    if (Lex.PeekToken().Value == "|") {
                        Lex.GetToken();

                        ptrn.Alternate = GetRulePattern(Lex, nonterms);
                    }

                    continue;
                }

                if (rule.Value == ".") {
                     NonTerminal r = new NonTerminal(new Rule(Lex.GetToken().Value, null));

                    if (Lex.PeekToken().Value == "*") {
                        Lex.GetToken();

                        Console.WriteLine("Repeat");

                        r.Repeat = true;
                    }

                    ptrn.Add(r);

                    if (Lex.PeekToken().Value == "|") {
                        Lex.GetToken();

                        ptrn.Alternate = GetRulePattern(Lex, nonterms);
                    }

                    continue;
                }

                NonTerminal added = new NonTerminal(new Rule(null, rule.Value));

                if (Lex.PeekToken().Value == "*") {
                    Lex.GetToken();

                    Console.WriteLine("Repeat");

                    added.Repeat = true;
                }
                
                ptrn.Add(added);

                if (Lex.PeekToken().Value == "|") {
                    Lex.GetToken();

                    Console.WriteLine("Alternate");

                    ptrn.Alternate = GetRulePattern(Lex, nonterms);
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

                GrammarPattern ptrn = GetRulePattern(Lex, nonterms);

                nonterms[name.Value].Add(new Rule(ptrn));

                spec.NonTerminals.Add(nonterms[name.Value]);

                Lex.GetToken();
            }

            return spec;
        }
    }
}