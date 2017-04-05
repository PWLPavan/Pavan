using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using FGUnity.Utils;

namespace FGUnity.Utils
{
    /// <summary>
    /// Reference to a smart coroutine.
    /// Protects against stale references interfering
    /// with CoroutineInstance's pooling.
    /// </summary>
    public struct CoroutineHandle : IEquatable<CoroutineHandle>
    {
        public CoroutineHandle(uint inValue)
        {
            m_Value = inValue;
        }

        private uint m_Value;
        public uint Value { get { return m_Value; } }

        static public readonly CoroutineHandle Null = new CoroutineHandle(0);

        public bool Equals(CoroutineHandle other)
        {
            return Value == other.Value;
        }

        static public bool operator ==(CoroutineHandle first, CoroutineHandle second)
        {
            return first.Equals(second);
        }

        static public bool operator !=(CoroutineHandle first, CoroutineHandle second)
        {
            return !first.Equals(second);
        }

        static public implicit operator bool(CoroutineHandle inHandle)
        {
            return inHandle.Value != 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is CoroutineHandle)
                return Equals((CoroutineHandle)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        /// <summary>
        /// Pauses the execution of the routine.
        /// </summary>
        public void Pause()
        {
            var instance = CoroutineInstance.Find(this);
            if (instance != null)
                instance.Pause();
        }

        /// <summary>
        /// Resumes the execution of the routine.
        /// </summary>
        public void Resume()
        {
            var instance = CoroutineInstance.Find(this);
            if (instance != null)
                instance.Resume();
        }

        /// <summary>
        /// Stops the routine.
        /// </summary>
        public void Stop()
        {
            var instance = CoroutineInstance.Find(this);
            if (instance != null)
                instance.Stop();
        }

        /// <summary>
        /// Stops the routine and clears out my reference.
        /// </summary>
        public void Clear()
        {
            if (m_Value != 0)
            {
                Stop();
                m_Value = 0;
            }
        }

        /// <summary>
        /// Waits for the routine to finish
        /// or be cancelled.
        /// </summary>
        public IEnumerator Wait()
        {
            var instance = CoroutineInstance.Find(this);
            if (instance != null)
                return instance.Wait();
            return null;
        }

        /// <summary>
        /// Returns if the routine is still running.
        /// </summary>
        public bool IsRunning()
        {
            return m_Value != 0 && CoroutineInstance.Find(this) != null;
        }
    }
}