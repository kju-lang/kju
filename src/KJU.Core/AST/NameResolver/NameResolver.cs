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

            private readonly Dictionary<string, Stack<List<FunctionDeclaration>>> functions =
                new Dictionary<string, Stack<List<FunctionDeclaration>>>();

            private readonly Stack<Dictionary<string, List<FunctionDeclaration>>> functionBlocks =
                new Stack<Dictionary<string, List<FunctionDeclaration>>>();

            private readonly Dictionary<string, Stack<StructDeclaration>> structs =
                new Dictionary<string, Stack<StructDeclaration>>();

            private readonly Stack<HashSet<string>> structBlocks = new Stack<HashSet<string>>();

            private readonly List<Exception> exceptions = new List<Exception>();

            private readonly Dictionary<string, DataType> dataTypes =
                new Dictionary<string, DataType>();

            private readonly HashSet<string> buildInTypes
                = new HashSet<string>();

            public ResolveProcess()
            {
                this.dataTypes["Int"] = BuiltinTypes.IntType.Instance;
                this.dataTypes["Bool"] = BuiltinTypes.BoolType.Instance;
                this.dataTypes["Unit"] = BuiltinTypes.UnitType.Instance;
                this.buildInTypes.UnionWith(this.dataTypes.Keys);
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
                        this.ProcessProgram(program, diagnostics);
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
                    case UnApplication app:
                        this.ProcessUnApplication(app, diagnostics);
                        break;
                    case ArrayAlloc array:
                        array.ElementType = this.ResolveDataType(array.ElementType, diagnostics);
                        break;
                    case StructDeclaration structDeclaration:
                        this.ProcessStructDeclaration(structDeclaration, diagnostics);
                        break;
                    case StructAlloc structAlloc:
                        this.ProcessStructAllocation(structAlloc, diagnostics);
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
                                $"Unexpected type identifier: '{unresolvedType.Type}'",
                                new List<Range> { unresolvedType.InputRange });
                            diagnostics.Add(diag);
                            throw new NameResolverException($"unexpected type identifier: {unresolvedType.Type}");
                        }

                        return this.dataTypes[unresolvedType.Type];

                    case UnresolvedArrayType unresolvedArray:
                        var childDataType = this.ResolveDataType(unresolvedArray.Child, diagnostics);
                        return ArrayType.GetInstance(childDataType);

                    case UnresolvedFunType unresolved:
                        return new FunType(
                            argTypes: unresolved.ArgTypes.Select(x => this.ResolveDataType(x, diagnostics)).ToList(),
                            resultType: this.ResolveDataType(unresolved.ResultType, diagnostics));
                }

                return type;
            }

            private void AddToPeek(string functionName, FunctionDeclaration declaration)
            {
                if (!this.functionBlocks.Peek().ContainsKey(functionName))
                {
                    this.functionBlocks.Peek().Add(functionName, new List<FunctionDeclaration>());
                    this.functions[functionName].Push(new List<FunctionDeclaration>());
                }

                this.functionBlocks.Peek()[functionName].Add(declaration);
                this.functions[functionName].Peek().Add(declaration);
            }

            private void ProcessProgram(Program program, IDiagnostics diagnostics)
            {
                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());
                this.structBlocks.Push(new HashSet<string>());

                foreach (var fun in program.Functions)
                {
                    var id = fun.Identifier;

                    if (!this.functions.ContainsKey(id))
                    {
                        this.functions.Add(id, new Stack<List<FunctionDeclaration>>());
                    }

                    this.AddToPeek(id, fun);
                }

                foreach (var structDecl in program.Structs)
                {
                    if (this.structs.ContainsKey(structDecl.Name))
                    {
                        var diagnostic = new Diagnostic(
                            DiagnosticStatus.Error,
                            MultipleDeclarationsDiagnostic,
                            $"Multiple declarations of struct {structDecl.Name}",
                            new List<Range> { structDecl.InputRange });
                        diagnostics.Add(diagnostic);
                        this.exceptions.Add(new NameResolverInternalException($"Multiple declarations struct {structDecl.Name}"));
                    }

                    this.dataTypes[structDecl.Name] = StructType.GetInstance(structDecl);
                    this.structs[structDecl.Name] = new Stack<StructDeclaration>();
                    this.structs[structDecl.Name].Push(structDecl);
                    this.structBlocks.Peek().Add(structDecl.Name);
                }

                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());
                this.structBlocks.Push(new HashSet<string>());
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
                    this.functions.Add(id, new Stack<List<FunctionDeclaration>>());
                }

                this.AddToPeek(id, fun);
                this.functionBlocks.Push(new Dictionary<string, List<FunctionDeclaration>>());
                this.variableBlocks.Push(new HashSet<string>());
                this.structBlocks.Push(new HashSet<string>());
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
                this.structBlocks.Push(new HashSet<string>());
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

            private void ProcessUnApplication(UnApplication unapplication, IDiagnostics diagnostics)
            {
                var identifier = unapplication.FunctionName;
                if (!this.functions.ContainsKey(identifier) || this.functions[identifier].Count == 0)
                {
                    var message = $"No function of name '{identifier}'";
                    var diagnostic = new Diagnostic(
                        DiagnosticStatus.Error,
                        IdentifierNotFoundDiagnostic,
                        message,
                        new List<Range> { unapplication.InputRange });
                    diagnostics.Add(diagnostic);
                    this.exceptions.Add(new NameResolverInternalException(message));
                }
                else
                {
                    unapplication.Candidates = this.GetDeclarationCandidates(identifier);
                }
            }

            private void ProcessStructAllocation(StructAlloc structAlloc, IDiagnostics diagnostics)
            {
                if (structAlloc.AllocType is UnresolvedType unresolvedType)
                {
                    var name = unresolvedType.Type;
                    structAlloc.AllocType = this.ResolveDataType(unresolvedType, diagnostics);
                    structAlloc.Declaration = this.structs[name].Peek();
                }
            }

            private void ProcessStructDeclaration(StructDeclaration structDeclaration, IDiagnostics diagnostics)
            {
                string name = structDeclaration.Name;

                if (this.structBlocks.Peek().Contains(name))
                {
                    var diagnostic = new Diagnostic(
                        DiagnosticStatus.Error,
                        MultipleDeclarationsDiagnostic,
                        $"Multiple declarations of struct {name}",
                        new List<Range> { structDeclaration.InputRange });
                    diagnostics.Add(diagnostic);
                    this.exceptions.Add(new NameResolverInternalException($"Multiple declarations struct {name}"));
                }

                if (this.buildInTypes.Contains(name))
                {
                    var diagnostic = new Diagnostic(
                        DiagnosticStatus.Error,
                        TypeIdentifierErrorDiagnosticsType,
                        $"Cannot use builtin type name {name}",
                        new List<Range> { structDeclaration.InputRange });
                    diagnostics.Add(diagnostic);
                    this.exceptions.Add(new NameResolverInternalException($"Cannot use builtin type name {name}"));
                }

                if (!this.structs.ContainsKey(name))
                {
                    this.structs.Add(name, new Stack<StructDeclaration>());
                }

                this.structs[name].Push(structDeclaration);
                this.structBlocks.Peek().Add(name);
                this.dataTypes[name] = StructType.GetInstance(structDeclaration);

                var fieldNames = new HashSet<string>();
                foreach (var field in structDeclaration.Fields)
                {
                    var fieldName = field.Name;
                    if (fieldNames.Contains(fieldName))
                    {
                        var diagnostic = new Diagnostic(
                            DiagnosticStatus.Error,
                            MultipleDeclarationsDiagnostic,
                            $"Multiple declarations of field: {fieldName} in struct: {name}",
                            new List<Range> { structDeclaration.InputRange });
                        diagnostics.Add(diagnostic);
                        this.exceptions.Add(new NameResolverInternalException($"Multiple field declarations struct {name}"));
                    }

                    fieldNames.Add(fieldName);
                }

                foreach (var field in structDeclaration.Fields)
                {
                    field.Type = this.ResolveDataType(field.Type, diagnostics);
                }
            }

            private List<FunctionDeclaration> GetDeclarationCandidates(string identifier)
            {
                var declarationCandidates = new List<FunctionDeclaration>();
                foreach (var functionDeclaration in this.functions[identifier].Peek())
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
                    this.functions[fun.Key].Pop();
                }

                var structs = this.structBlocks.Pop();
                foreach (var structName in structs)
                {
                    this.structs[structName].Pop();
                    if (this.structs[structName].Count > 0)
                    {
                        var structDecl = this.structs[structName].Peek();
                        this.dataTypes[structName] = StructType.GetInstance(structDecl);
                    }
                    else
                    {
                        this.dataTypes.Remove(structName);
                    }
                }
            }
        }
    }
}
