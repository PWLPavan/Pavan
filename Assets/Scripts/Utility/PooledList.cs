using System;
using System.Collections.Generic;

namespace FGUnity.Utils
{
    /// <summary>
    /// Pooled version of a List.
    /// </summary>
    public class PooledList<T> : List<T>, IDisposable
    {
        private PooledList()
        {
        }

        private void Reset()
        {
            Clear();
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
        static private Pool<PooledList<T>> s_ObjectPool = new Pool<PooledList<T>>(POOL_SIZE, poolConstructor);

        /// <summary>
        /// Retrieves a PooledStringBuilder for use.
        /// </summary>
        static public PooledList<T> Create()
        {
            return s_ObjectPool.Pop();
        }

        // Creates a PooledStringBuilder for the pool.
        static private PooledList<T> poolConstructor(Pool<PooledList<T>> inPool)
        {
            return new PooledList<T>();
        }

        #endregion
    }
}
