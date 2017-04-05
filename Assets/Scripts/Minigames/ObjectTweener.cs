using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    [DisallowMultipleComponent]
    public class ObjectTweener : MonoBehaviour
    {
        public float LerpSpeed = 0.9f;
        public float LerpPeriod = 1.0f;
        public float LerpThreshold = 5f;

        private Vector3 m_TargetPosition;
        private Transform m_TargetTransform;

        public event Action<ObjectTweener> OnTweenStart;
        public event Action<ObjectTweener> OnTweenEnd;

        private void Start()
        {
            m_TargetPosition = transform.position;
        }

        private void Update()
        {
            Vector3 target = Target;
            bool bShouldLerp = NeedsToMove();
            if (bShouldLerp)
                transform.position = Vector3.Lerp(transform.position, target, MathUtilities.TimeIndependentLerp(LerpSpeed, LerpPeriod, Time.deltaTime));
            if (bShouldLerp != IsMoving)
            {
                IsMoving = bShouldLerp;
                if (bShouldLerp && OnTweenStart != null)
                    OnTweenStart(this);
                else if (!bShouldLerp)
                {
                    transform.position = target;
                    if (OnTweenEnd != null)
                        OnTweenEnd(this);
                }
            }
        }

        public Vector3 Target
        {
            get { return (m_TargetTransform != null ? m_TargetTransform.position : m_TargetPosition); }
        }

        public bool IsMoving { get; private set; }

        public void SetTarget(Vector3 inPosition)
        {
            bool bIsDifferent = m_TargetTransform != null || inPosition != m_TargetPosition;

            m_TargetPosition = inPosition;
            m_TargetTransform = null;

            if (bIsDifferent)
            {
                IsMoving = true;
                if (OnTweenStart != null)
                    OnTweenStart(this);
            }
        }

        public void SetTarget(Transform inTransform)
        {
            bool bIsDifferent = inTransform != m_TargetTransform;

            m_TargetTransform = inTransform;
            if (inTransform != null)
            {
                m_TargetPosition = inTransform.position;
                if (bIsDifferent && NeedsToMove())
                {
                    IsMoving = true;
                    if (OnTweenStart != null)
                        OnTweenStart(this);
                }
            }
        }

        public void ClearTarget()
        {
            m_TargetPosition = transform.position;
            m_TargetTransform = transform;

            if (IsMoving)
            {
                IsMoving = false;
                if (OnTweenEnd != null)
                    OnTweenEnd(this);
            }
        }

        public IEnumerator WaitUntilFinished()
        {
            while (IsMoving)
                yield return null;
        }

        private bool NeedsToMove()
        {
            Vector3 difference = Target - transform.position;
            return difference.sqrMagnitude > LerpThreshold * LerpThreshold;
        }
    }
}
