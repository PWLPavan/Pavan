using System;
using System.Text;

namespace FGUnity.Utils
{
    /// <summary>
    /// Pooled version of a StringBuilder.
    /// </summary>
    public class PooledStringBuilder : IDisposable
    {
        private PooledStringBuilder()
        {
            Builder = new StringBuilder(256);
        }

        /// <summary>
        /// The internal StringBuilder object.
        /// </summary>
        public readonly StringBuilder Builder;

        private void Reset()
        {
            Builder.Length = 0;
            Builder.EnsureCapacity(256);
        }

        /// <summary>
        /// Resets and recycles the builder to the pool.
        /// </summary>
        public void Dispose()
        {
            Reset();
            s_ObjectPool.Push(this);
        }

        #region Pool

        // Maximum number to hold in pool at a time.
        private const int POOL_SIZE = 8;

        // Object pool to hold available StringBuilders.
        static private Pool<PooledStringBuilder> s_ObjectPool = new Pool<PooledStringBuilder>(POOL_SIZE, poolConstructor);

        /// <summary>
        /// Retrieves a PooledStringBuilder for use.
        /// </summary>
        static public PooledStringBuilder Create()
        {
            return s_ObjectPool.Pop();
        }

        // Creates a PooledStringBuilder for the pool.
        static private PooledStringBuilder poolConstructor(Pool<PooledStringBuilder> inPool)
        {
            return new PooledStringBuilder();
        }

        #endregion
    }
}
