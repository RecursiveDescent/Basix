# Basix

Basix is a parser generator that can generate C# and JavaScript recursive descent parsers from a simplistic grammar definition language with an easily extensible code generation layer.

#### Running
You can generate a parser instantly by moving into the project folder and running `dotnet run <GRAMMARFILE> [-o <OUTPUTFILE>]`

## Defining a grammar
The definition syntax of Basix is very simple and subject to change in the future.

A simple use of the grammar syntax could be parsing a math equation.
```
Literal > .NUM | .ID | .STR;

=Div;
DivQ > / Literal;
Div > Literal DivQ | Literal;

=Mul;
MulQ > * Mul;
Mul > Div MulQ | Div;

=Sub;
SubQ > - Sub;
Sub > Mul SubQ | Mul;

=Add;
AddQ > + Add;
Add > Sub AddQ | Sub;
```

*Note the use of = in this grammar, basix grammars are currently parsed from top to bottom with a single pass, meaning if you want to use a rule defined later in the file you need to tell the program it is going to be defined later*

This grammar uses two rules for each operator, necessary to parse the equation correctly as it eliminates left-recursion [<sup>wikipedia</sup>](https://en.wikipedia.org/wiki/Left_recursion) while correctly parsing left-associative operators (addition is left-associative because you do 1 + 2 before 3 in 1 + 2 + 3).

There are some cases where you need to match 0 or more of the same rule, in which case you can use the * symbol (when matching * or ? as part of the grammar, use .MUL and .QMARK instead of */?, or `A * B` could be parsed as `A* B`).

`List > [ Element* ];`