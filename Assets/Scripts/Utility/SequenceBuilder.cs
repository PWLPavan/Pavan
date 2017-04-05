using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FGUnity.Utils
{
    /// <summary>
    /// Generates a coroutine from individual functions.
    /// </summary>
    public class SequenceBuilder
    {
        /// <summary>
        /// Generation function for a sequence.
        /// Since we're working off of IEnumerators, we
        /// can't clone them or preserve their initial state.
        /// So we have to make them fresh each time.
        /// </summary>
        public delegate SequenceBuilder Generator();

        private Queue<IEnumerator> m_Sequence = new Queue<IEnumerator>();

        /// <summary>
        /// Event when the sequence is finished.
        /// </summary>
        public event Action OnComplete;

        /// <summary>
        /// Indicates if the sequence is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Clears all events in the sequence.
        /// </summary>
        public void Clear()
        {
            Assert.True(!IsRunning, "Sequence is not running.", "Cannot modify SequenceBuilder while running.");

            while (m_Sequence.Count > 0)
            {
                IEnumerator next = m_Sequence.Dequeue();
                ((IDisposable)next).Dispose();
            }
            OnComplete = null;
        }

        /// <summary>
        /// Starts a sequence.
        /// </summary>
        public SequenceBuilder Start(IEnumerator inSequence)
        {
            Assert.True(!IsRunning, "Sequence is not running.", "Cannot modify SequenceBuilder while running.");
            Assert.True(inSequence != null, "Sequence is not null.");

            Clear();
            m_Sequence.Enqueue(inSequence);

            return this;
        }

        /// <summary>
        /// Appends to the end of the sequence.
        /// </summary>
        public SequenceBuilder Then(IEnumerator inNext)
        {
            Assert.True(!IsRunning, "Sequence is not running.", "Cannot modify SequenceBuilder while running.");
            Assert.True(inNext != null, "Sequence is not null.");

            m_Sequence.Enqueue(inNext);

            return this;
        }

        /// <summary>
        /// Adds an action to occur at the end of the sequence.
        /// </summary>
        public SequenceBuilder Finally(Action inAction)
        {
            Assert.True(!IsRunning, "Sequence is not running.", "Cannot modify SequenceBuilder while running.");
            OnComplete += inAction;

            return this;
        }

        /// <summary>
        /// If the Sequence has any actions left to perform.
        /// </summary>
        public bool HasActions
        {
            get { return m_Sequence.Count > 0; }
        }

        /// <summary>
        /// Runs the sequence.
        /// </summary>
        public IEnumerator Run()
        {
            Assert.True(!IsRunning, "Sequence is not running.", "Cannot run SequenceBuilder multiple times.");
            IsRunning = true;

            // This is because the sequence might be stopped somewhere
            // in the middle, and we might 
            using (var disposeGuaranteed = new ClearSequenceBuilderWrapper(this))
            {
                while (m_Sequence.Count > 0)
                {
                    IEnumerator action = m_Sequence.Peek();
                    yield return action;
                    m_Sequence.Dequeue();
                }

                IsRunning = false;

                if (OnComplete != null)
                {
                    Action currentComplete = OnComplete;
                    OnComplete = null;
                    currentComplete();
                }
            }
        }

        // Guarantees that the sequence is cleaned up if stopped,
        // regardless of where it is stopped.
        private class ClearSequenceBuilderWrapper : IDisposable
        {
            private SequenceBuilder m_Parent;

            public ClearSequenceBuilderWrapper(SequenceBuilder inBuilder)
            {
                m_Parent = inBuilder;
            }

            public void Dispose()
            {
                m_Parent.IsRunning = false;
                m_Parent.Clear();
            }
        }
    }

    static public class SequenceHelpers
    {
        /// <summary>
        /// Starts a sequence of events.
        /// </summary>
        /// <param name="inSequence">The sequence to run.</param>
        /// <param name="inTimesToRun">Number of times to run the sequence.</param>
        static public CoroutineHandle StartSequence(this MonoBehaviour inBehavior, SequenceBuilder inSequence)
        {
            Assert.True(inBehavior != null, "Host is not null.");
            Assert.True(inSequence != null, "Sequence is not null.");
            return inBehavior.SmartCoroutine(inSequence.Run());
        }
    }
}