using System.Collections.Generic;
using UnityEngine.Importer;
using UnityEngine.Importer.Validations;

namespace UnityEditor.Importer.Validation
{
    internal class ImportSettingOverrideDontHaveDuplicatedIdValidation : IModularIImporterValidation
    {
        private const string k_ErrorMessage = "Multiple Import Settings override are using the same setting Id '{0}'.";

        public void Validate(ModularImporterValidationContext context)
        {
            var allIds = new Dictionary<string, List<IGraphValue>>();
            foreach (var setting in context.ImportSettingOverrides)
            {
                if (setting == null)
                    continue;

                if (!allIds.TryGetValue(setting.Id, out var values))
                {
                    values = new List<IGraphValue>();
                    allIds.Add(setting.Id, values);
                }
                values.Add(setting);
            }

            foreach (var id in allIds)
            {
                if (id.Value.Count > 1)
                {
                    context.Errors.Add(new ConflictingComponentsValidationError<IGraphValue>(id.Value, string.Format(k_ErrorMessage, id.Key), typeof(ImportSettingOverrideDontHaveDuplicatedIdValidation)));
                }
            }
        }
    }
}
