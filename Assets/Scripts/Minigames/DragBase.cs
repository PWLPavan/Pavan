using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Represents the base class of any DragObject or DragHolder.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class DragBase : MonoBehaviour
    {
        /// <summary>
        /// Type of object.
        /// </summary>
        public abstract DragBaseType Type { get; }

        /// <summary>
        /// Controller for this object.
        /// </summary>
        public DragController Controller
        {
            get
            {
                if (!m_DragController)
                    m_DragController = GetComponentInParent<DragController>();
                return m_DragController;
            }
        }

        /// <summary>
        /// Toggle to allow/disallow dragging and dropping.
        /// </summary>
        public bool Touchable = true;

        /// <summary>
        /// If the DragBase is able to be dragged / dropped.
        /// </summary>
        public virtual bool AllowTouch
        {
            get { return Touchable && Controller.Touchable; }
        }

        protected virtual void Awake()
        {
        }

        private DragController m_DragController;
    }
}
