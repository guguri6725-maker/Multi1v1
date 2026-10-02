/// <summary>
/// The periodicity of the curve.
/// </summary>
public enum CurveWrap
{
    /// <summary>
    /// Undefined.
    /// </summary>
    Undefined,
    /// <summary>
    /// A regular, non-repeating curve.
    /// </summary>
    NonPeriodic,
    /// <summary>
    /// A curve where (4 - v-step) vertices are repeated at the end.
    /// </summary>
    Periodic,
    /// <summary>
    /// A non-periodic curve where a phantom vertex is inserted at the start and the end of the curve to 'pin' its starting and end point.
    /// </summary>
    Pinned
}
