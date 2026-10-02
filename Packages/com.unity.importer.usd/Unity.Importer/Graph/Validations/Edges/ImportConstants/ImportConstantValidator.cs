using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class ImportConstantValidator : BaseGraphComponentValidator<IConstantValue>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<IConstantValue>> AddValidations => new List<INewGraphComponentValidation<IConstantValue>>
        {
            new ImportConstantIsUniqueValidation(),
            new ImportConstantIsNotNullValidation(),
            new ImportConstantConnectsExistingNodeValidation(),
            new ImportConstantConnectsValidFieldValidation(),
            new EdgeConnectedToFreePortValidation<IConstantValue>(),
            new ImportConstantIsDeclaredTypeValidation(),
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<IConstantValue>> RemoveValidations => new List<IRemoveGraphComponentValidation<IConstantValue>>
        {
            new ImportConstantPresentInGraphValidation(),
            new ImportConstantIsNotNullValidation(),
        };

        protected override string GetAdditionErrorHeader(IConstantValue constant) =>
            $"Could not add ImportConstant '{constant}' due to the following errors :";

        protected override string GetRemovalErrorHeader(IConstantValue constant) =>
            $"Could not remove ImportConstant '{constant}' due to the following errors :";
    }
}
