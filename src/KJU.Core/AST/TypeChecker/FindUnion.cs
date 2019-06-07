namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// For each set in the structure we want to be able to return the "representant" of the set.
    /// When we merge to sets the representant of the new set is decided by the arbiter
    /// i.e. representant[union(x, y)] := (x if arbiter(representant[x], representant[y]) < 0 else y).
    /// </summary>
    public class FindUnion<T>
    {
        public FindUnion(Comparison<T> arbiter)
        {
            this.Checkpoints = new Stack<int>();
            this.Parent = new List<int>();
            this.Elems = new List<T>();
            this.ElemsRev = new Dictionary<T, int>();
            this.History = new Stack<HistoryEntry>();
            this.Arbiter = arbiter;
        }

        private Stack<int> Checkpoints { get; }

        private IList<int> Parent { get; }

        private IList<T> Elems { get; }

        private IDictionary<T, int> ElemsRev { get; }

        private Stack<HistoryEntry> History { get; }

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
                throw new TypeCheckerInternalException("Nodes considered equal occuring twice in Find-Union");
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
            return ret;
        }

        private void PopHistoryEntry()
        {
            var entry = this.History.Pop();
            this.Parent[entry.Index] = entry.OldValue;
        }

        private void SetParent(int index, int parent)
        {
            this.History.Push(new HistoryEntry { Index = index, OldValue = this.Parent[index] });
            this.Parent[index] = parent;
        }

        private int Find(int index)
        {
            if (this.Parent[index] == index)
                return index;
            int ret = this.Find(this.Parent[index]);
            this.SetParent(index, ret);
            return ret;
        }

        private struct HistoryEntry
        {
            public int Index;
            public int OldValue;
        }
    }
}
