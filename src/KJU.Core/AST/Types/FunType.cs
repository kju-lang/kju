namespace KJU.Core.AST.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FunType : DataType, IEquatable<FunType>
    {
        private static Stack<Tuple<FunType, FunType>> equalsStack = new Stack<Tuple<FunType, FunType>>();

        public FunType(IEnumerable<DataType> argTypes, DataType resultType)
        {
            this.ArgTypes = argTypes.ToList();
            this.ResultType = resultType;
        }

        public FunType(FunctionDeclaration declaration)
            : this(declaration.Parameters.Select(param => param.VariableType), declaration.ReturnType)
        {
        }

        public override string LayoutLabel
        {
            get
            {
                return "1";
            }
        }

        public IReadOnlyList<DataType> ArgTypes { get; set; }

        public DataType ResultType { get; set; }

        public override bool IsHeapType()
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as FunType);
        }

        public bool Equals(FunType other)
        {
            return other != null
                && SmartEquals(this, other);
        }

        public override int GetHashCode()
        {
            return this.ArgTypes.Aggregate(
                this.ResultType.GetHashCode(),
                (acc, type) => (acc, type).GetHashCode());
        }

        public override IEnumerable<IHerbrandObject> GetArguments()
        {
            yield return this.ResultType;
            foreach (DataType param in this.ArgTypes)
                yield return param;
        }

        private static bool SmartEquals(FunType first, FunType second)
        {
            if (equalsStack.Any(entry => entry.Item1 == first && entry.Item2 == second))
                return true;

            equalsStack.Push(new Tuple<FunType, FunType>(first, second));

            bool result =
                first.ResultType.Equals(second.ResultType) && Enumerable.SequenceEqual(first.ArgTypes, second.ArgTypes);

            equalsStack.Pop();

            return result;
        }
    }
}
