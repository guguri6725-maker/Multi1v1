namespace UnityEngine.Importer.Validations
{
    internal class ImportSettingExistsValidation : IRemoveGraphComponentValidation<IGraphValue>
    {
        private const string k_ErrorMessage = "The import setting can't be find in the ImporterGraph.";

        public void ValidateComponentRemoval(IGraphValue setting, GraphValidationContext context)
        {
            if (!context.UniqueImportSettings.Contains(setting))
            {
                context.Errors.Add(new ImportSettingValidationError(setting, k_ErrorMessage, typeof(ImportSettingExistsValidation)));
            }
        }
    }
}
