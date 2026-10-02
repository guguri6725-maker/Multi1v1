using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation or a new setting edge addition/removal on a graph.
    /// </summary>
    public class SettingEdgeValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="SettingEdge"/>.
        /// </summary>
        public SettingEdge SettingEdge { get; }

        internal SettingEdgeValidationError(SettingEdge SettingEdge, string message, Type validationType) : base(message, validationType)
        {
            this.SettingEdge = SettingEdge;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"SettingEdge {SettingEdge} : {Message}";
        }
    }
}
