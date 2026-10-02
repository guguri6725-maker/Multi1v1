using System;
using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class MainResultValidator : BaseGraphComponentValidator<ResultEdge>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<ResultEdge>> AddValidations =>
            new List<INewGraphComponentValidation<ResultEdge>>
        {
            new ResultEdgeIsPresentValidation(),
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<ResultEdge>> RemoveValidations =>
            throw new InvalidOperationException("The main Result Edges cannot be removed, only set.");

        protected override string GetAdditionErrorHeader(ResultEdge edge) =>
            $"Could not set main ResultEdge '{edge}' due to the following errors :";

        protected override string GetRemovalErrorHeader(ResultEdge edge) =>
            throw new InvalidOperationException("The main Result Edges cannot be removed, only set.");
    }
}
