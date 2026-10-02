using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Importer.Validations;

namespace UnityEditor.Importer.Validation
{
    /// <summary>
    /// The results of a <see cref="ModularImporter"/> validation.
    /// </summary>
    /// <remarks>
    /// Contains a list of all errors generated during the validation.
    /// </remarks>
    public class ModularImporterValidationResult
    {
        /// <summary>
        /// The errors generated during the validation of a <see cref="ModularImporter"/>.
        /// </summary>
        public IReadOnlyList<BaseGraphValidationError> Errors { get; }

        /// <summary>
        /// Overall result of the validation.
        /// </summary>
        public bool IsValid => Errors.Count == 0;

        private readonly string m_ErrorHeader;

        internal ModularImporterValidationResult(ModularImporterValidationContext context, string header = null)
        {
            Errors = context.Errors;
            m_ErrorHeader = header;
        }

        /// <summary>
        /// Display a readable list of validation error in the Unity console.
        /// </summary>
        public void DisplayErrors()
        {
            Debug.LogError(ToString());
        }

        /// <summary>
        /// Get a string representing the errors of this validation.
        /// </summary>
        /// <returns>A string representing the errors of this validation</returns>
        public override string ToString()
        {
            var strBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(m_ErrorHeader))
            {
                strBuilder.Append($"{m_ErrorHeader}\n");
            }

            foreach (var error in Errors)
            {
                strBuilder.Append($"- {error}\n");
            }

            return strBuilder.ToString();
        }
    }
}
