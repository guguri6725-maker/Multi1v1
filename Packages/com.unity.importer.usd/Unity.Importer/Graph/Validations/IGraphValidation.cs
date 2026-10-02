namespace UnityEngine.Importer.Validations
{
    internal interface IGraphValidation
    {
        void Validate(GraphValidationContext context);
    }

    internal interface INewGraphComponentValidation<in T>
    {
        void ValidateNewComponent(T component, GraphValidationContext context);
    }

    internal interface IRemoveGraphComponentValidation<in T>
    {
        void ValidateComponentRemoval(T component, GraphValidationContext context);
    }
}
