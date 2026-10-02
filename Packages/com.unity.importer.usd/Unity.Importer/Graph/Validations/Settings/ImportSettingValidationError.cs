using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation or a new import setting addition to the <see cref="ImporterGraph"/>.
    /// </summary>
    public class ImportSettingValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="ImportSetting{T}"/>.
        /// </summary>
        public IGraphValue ImportSetting { get; }

        internal ImportSettingValidationError(IGraphValue setting, string message, Type validationType) : base(message, validationType)
        {
            ImportSetting = setting;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"ImportSetting {(ImportSetting != null ? ImportSetting : "Null-ImportSetting")} : {Message}";
        }
    }
}
