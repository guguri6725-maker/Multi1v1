using System.Collections.Generic;
using UnityEngine.Importer;
using Object = UnityEngine.Object;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideLazyLoadReferenceValidation : IModularIImporterValidation, INewModularImporterComponentValidation<IGraphValue>
    {
        private const string k_ObjectErrorMessage = "Due to deferred asset loading during import, setting override can't be of type '{0}'. Use a {1} instead.";

        public void Validate(ModularImporterValidationContext context)
        {
            foreach (var settingOverride in context.ImportSettingOverrides)
            {
                ValidateSettingOverride(settingOverride, context);
            }
        }

        public void ValidateNewComponent(IGraphValue settingOverride, ModularImporterValidationContext context)
        {
            ValidateSettingOverride(settingOverride, context);
        }

        private void ValidateSettingOverride(IGraphValue settingOverride, ModularImporterValidationContext context)
        {
            if (settingOverride?.Value == null)
                return;

            if (typeof(Object).IsAssignableFrom(settingOverride.Type))
            {
                var message = string.Format(k_ObjectErrorMessage, settingOverride.Type, $"LazyLoadReference<{settingOverride.Type}>");
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, message, typeof(ImportSettingOverrideLazyLoadReferenceValidation)));
            }
            else if (settingOverride.Type.IsArray && typeof(Object).IsAssignableFrom(settingOverride.Type.GetElementType()))
            {
                var message = string.Format(k_ObjectErrorMessage, settingOverride.Type, $"LazyLoadReference<{settingOverride.Type.GetElementType()}>[]");
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, message, typeof(ImportSettingOverrideLazyLoadReferenceValidation)));
            }
            else if (settingOverride.Type.IsGenericType
                     && settingOverride.Type.GetGenericTypeDefinition() == typeof(List<>)
                     && typeof(Object).IsAssignableFrom(settingOverride.Type.GetGenericArguments()[0]))
            {
                var message = string.Format(k_ObjectErrorMessage, settingOverride.Type, $"List<LazyLoadReference<{settingOverride.Type.GetGenericArguments()[0]}>>");
                context.Errors.Add(new ImportSettingOverrideValidationError(settingOverride, message, typeof(ImportSettingOverrideLazyLoadReferenceValidation)));
            }
        }
    }
}
