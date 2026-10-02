using System.Linq;
using UnityEngine.Importer;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideExistsValidation : IRemoveModularImporterComponentValidation<IGraphValue>
    {
        private const string k_ErrorMessage = "Import setting override not found on this importer.";

        public void ValidateComponentRemoval(IGraphValue settingOverride, ModularImporterValidationContext context)
        {
            if (!context.ImportSettingOverrides.Contains(settingOverride))
            {
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, k_ErrorMessage, typeof(ImportSettingOverrideExistsValidation)));
            }
        }
    }
}
