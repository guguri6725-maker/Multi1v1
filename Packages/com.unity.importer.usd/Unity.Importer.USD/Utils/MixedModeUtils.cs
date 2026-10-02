using System;

namespace Unity.Importer.USD
{
    /// <summary>
    /// Structure that guarantees the held object will survive until the the structure is disposed
    /// </summary>
    struct ReferenceHolder<T> : IDisposable where T : class
    {
        T m_HeldObject;

        internal ReferenceHolder(T objectToHold)
        {
            m_HeldObject = objectToHold;
        }

        /// <summary>
        /// See documentation for &lt;IDisposable.Dispose&gt; for more details
        /// </summary>
        public void Dispose()
        {
            GC.KeepAlive(m_HeldObject);
            m_HeldObject = null;
        }
    }
}
