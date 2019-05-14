namespace KJU.Core.Intermediate.FunctionGeneration.CallingSiblingFinder
{
    using FunctionGenerator;

    public class CallingSiblingFinder
    {
        public Function.Function GetCallingSibling(Function.Function caller, Function.Function parent)
        {
            var result = caller;
            while (result.Parent != parent)
            {
                result = result.Parent;
            }

            return result;
        }
    }
}