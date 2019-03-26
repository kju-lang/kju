namespace KJU.Tests.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KJU.Core.AST;
    using KJU.Core.AST.BuiltinTypes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

#pragma warning disable SA1118  // Parameter must not span multiple lines
#pragma warning disable SA1202 // Elements must be ordered by access

    public class TypeCheckerHelper
    {
        public bool TypeCompareAst(Node expected, Node result)
        {
            // helper funciton to comapre two AST to check types, other things dont matter
            if (expected == null && result == null)
                return true;
            if (expected == null || result == null)
                return false;

            var childrenLeft = expected.Children().ToList();
            var childrenRight = result.Children().ToList();

            if (childrenLeft.Count() != childrenRight.Count())
                return false;

            for (int i = 0; i < childrenLeft.Count(); i++)
            {
                if (!this.TypeCompareAst(childrenLeft[i], childrenRight[i]))
                    return false;
            }

            return this.TypeCompareAst(expected as Expression, result as Expression);
        }

        private bool TypeCompareAst(Expression expected, Expression result)
        {
            if (expected == null && result == null)
                return true;
            if (expected == null || result == null)
                return false;

            if (expected.Type != result.Type)
            {
                Console.WriteLine(
                    $"Expected type \"{expected.Type}\", result: \"{result.Type}\" in Expression {expected}");
                return false;
            }

            return true;
        }

        public string AstToJson(Node node)
        {
            return JsonConvert.SerializeObject(
                new HelperStruct(node),
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All
                });
        }

        public Node JsonToAst(string json)
        {
            Node result = ((HelperStruct)JsonConvert.DeserializeObject(
                json,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All
                })).Node;

            this.ConvertTypes(result);
            return result;
        }

        private DataType DataTypeInstance(DataType dataType)
        {
            switch (dataType)
            {
                case IntType intType:
                    return IntType.Instance;
                case BoolType boolType:
                    return BoolType.Instance;
                case UnitType unitType:
                    return UnitType.Instance;
            }

            return null;
        }

        private void ConvertTypes(Node node)
        {
            foreach (var child in node.Children())
            {
                this.ConvertTypes(child);
            }

            switch (node)
            {
                case FunctionDeclaration fun:
                    fun.ReturnType = this.DataTypeInstance(fun.ReturnType);
                    break;
                case VariableDeclaration var:
                    var.VariableType = this.DataTypeInstance(var.VariableType);
                    break;
            }

            var expr = node as Expression;
            if (expr == null) return;
            expr.Type = this.DataTypeInstance(expr.Type);
        }

        private class HelperStruct
        {
            public HelperStruct(Node node)
            {
                this.Node = node;
            }

            public UnitType UnitType { get; set; } = UnitType.Instance;

            public IntType IntType { get; set; } = IntType.Instance;

            public BoolType BoolType { get; set; } = BoolType.Instance;

            public Node Node { get; set; }
        }
    }
}