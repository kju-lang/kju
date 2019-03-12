namespace KJU.Tests.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Automata;

    public static class DfaEquivalence<TLabel, Symbol>
    {
        public static bool AreEquivalent(IDfa<TLabel, Symbol> firstDfa, IDfa<TLabel, Symbol> secondDfa)
        {
            var reached = new HashSet<Tuple<IState, IState>>();
            var queue = new Queue<Tuple<IState, IState>>();

            queue.Enqueue(Tuple.Create(firstDfa.StartingState(), secondDfa.StartingState()));
            reached.Add(queue.Peek());

            while (queue.Count > 0)
            {
                var(firstState, secondState) = queue.Dequeue();
                if (!firstDfa.Label(firstState).Equals(secondDfa.Label(secondState)))
                {
                    return false;
                }

                var firstStateTransitions = firstDfa.Transitions(firstState);
                var secondStateTransitions = secondDfa.Transitions(secondState);

                if (!firstStateTransitions.Count.Equals(secondStateTransitions.Count))
                {
                    return false;
                }

                foreach (var(c, firstNextState) in firstStateTransitions)
                {
                    if (!secondStateTransitions.ContainsKey(c))
                    {
                        return false;
                    }

                    var secondNextState = secondStateTransitions[c];

                    var pair = Tuple.Create(firstNextState, secondNextState);
                    if (!reached.Contains(pair))
                    {
                        reached.Add(pair);
                        queue.Enqueue(pair);
                    }
                }
            }

            return true;
        }
    }
}
