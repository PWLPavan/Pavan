using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    [DisallowMultipleComponent]
    public class SeatTweener : MonoBehaviour
    {
        private DragObject m_DragObject;
        private ObjectTweener m_Tweener;

        private void Awake()
        {
            m_DragObject = GetComponent<DragObject>();
            m_Tweener = GetComponent<ObjectTweener>();

            m_DragObject.OnOwnerChange += m_DragObject_OnOwnerChange;
            m_DragObject.OnSorted += m_DragObject_OnSorted;
        }

        void m_DragObject_OnSorted(DragObject arg1, int arg2)
        {
            if (arg1.Owner.HasSeats)
            {
                Transform nextSeat = arg1.Owner.GetSeat(arg1);
                m_Tweener.SetTarget(nextSeat);
            }
        }

        void m_DragObject_OnOwnerChange(DragObject arg1, DragHolder arg2)
        {
            if (arg1.Owner != null)
            {
                m_DragObject_OnSorted(arg1, arg1.Index);
            }
        }

    }
}
