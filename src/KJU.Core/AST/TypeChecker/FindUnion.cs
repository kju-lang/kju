namespace KJU.Core.AST.TypeChecker
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// For each set in the structure we want to be able to return the "representant" of the set.
    /// When we merge to sets the representant of the new set is decided by the arbiter
    /// Arbiter, given two representants, returns negative value if second one is preferred and positive otherwise.
    /// When result is 0, arbitrary(hehe) representant is chosen.
    /// </summary>
    public class FindUnion<T>
    {
        public FindUnion(Comparison<T> arbiter)
        {
            this.Checkpoints = new Stack<int>();
            this.Parent = new List<int>();
            this.Rank = new List<int>();
            this.Representant = new List<int>();
            this.Elems = new List<T>();
            this.ElemsRev = new Dictionary<T, int>();
            this.History = new Stack<HistoryEntry>();
            this.Arbiter = arbiter;
        }

        private Stack<int> Checkpoints { get; }

        private IList<int> Parent { get; }

        private IList<int> Rank { get; }

        private IList<int> Representant { get; }

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
            int root;
            int child;
            if (this.Rank[ra] >= this.Rank[rb])
            {
                root = ra;
                child = rb;
            }
            else
            {
                root = rb;
                child = ra;
            }

            this.SetParent(child, root);
            var rep = this.Arbiter(this.Elems[this.Representant[root]], this.Elems[this.Representant[child]]);
            if (rep < 0)
                this.Representant[root] = this.Representant[child];
        }

        public T GetParent(T x)
        {
            return this.Elems[this.Parent[this.GetElemId(x)]];
        }

        public T GetRepresentant(T x)
        {
            return this.Elems[this.Representant[this.Find(this.GetElemId(x))]];
        }

        private int GetElemId(T x)
        {
            if (this.ElemsRev.ContainsKey(x))
                return this.ElemsRev[x];
            int ret = this.Elems.Count;
            this.ElemsRev.Add(x, ret);
            this.Elems.Add(x);
            this.Parent.Add(ret);
            this.Representant.Add(ret);
            this.Rank.Add(1);
            return ret;
        }

        private void PopHistoryEntry()
        {
            var entry = this.History.Pop();
            this.Rank[this.Parent[entry.Index]] -= this.Rank[entry.Index];
            this.Representant[this.Parent[entry.Index]] = entry.OldRepresentant;
            this.Parent[entry.Index] = entry.Index;
        }

        private void SetParent(int index, int parent)
        {
            this.History.Push(new HistoryEntry { Index = index, OldRepresentant = this.Representant[parent] });
            this.Rank[parent] += this.Rank[index];
            this.Parent[index] = parent;
        }

        private int Find(int index)
        {
            while (this.Parent[index] != index)
                index = this.Parent[index];
            return index;
        }

        private struct HistoryEntry
        {
            public int Index;           // child's index
            public int OldRepresentant; // parent's old representant
        }
    }
}
