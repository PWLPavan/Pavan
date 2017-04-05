using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    [DisallowMultipleComponent]
    public class StartingSeat : MonoBehaviour
    {
        public DragHolder Holder;

        private DragObject m_DragObject;

        private void Awake()
        {
            m_DragObject = GetComponent<DragObject>();
        }

        private void Start()
        {
            if (Holder != null)
                SetHolder(Holder);
        }

        public void SetHolder(DragHolder inHolder)
        {
            Holder = inHolder;
            m_DragObject.SetOwner(Holder, true);
            if (Holder.HasSeats)
            {
                Transform targetPosition = m_DragObject.Owner.GetSeat(m_DragObject);
                m_DragObject.transform.position = targetPosition.position;
            }
        }
    }
}
