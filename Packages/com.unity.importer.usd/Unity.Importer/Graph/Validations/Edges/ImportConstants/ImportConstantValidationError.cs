using System;

namespace UnityEngine.Importer.Validations
{
    /// <summary>
    /// An error created during a graph validation or a new constant addition to a graph.
    /// </summary>
    public class ImportConstantValidationError : BaseGraphValidationError
    {
        /// <summary>
        /// The invalid <see cref="ImportConstant{T}"/>.
        /// </summary>
        public IConstantValue ImportConstant { get; }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"ImportConstant {(ImportConstant != null ? ImportConstant : "Null-ImportConstant")} : {Message}";
        }

        internal ImportConstantValidationError(IConstantValue importConstant, string error, Type type)
            : base(error, type)
        {
            ImportConstant = importConstant;
        }
    }
}
