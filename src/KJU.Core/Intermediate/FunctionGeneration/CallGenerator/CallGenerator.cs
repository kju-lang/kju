namespace KJU.Core.Intermediate.FunctionGeneration.CallGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using AST.Types;
    using CallingSiblingFinder;
    using ReadWrite;

    public class CallGenerator
    {
        private readonly ILabelFactory labelFactory;
        private readonly CallingSiblingFinder callingSiblingFinder;
        private readonly ReadWriteGenerator readWriteGenerator;

        public CallGenerator(
            ILabelFactory labelFactory,
            CallingSiblingFinder callingSiblingFinder,
            ReadWriteGenerator readWriteGenerator)
        {
            this.labelFactory = labelFactory;
            this.callingSiblingFinder = callingSiblingFinder;
            this.readWriteGenerator = readWriteGenerator;
        }

        public (Node readLink, StructType linkType) GetClosureForFunction(Function.Function callerFunction, Function.Function parentFunction)
        {
            if (callerFunction == parentFunction)
            {
                return (this.readWriteGenerator.GenerateRead(callerFunction, callerFunction.ClosurePointer), parentFunction.ClosureType);
            }
            else
            {
                var sibling = this.callingSiblingFinder.GetCallingSibling(callerFunction, parentFunction);
                return (this.readWriteGenerator.GenerateRead(callerFunction, sibling.Link), parentFunction.ClosureType);
            }
        }

        // We will use standard x86-64 conventions -> RDI, RSI, RDX, RCX, R8, R9.
        // TODO: instruction templates covering hw register modifications
        public ILabel GenerateCall(
            ILocation result,
            IEnumerable<VirtualRegister> callArguments,
            ILabel onReturn,
            Function.Function callerFunction,
            Function.Function function)
        {
            var needStackOffset = function.GetStackArgumentsCount() % 2 == 1 ? 8 : 0;
            var preCall = new List<Node>
                {
                    new AlignStackPointer(needStackOffset)
                }
                .Append(new Comment("Pass arguments"))
                .Concat(this.PassArguments(callerFunction, callArguments, function.Parent))
                .Append(new ClearDF())
                .Append(new Comment($"Call {function.MangledName}"))
                .Append(
                    new UsesDefinesNode(
                        HardwareRegisterUtils.ArgumentRegisters.Take(callArguments.Count() + 1).ToList(),
                        HardwareRegisterUtils.CallerSavedRegisters));

            var postCall = new List<Node>
            {
                new Comment("Copy function result to variable"),
                this.readWriteGenerator.GenerateWrite(callerFunction, result, new RegisterRead(HardwareRegister.RAX)),
                new Comment("Restore RSP alignment"),
                new AlignStackPointer(-(needStackOffset + (8 * function.GetStackArgumentsCount()))),
                new Comment("End of call"),
            };

            var controlFlow = new FunctionCall(function, postCall.MakeTreeChain(this.labelFactory, onReturn));
            return preCall.MakeTreeChain(this.labelFactory, controlFlow);
        }

        public ILabel GenerateClosureCall(
            ILocation result,
            IEnumerable<VirtualRegister> callArguments,
            ILabel onReturn,
            Function.Function callerFunction,
            FunType funType,
            VirtualRegister funPtr)
        {
            var needStackOffset = funType.GetStackArgumentsCount() % 2 == 1 ? 8 : 0;
            var funPtrRead = new RegisterRead(funPtr);
            var funCodePtr = new MemoryRead(
                new ArithmeticBinaryOperation(AST.ArithmeticOperationType.Addition, funPtrRead, new IntegerImmediateValue(0)));
            var funClosurePtr = new MemoryRead(
                new ArithmeticBinaryOperation(AST.ArithmeticOperationType.Addition, funPtrRead, new IntegerImmediateValue(16)));
            var preCall = new List<Node>
                {
                    new AlignStackPointer(needStackOffset)
                }
                .Append(new Comment("Pass arguments"))
                .Concat(this.PassClosureArguments(callArguments, funClosurePtr))
                .Append(new ClearDF())
                .Append(new Comment($"Call closure function"))
                .Append(
                    new UsesDefinesNode(
                        HardwareRegisterUtils.ArgumentRegisters.Take(callArguments.Count() + 1).ToList(),
                        HardwareRegisterUtils.CallerSavedRegisters))
                .Append(new RegisterWrite(HardwareRegister.RAX, funCodePtr));

            var postCall = new List<Node>
            {
                new Comment("Copy function result to variable"),
                this.readWriteGenerator.GenerateWrite(callerFunction, result, new RegisterRead(HardwareRegister.RAX)),
                new Comment("Restore RSP alignment"),
                new AlignStackPointer(-(needStackOffset + (8 * funType.GetStackArgumentsCount()))),
                new Comment("End of call"),
            };

            var controlFlow = new ComputedFunctionCall(postCall.MakeTreeChain(this.labelFactory, onReturn));
            return preCall.MakeTreeChain(this.labelFactory, controlFlow);
        }

        private IEnumerable<Node> PassClosureArguments(
            IEnumerable<VirtualRegister> argRegisters, Node closure)
        {
            var values = argRegisters.Select(argVR => (Node)new RegisterRead(argVR)).ToList();
            values.Add(closure);
            var result = values.Skip(HardwareRegisterUtils.ArgumentRegisters.Count).Reverse().Select(value => (Node)new Push(value)).ToList();
            result.AddRange(values.Zip(HardwareRegisterUtils.ArgumentRegisters, (value, hwReg) => new RegisterWrite(hwReg, value)));
            return result;
        }

        /*
                    Argument position on wrt. stack frame (if needed):
                           |             ...            |
                           | (i+7)th argument           | rbp + 16 + 8i
                           |             ...            |
                           | 7th argument               | rbp + 16
                           | return stack pointer value | rbp + 8
                    rbp -> | previous rbp value         |
                    Static link is the last argument, either in register or on stack.
            */
        private IEnumerable<Node> PassArguments(
            Function.Function callerFunction,
            IEnumerable<VirtualRegister> argRegisters,
            Function.Function parentFunction)
        {
            Node readStaticLink = null;
            if (parentFunction != null)
                readStaticLink = this.GetClosureForFunction(callerFunction, parentFunction).readLink;

            var values = argRegisters
                .Select(argVR => (Node)new RegisterRead(argVR)).ToList();

            if (parentFunction != null)
                values.Add(readStaticLink);

            var result = values.Skip(HardwareRegisterUtils.ArgumentRegisters.Count).Reverse().Select(value => (Node)new Push(value)).ToList();
            result.AddRange(values.Zip(HardwareRegisterUtils.ArgumentRegisters, (value, hwReg) => new RegisterWrite(hwReg, value)));
            return result;
        }
    }
}
