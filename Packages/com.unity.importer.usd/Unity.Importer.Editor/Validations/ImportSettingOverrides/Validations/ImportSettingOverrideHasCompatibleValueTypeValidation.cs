using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;
using Object = UnityEngine.Object;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideHasCompatibleValueTypeValidation : IModularIImporterValidation, INewModularImporterComponentValidation<IGraphValue>
    {
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
            if (settingOverride == null || !context.IdToImportSettings.TryGetValue(settingOverride.Id, out var setting) || setting == null)
                return;

            //if type are compatible, just return
            if (setting.Type.IsAssignableFrom(settingOverride.Type))
                return;

            var settingOverrideType = settingOverride.Type;
            var settingType = setting.Type;

            //if setting is T with T:Object and override is U, ensure that U is LazyLoadReference<T>
            if (typeof(Object).IsAssignableFrom(settingType))
            {
                if (!IsLazyLoadRefOfSettingType(settingOverrideType, settingType))
                {
                    var message = $"Setting override type '{settingOverrideType}' is invalid. Allowed type for this setting is 'LazyLoadReference<{settingType}>'.";
                    context.Errors.Add(new ImportSettingOverrideValidationError(setting, message, typeof(ImportSettingOverrideHasCompatibleValueTypeValidation)));
                }
            }
            //else if setting is T[] with T:Object and override is U, ensure that U is an array of LazyLoadReference<T>
            else if (settingType.IsArray)
            {
                var elementType = settingType.GetElementType();
                if (typeof(Object).IsAssignableFrom(elementType) && !IsArrayOfLazyLoadRefOfSettingType(settingOverrideType, elementType))
                {
                    var message = $"Setting override type '{settingOverrideType}' is invalid. Allowed type for this setting is 'LazyLoadReference<{elementType}>[]'.";
                    context.Errors.Add(new ImportSettingOverrideValidationError(setting, message, typeof(ImportSettingOverrideHasCompatibleValueTypeValidation)));
                }
            }
            //else if setting is List<T> with T:Object and override is U, ensure that U is a list of LazyLoadReference<T>
            else if (settingType.IsGenericType && settingType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = settingType.GetGenericArguments()[0];
                if (typeof(Object).IsAssignableFrom(elementType)
                    && !IsListOfLazyLoadRefOfSettingType(settingOverrideType, elementType))
                {
                    var message = $"Setting override type '{settingOverrideType}' is invalid. Allowed type for this setting is'List<LazyLoadReference<{elementType}>>'.";
                    context.Errors.Add(new ImportSettingOverrideValidationError(setting, message, typeof(ImportSettingOverrideHasCompatibleValueTypeValidation)));
                }
            }
            //else our types are not compatible at all
            else
            {
                var message = $"Setting override type '{settingOverrideType}' is incompatible with setting type '{settingType}'.";
                context.Errors.Add(new ImportSettingOverrideValidationError(setting, message, typeof(ImportSettingOverrideHasCompatibleValueTypeValidation)));
            }
        }

        private bool IsLazyLoadRefOfSettingType(Type settingOverrideType, Type settingType)
        {
            return settingOverrideType.IsGenericType
                && settingOverrideType.GetGenericTypeDefinition() == typeof(LazyLoadReference<>)
                && settingType.IsAssignableFrom(settingOverrideType.GetGenericArguments()[0]);
        }

        private bool IsArrayOfLazyLoadRefOfSettingType(Type settingOverrideType, Type settingType)
        {
            return settingOverrideType.IsArray && IsLazyLoadRefOfSettingType(settingOverrideType.GetElementType(), settingType);
        }

        private bool IsListOfLazyLoadRefOfSettingType(Type settingOverrideType, Type settingType)
        {
            return settingOverrideType.IsGenericType
                && settingOverrideType.GetGenericTypeDefinition() == typeof(List<>)
                && IsLazyLoadRefOfSettingType(settingOverrideType.GetGenericArguments()[0], settingType);
        }
    }
}
