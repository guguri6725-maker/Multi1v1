using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Importer;

namespace UnityEditor.Importer
{
    /// <summary>
    /// This class is used to duplicate SerializeReferences in managed memory
    /// so that the Inspector doesn't edit either the ImporterGraph values
    /// or the ModularImporter values directly.
    /// </summary>
    /// <seealso cref="IGraphValueUtility.DeepCopy(System.Collections.Generic.List<UnityEngine.Importer.IGraphValue>)"/>
    class GraphValueCopy : ScriptableObject
    {
        [SerializeReference] public List<IGraphValue> values;
    }
}
