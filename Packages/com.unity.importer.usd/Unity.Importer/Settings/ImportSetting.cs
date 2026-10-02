using System;
using System.Collections.Generic;

namespace UnityEngine.Importer
{
    /// <summary>
    /// Represents a setting in the <see cref="ImporterGraph"/> that contains both a setting <see cref="Id"/> and it's <see cref="Value"/>.
    /// </summary>
    /// <remarks>
    /// Settings can be added and removed from the <see cref="ImporterGraph"/> using <see cref="ImporterGraph.AddImportSetting"/> and <see cref="ImporterGraph.RemoveImportSetting"/>.
    /// <code>
    /// using UnityEngine.Importer;
    /// using UnityEngine;
    ///
    /// public static class ExampleClass
    /// {
    ///     public static void AddNewSettingToAGraph()
    ///     {
    ///         var graph = ScriptableObject.CreateInstance&lt;ImporterGraph&gt;();
    ///         var newSetting = new ImportSetting&lt;int&gt;("MyIntSetting", 5);
    ///         graph.AddImportSetting(newSetting);
    ///     }
    /// }
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The setting's type.</typeparam>
    [Serializable]
    public class ImportSetting<T> : IGraphValue, IEquatable<ImportSetting<T>>
    {
        [SerializeField] private string id;
        [SerializeField] private T value;

        /// <summary>
        /// Creates a new instance of an <see cref="ImportSetting{T}"/>.
        /// </summary>
        /// <param name="id">The id of the setting.</param>
        /// <param name="value">The default value of the setting.</param>
        public ImportSetting(string id, T value)
        {
            this.id = id;
            this.value = value;
        }

        /// <summary>
        /// The id of the setting.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// The default value of the setting.
        /// </summary>
        public T Value => value;

        /// <inheritdoc cref="IGraphValue.Value"/>
        object IGraphValue.Value => value;

        /// <summary>
        /// The Type of the setting.
        /// </summary>
        public Type Type => typeof(T);

        /// <inheritdoc cref="object.Equals(object)"/>
        public bool Equals(ImportSetting<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return id == other.id && EqualityComparer<T>.Default.Equals(value, other.value);
        }

        /// <inheritdoc cref="object.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ImportSetting<T>)obj);
        }

        /// <inheritdoc cref="object.GetHashCode"/>
        public override int GetHashCode()
        {
            return HashCode.Combine(id, value);
        }

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return $"{id} : {value}";
        }
    }
}
