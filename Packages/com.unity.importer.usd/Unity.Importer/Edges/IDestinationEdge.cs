namespace UnityEngine.Importer
{
    /// <summary>
    /// Interface implemented by edges that have a nodePort as the destination (e.g <see cref="Edge"/>, <see cref="SettingEdge"/>).
    /// </summary>
    public interface IDestinationEdge
    {
        /// <summary>
        /// The <see cref="NodePort"/> destination of the edge.
        /// </summary>
        NodePort Destination { get; }
    }
}
