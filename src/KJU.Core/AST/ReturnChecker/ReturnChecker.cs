namespace KJU.Core.AST.ReturnChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Diagnostics;
    using Lexer;
    using Nodes;

    public class ReturnChecker : IPhase
    {
        public const string MissingReturnDiagnostic = "ReturnChecker.MissingReturn";

        public void Run(Node root, IDiagnostics diagnostics)
        {
            new ReturnCheckerProcess(diagnostics).CheckProgram(root);
        }

        private class ReturnCheckerProcess
        {
            private readonly IDiagnostics diagnostics;
            private readonly List<Exception> exceptions = new List<Exception>();

            public ReturnCheckerProcess(IDiagnostics diagnostics)
            {
                this.diagnostics = diagnostics;
            }

            public void CheckProgram(Node root)
            {
                foreach (var declaration in root.ChildrenRecursive().OfType<FunctionDeclaration>())
                {
                    if (declaration.ReturnType != BuiltinTypes.UnitType.Instance && !declaration.IsForeign && !CheckNode(declaration.Body))
                    {
                        var message = $"Function '{declaration.Identifier}' is missing return statement";
                        this.diagnostics.Add(new Diagnostic(
                            DiagnosticStatus.Error,
                            MissingReturnDiagnostic,
                            message,
                            new List<Range> { declaration.InputRange }));
                        this.exceptions.Add(new ReturnCheckerInnerException(message));
                    }
                }

                if (this.exceptions.Any())
                {
                    throw new ReturnCheckerException("Return checking failed.", this.exceptions);
                }
            }

            private static bool CheckNode(Node root)
            {
                switch (root)
                {
                    case ReturnStatement _:
                        return true;
                    case IfStatement stmt:
                        return CheckNode(stmt.ThenBody) && CheckNode(stmt.ElseBody);
                    case WhileStatement stmt:
                        var neverJumpsToAfterWhile = stmt.Condition.AsBool() == true && !CanBreakWhile(stmt.Body);
                        var canReturnFromInside = CanReturnFromFunction(stmt.Body);
                        return neverJumpsToAfterWhile && canReturnFromInside;
                    case InstructionBlock stmt:
                        if (stmt.Instructions.Count == 0)
                        {
                            return false;
                        }

                        return CheckNode(stmt.Instructions.Last());
                }

                return false;
            }

            private static bool CanReturnFromFunction(Node node)
            {
                switch (node)
                {
                    case ReturnStatement _:
                        return true;
                    case FunctionDeclaration _:
                        return false;
                }

                return node.Children().Any(CanReturnFromFunction);
            }

            private static bool CanBreakWhile(Node node)
            {
                switch (node)
                {
                    case BreakStatement _:
                        return true;
                    case WhileStatement _:
                        return false;
                }

                return node.Children().Any(CanBreakWhile);
            }
        }
    }
}