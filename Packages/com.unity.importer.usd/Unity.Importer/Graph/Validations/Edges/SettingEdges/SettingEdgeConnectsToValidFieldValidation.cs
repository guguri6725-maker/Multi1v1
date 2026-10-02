namespace UnityEngine.Importer.Validations
{
    internal class SettingEdgeConnectsToValidFieldValidation : IGraphValidation, INewGraphComponentValidation<SettingEdge>
    {
        public void Validate(GraphValidationContext context)
        {
            foreach (var edge in context.UniqueSettingEdges)
            {
                ValidateSettingEdge(edge, context);
            }
        }

        public void ValidateNewComponent(SettingEdge edge, GraphValidationContext context)
        {
            ValidateSettingEdge(edge, context);
        }

        private void ValidateSettingEdge(SettingEdge edge, GraphValidationContext context)
        {
            if (edge.Destination.Node == null)
                return;

            var destinationField = edge.Destination.ToFieldInfo();
            if (destinationField == null)
            {
                context.Errors.Add(new SettingEdgeValidationError(edge,
                    $"Could not find Input field '{edge.Destination.FieldName}' on NodePorts type '{edge.Destination.Node}'.",
                    typeof(SettingEdgeConnectsToValidFieldValidation)));
            }

            IGraphValue importSetting = null;
            foreach (var setting in context.UniqueImportSettings)
            {
                if (setting.Id == edge.Id)
                {
                    importSetting = setting;
                    break;
                }
            }

            if (destinationField != null && importSetting != null && !destinationField.FieldType.IsAssignableFrom(importSetting.Type))
            {
                var message = $"Types of import setting and destination field are different. setting : '{importSetting.Type}' Destination : '{destinationField.FieldType.Name}'";
                context.Errors.Add(new SettingEdgeValidationError(edge, message, typeof(SettingEdgeConnectsToValidFieldValidation)));
            }
        }
    }
}
