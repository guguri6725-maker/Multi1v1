using System.Collections.Generic;

namespace UnityEditor.Importer.Validation
{
    internal abstract class BaseModularImporterComponentValidator<T>
    {
        protected abstract IReadOnlyList<INewModularImporterComponentValidation<T>> AddValidations { get; }

        protected abstract IReadOnlyList<IRemoveModularImporterComponentValidation<T>> RemoveValidations { get; }

        protected abstract string GetAdditionErrorHeader(T component);

        protected abstract string GetRemovalErrorHeader(T component);

        internal ModularImporterValidationResult ValidateAddition(T component, ModularImporter importer)
        {
            var context = new ModularImporterValidationContext(importer);

            foreach (var validation in AddValidations)
            {
                validation.ValidateNewComponent(component, context);
            }

            return new ModularImporterValidationResult(context, GetAdditionErrorHeader(component));
        }

        internal ModularImporterValidationResult ValidateRemoval(T component, ModularImporter importer)
        {
            var context = new ModularImporterValidationContext(importer);

            foreach (var validation in RemoveValidations)
            {
                validation.ValidateComponentRemoval(component, context);
            }

            return new ModularImporterValidationResult(context, GetRemovalErrorHeader(component));
        }
    }
}
