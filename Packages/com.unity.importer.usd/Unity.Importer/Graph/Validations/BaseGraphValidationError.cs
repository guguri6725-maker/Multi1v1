using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// Represents an error that occured during the validation of a graph or one of its components.
    /// </summary>
    public abstract class BaseGraphValidationError
    {
        /// <summary>
        /// Verbose message detailing the error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The validation that generated the error.
        /// </summary>
        internal Type ValidationType { get; }

        /// <summary>
        /// Represents an error generated during a graph or graph component validation.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="validationType">The validation that generated the error.</param>
        internal BaseGraphValidationError(string message, Type validationType)
        {
            Message = message;
            ValidationType = validationType;
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return Message;
        }
    }
}
