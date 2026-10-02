using System.Reflection;

namespace UnityEngine.Importer.Validations
{
    internal class EdgeConnectsValidFieldsValidation : IGraphValidation, INewGraphComponentValidation<Edge>
    {
        public void Validate(GraphValidationContext context)
        {
            foreach (var edge in context.graph.Edges)
            {
                ValidateEdge(edge, context);
            }
        }

        public void ValidateNewComponent(Edge edge, GraphValidationContext context)
        {
            ValidateEdge(edge, context);
        }

        private void ValidateEdge(Edge edge, GraphValidationContext context)
        {
            FieldInfo originField = null;
            if (edge.Origin.Node != null)
            {
                originField = edge.Origin.ToFieldInfo();
                if (originField == null)
                {
                    context.Errors.Add(new EdgeValidationError(edge,
                        $"Could not find Output field '{edge.Origin.FieldName}' on OutputPorts type '{edge.Origin.Node}'.",
                        typeof(EdgeConnectsValidFieldsValidation)));
                }
            }

            FieldInfo destinationField = null;
            if (edge.Destination.Node != null)
            {
                destinationField = edge.Destination.ToFieldInfo();
                if (destinationField == null)
                {
                    context.Errors.Add(new EdgeValidationError(edge,
                        $"Could not find Input field '{edge.Destination.FieldName}' on InputPorts type '{edge.Destination.Node}'.",
                        typeof(EdgeConnectsValidFieldsValidation)));
                }
            }

            if (originField != null && destinationField != null && !destinationField.FieldType.IsAssignableFrom(originField.FieldType))
            {
                var message = $"Types of origin and destination field are different. Origin : '{originField.FieldType.Name}' Destination : '{destinationField.FieldType.Name}'";
                context.Errors.Add(new EdgeValidationError(edge, message, typeof(EdgeConnectsValidFieldsValidation)));
            }
        }
    }
}
