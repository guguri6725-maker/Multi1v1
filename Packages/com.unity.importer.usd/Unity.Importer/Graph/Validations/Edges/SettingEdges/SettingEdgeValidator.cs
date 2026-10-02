using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class SettingEdgeValidator : BaseGraphComponentValidator<SettingEdge>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<SettingEdge>> AddValidations => new List<INewGraphComponentValidation<SettingEdge>>
        {
            new SettingEdgeIsUniqueValidation(),
            new SettingEdgeConnectsToExistingNodeValidation(),
            new SettingEdgeConnectsToValidFieldValidation(),
            new EdgeConnectedToFreePortValidation<SettingEdge>(),
            new SettingEdgeConnectExistingSettingIdValidation()
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<SettingEdge>> RemoveValidations => new List<IRemoveGraphComponentValidation<SettingEdge>>
        {
            new SettingEdgePresentInGraphValidation()
        };

        protected override string GetAdditionErrorHeader(SettingEdge SettingEdge) =>
            $"Could not add SettingEdge '{SettingEdge}' due to the following errors :";

        protected override string GetRemovalErrorHeader(SettingEdge SettingEdge) =>
            $"Could not remove SettingEdge '{SettingEdge}' due to the following errors :";
    }
}
