namespace KJU.Core.CodeGeneration.RegisterAllocation.Coalescing.Predicates
{
    using System.Collections.Generic;
    using KJU.Core.Intermediate;

    internal interface ICoalescePredicate
    {
        bool CanCoalesce(HashSet<VirtualRegister> left, HashSet<VirtualRegister> right);
    }
}