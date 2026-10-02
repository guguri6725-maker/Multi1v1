using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;
using Object = UnityEngine.Object;

namespace UnityEditor.Importer
{
    /// <summary>
    /// This class is used to duplicate SerializeReferences in managed memory
    /// so that the Inspector doesn't edit either the ImporterGraph values
    /// or the ModularImporter values directly.
    /// It is also used to compare two IGraphValue instances at their serialization level.
    /// </summary>
    /// <seealso cref="IGraphValueUtility.DeepCopy(UnityEngine.Importer.IGraphValue)"/>
    /// <seealso cref="ImportSettingComparer"/>
    class SerializedImportSetting : ScriptableObject
    {
        [SerializeReference] public IGraphValue value;
    }

    /// <summary>
    /// Intent: We cannot compare the pure content of IGraphValue, if the .Type is an Object type,
    /// it'll always compare the instance and return false even if the contents are the same.
    /// Getting through the Unity serialization is making sure we are comparing the exact serialized values.
    /// </summary>
    internal class ImportSettingComparer : IEqualityComparer<IGraphValue>, IDisposable
    {
        private SerializedImportSetting x;
        private SerializedImportSetting y;
        private bool disposed = false;

        public ImportSettingComparer()
        {
            x = ScriptableObject.CreateInstance<SerializedImportSetting>();
            y = ScriptableObject.CreateInstance<SerializedImportSetting>();
        }

        ~ImportSettingComparer()
        {
            if (!disposed)
                Dispose();
        }

        public bool Equals(IGraphValue x, IGraphValue y)
        {
            this.x.value = x;
            this.y.value = y;
            using var soX = new SerializedObject(this.x);
            using var soY = new SerializedObject(this.y);
            using var propX = soX.FindProperty("value");
            using var propY = soY.FindProperty("value");
            return SerializedProperty.DataEquals(propX, propY);
        }

        public int GetHashCode(IGraphValue obj)
        {
            return 0;
        }

        public void Dispose()
        {
            disposed = true;
            Object.DestroyImmediate(x);
            Object.DestroyImmediate(y);
        }
    }
}
