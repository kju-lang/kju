namespace KJU.Core.AST
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KJU.Core.Diagnostics;
    using KJU.Core.Lexer;

    public class NameResolver : INameResolver
    {
        private const string ErrorString = "LinkFail";
        private readonly Dictionary<string, Stack<Node>> declarations;
        private readonly Stack<HashSet<string>> blocks;

        public NameResolver()
        {
            this.declarations = new Dictionary<string, Stack<Node>>();
            this.blocks = new Stack<HashSet<string>>();
        }

        public void LinkNames(Node root, IDiagnostics diagnostics)
        {
            this.Process(root, diagnostics);
        }

        private void ProcessProgram(Program prog, IDiagnostics diagnostics)
        {
            this.blocks.Push(new HashSet<string>());
            foreach (FunctionDeclaration fun in prog.Functions)
            {
                string id = fun.Identifier;
                if (this.blocks.Peek().Contains(id))
                {
                    Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, "Multiple declarations of name " + id, new List<Range> { fun.InputRange });
                    diagnostics.Add(diagnostic);
                }

                if (!this.declarations.ContainsKey(id))
                {
                    this.declarations.Add(id, new Stack<Node>());
                }

                this.declarations[id].Push(fun);
                this.blocks.Peek().Add(id);
            }

            this.blocks.Push(new HashSet<string>());
        }

        private void ProcessFunctionDeclaration(FunctionDeclaration fun, IDiagnostics diagnostics)
        {
            string id = fun.Identifier;
            if (this.blocks.Peek().Contains(id))
            {
                Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, $"Multiple declarations of name {id}", new List<Range> { fun.InputRange });
                diagnostics.Add(diagnostic);
            }

            if (!this.declarations.ContainsKey(id))
            {
                this.declarations.Add(id, new Stack<Node>());
            }

            this.declarations[id].Push(fun);
            this.blocks.Peek().Add(id);
            this.blocks.Push(new HashSet<string>());
        }

        private void ProcessVariableDeclaration(VariableDeclaration var, IDiagnostics diagnostics)
        {
            string id = var.Identifier;
            if (this.blocks.Peek().Contains(id))
            {
                Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, $"Multiple declarations of name {id}", new List<Range> { var.InputRange });
                diagnostics.Add(diagnostic);
            }

            if (!this.declarations.ContainsKey(id))
            {
                this.declarations.Add(id, new Stack<Node>());
            }

            this.declarations[id].Push(var);
            this.blocks.Peek().Add(id);
        }

        private void ProcessInstructionBlock(InstructionBlock block, IDiagnostics diagnostics)
        {
            this.blocks.Push(new HashSet<string>());
        }

        private void ProcessVariable(Variable var, IDiagnostics diagnostics)
        {
            string id = var.Identifier;
            if (!this.declarations.ContainsKey(id))
            {
                Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, $"No identifier of name {id}", new List<Range> { var.InputRange });
                diagnostics.Add(diagnostic);
            }
            else
            {
                if (this.declarations[id].Peek() is VariableDeclaration variableDeclaration)
                {
                    var.Declaration = variableDeclaration;
                }
                else
                {
                    Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, $"{id} is not a variable", new List<Range> { var.InputRange });
                    diagnostics.Add(diagnostic);
                }
            }
        }

        private void ProcessFunctionCall(FunctionCall fun, IDiagnostics diagnostics)
        {
            string id = fun.Function;
            if (!this.declarations.ContainsKey(id))
            {
                Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, $"No identifier of name {id}", new List<Range> { fun.InputRange });
                diagnostics.Add(diagnostic);
            }
            else
            {
                if (this.declarations[id].Peek() is FunctionDeclaration functionDeclaration)
                {
                    fun.Declaration = functionDeclaration;
                }
                else
                {
                    Diagnostic diagnostic = new Diagnostic(DiagnosticStatus.Error, ErrorString, $"{id} is not a function", new List<Range> { fun.InputRange });
                    diagnostics.Add(diagnostic);
                }
            }
        }

        private void PopBlock()
        {
            HashSet<string> ids = this.blocks.Pop();
            foreach (string id in ids)
            {
                this.declarations[id].Pop();
            }
        }

        private void Process(Node node, IDiagnostics diagnostics)
        {
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
                case InstructionBlock block:
                    this.ProcessInstructionBlock(block, diagnostics);
                    break;
                case Variable var:
                    this.ProcessVariable(var, diagnostics);
                    break;
                case FunctionCall fun:
                    this.ProcessFunctionCall(fun, diagnostics);
                    break;
            }

            foreach (Node child in node.Children())
            {
                this.Process(child, diagnostics);
            }

            switch (node)
            {
                case Program program:
                    this.PopBlock();
                    this.PopBlock();
                    break;
                case FunctionDeclaration fun:
                    this.PopBlock();
                    break;
                case InstructionBlock block:
                    this.PopBlock();
                    break;
            }
        }
    }
}
