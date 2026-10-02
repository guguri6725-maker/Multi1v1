namespace UnityEngine.Importer
{
    /// <summary>
    /// Interface implemented by edges that have a nodePort as the origin (e.g <see cref="Edge"/>, <see cref="ResultEdge"/>).
    /// </summary>
    public interface IOriginEdge
    {
        /// <summary>
        /// The <see cref="NodePort"/> origin of the edge.
        /// </summary>
        NodePort Origin { get; }
    }
}
