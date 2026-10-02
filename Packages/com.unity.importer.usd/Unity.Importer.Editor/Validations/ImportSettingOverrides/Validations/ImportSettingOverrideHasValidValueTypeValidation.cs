using UnityEngine.Importer;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideHasValidValueTypeValidation : IModularIImporterValidation, INewModularImporterComponentValidation<IGraphValue>
    {
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
            if (settingOverride == null || settingOverride.Value == null)
                return;

            if (!settingOverride.Type.IsInstanceOfType(settingOverride.Value))
            {
                var message = $"Setting override value is not of the right type (actual '{settingOverride.Value.GetType()}', expected '{settingOverride.Type}').";
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, message, typeof(ImportSettingOverrideHasValidValueTypeValidation)));
            }
        }
    }
}
