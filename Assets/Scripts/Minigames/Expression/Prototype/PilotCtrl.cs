using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;
using UnityEngine.UI;

namespace Minigames
{
    public class PilotCtrl : MonoBehaviour
    {
        private Animator m_Animator;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        public void SetTrigger(string inTriggerName)
        {
            m_Animator.SetTrigger(inTriggerName);
        }

        public void SetBool(string inBoolName, bool inValue)
        {
            m_Animator.SetBool(inBoolName, inValue);
        }

        public void Reset()
        {
            
        }
    }
}
