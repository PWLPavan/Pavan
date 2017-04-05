using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using FGUnity.Utils;
using UnityEngine.UI;

namespace Minigames
{
    public class ExpressionSlotFinder : MonoBehaviour
    {
        private List<DragHolder> m_Slots = new List<DragHolder>();

        public void ClearSlots()
        {
            m_Slots.Clear();
        }

        public void AddSlot(DragHolder inSlot)
        {
            m_Slots.Add(inSlot);
        }

        public DragHolder FindNearestSlot(DragObject inObject)
        {
            DragHolder nearest = null;
            float nearestSqrDistance = 0;
            for(int i = 0; i < m_Slots.Count; ++i)
            {
                DragHolder holder = m_Slots[i];
                if (holder.Count < holder.MaxAllowed)
                {
                    float sqrDistance = (holder.transform.position - inObject.transform.position).sqrMagnitude;
                    if (nearest == null || sqrDistance < nearestSqrDistance)
                    {
                        nearest = holder;
                        nearestSqrDistance = sqrDistance;
                    }
                }
            }

            return nearest;
        }
    }
}
