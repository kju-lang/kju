namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Diagnostics;
    using KJU.Core.AST.Types;
    using Lexer;

    public class NameResolver : IPhase
    {
        public const string MultipleDeclarationsDiagnostic = "NameResolver.MultipleDeclarations";
        public const string IdentifierNotFoundDiagnostic = "NameResolver.IdentifierNotFound";
        public const string TypeIdentifierErrorDiagnosticsType = "NameResolver.UnexpectedTypeIdentifier";

        public void Run(Node root, IDiagnostics diagnostics)
        {
            new ResolveProcess().Process(root, diagnostics);
        }

        private class ResolveProcess
        {
            private readonly Dictionary<string, Stack<VariableDeclaration>> variables =
                new Dictionary<string, Stack<VariableDeclaration>>();

            private readonly Stack<HashSet<string>> variableBlocks = new Stack<HashSet<string>>();

            private readonly Dictionary<string, Stack<FunctionDeclaration>> functions =
                new Dictionary<string, Stack<FunctionDeclaration>>();

            private readonly Stack<Dictionary<string, List<FunctionDeclaration>>> functionBlocks =
                new Stack<Dictionary<string, List<FunctionDeclaration>>>();

            private readonly List<Exception> exceptions = new List<Exception>();

            private readonly Dictionary<string, DataType> dataTypes =
                new Dictionary<string, DataType>();

            public ResolveProcess()
            {
                this.dataTypes["Int"] = BuiltinTypes.IntType.Instance;
                this.dataTypes["Bool"] = BuiltinTypes.BoolType.Instance;
                this.dataTypes["Unit"] = BuiltinTypes.UnitType.Instance;
            }

            public void Process(Node node, IDiagnostics diagnostics)
            {
                this.ProcessNode(node, diagnostics);
                if (this.exceptions.Any())
                {
                    throw new NameResolverException("Name resolver fail.", this.exceptions);
                }
            }

            private void ProcessNode(Node node, IDiagnostics diagnostics)
            {
                if (node is Expression expression)
                {
                    expression.Type = this.ResolveDataType(expression.Type, diagnostics);
                }

                switch (node)
                {
                    case Program program:
                        this.ProcessProgram(program);
                        break;
                    case FunctionDeclaration fun:
                        this.ProcessFunctionDeclaration(fun, diagnostics);
                        break;
                    case VariableDeclaration var:
                        this.ProcessVariableDeclaration(var, diagnostics);
                        break;
                    case InstructionBlock _:
                        this.ProcessInstructionBlock();
                        break;
                    case Variable var:
                        this.ProcessVariable(var, diagnostics);
                        break;
                    case FunctionCall fun:
                        this.ProcessFunctionCall(fun, diagnostics);
                        break;
                    case ArrayAlloc array:
                        array.ElementType = this.ResolveDataType(array.ElementType, diagnostics);
                        break;
                }

                foreach (var child in node.Children())
                {
                    this.ProcessNode(child, diagnostics);
                }

                switch (node)
                {
                    case Program _:
                        this.PopBlock();
                        this.PopBlock();
                        break;
                    case FunctionDeclaration _:
                        this.PopBlock();
                        break;
                    case InstructionBlock _:
                        this.PopBlock();
                        break;
                }
            }

            private DataType ResolveDataType(DataType type, IDiagnostics diagnostics)
            {
                switch (type)
                {
                    case UnresolvedType unresolvedType:
                        if (!this.dataTypes.ContainsKey(unresolvedType.Type))
                        {
                            var diag = new Diagnostic(
                                DiagnosticStatus.Error,
                                TypeIdentifierErrorDiagnosticsType,
                                $"{{0}} Unexpected type identifier: '{unresolvedType.Type}'",
                                new List<Range> { unresolvedType.InputRange });
                            diagnostics.Add(diag);
                            throw new NameResolverException($"unexpected type identifier: {unresolvedType.Type}");
                        }

                        return this.dataTypes[unresolvedType.Type];

                    case UnresolvedArrayType unresolvedArray:
                        var childDataType = this.ResolveDataType(unresolvedArray.Child, diagnostics);
                        return ArrayType.GetInstance(childDataType);
                }

                return type;
            }

            private void AddToPeek(string functionName, FunctionDeclaration declaration)
            {
                if (!this.functionBlocks.Peek().ContainsKey(functionName))
                {
                    this.functionBlocks.Peek().Add(functionName, new List<FunctionDeclaration>());
                }

                this.functionBlocks.Peek()[functionName].Add(declaration);
            }

            private void ProcessProgram(Program program)
            {
                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());

                foreach (var fun in program.Functions)
                {
                    var id = fun.Identifier;

                    if (!this.functions.ContainsKey(id))
                    {
                        this.functions.Add(id, new Stack<FunctionDeclaration>());
                    }

                    this.functions[id].Push(fun);
                    this.AddToPeek(id, fun);
                }

                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());
            }

            private void ProcessFunctionDeclaration(FunctionDeclaration fun, IDiagnostics diagnostics)
            {
                var id = fun.Identifier;
                fun.ReturnType = this.ResolveDataType(fun.ReturnType, diagnostics);

                // It is important to do it here, so FunctionDeclaration.ParametersTypesEquals below works correctly
                for (int i = 0; i < fun.Parameters.Count; i++)
                {
                    fun.Parameters[i].VariableType = this.ResolveDataType(fun.Parameters[i].VariableType, diagnostics);
                }

                if (this.functionBlocks.Peek().ContainsKey(id))
                {
                    foreach (var f in this.functionBlocks.Peek()[id])
                    {
                        if (FunctionDeclaration.ParametersTypesEquals(f, fun))
                        {
                            var diagnostic = new Diagnostic(
                                DiagnosticStatus.Error,
                                MultipleDeclarationsDiagnostic,
                                $"Multiple declarations of function name {id}",
                                new List<Range> { fun.InputRange });
                            diagnostics.Add(diagnostic);
                            this.exceptions.Add(
                                new NameResolverInternalException($"Multiple declarations of name {id}"));
                        }
                    }
                }

                if (!this.functions.ContainsKey(id))
                {
                    this.functions.Add(id, new Stack<FunctionDeclaration>());
                }

                this.functions[id].Push(fun);
                this.AddToPeek(id, fun);
                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());
            }

            private void ProcessVariableDeclaration(VariableDeclaration var, IDiagnostics diagnostics)
            {
                string id = var.Identifier;
                var.VariableType = this.ResolveDataType(var.VariableType, diagnostics);

                if (this.variableBlocks.Peek().Contains(id))
                {
                    var diagnostic = new Diagnostic(
                        DiagnosticStatus.Error,
                        MultipleDeclarationsDiagnostic,
                        $"Multiple declarations of variable name {id}",
                        new List<Range> { var.InputRange });
                    diagnostics.Add(diagnostic);
                    this.exceptions.Add(new NameResolverInternalException($"Multiple declarations of name {id}"));
                }

                if (!this.variables.ContainsKey(id))
                {
                    this.variables.Add(id, new Stack<VariableDeclaration>());
                }

                this.variables[id].Push(var);
                this.variableBlocks.Peek().Add(id);
            }

            private void ProcessInstructionBlock()
            {
                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());
            }

            private void ProcessVariable(Variable var, IDiagnostics diagnostics)
            {
                string id = var.Identifier;
                if (!this.variables.ContainsKey(id) || this.variables[id].Count == 0)
                {
                    Diagnostic diagnostic = new Diagnostic(
                        DiagnosticStatus.Error,
                        IdentifierNotFoundDiagnostic,
                        $"No variable of name {id}",
                        new List<Range> { var.InputRange });
                    diagnostics.Add(diagnostic);
                    this.exceptions.Add(new NameResolverInternalException($"No variable of name {id}"));
                }
                else
                {
                    var.Declaration = this.variables[id].Peek();
                }
            }

            private void ProcessFunctionCall(FunctionCall functionCall, IDiagnostics diagnostics)
            {
                var identifier = functionCall.Identifier;
                if (!this.functions.ContainsKey(identifier) || this.functions[identifier].Count == 0)
                {
                    var message = $"No function of name '{identifier}'";
                    var diagnostic = new Diagnostic(
                        DiagnosticStatus.Error,
                        IdentifierNotFoundDiagnostic,
                        message,
                        new List<Range> { functionCall.InputRange });
                    diagnostics.Add(diagnostic);
                    this.exceptions.Add(new NameResolverInternalException(message));
                }
                else
                {
                    functionCall.DeclarationCandidates = this.GetDeclarationCandidates(identifier);
                }
            }

            private List<FunctionDeclaration> GetDeclarationCandidates(string identifier)
            {
                var declarationCandidates = new List<FunctionDeclaration>();
                foreach (var functionDeclaration in this.functions[identifier])
                {
                    if (!declarationCandidates.Any(
                        addedFun =>
                            FunctionDeclaration.ParametersTypesEquals(addedFun, functionDeclaration)))
                    {
                        declarationCandidates.Add(functionDeclaration);
                    }
                }

                return declarationCandidates;
            }

            private void PopBlock()
            {
                var ids = this.variableBlocks.Pop();
                foreach (var id in ids)
                {
                    this.variables[id].Pop();
                }

                var funs = this.functionBlocks.Pop();
                foreach (var fun in funs)
                {
                    for (int i = 0; i < fun.Value.Count; i++)
                    {
                        this.functions[fun.Key].Pop();
                    }
                }
            }
        }
    }
}