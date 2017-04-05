using System;

namespace FGUnity.Utils
{
    /// <summary>
    /// A static object pool.
    /// </summary>
    /// <typeparam name="T">Type of object to pool.</typeparam>
    public sealed class Pool<T> where T : class
    {
        // Array of all stored objects.
        // More lightweight than using Stack<T>
        private T[] m_ObjectPool;

        // Current index in the pool
        private int m_CurrentIndex;

        // Function to call if a new object needs to be constructed.
        private Func<Pool<T>, T> m_Constructor;

        /// <summary>
        /// Total capacity of the pool.
        /// </summary>
        public readonly int Capacity;

        public Pool(int inCapacity, Func<Pool<T>, T> inConstructor)
        {
            Assert.True(inCapacity > 0, "Capacity is valid.", "Pool capacity of {0} is invalid.", inCapacity);
            Assert.True(inConstructor != null, "Constructor is not null.", "Cannot use null constructor function for pool.");
            Assert.True(!typeof(T).IsAbstract, "Type is constructable.", "Cannot create pool of abstract class.");

            Capacity = inCapacity;
            m_Constructor = inConstructor;

            m_ObjectPool = new T[Capacity];
            m_CurrentIndex = 0;

            Reset();
        }

        /// <summary>
        /// Resets the pool back to maximum capacity.
        /// </summary>
        public void Reset()
        {
            while(m_CurrentIndex < Capacity)
            {
                T newObject = m_Constructor(this);
                VerifyObject(newObject);

                m_ObjectPool[m_CurrentIndex++] = newObject;
            }
        }

        /// <summary>
        /// Retrieves the next usable object.
        /// </summary>
        public T Pop()
        {
            T pooledObject;

            if (m_CurrentIndex > 0)
            {
                // We don't have to verify this, since it's already part of the pool.
                pooledObject = m_ObjectPool[--m_CurrentIndex];
                m_ObjectPool[m_CurrentIndex] = null;
            }
            else
            {
                pooledObject = m_Constructor(this);
                VerifyObject(pooledObject);
            }

            return pooledObject;
        }

        /// <summary>
        /// Puts an object back into the pool.
        /// </summary>
        public void Push(T inObject)
        {
            VerifyObject(inObject);

            if (m_CurrentIndex < Capacity)
            {
                m_ObjectPool[m_CurrentIndex++] = inObject;
            }
        }

        [System.Diagnostics.Conditional("DEVELOPMENT")]
        private void VerifyObject(T inObject)
        {
            // We need to make sure pools aren't storing null objects or objects of derived types.
            // This is to ensure we're providing the program with consistent resources.
            Assert.True(inObject != null, "Pooled object is not null.", "Null object provided for pool.");
            Assert.True(inObject.GetType() == typeof(T), "Pooled object is of correct type.", "Cannot provide derived type for pool.");
        }
    }
}
