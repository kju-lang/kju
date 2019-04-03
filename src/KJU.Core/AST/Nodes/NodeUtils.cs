namespace KJU.Core.AST.Nodes
{
    using System.Collections.Generic;
    using System.Linq;

    public static class NodeUtils
    {
        public static IEnumerable<Node> ChildrenRecursive(this Node root)
        {
            var children = root.Children();
            var descendants = root.Children().SelectMany(child => child.ChildrenRecursive());
            return children.Concat(descendants);
        }
    }
}