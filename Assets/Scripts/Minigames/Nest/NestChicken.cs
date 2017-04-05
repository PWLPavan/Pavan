using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;
using UnityEngine.UI;
using Ekstep;

namespace Minigames
{
    public class NestChicken : MonoBehaviour
    {
        public DragObject Dragger;

        private Transform m_InnerChicken;
        private CoroutineHandle m_WanderRoutine;
        private float m_PanicProgress = 0;

        public float WanderDistance = 64;
        public float WanderDelay = 1.5f;
        public float WanderSpeed = 0.25f;

        public float PanicDistance = 80;
        public float PanicDelay = 0.5f;
        public float PanicSpeed = 0.5f;

        public void SetPanicProgress(float inPanic)
        {
            m_PanicProgress = inPanic;
        }

        private void Awake()
        {
            ObjectTweener tweener = GetComponent<ObjectTweener>();

            m_InnerChicken = transform.Find("chicken");

            this.WaitFramesThen(2, BeginWandering);
        }

        private void Start()
        {
            Dragger.OnDragStart += Dragger_OnDragStart;
            Dragger.OnDragEnd += Dragger_OnDragEnd;
        }

        void Dragger_OnDragEnd(DragObject arg1, DragEndState arg2)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "minigame.make10.chicken"));
			GetComponent<Animator>().SetBool("isDragging", false);
			if(Dragger.Owner && Dragger.Owner.NumSeats > 0)
            {
				GetComponent<Animator>().SetTrigger("startHappy");
				Dragger.Owner.GetComponentInParent<Animator>().SetTrigger("bounce");
                Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DROP, "minigame.make10.nest"));
                SoundManager.instance.PlayOneShot(SoundManager.instance.minigameDropNest);
			}
            else
            {
                GetComponent<Animator>().SetTrigger("reset");
                RectTransform ownerTransform = Dragger.Owner.GetComponent<RectTransform>();
                float yThreshold = ownerTransform.TransformPoint(ownerTransform.localPosition.x, ownerTransform.rect.yMax, 0).y;
                if (transform.position.y > yThreshold)
                {
                    SoundManager.instance.PlayOneShot(SoundManager.instance.minigameDropEmpty);
                    GetComponent<ObjectTweener>().ClearTarget();
                    m_WanderRoutine = this.SmartCoroutine(DropRoutine(yThreshold));
                }
                else
                {
                    m_WanderRoutine = this.SmartCoroutine(WanderRoutine());
                }
            }
        }

        void Dragger_OnDragStart(DragObject obj)
        {
            Genie.I.LogEvent(OE_INTERACT.Create(OE_INTERACT.Type.DRAG, "minigame.make10.chicken"));
            SoundManager.instance.PlayRandomOneShot(SoundManager.instance.chickenDrag);
			GetComponent<Animator>().SetBool("isDragging", true);
			GetComponent<Animator>().SetTrigger("startDrag");
            GetComponent<Animator>().SetBool("isWalking", false);
            m_WanderRoutine.Clear();

            Vector3 localScale = m_InnerChicken.localScale;
            localScale.x = 1;
            m_InnerChicken.localScale = localScale;
        }

        private void BeginWandering()
        {
            if (Dragger.Owner != null)
                m_WanderRoutine = this.SmartCoroutine(WanderRoutine());
        }

        private IEnumerator DropRoutine(float inNewY)
        {
            ObjectTweener tweener = GetComponent<ObjectTweener>();

            tweener.SetTarget(new Vector3(transform.position.x, inNewY, transform.position.z));
            yield return tweener.WaitUntilFinished();
            m_WanderRoutine = this.SmartCoroutine(WanderRoutine());
        }

        private IEnumerator WanderRoutine()
        {
            ObjectTweener tweener = GetComponent<ObjectTweener>();
            RectTransform ownerTransform = Dragger.Owner.GetComponent<RectTransform>();

            while(true)
            {
                float delay = Mathf.Lerp(WanderDelay, PanicDelay, m_PanicProgress);
                yield return RNG.Instance.NextFloat(0.5f, 1.0f) * delay;

                Rect ownerRect = ownerTransform.rect;
                Vector3 localPosition = ownerTransform.InverseTransformPoint(Dragger.transform.position);

                float distance = Mathf.Lerp(WanderDistance, PanicDistance, m_PanicProgress);

                float nextAngle = RNG.Instance.NextFloat(Mathf.PI * 2);
                float nextDistance = RNG.Instance.NextFloat(0.8f, 1) * distance;
                float nextX = localPosition.x + Mathf.Cos(nextAngle) * nextDistance;
                float nextY = localPosition.y + Mathf.Sin(nextAngle) * nextDistance;
                nextX = Mathf.Clamp(nextX, ownerRect.xMin, ownerRect.xMax);
                nextY = Mathf.Clamp(nextY, ownerRect.yMin, ownerRect.yMax);

                Vector3 nextPosition = new Vector3(nextX, nextY, 0);
                nextPosition = ownerTransform.TransformPoint(nextPosition);

                Vector3 localScale = m_InnerChicken.localScale;
                if (nextPosition.x > transform.position.x)
                {
                    localScale.x = -1;
                }
                else
                {
                    localScale.x = 1;
                }
                m_InnerChicken.localScale = localScale;

                float lerpSpeed = Mathf.Lerp(WanderSpeed, PanicSpeed, m_PanicProgress);
                float lerpTime = (transform.position - nextPosition).magnitude / lerpSpeed;

                GetComponent<Animator>().SetBool("isWalking", true);
                tweener.ClearTarget();

                yield return transform.MoveTo(nextPosition, lerpTime);

                GetComponent<Animator>().SetBool("isWalking", false);
            }
        }
    }
}
