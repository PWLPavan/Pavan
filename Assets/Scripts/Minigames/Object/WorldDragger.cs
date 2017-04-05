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
    public class WorldDragger : BaseDragger
    {
        public void OnMouseDown()
        {
            if (m_DragObject.AllowTouch)
            {
                m_DragObject.Controller.StartDrag(m_DragObject);
            }
        }

        public void OnMouseDrag()
        {
            if (m_DragObject.IsDragging)
            {
                Vector3 position = Input.mousePosition;
                position.z = m_DragObject.transform.position.z - Camera.main.transform.position.z;
                GetComponent<ObjectTweener>().SetTarget(Camera.main.ScreenToWorldPoint(position));
            }
        }

        public void OnMouseUp()
        {
            if (m_DragObject.IsDragging)
            {
                m_DragObject.Controller.CancelDrag();
            }
        }
        
    }
}
