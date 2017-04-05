using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Represents the base class of any DragObject or DragHolder.
    /// </summary>
    [DisallowMultipleComponent]
    public class Timer : MonoBehaviour
    {
        #region Inspector

        /// <summary>
        /// If the timer is operating.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Maximum amount of time for the timer.
        /// </summary>
        public float MaxTime = 60;

        /// <summary>
        /// Times at which warnings are given.
        /// </summary>
        public float[] WarningThresholds = new float[] { 15f };

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when the timer starts.
        /// </summary>
        public event Action<Timer> OnStart;

        /// <summary>
        /// Called when the timer value changes.
        /// </summary>
        public event Action<Timer, float> OnValueChange;

        /// <summary>
        /// Called when bonus time is added.
        /// </summary>
        public event Action<Timer, float> OnTimeAdded;

        /// <summary>
        /// Called when time is removed.
        /// </summary>
        public event Action<Timer, float> OnTimeRemoved;

        /// <summary>
        /// Called when the timer reaches a warning threshold.
        /// </summary>
        public event Action<Timer, float> OnWarning;

        /// <summary>
        /// Called when the timer runs out.
        /// </summary>
        public event Action<Timer> OnFinish;

        #endregion

        /// <summary>
        /// Amount of time left.
        /// </summary>
        public float TimeRemaining { get; private set; }

        #region Unity Events

        private void Start()
        {
            if (Enabled)
                StartTimer();
        }

        private void Update()
        {
            if (!Enabled)
                return;

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining < 0)
                TimeRemaining = 0;

            if (OnValueChange != null)
                OnValueChange(this, TimeRemaining);

            CheckForThresholds(TimeRemaining + Time.deltaTime, TimeRemaining);

            if (TimeRemaining == 0)
            {
                Enabled = false;
                if (OnFinish != null)
                    OnFinish(this);
            }
        }

        private void CheckForThresholds(float inOldTime, float inNewTime)
        {
            if (OnWarning != null)
            {
                foreach(float threshold in WarningThresholds)
                {
                    if (inOldTime > threshold && inNewTime <= threshold)
                        OnWarning(this, threshold);
                }
            }
        }

        #endregion

        /// <summary>
        /// Starts running the timer.
        /// </summary>
        public void StartTimer()
        {
            StartTimer(MaxTime);
        }

        /// <summary>
        /// Starts running the timer with a set amount of time.
        /// </summary>
        public void StartTimer(float inMaxTime)
        {
            Enabled = true;
            TimeRemaining = MaxTime = inMaxTime;
            if (OnStart != null)
                OnStart(this);
            if (OnValueChange != null)
                OnValueChange(this, TimeRemaining);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void StopTimer()
        {
            Enabled = false;
        }

        /// <summary>
        /// Resumes the timer.
        /// </summary>
        public void ResumeTimer()
        {
            Enabled = true;
        }

        /// <summary>
        /// Resets the timer to max.
        /// </summary>
        public void ResetTimer()
        {
            Enabled = false;
            TimeRemaining = MaxTime;
            if (OnValueChange != null)
                OnValueChange(this, TimeRemaining);
        }

        public void AddBonusTime(float inBonusTime)
        {
            TimeRemaining += inBonusTime;
            if (OnValueChange != null)
                OnValueChange(this, TimeRemaining);
            if (OnTimeAdded != null)
                OnTimeAdded(this, inBonusTime);
        }

        public void RemoveTime(float inTimeToRemove)
        {
            TimeRemaining -= inTimeToRemove;
            if (TimeRemaining < 0)
                TimeRemaining = 0;

            if (OnValueChange != null)
                OnValueChange(this, TimeRemaining);
            if (OnTimeRemoved != null)
                OnTimeRemoved(this, inTimeToRemove);

            CheckForThresholds(TimeRemaining + inTimeToRemove, TimeRemaining);

            if (TimeRemaining == 0)
            {
                Enabled = false;
                if (OnFinish != null)
                    OnFinish(this);
            }
        }
    }
}
