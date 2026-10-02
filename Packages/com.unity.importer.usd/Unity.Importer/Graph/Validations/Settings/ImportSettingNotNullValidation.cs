namespace UnityEngine.Importer.Validations
{
    internal class ImportSettingNotNullValidation : INewGraphComponentValidation<IGraphValue>
    {
        private const string k_ErrorMessage = "The import setting is null.";

        public void ValidateNewComponent(IGraphValue setting, GraphValidationContext context)
        {
            if (setting == null)
            {
                context.Errors.Add(new ImportSettingValidationError(setting, k_ErrorMessage, typeof(ImportSettingNotNullValidation)));
            }
        }
    }
}
