using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation that highlight several conflicting graph components.
    /// </summary>
    /// <typeparam name="T">A graph component type (node, edge...)</typeparam>
    public class ConflictingComponentsValidationError<T> : BaseGraphValidationError
    {
        /// <summary>
        /// The graph components that are conflicting.
        /// </summary>
        public List<T> Components { get; }

        internal ConflictingComponentsValidationError(List<T> components, string message, Type validationType) : base(message, validationType)
        {
            Components = components;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine(Message);

            for (var i = 0; i < Components.Count - 1; i++)
            {
                strBuilder.AppendLine($"    {Components[i]}");
            }
            strBuilder.Append($"    {Components[^1]}");
            return strBuilder.ToString();
        }
    }
}
