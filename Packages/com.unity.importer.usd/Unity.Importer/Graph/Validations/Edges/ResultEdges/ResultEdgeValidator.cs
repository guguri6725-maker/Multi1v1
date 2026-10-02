using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class ResultEdgeValidator : BaseGraphComponentValidator<ResultEdge>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<ResultEdge>> AddValidations =>
            new List<INewGraphComponentValidation<ResultEdge>>
        {
            new ResultEdgeExistingOriginValidation(),
            new ResultEdgeOriginFieldValidation(),
            new ResultEdgeHasUniqueIdValidation(),
            new ResultEdgeIsUniqueValidation()
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<ResultEdge>> RemoveValidations =>
            new List<IRemoveGraphComponentValidation<ResultEdge>>()
        {
            new ResultEdgeIsPresentValidation(),
        };

        protected override string GetAdditionErrorHeader(ResultEdge edge) =>
            $"Could not add ResultEdge '{edge}' due to the following errors :";

        protected override string GetRemovalErrorHeader(ResultEdge edge) =>
            $"Could not remove ResultEdge '{edge}' due to the following errors :";
    }
}
