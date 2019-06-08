namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// For each set in the structure we want to be able to return the "representant" of the set.
    /// When we merge to sets the representant of the new set is decided by the arbiter
    /// When arbiter is not 0, then representant[x] wins for negative values, else representant[y] is chosen.
    /// When arbiter is 0, bigger tree wins.
    /// </summary>
    public class FindUnion<T>
    {
        public FindUnion(Comparison<T> arbiter)
        {
            this.Checkpoints = new Stack<int>();
            this.Parent = new List<int>();
            this.Rank = new List<int>();
            this.Elems = new List<T>();
            this.ElemsRev = new Dictionary<T, int>();
            this.History = new Stack<int>();
            this.Arbiter = arbiter;
        }

        private Stack<int> Checkpoints { get; }

        private IList<int> Parent { get; }

        private IList<int> Rank { get; }

        private IList<T> Elems { get; }

        private IDictionary<T, int> ElemsRev { get; }

        private Stack<int> History { get; }

        private Comparison<T> Arbiter { get; }

        public void PushCheckpoint()
        {
            this.Checkpoints.Push(this.History.Count);
        }

        public void PopCheckpoint()
        {
            int toSize = this.Checkpoints.Pop();
            while (this.History.Count > toSize)
                this.PopHistoryEntry();
        }

        public void Union(T a, T b)
        {
            int ra = this.Find(this.GetElemId(a));
            int rb = this.Find(this.GetElemId(b));
            if (ra == rb)
                return;
            var rep = this.Arbiter(this.Elems[ra], this.Elems[rb]);
            if (rep == 0)
                rep = this.Rank[ra] >= this.Rank[rb] ? -1 : 1;
            if (rep > 0)
                this.SetParent(ra, rb);
            else
                this.SetParent(rb, ra);
        }

        public T GetParent(T x)
        {
            return this.Elems[this.Parent[this.GetElemId(x)]];
        }

        public T GetRepresentant(T x)
        {
            return this.Elems[this.Find(this.GetElemId(x))];
        }

        private int GetElemId(T x)
        {
            if (this.ElemsRev.ContainsKey(x))
                return this.ElemsRev[x];
            int ret = this.Elems.Count;
            this.ElemsRev.Add(x, ret);
            this.Elems.Add(x);
            this.Parent.Add(ret);
            this.Rank.Add(1);
            return ret;
        }

        private void PopHistoryEntry()
        {
            int idx = this.History.Pop();
            this.Rank[this.Parent[idx]] -= this.Rank[idx];
            this.Parent[idx] = idx;
        }

        private void SetParent(int index, int parent)
        {
            this.History.Push(index);
            this.Rank[parent] += this.Rank[index];
            this.Parent[index] = parent;
        }

        private int Find(int index)
        {
            while (this.Parent[index] != index)
                index = this.Parent[index];
            return index;
        }
    }
}
