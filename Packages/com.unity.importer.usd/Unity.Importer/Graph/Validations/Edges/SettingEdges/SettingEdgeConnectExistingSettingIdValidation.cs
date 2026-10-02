namespace UnityEngine.Importer.Validations
{
    internal class SettingEdgeConnectExistingSettingIdValidation : INewGraphComponentValidation<SettingEdge>
    {
        public void ValidateNewComponent(SettingEdge edge, GraphValidationContext context)
        {
            foreach (var setting in context.UniqueImportSettings)
            {
                if (setting.Id == edge.Id)
                {
                    return;
                }
            }
            context.Errors.Add(new SettingEdgeValidationError(edge, $"Could not find an import setting with id '{edge.Id}'", typeof(SettingEdgeConnectExistingSettingIdValidation)));
        }
    }
}
