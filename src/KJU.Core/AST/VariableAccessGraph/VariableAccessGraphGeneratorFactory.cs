namespace KJU.Core.AST.VariableAccessGraph
{
    using System.Collections.Generic;
    using KJU.Core.AST.CallGraph;

    public class VariableAccessGraphGeneratorFactory
    {
        public IVariableAccessGraphGenerator GetGenerator()
        {
            var callGraphGenerator = new CallGraphGenerator();
            var nodeInfoExtractors = new Dictionary<VariableInfo, INodeInfoExtractor>
            {
                [VariableInfo.Access] = new AccessInfoExtractor(),
                [VariableInfo.Modifications] = new ModifyInfoExtractor(),
            };
            return new VariableAccessGraphGenerator(callGraphGenerator, nodeInfoExtractors);
        }
    }
}