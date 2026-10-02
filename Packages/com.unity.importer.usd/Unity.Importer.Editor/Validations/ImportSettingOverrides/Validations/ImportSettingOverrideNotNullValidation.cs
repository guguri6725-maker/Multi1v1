using UnityEngine.Importer;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideNotNullValidation : IModularIImporterValidation, INewModularImporterComponentValidation<IGraphValue>
    {
        private const string k_ErrorMessage = "The import setting override can't be null.";

        public void Validate(ModularImporterValidationContext context)
        {
            foreach (var settingOverride in context.ImportSettingOverrides)
            {
                ValidateSettingOverride(settingOverride, context);
            }
        }

        public void ValidateNewComponent(IGraphValue settingOverride, ModularImporterValidationContext context)
        {
            ValidateSettingOverride(settingOverride, context);
        }

        private void ValidateSettingOverride(IGraphValue settingOverride, ModularImporterValidationContext context)
        {
            if (settingOverride == null)
            {
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, k_ErrorMessage, typeof(ImportSettingOverrideNotNullValidation)));
            }
        }
    }
}
