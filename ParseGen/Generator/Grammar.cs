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

        public List<Rule> Rules = new List<Rule>();

        public string Output;

        private bool Produced = false;

        private FormatState State = new FormatState();

        public List<Rule> Terminals = new List<Rule>();

        public bool Repeat = false;

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

        public void Produce() {
            if (Produced)
                return;
            
            foreach (var rule in Rules) {
                Output += rule.Produce(State, Name) + '\n';
            }

            State.IndentLevel--;

            Output += State.Indent() + "outnode = node; return true;\n";

            Output += State.Indent() + "}\n\n";

            Produced = true;
        }

        public NonTerminal(string name) {
            Name = name;
            
            Output = State.Indent() + $"public bool {name}(out Node outnode) {{\n";

            Output += State.Indent() + $"\tNode node = new Node(); node.NonTerminal = \"{name}\";\n";

            Output += State.Indent() + "\tNode rule = null;\n";

            Output += State.Indent() + "\tint pos = Lex.Position;\n";

            State.IndentLevel++;

            Output += State.Indent() + "Token next = Lex.PeekToken();\n\n";
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

        public string Produce(FormatState state = null, string name = "") {
            string output = state.Indent() + "\n";

            state = state ?? State;

            if (Terminal) {
                if (Recognizer != null) {

                    foreach (NonTerminal rule in Recognizer.Expected) {
                        string conditional = rule.Repeat ? "while" : "if";

                        if (rule.Name != null) {
                            output += state.Indent() + $"{conditional} ({ rule.Name }(out rule)) {{\n";

                            state.IndentLevel++;

                            output += state.Indent() + "\tnode.Children.Add(rule);\n";

                            state.IndentLevel--;

                            output += state.Indent() + "}\n";

                            if (Recognizer.Alternate != null && ! rule.Repeat) {
                                output += state.Indent() + "else {\n";

                                state.IndentLevel++;

                                output += state.Indent() + $"\tnode = new Node(); node.NonTerminal = \"{name}\";\n";

                                output += state.Indent() + "\tLex.Position = pos;\n";

                                output += new Rule(Recognizer.Alternate).Produce(state);

                                output += state.Indent() + "outnode = node; return true;\n";

                                state.IndentLevel--;

                                output += state.Indent() + "}\n";
                            }
                            else if (! rule.Repeat) {
                                output += state.Indent() + "else {\n";

                                state.IndentLevel++;

                                output += state.Indent() + "\tLex.Position = pos;\n";

                                output += state.Indent() + "\toutnode = null;\n";

                                output += state.Indent() + "\treturn false;\n";

                                state.IndentLevel--;

                                output += state.Indent() + "}\n";
                            }
                        }
                        else {
                            if (rule.Rules[0].TerminalType == null) {
                                output += state.Indent() + $"{conditional} (Lex.PeekToken().Value == \"{rule.Rules[0].Value}\") {{\n";
                            }
                            else {
                                if (rule.Rules[0].Value == null)
                                    output += state.Indent() + $"{conditional} (Lex.PeekToken().Type == \"{rule.Rules[0].TerminalType}\") {{\n";
                                else
                                    output += state.Indent() + $"{conditional} (Lex.PeekToken().Type == \"{rule.Rules[0].TerminalType}\" && Lex.PeekToken().Value == \"{rule.Rules[0].Value}\") {{\n";
                            }

                            state.IndentLevel++;

                            output += state.Indent() + "\tnode.Children.Add(new Node(Lex.GetToken()));\n";

                            state.IndentLevel--;

                            output += state.Indent() + "}\n";

                            if (rule.Repeat)
                                continue;

                            output += state.Indent() + "else {\n";

                            state.IndentLevel++;

                            if (Recognizer.Alternate != null) {
                                output += state.Indent() + $"\tnode = new Node(); node.NonTerminal = \"{name}\";\n";

                                output += state.Indent() + "\tLex.Position = pos;\n";

                                output += new Rule(Recognizer.Alternate).Produce(state);

                                output += state.Indent() + "outnode = node; return true;\n";
                            }
                            else {
                                output += state.Indent() + "\tLex.Position = pos;\n";

                                output += state.Indent() + "\toutnode = null;\n";

                                output += state.Indent() + "\treturn false;\n";
                            }

                            state.IndentLevel--;

                            output += state.Indent() + "}\n";
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
                        output += state.Indent() + $"if (next.Value == \"{Value}\") {{\n";
                    }
                    else {
                        if (Value == null)
                            output += state.Indent() + $"if (next.Type == \"{TerminalType}\") {{\n";
                        else
                            output += state.Indent() + $"if (next.Type == \"{TerminalType}\" && next.Value == \"{Value}\") {{\n";
                    }

                    state.IndentLevel++;

                    output += state.Indent() + $"node.Children.Add(new Node(Lex.GetToken()));\n";

                    state.IndentLevel--;

                    output += state.Indent() + $"}}\n\n";
                }
            }
            else {
                output += state.Indent() + $"{NonTerminal.Name}(rule); outnode = rule; return true;";
            }

            return output;
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