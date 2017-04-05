using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;
using UnityEngine.EventSystems;

namespace Minigames
{
    /// <summary>
    /// Controls canvas objects.
    /// </summary>
    public class CanvasDragger : BaseDragger, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_DragObject.AllowTouch)
            {
                m_DragObject.Controller.StartDrag(m_DragObject);
                m_DragObject.transform.SetAsLastSibling();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (m_DragObject.IsDragging)
            {
                DragHolder holder = FindHolder(eventData);
                m_DragObject.Controller.EndDrag(holder);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_DragObject.IsDragging)
            {
                GetComponent<ObjectTweener>().SetTarget(Input.mousePosition);
            }
        }

        private DragHolder FindHolder(PointerEventData inEventData)
        {
            using (PooledList<RaycastResult> raycastResult = PooledList<RaycastResult>.Create())
            {
                EventSystem.current.RaycastAll(inEventData, raycastResult);

                foreach (var result in raycastResult)
                {
                    var b = result.gameObject.GetComponentInParent<DragBase>();
                    if (b != null && b.Type == DragBaseType.Holder)
                    {
                        return (DragHolder)b;
                    }
                }

                return null;
            }
        }
    }
}
