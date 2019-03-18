namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using KJU.Core.Automata;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;
    using KJU.Core.Util;

    public class Parser<TLabel>
    {
        private CompiledGrammar<TLabel> grammar;
        private IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>, ParseAction<TLabel>> table;

        public Parser(CompiledGrammar<TLabel> grammar, IReadOnlyDictionary<Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>, ParseAction<TLabel>> table)
        {
            this.grammar = grammar;
            this.table = table;
        }

        public ParseTree<TLabel> Parse(IEnumerable<Token<TLabel>> tokens)
        {
            TLabel start = this.grammar.StartSymbol;
            IReadOnlyDictionary<TLabel, IDfa<Optional<Rule<TLabel>>, TLabel>> rules = this.grammar.Rules;
            Stack<IState> statesStack = new Stack<IState>();
            Stack<IDfa<Optional<Rule<TLabel>>, TLabel>> dfaStack = new Stack<IDfa<Optional<Rule<TLabel>>, TLabel>>();
            Stack<List<ParseTree<TLabel>>> nodesStack = new Stack<List<ParseTree<TLabel>>>();

            statesStack.Push(rules[start].StartingState());
            dfaStack.Push(rules[start]);
            nodesStack.Push(new List<ParseTree<TLabel>>());

            ParseTree<TLabel> root = null;

            IEnumerator<Token<TLabel>> enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();
            while (statesStack.Count != 0)
            {
                Token<TLabel> token = enumerator.Current;
                IState state = statesStack.Peek();
                IDfa<Optional<Rule<TLabel>>, TLabel> dfa = dfaStack.Peek();

                ParseAction<TLabel> action = this.table[new Tuple<IDfa<Optional<Rule<TLabel>>, TLabel>, IState, TLabel>(dfa, state, token.Category)];
                if (action.Kind == ParseAction<TLabel>.ActionKind.Shift)
                {
                    nodesStack.Peek().Add(token);
                    statesStack.Pop();
                    statesStack.Push(dfa.Transitions(state)[token.Category]);
                    if (!enumerator.MoveNext())
                    {
                        while (true)
                        {
                            state = statesStack.Peek();
                            dfa = dfaStack.Peek();

                            if (dfa.Label(state).IsNone())
                            {
                                throw new Exception("Invalid reduce action");
                            }

                            Brunch<TLabel> brunch = new Brunch<TLabel>();
                            brunch.Rule = dfa.Label(state).Get();
                            brunch.Children = nodesStack.Peek();
                            brunch.Category = brunch.Rule.Lhs;
                            Range range = null;
                            ILocation begin = null;
                            ILocation end = null;
                            foreach (ParseTree<TLabel> node in nodesStack.Peek())
                            {
                                if (node.InputRange != null)
                                {
                                    if (begin == null)
                                    {
                                        begin = node.InputRange.Begin;
                                    }

                                    end = node.InputRange.End;
                                }
                            }

                            if (begin != null)
                            {
                                range = new Range();
                                range.Begin = begin;
                                range.End = end;
                            }

                            brunch.InputRange = range;

                            statesStack.Pop();
                            dfaStack.Pop();
                            nodesStack.Pop();

                            root = brunch;

                            if (nodesStack.Count == 0)
                            {
                                return root;
                            }

                            nodesStack.Peek().Add(root);
                        }
                    }
                }
                else if (action.Kind == ParseAction<TLabel>.ActionKind.Reduce)
                {
                    if (dfa.Label(state).IsNone())
                    {
                        throw new Exception("Invalid reduce action");
                    }

                    Brunch<TLabel> brunch = new Brunch<TLabel>();
                    brunch.Rule = dfa.Label(state).Get();
                    brunch.Children = nodesStack.Peek();
                    brunch.Category = brunch.Rule.Lhs;
                    Range range = new Range();
                    // range.Begin = nodesStack.Peek()[0].InputRange.Begin;
                    // range.End = nodesStack.Peek()[nodesStack.Peek().Count - 1].InputRange.End;
                    brunch.InputRange = range;

                    statesStack.Pop();
                    dfaStack.Pop();
                    nodesStack.Pop();

                    root = brunch;

                    if (nodesStack.Count != 0)
                    {
                        nodesStack.Peek().Add(root);
                    }
                }
                else if (action.Kind == ParseAction<TLabel>.ActionKind.Call)
                {
                    TLabel symbol = action.Label;
                    statesStack.Pop();
                    statesStack.Push(dfa.Transitions(state)[symbol]);
                    dfaStack.Push(rules[symbol]);
                    statesStack.Push(rules[symbol].StartingState());
                    nodesStack.Push(new List<ParseTree<TLabel>>());
                }
            }

            if (enumerator.MoveNext())
            {
                throw new Exception("Parsing finished before reading all tokens");
            }

            return root;
        }
    }
}