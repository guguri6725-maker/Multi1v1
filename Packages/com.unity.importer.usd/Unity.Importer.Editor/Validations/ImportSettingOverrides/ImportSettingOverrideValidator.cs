using System.Collections.Generic;
using UnityEngine.Importer;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideValidator : BaseModularImporterComponentValidator<IGraphValue>
    {
        protected override IReadOnlyList<INewModularImporterComponentValidation<IGraphValue>> AddValidations => new INewModularImporterComponentValidation<IGraphValue>[]
        {
            new ImportSettingOverrideHasCompatibleValueTypeValidation(),
            new ImportSettingOverrideIdExistsValidation(),
            new ImportSettingOverrideNotNullValidation(),
            new ImportSettingOverrideLazyLoadReferenceValidation(),
            new ImportSettingOverrideHasValidValueTypeValidation(),
            new ImportSettingOverrideIdIsNewValidation()
        };

        protected override IReadOnlyList<IRemoveModularImporterComponentValidation<IGraphValue>> RemoveValidations => new IRemoveModularImporterComponentValidation<IGraphValue>[]
        {
            new ImportSettingOverrideExistsValidation()
        };

        protected override string GetAdditionErrorHeader(IGraphValue component)
        {
            var settingString = component != null ? $"{component.Id}.{component.Type}" : "Null-Setting";
            return $"Could not add setting override '{settingString}' due to the following errors :";
        }

        protected override string GetRemovalErrorHeader(IGraphValue component)
        {
            var settingString = component != null ? $"{component.Id}.{component.Type}" : "Null-Setting";
            return $"Could not remove setting override '{settingString}' due to the following errors :";
        }
    }
}
