namespace KJU.Core.Intermediate.FunctionGeneration.CallGenerator
{
    using System.Collections.Generic;
    using System.Linq;
    using CallingSiblingFinder;
    using ReadWrite;

    public class CallGenerator
    {
        private readonly ILabelFactory labelFactory;
        private readonly CallingSiblingFinder callingSiblingFinder;
        private readonly ReadWriteGenerator readWriteGenerator;

        public CallGenerator(ILabelFactory labelFactory, CallingSiblingFinder callingSiblingFinder, ReadWriteGenerator readWriteGenerator)
        {
            this.labelFactory = labelFactory;
            this.callingSiblingFinder = callingSiblingFinder;
            this.readWriteGenerator = readWriteGenerator;
        }

        // We will use standard x86-64 conventions -> RDI, RSI, RDX, RCX, R8, R9.
        // TODO: instruction templates covering hw register modifications
        public ILabel GenerateCall(
            VirtualRegister result,
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
                .Append(new UsesDefinesNode(null, HardwareRegisterUtils.CallerSavedRegisters));

            var postCall = new List<Node>
            {
                new Comment("Copy function result to variable"),
                result.CopyFrom(HardwareRegister.RAX),
                new Comment("Restore RSP alignment"),
                new AlignStackPointer(-needStackOffset),
                new Comment("End of call"),
            };

            var controlFlow = new FunctionCall(function, postCall.MakeTreeChain(this.labelFactory, onReturn));
            return preCall.MakeTreeChain(this.labelFactory, controlFlow);
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
            Node readStaticLink;
            if (callerFunction == parentFunction)
            {
                readStaticLink = new RegisterRead(HardwareRegister.RBP);
            }
            else
            {
                var siblingLink = this.callingSiblingFinder.GetCallingSibling(callerFunction, parentFunction).Link;
                readStaticLink = this.readWriteGenerator.GenerateRead(
                    callerFunction,
                    siblingLink);
            }

            var values = argRegisters
                .Select(argVR => new RegisterRead(argVR))
                .Append(readStaticLink).ToList();

            return values.Skip(HardwareRegisterUtils.ArgumentRegisters.Count).Reverse()
                .Select(value => new Push(value)).Concat<Node>(values.Zip(
                    HardwareRegisterUtils.ArgumentRegisters,
                    (value, hwReg) => new RegisterWrite(hwReg, value)));
        }
    }
}