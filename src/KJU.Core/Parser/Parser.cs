namespace KJU.Core.Parser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Automata;
    using Diagnostics;
    using Lexer;
    using Util;

    public class Parser<TLabel>
    {
        public const string UnexpectedSymbolDiagnosticType = "Parser.UnexpectedSymbol";
        public const string PrematureEofDiagnosticType = "Parser.PrematureEof";
        public const string InvalidReduceActionDiagnosticType = "Parser.InvalidReduceAction";
        public const string ParsingFinishedBeforeEofDiagnosticType = "Parser.ParsingFinishedBeforeEof";

        private readonly CompiledGrammar<TLabel> grammar;

        private readonly IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>,
            ParseAction<TLabel>> table;

        public Parser(
            CompiledGrammar<TLabel> grammar,
            IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>,
                ParseAction<TLabel>> table)
        {
            this.grammar = grammar;
            this.table = table;
        }

        public ParseTree<TLabel> Parse(IEnumerable<Token<TLabel>> tokens, IDiagnostics diagnostics)
        {
            return new ParseProcess(this.grammar, this.table, diagnostics).Parse(tokens);
        }

        private class ParseProcess
        {
            private readonly IReadOnlyDictionary<TLabel, IDfa<Optional<Rule<TLabel>>, TLabel>> rules;

            private readonly IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>,
                ParseAction<TLabel>> table;

            private readonly IDiagnostics diagnostics;

            private readonly Stack<IState> statesStack;
            private readonly Stack<IDfa<Optional<Rule<TLabel>>, TLabel>> dfaStack;
            private readonly Stack<List<ParseTree<TLabel>>> nodesStack;

            public ParseProcess(
                CompiledGrammar<TLabel> grammar,
                IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>,
                    ParseAction<TLabel>> table, IDiagnostics diagnostics)
            {
                this.rules = grammar.Rules;
                this.table = table;
                this.diagnostics = diagnostics;
                var startSymbol = grammar.StartSymbol;
                this.statesStack = new Stack<IState>(new[] { this.rules[startSymbol].StartingState() });
                this.dfaStack = new Stack<IDfa<Optional<Rule<TLabel>>, TLabel>>(new[] { this.rules[startSymbol] });
                this.nodesStack = new Stack<List<ParseTree<TLabel>>>(new[] { new List<ParseTree<TLabel>>() });
            }

            public ParseTree<TLabel> Parse(IEnumerable<Token<TLabel>> tokens)
            {
                ParseTree<TLabel> root = null;

                using (var enumerator = tokens.GetEnumerator())
                {
                    enumerator.MoveNext();
                    while (this.statesStack.Count != 0)
                    {
                        var currentToken = enumerator.Current;
                        var state = this.statesStack.Peek();
                        var dfa = this.dfaStack.Peek();
                        var currentCategory = currentToken.Category;
                        var key = new Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>(
                            dfa,
                            state,
                            currentCategory);
                        if (!this.table.ContainsKey(key))
                        {
                            this.diagnostics.Add(new Diagnostic(
                                DiagnosticStatus.Error,
                                UnexpectedSymbolDiagnosticType,
                                $"Unexpected symbol: {Diagnostic.EscapeForMessage(currentToken.ToString())} at {{0}}",
                                new List<Range> { currentToken.InputRange }));
                            throw new ParseException($"Unexpected symbol: {currentToken} at {currentToken.InputRange}");
                        }

                        var action = this.table[key];
                        switch (action.Kind)
                        {
                            case ParseAction<TLabel>.ActionKind.Shift:
                            {
                                this.Shift(currentToken, dfa, state, currentCategory, enumerator);
                                break;
                            }

                            case ParseAction<TLabel>.ActionKind.Reduce:
                            {
                                root = this.Reduce(dfa, state, currentToken);
                                break;
                            }

                            case ParseAction<TLabel>.ActionKind.Call:
                            {
                                this.Call(dfa, state, action.Label);
                                break;
                            }
                        }
                    }

                    if (enumerator.MoveNext())
                    {
                        this.diagnostics.Add(new Diagnostic(
                            DiagnosticStatus.Error,
                            ParsingFinishedBeforeEofDiagnosticType,
                            "Parsing finished before reading all tokens",
                            new List<Range>()));
                        throw new ParseException("Parsing finished before reading all tokens");
                    }
                }

                return root;
            }

            private static Range GetRange(IEnumerable<ParseTree<TLabel>> nodes)
            {
                var ranges = nodes.Select(x => x.InputRange).Where(x => x != null).ToList();
                var begin = ranges.FirstOrDefault()?.Begin;
                var end = ranges.LastOrDefault()?.End;
                return begin != null ? new Range(begin, end) : null;
            }

            private void Call(IDfa<Optional<Rule<TLabel>>, TLabel> dfa, IState state, TLabel actionLabel)
            {
                this.statesStack.Pop();
                this.statesStack.Push(dfa.Transitions(state)[actionLabel]);
                this.dfaStack.Push(this.rules[actionLabel]);
                this.statesStack.Push(this.rules[actionLabel].StartingState());
                this.nodesStack.Push(new List<ParseTree<TLabel>>());
            }

            private ParseTree<TLabel> Reduce(
                IDfa<Optional<Rule<TLabel>>, TLabel> dfa,
                IState state,
                ParseTree<TLabel> currentToken)
            {
                if (dfa.Label(state).IsNone())
                {
                    this.diagnostics.Add(new Diagnostic(
                        DiagnosticStatus.Error,
                        InvalidReduceActionDiagnosticType,
                        "Invalid reduce action at {0}",
                        new List<Range> { currentToken.InputRange }));
                    throw new ParseException("Invalid reduce action");
                }

                var rule = dfa.Label(state).Get();
                var topNodes = this.nodesStack.Peek();
                var newCategory = rule.Lhs;
                var range = GetRange(topNodes);
                ParseTree<TLabel> root = new Brunch<TLabel>
                {
                    Rule = rule, Children = topNodes, Category = newCategory, InputRange = range
                };
                this.statesStack.Pop();
                this.dfaStack.Pop();
                this.nodesStack.Pop();
                if (this.nodesStack.Count != 0)
                {
                    this.nodesStack.Peek().Add(root);
                }

                return root;
            }

            private void Shift(
                ParseTree<TLabel> currentToken,
                IDfa<Optional<Rule<TLabel>>, TLabel> dfa,
                IState state,
                TLabel currentCategory,
                IEnumerator enumerator)
            {
                this.nodesStack.Peek().Add(currentToken);
                this.statesStack.Pop();
                this.statesStack.Push(dfa.Transitions(state)[currentCategory]);
                if (!enumerator.MoveNext())
                {
                    this.diagnostics.Add(new Diagnostic(
                        DiagnosticStatus.Error,
                        PrematureEofDiagnosticType,
                        "Parsing finished before reading all tokens",
                        new List<Range>()));
                    throw new ParseException("Premature EOF");
                }
            }
        }
    }
}