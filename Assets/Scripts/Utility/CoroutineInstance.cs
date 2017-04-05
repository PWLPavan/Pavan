using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FGUnity.Utils;

namespace FGUnity.Utils
{
    /// <summary>
    /// Custom coroutine implementation.
    /// </summary>
    public class CoroutineInstance
    {
        private CoroutineHandle m_Handle;
        private MonoBehaviour m_Host;
        private Stack<IEnumerator> m_Stack = new Stack<IEnumerator>();
        private bool m_Paused = false;
        private float m_WaitTime = 0.0f;
        private bool m_Disposing = false;
        private Coroutine m_UnityWait = null;

        private CoroutineHandle Start(uint inIndex, MonoBehaviour inHost, IEnumerator inStartingPoint)
        {
            m_Handle = new CoroutineHandle(inIndex);
            m_Host = inHost;
            if (m_Host == null)
            {
                m_Host = s_RoutineHost;
                Assert.True(m_Host != null, "Host has been specified.");
            }
            ClearStack();
            m_Stack.Push(inStartingPoint);
            m_Paused = false;
            m_Disposing = false;
            m_UnityWait = null;
            m_WaitTime = 0.0f;

            s_CurrentlyRunning.Add(this);

            return m_Handle;
        }

        /// <summary>
        /// Pauses the execution of the routine.
        /// </summary>
        public void Pause()
        {
            m_Paused = true;
        }

        /// <summary>
        /// Resumes the execution of the routine.
        /// </summary>
        public void Resume()
        {
            m_Paused = false;
        }

        /// <summary>
        /// Stops the routine and clears it.
        /// </summary>
        public void Stop()
        {
            m_Disposing = true;
        }

        /// <summary>
        /// Waits until the given coroutine is finished.
        /// </summary>
        /// <returns></returns>
        public IEnumerator Wait()
        {
            CoroutineHandle currentHandle = m_Handle;
            while (currentHandle == m_Handle)
                yield return null;
        }

        private void Dispose()
        {
            if (m_Handle)
            {
                m_Host = null;
                m_Paused = false;

                ClearStack();

                if (m_UnityWait != null)
                {
                    s_RoutineHost.StopCoroutine(m_UnityWait);
                    m_UnityWait = null;
                }

                s_Pool.Push(this);
                s_CurrentlyRunning.Remove(this);

                m_Handle = CoroutineHandle.Null;
            }
        }

        private void Update()
        {
            if (m_Stack.Count == 0 || !m_Host)
            {
                m_Disposing = true;
                return;
            }

            if (m_Paused || !m_Host.isActiveAndEnabled || m_UnityWait != null)
                return;

            if (m_WaitTime > 0)
            {
                m_WaitTime -= Time.deltaTime;
                return;
            }

            IEnumerator current = m_Stack.Peek();
            bool bMovedNext = current.MoveNext();

            if (m_Disposing)
                return;

            if (bMovedNext)
            {
                if (current.Current != null)
                {
                    if (current.Current is float || current.Current is int)
                    {
                        m_WaitTime = (float)current.Current;
                    }
                    else if (current.Current is IEnumerator)
                    {
                        m_Stack.Push((IEnumerator)current.Current);
                    }
                    else if (current.Current is CoroutineHandle)
                    {
                        CoroutineHandle handle = (CoroutineHandle)current.Current;
                        IEnumerator waitSequence = handle.Wait();
                        if (waitSequence != null)
                            m_Stack.Push(waitSequence);
                    }
                    else if (current.Current is YieldInstruction)
                    {
                        m_UnityWait = s_RoutineHost.StartCoroutine(UnityWait((YieldInstruction)current.Current));
                    }
                }
            }
            else
            {
                IEnumerator enumerator = m_Stack.Pop();
                ((IDisposable)enumerator).Dispose();
            }
        }

        private void ClearStack()
        {
            IEnumerator enumerator;
            while (m_Stack.Count > 0)
            {
                enumerator = m_Stack.Pop();
                ((IDisposable)enumerator).Dispose();
            }
        }

        #region Processing Unity Coroutines

        private IEnumerator UnityWait(YieldInstruction inYieldInstruction)
        {
            yield return inYieldInstruction;
            m_UnityWait = null;
        }

        #endregion

        /// <summary>
        /// Runs a coroutine from the given host.
        /// </summary>
        static public CoroutineHandle Run(MonoBehaviour inHost, IEnumerator inRoutine)
        {
            CreateHost();

            CoroutineInstance instance = s_Pool.Pop();
            CoroutineHandle handle = instance.Start(s_CurrentIndex, inHost, inRoutine);
            if (s_CurrentIndex == uint.MaxValue)
                s_CurrentIndex = 1;
            else
                ++s_CurrentIndex;
            return handle;
        }

        /// <summary>
        /// Returns the running CoroutineInstance with the given handle.
        /// </summary>
        static public CoroutineInstance Find(CoroutineHandle inHandle)
        {
            if (inHandle)
            {
                foreach (CoroutineInstance instance in s_CurrentlyRunning)
                    if (instance.m_Handle == inHandle)
                        return instance;
            }

            return null;
        }

        static private HashSet<CoroutineInstance> s_CurrentlyRunning = new HashSet<CoroutineInstance>();
        static private uint s_CurrentIndex = 1;

        #region Pooling

        static private Pool<CoroutineInstance> s_Pool = new Pool<CoroutineInstance>(64, constructor);
        static private CoroutineInstance constructor(Pool<CoroutineInstance> inPool)
        {
            return new CoroutineInstance();
        }

        #endregion

        #region Host

        static CoroutineInstance()
        {
            CreateHost();
        }

        static private void CreateHost()
        {
            if (s_RoutineHost == null)
            {
                s_RoutineHost = new GameObject("CoroutineHost").AddComponent<CoroutineHost>();
                s_RoutineHost.hideFlags = s_RoutineHost.gameObject.hideFlags = HideFlags.HideAndDontSave;
                CoroutineHost.DontDestroyOnLoad(s_RoutineHost);
            }
        }

        static private CoroutineHost s_RoutineHost;
        private class CoroutineHost : MonoBehaviour
        {
            private void Awake()
            {
                MessageHook.instance.OnUpdate += UpdateInstances;
                KeepAlive.Apply(this);
            }

            private void OnDestroy()
            {
                using (PooledList<CoroutineInstance> instances = PooledList<CoroutineInstance>.Create())
                {
                    instances.AddRange(s_CurrentlyRunning);
                    foreach(var instance in instances)
                    {
                        instance.Dispose();
                    }
                }
            }

            private void UpdateInstances()
            {
                using(PooledList<CoroutineInstance> instances = PooledList<CoroutineInstance>.Create())
                {
                    instances.AddRange(s_CurrentlyRunning);
                    for (int i = 0; i < instances.Count; ++i)
                    {
                        var instance = instances[i];
                        if (!instance.m_Disposing)
                            instance.Update();
                        if (instance.m_Disposing)
                            instance.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}