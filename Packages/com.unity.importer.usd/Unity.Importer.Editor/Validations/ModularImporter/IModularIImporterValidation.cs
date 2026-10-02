namespace UnityEditor.Importer.Validation
{
    internal interface IModularIImporterValidation
    {
        void Validate(ModularImporterValidationContext context);
    }

    internal interface INewModularImporterComponentValidation<in T>
    {
        void ValidateNewComponent(T component, ModularImporterValidationContext context);
    }

    internal interface IRemoveModularImporterComponentValidation<in T>
    {
        void ValidateComponentRemoval(T component, ModularImporterValidationContext context);
    }
}
