using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal class ImportSettingValidator : BaseGraphComponentValidator<IGraphValue>
    {
        protected override IReadOnlyList<INewGraphComponentValidation<IGraphValue>> AddValidations => new List<INewGraphComponentValidation<IGraphValue>>
        {
            new ImportSettingIdIsNewValidation(),
            new ImportSettingNotNullValidation(),
            new ImportSettingHasValidValueTypeValidation()
        };

        protected override IReadOnlyList<IRemoveGraphComponentValidation<IGraphValue>> RemoveValidations => new List<IRemoveGraphComponentValidation<IGraphValue>>
        {
            new ImportSettingExistsValidation()
        };

        protected override string GetAdditionErrorHeader(IGraphValue setting) =>
            $"Could not add ImportSetting '{(setting != null ? setting : "null")}' to graph due to the following errors :";

        protected override string GetRemovalErrorHeader(IGraphValue setting) =>
            $"Could not remove ImportSetting '{(setting != null ? setting : "null")}' due to the following errors :";
    }
}
