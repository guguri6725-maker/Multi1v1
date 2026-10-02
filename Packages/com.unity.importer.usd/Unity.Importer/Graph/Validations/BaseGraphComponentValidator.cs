using System.Collections.Generic;

namespace UnityEngine.Importer.Validations
{
    internal abstract class BaseGraphComponentValidator<T>
    {
        protected abstract IReadOnlyList<INewGraphComponentValidation<T>> AddValidations { get; }

        protected abstract IReadOnlyList<IRemoveGraphComponentValidation<T>> RemoveValidations { get; }

        protected abstract string GetAdditionErrorHeader(T component);

        protected abstract string GetRemovalErrorHeader(T component);

        internal GraphValidationResult ValidateAddition(T component, ImporterGraph graph)
        {
            var context = new GraphValidationContext(graph);

            foreach (var validation in AddValidations)
            {
                validation.ValidateNewComponent(component, context);
            }

            return new GraphValidationResult(context, GetAdditionErrorHeader(component));
        }

        internal GraphValidationResult ValidateRemoval(T component, ImporterGraph graph)
        {
            var context = new GraphValidationContext(graph);

            foreach (var validation in RemoveValidations)
            {
                validation.ValidateComponentRemoval(component, context);
            }

            return new GraphValidationResult(context, GetRemovalErrorHeader(component));
        }
    }
}
