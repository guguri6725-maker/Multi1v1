using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.Importer;
using Object = UnityEngine.Object;

namespace UnityEditor.Importer
{
    static class IGraphValueUtility
    {
        /// <summary>
        /// Creates a deep serialized copy of the <paramref name="value"/>.
        /// Use this extension method to make sure any managed reference in the <paramref name="value"/>
        /// is copied in memory and not shared anymore.
        /// </summary>
        /// <param name="value">The <see cref="IGraphValue"/> to copy.</param>
        /// <returns>The new instance of <see cref="IGraphValue"/>.</returns>
        public static IGraphValue DeepCopy(this IGraphValue value)
        {
            var settingCopy = ScriptableObject.CreateInstance<SerializedImportSetting>();
            settingCopy.value = value;
            var deepCopy = Object.Instantiate(settingCopy);
            var deepCopyValue = deepCopy.value;
            Object.DestroyImmediate(settingCopy);
            Object.DestroyImmediate(deepCopy);
            return deepCopyValue;
        }

        /// <summary>
        /// Creates a deep serialized copy of each <see cref="IGraphValue"/> inside <paramref name="values"/>.
        /// Use this extension method to make sure any managed reference in the <paramref name="values"/>
        /// is copied in memory and not shared anymore.
        /// </summary>
        /// <remarks>
        /// This method changes the IEnumeration into a <see cref="List{T}"/> for serialization performances reasons.
        /// It is possible to keep an enumeration by calling <see cref="IGraphValueUtility.DeepCopy(UnityEngine.Importer.IGraphValue)"/>
        /// on each member of the enumeration instead.
        /// </remarks>
        /// <param name="values">The <see cref="IGraphValue"/> enumeration to copy.</param>
        /// <returns>The new instances of <see cref="IGraphValue"/>.</returns>
        public static List<IGraphValue> DeepCopy(this IEnumerable<IGraphValue> values)
        {
            var settingCopy = ScriptableObject.CreateInstance<GraphValueCopy>();
            settingCopy.values = new List<IGraphValue>(values);
            var deepCopy = Object.Instantiate(settingCopy);
            var deepCopyValue = deepCopy.values;
            Object.DestroyImmediate(settingCopy);
            Object.DestroyImmediate(deepCopy);
            return deepCopyValue;
        }

        /// <summary>
        /// Convert <see cref="UnityEngine.Object"/>, UnityEngine.Object[], and List&lt;UnityEngine.Object&gt; contained in <paramref name="values"/>
        /// into their <see cref="LazyLoadReference{T}"/> equivalents.
        /// Unity Importer serialization doesn't support direct references to <see cref="UnityEngine.Object"/>
        /// and this method is used to convert <see cref="ImportSetting{T}"/> used in a graph into serializable overrides for a <see cref="ModularImporter"/>.
        /// </summary>
        /// <remarks>
        /// If an <see cref="IGraphValue"/> in the <paramref name="values"/> doesn't contain anything to convert, it will be returned without any modification.
        /// </remarks>
        /// <param name="values">An enumeration of <see cref="IGraphValue"/> to be converted from direct references to <see cref="LazyLoadReference{T}"/></param>
        /// <returns>The enumeration with direct references changed.</returns>
        public static IEnumerable<IGraphValue> ConvertObjectToLazyLoadObject(this IEnumerable<IGraphValue> values)
        {
            foreach (var value in values)
            {
                yield return value.ConvertObjectToLazyLoadObject();
            }
        }

        /// <summary>
        /// Convert <see cref="UnityEngine.Object"/>, UnityEngine.Object[], and List&lt;UnityEngine.Object&gt; contained in <paramref name="value"/>
        /// into their <see cref="LazyLoadReference{T}"/> equivalents.
        /// Unity Importer serialization doesn't support direct references to <see cref="UnityEngine.Object"/>
        /// and this method is used to convert <see cref="ImportSetting{T}"/> used in a graph into serializable overrides for a <see cref="ModularImporter"/>.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="value"/> doesn't contain anything to convert, it will be returned without any modification.
        /// </remarks>
        /// <param name="value">An <see cref="IGraphValue"/> to be converted from direct references to <see cref="LazyLoadReference{T}"/></param>
        /// <returns>The <see cref="IGraphValue"/> with direct references changed.</returns>
        public static IGraphValue ConvertObjectToLazyLoadObject(this IGraphValue value)
        {
            if (typeof(Object).IsAssignableFrom(value.Type))
            {
                return ConvertToLazyLoadFromObject(value);
            }
            if (value.Type.IsArray && typeof(Object).IsAssignableFrom(value.Type.GetElementType()))
            {
                return ConvertToLazyLoadFromObjectArray(value);
            }
            if (value.Type.IsGenericType
                && value.Type.GetGenericTypeDefinition() == typeof(List<>)
                && typeof(Object).IsAssignableFrom(value.Type.GetGenericArguments()[0]))
            {
                return ConvertToLazyLoadFromObjectList(value);
            }

            return value;
        }

        private static IGraphValue ConvertToLazyLoadFromObject(IGraphValue value)
        {
            var lazyLoadType = typeof(LazyLoadReference<>).MakeGenericType(value.Type);
            var lazyLoadObject = Activator.CreateInstance(lazyLoadType, value.Value);
            var importSettingType = typeof(ImportSetting<>).MakeGenericType(lazyLoadType);
            return Activator.CreateInstance(importSettingType, value.Id, lazyLoadObject) as IGraphValue;
        }

        private static IGraphValue ConvertToLazyLoadFromObjectArray(IGraphValue value)
        {
            var objectType = value.Type.GetElementType();
            var lazyLoadType = typeof(LazyLoadReference<>).MakeGenericType(objectType);
            var lazyLoadTypeArray = lazyLoadType.MakeArrayType();

            var valueArray = (Array)value.Value;
            var lazyLoadObjectArray = Activator.CreateInstance(lazyLoadTypeArray, valueArray.Length) as Array;
            for (int j = 0; j < lazyLoadObjectArray.Length; j++)
            {
                lazyLoadObjectArray.SetValue(Activator.CreateInstance(lazyLoadType, valueArray.GetValue(j)), j);
            }

            var importSetting = typeof(ImportSetting<>).MakeGenericType(lazyLoadTypeArray);
            return Activator.CreateInstance(importSetting, value.Id, lazyLoadObjectArray) as IGraphValue;
        }

        private static IGraphValue ConvertToLazyLoadFromObjectList(IGraphValue value)
        {
            var objectType = value.Type.GetGenericArguments()[0];
            var lazyLoadType = typeof(LazyLoadReference<>).MakeGenericType(objectType);
            var lazyLoadTypeList = typeof(List<>).MakeGenericType(lazyLoadType);

            var valueList = (IList)value.Value;
            var lazyLoadObjectList = Activator.CreateInstance(lazyLoadTypeList, valueList.Count) as IList;
            for (int j = 0; j < valueList.Count; j++)
            {
                lazyLoadObjectList.Add(Activator.CreateInstance(lazyLoadType, valueList[j]));
            }

            var importSetting = typeof(ImportSetting<>).MakeGenericType(lazyLoadTypeList);
            return Activator.CreateInstance(importSetting, value.Id, lazyLoadObjectList) as IGraphValue;
        }

        /// <summary>
        /// Convert <see cref="LazyLoadReference{T}"/>, LazyLoadReference[], and List&lt;LazyLoadReference&gt; contained in <paramref name="values"/>
        /// into their <see cref="UnityEngine.Object"/> equivalents.
        /// Unity Importer serialization doesn't support direct references to <see cref="UnityEngine.Object"/>
        /// and this method is used to convert <see cref="ImportSetting{T}"/> overrides saved in a <see cref="ModularImporter"/>
        /// into direct object references used in an <see cref="ImporterGraph"/>.
        /// </summary>
        /// <remarks>
        /// If an <see cref="IGraphValue"/> in the <paramref name="values"/> doesn't contain anything to convert, it will be returned without any modification.
        /// </remarks>
        /// <param name="values">An enumeration of <see cref="IGraphValue"/> to be converted from <see cref="LazyLoadReference{T}"/> to direct references.</param>
        /// <returns>The enumeration with <see cref="LazyLoadReference{T}"/> changed.</returns>
        public static List<IGraphValue> ConvertLazyLoadObjectToObject(this IEnumerable<IGraphValue> values, out HashSet<string> overrideIds, out HashSet<Object> instanceIds)
        {
            var convertedValues = new List<IGraphValue>();
            overrideIds = new HashSet<string>();
            instanceIds = new HashSet<Object>();
            foreach (var value in values)
            {
                overrideIds.Add(value.Id);
                if (value.Type.IsGenericType && value.Type.GetGenericTypeDefinition() == typeof(LazyLoadReference<>))
                {
                    convertedValues.Add(ConvertToObjectFromLazyLoad(value, instanceIds));
                }
                else if (value.Type.IsArray
                         && value.Type.GetElementType().IsGenericType
                         && value.Type.GetElementType().GetGenericTypeDefinition() == typeof(LazyLoadReference<>))
                {
                    convertedValues.Add(ConvertToObjectFromLazyLoadArray(value, instanceIds));
                }
                else if (value.Type.IsGenericType
                         && value.Type.GetGenericTypeDefinition() == typeof(List<>)
                         && value.Type.GetGenericArguments()[0].IsGenericType
                         && value.Type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(LazyLoadReference<>))
                {
                    convertedValues.Add(ConvertToObjectFromLazyLoadList(value, instanceIds));
                }
                else
                {
                    convertedValues.Add(value);
                }
            }

            return convertedValues;
        }

        private static IGraphValue ConvertToObjectFromLazyLoad(IGraphValue value, HashSet<Object> instanceIds)
        {
            var lazyLoadAssetGetter = value.Type.GetProperty("asset");
            var objectValue = lazyLoadAssetGetter.GetValue(value.Value);

            var objectType = value.Type.GetGenericArguments()[0];
            var importSettingType = typeof(ImportSetting<>).MakeGenericType(objectType);

            instanceIds.Add((Object)objectValue);

            return Activator.CreateInstance(importSettingType, value.Id, objectValue) as IGraphValue;
        }

        private static IGraphValue ConvertToObjectFromLazyLoadArray(IGraphValue value, HashSet<Object> instanceIds)
        {
            var lazyLoadAssetGetter = value.Type.GetElementType().GetProperty("asset");

            var valueArray = (Array)value.Value;
            var objectType = value.Type.GetElementType().GetGenericArguments()[0];
            var objectTypeArray = objectType.MakeArrayType();
            var objectArray = Activator.CreateInstance(objectTypeArray, valueArray.Length) as Array;
            for (int j = 0; j < objectArray.Length; j++)
            {
                var objectValue = lazyLoadAssetGetter.GetValue(valueArray.GetValue(j));
                objectArray.SetValue(objectValue, j);

                instanceIds.Add((Object)objectValue);
            }

            var importSetting = typeof(ImportSetting<>).MakeGenericType(objectTypeArray);
            return Activator.CreateInstance(importSetting, value.Id, objectArray) as IGraphValue;
        }

        private static IGraphValue ConvertToObjectFromLazyLoadList(IGraphValue value, HashSet<Object> instanceIds)
        {
            var lazyLoadAssetGetter = value.Type.GetGenericArguments()[0].GetProperty("asset");

            var valueList = (IList)value.Value;
            var objectType = value.Type.GetGenericArguments()[0].GetGenericArguments()[0];
            var objectTypeList = typeof(List<>).MakeGenericType(objectType);
            var objectList = Activator.CreateInstance(objectTypeList, valueList.Count) as IList;
            for (int j = 0; j < valueList.Count; j++)
            {
                var objectValue = lazyLoadAssetGetter.GetValue(valueList[j]);
                objectList.Add(objectValue);
                instanceIds.Add((Object)objectValue);
            }

            var importSetting = typeof(ImportSetting<>).MakeGenericType(objectTypeList);
            return Activator.CreateInstance(importSetting, value.Id, objectList) as IGraphValue;
        }

        public static void DependsOnArtifact(this AssetImportContext ctx, Object asset)
        {
            // Unity 6.6: keep object references instead of truncated legacy integer IDs.
            if (asset != null)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _))
                {
                    if (GUID.TryParse(guid, out var guidguid))
                    {
                        ctx.DependsOnArtifact(guidguid);
                        AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guidguid));
                    }
                }
            }
        }
    }
}
