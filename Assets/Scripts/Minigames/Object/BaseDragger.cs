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
    [DisallowMultipleComponent]
    public abstract class BaseDragger : MonoBehaviour
    {
        protected DragObject m_DragObject;

        protected virtual void Awake()
        {
            m_DragObject = GetComponent<DragObject>();

            m_DragObject.OnDragStart += OnDragStart;
        }

        protected void OnDragStart(DragObject inObject)
        {
            GetComponent<ObjectTweener>().SetTarget(m_DragObject.transform.position);
        }
    }
}
