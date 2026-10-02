using System;
using UnityEngine.Importer;
using UnityEngine.Importer.Validations;

namespace UnityEditor.Importer.Validation
{
    /// <summary>
    /// An error created during a <see cref="ModularImporter"/> validation or a new import setting override addition/removal to the <see cref="ModularImporter"/>.
    /// </summary>
    public class ImportSettingOverrideValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="ImportSetting{T}"/>.
        /// </summary>
        public IGraphValue ImportSettingOverride { get; }

        internal ImportSettingOverrideValidationError(IGraphValue settingOverride, string message, Type validationType) : base(message, validationType)
        {
            ImportSettingOverride = settingOverride;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            var settingString = ImportSettingOverride != null
                ? $"id:{ImportSettingOverride.Id} value:{ImportSettingOverride.Value} type:{ImportSettingOverride.Type}"
                : "null-ImportSettingOverride";
            return $"Setting Override {settingString} : {Message}";
        }
    }
}
