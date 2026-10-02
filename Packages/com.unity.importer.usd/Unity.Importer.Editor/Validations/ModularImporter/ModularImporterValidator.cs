using System.Collections.Generic;

namespace UnityEditor.Importer.Validation
{
    internal class ModularImporterValidator
    {
        private readonly IReadOnlyList<IModularIImporterValidation> ImportSettingOverrideValidations = new IModularIImporterValidation[]
        {
            new ImportSettingOverrideHasCompatibleValueTypeValidation(),
            new ImportSettingOverrideNotNullValidation(),
            new ImportSettingOverrideHasValidValueTypeValidation(),
            new ImportSettingOverrideDontHaveDuplicatedIdValidation(),
            new ImportSettingOverrideLazyLoadReferenceValidation()
        };

        internal ModularImporterValidationResult ValidateImporter(ModularImporter importer)
        {
            var context = new ModularImporterValidationContext(importer);

            foreach (var validation in ImportSettingOverrideValidations)
            {
                validation.Validate(context);
            }

            return new ModularImporterValidationResult(context, "The Importer validation generated the following errors :");
        }
    }
}
