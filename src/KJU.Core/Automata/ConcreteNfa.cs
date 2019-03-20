namespace KJU.Core.Automata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class ConcreteNfa<Symbol> : INfa<Symbol>
    {
        private readonly ValueState<int> startingState;
        private readonly IList<List<IState>> epsilonTransitions = new List<List<IState>>();
        private readonly IList<Dictionary<Symbol, IReadOnlyCollection<IState>>> transitions = new List<Dictionary<Symbol, IReadOnlyCollection<IState>>>();
        private readonly IList<bool> isAccepting = new List<bool>();

        private ConcreteNfa(ValueState<int> startingState)
        {
            this.startingState = startingState;
        }

        public static ConcreteNfa<Symbol> CreateFromNfa(INfa<Symbol> nfa)
        {
            var stateIds = new Dictionary<IState, int>();
            var newNfa = new ConcreteNfa<Symbol>(new ValueState<int>(0));
            newNfa.ConstructStates(stateIds, nfa, nfa.StartingState());
            return newNfa;
        }

        public IState StartingState()
        {
            return this.startingState;
        }

        public IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> Transitions(IState state)
        {
            return this.transitions[(state as ValueState<int>).Value];
        }

        public IReadOnlyCollection<IState> EpsilonTransitions(IState state)
        {
            return this.epsilonTransitions[(state as ValueState<int>).Value];
        }

        public bool IsAccepting(IState state)
        {
            return this.isAccepting[(state as ValueState<int>).Value];
        }

        private int ConstructStates(Dictionary<IState, int> stateIds, INfa<Symbol> nfa, IState state)
        {
            if (stateIds.ContainsKey(state))
                return stateIds[state];

            int stateId = stateIds.Count;
            stateIds[state] = stateId;

            this.epsilonTransitions.Add(null);
            this.transitions.Add(null);
            this.isAccepting.Add(nfa.IsAccepting(state));

            this.epsilonTransitions[stateId] = nfa.EpsilonTransitions(state).Select(target => new ValueState<int>(this.ConstructStates(stateIds, nfa, target)) as IState).ToList();
            this.transitions[stateId] = nfa.Transitions(state)
                .ToDictionary(
                    keySelector: kv => kv.Key,
                    elementSelector: kv => kv.Value.Select(target => new ValueState<int>(this.ConstructStates(stateIds, nfa, target))).ToList() as IReadOnlyCollection<IState>);

            return stateId;
        }
    }
}