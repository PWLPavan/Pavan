using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    [DisallowMultipleComponent]
    public class LastValidSeat : MonoBehaviour
    {
        public DragHolder LastValidHolder { get; private set; }
        public bool SnapBack = true;

        private DragObject m_DragObject;

        private void Awake()
        {
            m_DragObject = GetComponent<DragObject>();

            m_DragObject.OnDragEnd += m_DragObject_OnDragEnd;
            m_DragObject.OnOwnerChange += m_DragObject_OnOwnerChange;
        }

        void m_DragObject_OnOwnerChange(DragObject arg1, DragHolder arg2)
        {
            if (arg1.Owner != null)
                LastValidHolder = arg1.Owner;
        }

        void m_DragObject_OnDragEnd(DragObject arg1, DragEndState arg2)
        {
            if (SnapBack && arg2 != DragEndState.ValidZone && LastValidHolder != null)
            {
                if (LastValidHolder.AllowAddObject(m_DragObject))
                {
                    m_DragObject.SetOwner(LastValidHolder);
                }
            }
        }

    }
}
