namespace KJU.Core.Automata
{
    using System.Collections.Generic;
    using KJU.Core;

    public class DfaUtils
    {
        public static IReadOnlyCollection<IState> GetAllStates<TLabel, Symbol>(IDfa<TLabel, Symbol> dfa)
        {
            var visited = new HashSet<IState>();
            var queue = new Queue<IState>();

            queue.Enqueue(dfa.StartingState());
            visited.Add(dfa.StartingState());

            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                var transitions = dfa.Transitions(s);

                foreach (var nextState in transitions.Values)
                {
                    if (!visited.Contains(nextState))
                    {
                        queue.Enqueue(nextState);
                        visited.Add(nextState);
                    }
                }
            }

            return visited;
        }
    }
}