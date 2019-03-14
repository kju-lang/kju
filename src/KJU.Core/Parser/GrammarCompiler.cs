namespace KJU.Core.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.Automata;
    using KJU.Core.Automata.NfaToDfa;
    using KJU.Core.Input;
    using KJU.Core.Lexer;
    using KJU.Core.Regex;
    using KJU.Core.Util;

    public class GrammarCompiler<TLabel>
    {
        public static CompiledGrammar<TLabel> CompileGrammar(Grammar<TLabel> grammar)
        {
            var result = new CompiledGrammar<TLabel>();
            result.StartSymbol = grammar.StartSymbol;

            result.Rules = grammar.Rules.GroupBy((rule) => rule.Lhs).ToDictionary(
                keySelector: (group) => group.Key,
                elementSelector: (group) => CompileRules(group.ToList()));

            return result;
        }

        private static IDfa<bool, TLabel> RegexToDfa(Regex<TLabel> regex)
        {
            INfa<TLabel> nfa = RegexToNfaConverter<TLabel>.Convert(regex);
            IDfa<bool, TLabel> dfa = NfaToDfaConverter<TLabel>.Convert(nfa);
            return DfaMinimizer<bool, TLabel>.Minimize(dfa);
        }

        private static IDfa<Optional<Rule<TLabel>>, TLabel> CompileRules(IList<Rule<TLabel>> rules)
        {
            var dfas = rules.ToDictionary(
                keySelector: (rule) => Optional<Rule<TLabel>>.Some(rule),
                elementSelector: (rule) => RegexToDfa(rule.Rhs));
            var merged = DfaMerger<Optional<Rule<TLabel>>, TLabel>.Merge(
                dfas,
                conflictSolver: (matchesEnumerable) =>
                {
                    var matches = matchesEnumerable.ToList();
                    if (matches.Count == 0)
                    {
                        return Optional<Rule<TLabel>>.None();
                    }
                    else if (matches.Count == 1)
                    {
                        return matches[0];
                    }
                    else
                    {
                        throw new ArgumentException($"two rules have conflicting regexes (for symbol: {matches[0].Get().Lhs})");
                    }
                });
            return DfaMinimizer<Optional<Rule<TLabel>>, TLabel>.Minimize(merged);
        }
    }
}