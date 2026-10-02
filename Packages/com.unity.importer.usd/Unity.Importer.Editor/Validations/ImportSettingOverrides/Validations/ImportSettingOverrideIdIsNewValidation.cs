using UnityEngine.Importer;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideIdIsNewValidation : INewModularImporterComponentValidation<IGraphValue>
    {
        private const string k_ErrorMessage = "An import setting override with the same id already exists in the graph.";

        public void ValidateNewComponent(IGraphValue settingOverride, ModularImporterValidationContext context)
        {
            if (settingOverride == null)
                return;

            foreach (var s in context.ImportSettingOverrides)
            {
                if (s.Id == settingOverride.Id)
                {
                    context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, k_ErrorMessage, typeof(ImportSettingOverrideIdIsNewValidation)));
                    return;
                }
            }
        }
    }
}
