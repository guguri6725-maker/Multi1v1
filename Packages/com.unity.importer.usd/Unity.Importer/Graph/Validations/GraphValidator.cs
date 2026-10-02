using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class GraphValidator
    {
        private readonly IReadOnlyList<IGraphValidation> GraphValidations = new List<IGraphValidation>
        {
            // edges validators
            new EdgeConnectsExistingNodesValidation(),
            new EdgeConnectsValidFieldsValidation(),
            new SingleIncomingEdgeForInputPortValidation(),
            new EdgeIsUniqueValidation(),
            // nodes validators
            new NodeIsOfValidTypeValidation(),
            new NodesHaveUniqueResultEdgeIdValidation(),
            new NodeIsUniqueValidation(),
            // ResultEdges validators
            new ResultEdgeExistingOriginValidation(),
            new ResultEdgeOriginFieldValidation(),
            new ResultEdgeIsUniqueValidation(),
            // SettingEdges validators
            new SettingEdgeIsUniqueValidation(),
            new SettingEdgeConnectsToExistingNodeValidation(),
            new SettingEdgeConnectsToValidFieldValidation(),
            // graph validators
            new GraphHasNoCycleValidation(),
            // import settings validators
            new ImportSettingsDontHaveDuplicateIdValidation(),
            new ImportSettingHasValidValueTypeValidation(),
            // constant validators
            new ImportConstantIsUniqueValidation(),
            new ImportConstantConnectsExistingNodeValidation(),
            new ImportConstantConnectsValidFieldValidation(),
            new ImportConstantIsDeclaredTypeValidation(),
        };

        public GraphValidationResult ValidateGraph(ImporterGraph graph)
        {
            var context = new GraphValidationContext(graph);

            foreach (var validation in GraphValidations)
            {
                validation.Validate(context);
            }

            return new GraphValidationResult(context, "Graph validation generated the following errors :");
        }
    }
}
