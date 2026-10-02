namespace UnityEngine.Importer.Validations
{
    internal class ImportSettingHasValidValueTypeValidation : IGraphValidation, INewGraphComponentValidation<IGraphValue>
    {
        public void ValidateNewComponent(IGraphValue setting, GraphValidationContext context)
        {
            ValidateSetting(setting, context);
        }

        public void Validate(GraphValidationContext context)
        {
            foreach (var setting in context.graph.ImportSettings)
            {
                ValidateSetting(setting, context);
            }
        }

        void ValidateSetting(IGraphValue setting, GraphValidationContext context)
        {
            if (setting == null || setting.Value == null)
                return;

            if (!setting.Type.IsInstanceOfType(setting.Value))
            {
                var message = $"Setting override value is not of the right type (actual '{setting.Value.GetType()}', expected '{setting.Type}').";
                context.Errors.Add(new ImportSettingValidationError(setting, message, typeof(ImportSettingHasValidValueTypeValidation)));
            }
        }
    }
}
