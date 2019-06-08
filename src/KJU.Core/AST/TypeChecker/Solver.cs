namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AST.Types;

    /// <summary>
    ///   Solves satisfiability problem in propositional calculus with equality.
    /// </summary>
    public class Solver
    {
        public const string NoSolutionExceptionMessage = "Cannot satify the given clauses";
        public const string MultipleSolutionExceptionMessage = "Multiple solutions are possible for the given clauses";

        private List<Clause> clauses;
        private FindUnion<IHerbrandObject> findUnion;
        private Solution? solution;

        public Solver(List<Clause> clauses)
        {
            this.clauses = clauses;
            this.findUnion = new FindUnion<IHerbrandObject>((x, y) => x is TypeVariable ? -1 : +1);
        }

        /// <summary>
        ///   Solve the problem. Throws exception if:
        ///   - there are no solutions
        ///   - there are multiple solutions
        ///   - but does not need to throw if there are non-instantiated type variables
        ///
        ///   Solution may contain TypeVariables on right hand side - the only requirement is that
        ///   all Clauses have to be satisfied.
        /// </summary>
        public Solution Solve()
        {
            this.solution = null;
            var choices = new Dictionary<Clause, int>();

            this.Backtrack(0, choices);

            switch (this.solution)
            {
                case null:
                    throw new TypeCheckerException(NoSolutionExceptionMessage);

                case Solution finalSolution:
                    return finalSolution;
            }
        }

        private void Backtrack(int consideredClause, IDictionary<Clause, int> choices)
        {
            if (consideredClause == this.clauses.Count)
            {
                Console.WriteLine(choices.Count);
                this.solution = this.ConstructSolution(choices);
                this.solution.Value.TypeVariableMapping.ToList().ForEach(kvp => Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}"));
                /*if (this.solution.HasValue)
                {
                    throw new TypeCheckerException(MultipleSolutionExceptionMessage);
                }*/

                this.solution = this.ConstructSolution(choices);
                return;
            }

            var clause = this.clauses[consideredClause];
            var alternatives = this.clauses[consideredClause].Alternatives;
            for (var i = 0; i < alternatives.Count; ++i)
            {
                this.findUnion.PushCheckpoint();
                choices[clause] = i;

                var satisfied = alternatives[i].All(tuple => this.Unify(tuple.Item1, tuple.Item2));
                if (satisfied)
                {
                    this.Backtrack(consideredClause + 1, choices);
                }

                this.findUnion.PopCheckpoint();
            }
        }

        private bool Unify(IHerbrandObject x, IHerbrandObject y)
        {
            if (y is TypeVariable)
            {
                (x, y) = (y, x);
            }

            if (x is TypeVariable)
            {
                this.findUnion.Union(x, y);
                return true;
            }

            if (x.GetTag() != y.GetTag() || x.GetArguments().Count() != y.GetArguments().Count())
            {
                return false;
            }

            this.findUnion.Union(x, y);
            return x.GetArguments()
                .Zip(y.GetArguments(), (first, second) => (first, second))
                .All(tuple => this.Unify(tuple.first, tuple.second));
        }

        private Solution ConstructSolution(IDictionary<Clause, int> choices)
        {
            var variableMapping = new Dictionary<TypeVariable, IHerbrandObject>();
            var visited = new HashSet<IHerbrandObject>();
            foreach (var clause in this.clauses)
            {
                var chosenAlternative = choices[clause];
                foreach (var (x, y) in clause.Alternatives[chosenAlternative])
                {
                    this.Dfs(x, visited, variableMapping);
                    this.Dfs(y, visited, variableMapping);
                }
            }

            return new Solution { ChosenAlternative = choices, TypeVariableMapping = variableMapping };
        }

        private void Dfs(
            IHerbrandObject v,
            HashSet<IHerbrandObject> visited,
            IDictionary<TypeVariable, IHerbrandObject> variableMapping)
        {
            if (visited.Contains(v))
            {
                return;
            }

            visited.Add(v);
            if (v is TypeVariable typeVariable)
            {
                variableMapping.Add(typeVariable, this.findUnion.GetRepresentant(v));
            }
            else
            {
                foreach (var child in v.GetArguments())
                {
                    this.Dfs(child, visited, variableMapping);
                }
            }
        }
    }
}
