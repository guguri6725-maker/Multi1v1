using UnityEngine.Importer;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideIdExistsValidation : IModularIImporterValidation, INewModularImporterComponentValidation<IGraphValue>
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
            if (settingOverride == null)
                return;

            if (!context.IdToImportSettings.ContainsKey(settingOverride.Id))
            {
                var message = $"Could not find import setting id '{settingOverride.Id}' in the graph selected for this importer.";
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, message, typeof(ImportSettingOverrideIdExistsValidation)));
            }
        }
    }
}
