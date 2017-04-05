using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUnity.Utils
{
    /// <summary>
    /// Coordinates sequences.
    /// </summary>
    [DisallowMultipleComponent]
    public class Sequencer : MonoBehaviour
    {
        // List of all running sequences
        private List<SequenceData> m_RunningSequences = new List<SequenceData>();

        // Map of names to generators
        private Dictionary<string, Generator> m_Generators = new Dictionary<string, Generator>();

        // Finds the first SequenceData with the given id.
        private SequenceData FindSequence(string inID)
        {
            for(int i = 0; i < m_RunningSequences.Count; ++i)
            {
                if (m_RunningSequences[i].Name == inID)
                    return m_RunningSequences[i];
            }

            return null;
        }

        /// <summary>
        /// Registers a sequence with the given name.
        /// </summary>
        public void Register(string inID, SequenceBuilder.Generator inGenerator, ConcurrentBehavior inBehavior = ConcurrentBehavior.Restart)
        {
            Stop(inID);
            m_Generators[inID] = new Generator(inGenerator, inBehavior);
        }

        /// <summary>
        /// Returns if a sequence with the given name exists.
        /// </summary>
        public bool IsRegistered(string inID)
        {
            return m_Generators.ContainsKey(inID);
        }

        /// <summary>
        /// Unregisters the sequence with the given name.
        /// </summary>
        public void UnRegister(string inID)
        {
            m_Generators.Remove(inID);
            Stop(inID);
        }

        /// <summary>
        /// Starts running the sequence with the given name.
        /// Returns if it was able to start.
        /// </summary>
        public bool Run(string inID)
        {
            Generator generator;
            if (m_Generators.TryGetValue(inID, out generator))
            {
                if (generator.Behavior != ConcurrentBehavior.Allow)
                {
                    SequenceData runningAlready = FindSequence(inID);
                    if (runningAlready != null)
                    {
                        if (generator.Behavior == ConcurrentBehavior.Restart)
                            runningAlready.Stop();
                        else
                            return false;
                    }
                }

                SequenceData data = SequenceData.Create(inID, this, generator.Function());
                m_RunningSequences.Add(data);
                data.Start();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns if a sequence with the given name is running.
        /// </summary>
        public bool IsRunning(string inID)
        {
            return FindSequence(inID) != null;
        }

        /// <summary>
        /// Stops all sequences with the given name.
        /// Returns if any were stopped.
        /// </summary>
        public bool Stop(string inID)
        {
            bool bFoundOne = false;
            for (int i = 0; i < m_RunningSequences.Count; ++i)
            {
                if (m_RunningSequences[i].Name == inID)
                {
                    m_RunningSequences[i--].Stop();
                    bFoundOne = true;
                }
            }
            return bFoundOne;
        }

        /// <summary>
        /// Stops all executing sequences.
        /// </summary>
        public void StopAll()
        {
            using(PooledList<SequenceData> data = PooledList<SequenceData>.Create())
            {
                data.AddRange(m_RunningSequences);

                for (int i = 0; i < data.Count; ++i)
                    data[i].Stop();
            }
        }

        private class SequenceData
        {
            public string Name;
            public Sequencer Owner;
            public SequenceBuilder Sequence;
            public CoroutineHandle Running;

            public void Start()
            {
                Assert.True(!Running);
                Sequence.OnComplete += Stop;
                Running = Owner.StartSequence(Sequence);
            }

            public void Stop()
            {
                Running.Stop();
                Sequence.Clear();
                Owner.m_RunningSequences.Remove(this);
                s_Pool.Push(this);
            }

            private void Reset(string inName, Sequencer inOwner, SequenceBuilder inSequence)
            {
                Name = inName;
                Owner = inOwner;
                Sequence = inSequence;
                Running = CoroutineHandle.Null;
            }

            static private Pool<SequenceData> s_Pool = new Pool<SequenceData>(64, constructor);

            static private SequenceData constructor(Pool<SequenceData> inPool)
            {
                return new SequenceData();
            }

            static public SequenceData Create(string inName, Sequencer inOwner, SequenceBuilder inSequence)
            {
                SequenceData obj = s_Pool.Pop();
                obj.Reset(inName, inOwner, inSequence);
                return obj;
            }
        }

        private class Generator
        {
            public Generator(SequenceBuilder.Generator inFunction, ConcurrentBehavior inBehavior)
            {
                Function = inFunction;
                Behavior = inBehavior;
            }

            public SequenceBuilder.Generator Function;
            public ConcurrentBehavior Behavior;
        }

        /// <summary>
        /// Behavior when multiple sequences with the same name
        /// are attempting to run.
        /// </summary>
        public enum ConcurrentBehavior
        {
            /// <summary>
            /// Allows multiple sequences with the same name.
            /// </summary>
            Allow,

            /// <summary>
            /// Cancels other sequences with same name.
            /// </summary>
            Restart,

            /// <summary>
            /// Rejects attempts to start another sequence with the same name.
            /// </summary>
            Reject
        }
    }
}