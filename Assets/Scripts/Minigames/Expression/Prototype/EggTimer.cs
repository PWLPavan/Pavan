using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;

namespace Minigames
{
    public class EggTimer : MonoBehaviour
    {
        private Timer m_Timer;

        public Timer Timer { get { return m_Timer; } }

        public RectTransform SpinningStar;
        public float SpinSpeed = 15f;
        public RectTransform TimeMeter;
        public RectTransform LossMeter;

        public float LossDelay = 0.5f;
        public float LossTweenTime = 0.5f;

        private float m_BarOriginalHeight;
        private CoroutineHandle m_LossTweener;

        private void Awake()
        {
            m_Timer = GetComponent<Timer>();
        }

        private void Start()
        {
            m_Timer.OnValueChange += m_Timer_OnValueChange;
            m_Timer.OnTimeRemoved += m_Timer_OnTimeRemoved;
            m_Timer.OnFinish += m_Timer_OnFinish;

            m_BarOriginalHeight = TimeMeter.sizeDelta.y;

            SpinningStar.Rotate(0, 0, RNG.Instance.NextFloat(360f));
        }

        void m_Timer_OnFinish(Timer obj)
        {
            SoundManager.instance.PlayOneShot(SoundManager.instance.pilotAngry);
        }

        void m_Timer_OnTimeRemoved(Timer arg1, float arg2)
        {
            SoundManager.instance.PlayOneShot(SoundManager.instance.minigamePenalty);
            GetComponent<Animator>().SetTrigger("Shake");
            StartLossMeterTween(arg2);
        }

        private void Update()
        {
            SpinningStar.Rotate(0, 0, SpinSpeed * Time.deltaTime);
        }

        void m_Timer_OnValueChange(Timer arg1, float arg2)
        {
            Vector2 size = CalculateBarSize(arg2);
            TimeMeter.sizeDelta = size;

            if (LossMeter != null && !m_LossTweener)
                LossMeter.sizeDelta = size;
        }

        private void StopLossMeterTween()
        {
            if (LossMeter != null)
            {
                this.StopCoroutine(ref m_LossTweener);

                LossMeter.gameObject.SetActive(false);
            }
        }

        private void StartLossMeterTween(float inTimeRemoved)
        {
            if (LossMeter != null)
            {
                m_LossTweener.Stop();
                m_LossTweener = this.SmartCoroutine(LossMeterTweenRoutine(inTimeRemoved));
            }
        }

        IEnumerator LossMeterTweenRoutine(float inTimeRemoved)
        {
            Vector2 newSize;
            if (LossMeter.gameObject.activeSelf)
                newSize = LossMeter.sizeDelta;
            else
                newSize = CalculateBarSize(Timer.TimeRemaining + inTimeRemoved);
            LossMeter.sizeDelta = newSize;

            LossMeter.gameObject.SetActive(true);

            yield return LossDelay;

            float startY = LossMeter.sizeDelta.y;
            float endY = TimeMeter.sizeDelta.y;

            float timer = 0.0f;
            while(timer < LossTweenTime)
            {
                float newEnd = TimeMeter.sizeDelta.y;
                startY += newEnd - endY;
                endY = newEnd;

                newSize = LossMeter.sizeDelta;
                newSize.y = Mathf.Lerp(startY, endY, timer / LossTweenTime);
                LossMeter.sizeDelta = newSize;

                timer += Time.deltaTime;
                yield return null;
            }

            LossMeter.gameObject.SetActive(false);
        }

        private Vector2 CalculateBarSize(float inTimeRemaining)
        {
            Vector2 size = TimeMeter.sizeDelta;
            size.y = m_BarOriginalHeight * inTimeRemaining / Timer.MaxTime;
            return size;
        }
    }
}
