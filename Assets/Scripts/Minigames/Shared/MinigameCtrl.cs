using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    [DisallowMultipleComponent]
    public abstract class MinigameCtrl : MonoBehaviour
    {
        [Header("Common Controls")]
        public EggTimer Timer;
        public DragController Controller;
        public bool IsStandalone = false;
        public MinigameBackgroundScroll Background;

        [Header("Win/Loss Screens")]
        public Transform CorrectAnswerDisplay;
        public Transform FailedAnswerDisplay;

        [Header("Egg Awards")]
        public Transform EggScreen;
        public EggAwardCtrl EggAward;
        public EggCounterCtrl EggCounter;

        private Action m_OnEggFinishedAnimating;

        /// <summary>
        /// If the player won the minigame.
        /// </summary>
        public bool WinState { get; protected set; }

        /// <summary>
        /// If the minigame is running.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Called when the minigame form is opened.
        /// </summary>
        public event Action<MinigameCtrl> OnOpen;

        /// <summary>
        /// Called when the minigame is started.
        /// </summary>
        public event Action<MinigameCtrl> OnBegin;

        /// <summary>
        /// Called when the minigame is finished.
        /// </summary>
        public event Action<MinigameCtrl> OnEnd;

        /// <summary>
        /// Called when the minigame is closed.
        /// </summary>
        public event Action<MinigameCtrl> OnClose;

        protected virtual void Start()
        {
            Timer.Timer.OnFinish += Timer_OnFinish;
            Timer.Timer.OnWarning += Timer_OnWarning;
        }

        public virtual void Clear()
        {
            Timer.Timer.ResetTimer();

            FailedAnswerDisplay.gameObject.SetActive(false);
            CorrectAnswerDisplay.gameObject.SetActive(false);
            Controller.Touchable = false;
            Running = false;

            Background.SetSpeed(Background.animSpeed);
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);

            Clear();

            if (OnOpen != null)
                OnOpen(this);

            SoundManager.instance.PlayOneShot(SoundManager.instance.minigameReveal);
            SoundManager.instance.PlayMusicTransition(SoundManager.instance.minigameMusic, SoundManager.instance.TransitionTime);
        }

        public virtual void Close()
        {
            if (OnClose != null)
                OnClose(this);
        }

        public virtual void Begin()
        {
            WinState = false;
            Controller.Touchable = true;
            Timer.Timer.StartTimer();
            Running = true;

            if (OnBegin != null)
                OnBegin(this);
        }

        public virtual void End()
        {
            Controller.Touchable = false;
            Timer.Timer.StopTimer();
            Running = false;

            if (OnEnd != null)
                OnEnd(this);
        }

        protected void Win()
        {
            WinState = true;
            End();
        }

        protected void Lose()
        {
            WinState = false;
            End();
        }

        protected void AwardEgg(Action inFinishCallback)
        {
            EggScreen.gameObject.SetActive(true);
            EggScreen.GetComponent<TransformParentMemory>().ChangeTransform(this.transform, false);

            EggCounter.Show(true);
            EggAward.SpawnEggs(1);
            EggAward.UpdateSaveData(1);

            m_OnEggFinishedAnimating = inFinishCallback;
            EggAward.OnFinished += OnEggAnimationFinished;
        }

        #region Callbacks

        protected virtual void Timer_OnFinish(Timer obj)
        {
            Lose();
        }

        protected virtual void Timer_OnWarning(Timer arg1, float arg2)
        {
            SoundManager.instance.PlayMusicOneShot(SoundManager.instance.minigameStinger);
            SoundManager.instance.PlayMusicTransition(null, 1.0f);
            this.WaitSecondsThen(1.1f, () => { SoundManager.instance.PlayMusicTransition(SoundManager.instance.minigamePanic, 0.1f); });
            Background.SetSpeed(Background.fastSpeed);
        }

        private void OnEggAnimationFinished()
        {
            EggAward.OnFinished -= OnEggAnimationFinished;

            EggCounter.Show(false);
            EggCounter.OnClosed += OnEggCounterClose;
        }

        private void OnEggCounterClose()
        {
            EggCounter.OnClosed -= OnEggCounterClose;
            EggScreen.GetComponent<TransformParentMemory>().RestoreTransform();

            if (m_OnEggFinishedAnimating != null)
                m_OnEggFinishedAnimating();
            m_OnEggFinishedAnimating = null;
        }

        #endregion
    }
}
