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

        private List<Clause> clauses;
        private FindUnion<IHerbrandObject> findUnion;
        private Solution? solution;

        public Solver(List<Clause> clauses)
        {
            this.clauses = clauses;
            this.findUnion = new FindUnion<IHerbrandObject>((x, y) =>
            {
                if (x is TypeVariable == y is TypeVariable)
                {
                    return 0;
                }

                return x is TypeVariable ? -1 : 1;
            });
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

            var clauseId = this.Backtrack(0, choices);

            switch (this.solution)
            {
                case null:
                    throw new TypeCheckerException(
                        $"Cannot satisfy the clause {this.clauses[clauseId].InputRange}");

                case Solution finalSolution:
                    return finalSolution;
            }
        }

        private int Backtrack(int consideredClause, IDictionary<Clause, int> choices)
        {
            var result = consideredClause;
            if (consideredClause == this.clauses.Count)
            {
                if (this.solution is Solution firstSolution)
                {
                    var secondSolution = this.ConstructSolution(choices);

                    var firstVarMapping = firstSolution.TypeVariableMapping;
                    var secondVarMapping = secondSolution.TypeVariableMapping;

                    var differentKey = firstVarMapping.Keys
                        .FirstOrDefault(key => !firstVarMapping[key].Equals(secondVarMapping[key]));

                    switch (differentKey)
                    {
                        case null:
                            throw new TypeCheckerInternalException(
                                "Found two same solutions with different choices of clauses");

                        case TypeVariable typeVar:
                            throw new TypeCheckerException($"Cannot deduce type for {typeVar.InputRange}");
                    }
                }

                this.solution = this.ConstructSolution(choices);
                return result;
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
                    result = Math.Max(result, this.Backtrack(consideredClause + 1, choices));
                }

                this.findUnion.PopCheckpoint();
            }

            return result;
        }

        private bool Unify(IHerbrandObject x, IHerbrandObject y)
        {
            x = this.findUnion.GetRepresentant(x);
            y = this.findUnion.GetRepresentant(y);

            if (x == y)
            {
                return true;
            }

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

            return new Solution { ChosenAlternative = new Dictionary<Clause, int>(choices), TypeVariableMapping = variableMapping };
        }

        private void Dfs(
            IHerbrandObject v,
            ISet<IHerbrandObject> visited,
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
