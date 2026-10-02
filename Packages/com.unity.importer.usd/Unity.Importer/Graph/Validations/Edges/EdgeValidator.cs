using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class EdgeValidator : BaseGraphComponentValidator<Edge>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<Edge>> AddValidations => new List<INewGraphComponentValidation<Edge>>
        {
            new EdgeIsUniqueValidation(),
            new EdgeConnectsExistingNodesValidation(),
            new EdgeConnectsValidFieldsValidation(),
            new EdgeConnectedToFreePortValidation<Edge>(),
            new EdgeNotCreatingCycleValidation()
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<Edge>> RemoveValidations => new List<IRemoveGraphComponentValidation<Edge>>
        {
            new EdgePresentInGraphValidation()
        };

        protected override string GetAdditionErrorHeader(Edge edge) =>
            $"Could not add Edge '{edge}' due to the following errors :";

        protected override string GetRemovalErrorHeader(Edge edge) =>
            $"Could not remove Edge '{edge}' due to the following errors :";
    }
}
