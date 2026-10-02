namespace UnityEngine.Importer.Validations
{
    internal class ImportSettingIdIsNewValidation : INewGraphComponentValidation<IGraphValue>
    {
        private const string k_ErrorMessage = "An import setting with the same id already exists in the graph.";

        public void ValidateNewComponent(IGraphValue setting, GraphValidationContext context)
        {
            if (setting == null)
                return;

            foreach (var s in context.UniqueImportSettings)
            {
                if (s.Id == setting.Id)
                {
                    context.Errors.Add(new ImportSettingValidationError(setting, k_ErrorMessage, typeof(ImportSettingIdIsNewValidation)));
                    return;
                }
            }
        }
    }
}
