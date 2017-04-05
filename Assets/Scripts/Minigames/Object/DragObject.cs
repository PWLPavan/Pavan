using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Represents a draggable object.
    /// </summary>
    public class DragObject : DragBase
    {
        #region Inspector

        /// <summary>
        /// Indicates what Zones are valid drop points.
        /// </summary>
        [EnumFlag] public DragObjectCategory Categories = DragObjectCategory.Plain;

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when dragging has started.
        /// </summary>
        public event Action<DragObject> OnDragStart;

        /// <summary>
        /// Called when dragging.
        /// </summary>
        public event Action<DragObject> OnDragging;

        /// <summary>
        /// Called when dragging has ended.
        /// </summary>
        public event Action<DragObject, DragEndState> OnDragEnd;

        /// <summary>
        /// Called when the owner has changed.
        /// </summary>
        public event Action<DragObject, DragHolder> OnOwnerChange;

        /// <summary>
        /// Called when the owner has sorted this object.
        /// </summary>
        public event Action<DragObject, int> OnSorted;

        #endregion

        #region Unity Events

        protected override void Awake()
        {
            base.Awake();
            Owner = null;
            Index = -1;
        }

        #endregion

        public override DragBaseType Type
        {
            get { return DragBaseType.Object; }
        }

        public override bool AllowTouch
        {
            get { return base.AllowTouch && (Controller.DraggingObject == null || Controller.DraggingObject == this)
                && (Owner == null || Owner.AllowRemoveObject(this)); }
        }

        /// <summary>
        /// If the DragObject is currently being dragged.
        /// </summary>
        public bool IsDragging { get; private set; }

        /// <summary>
        /// DropZone that owns the DragObject.
        /// </summary>
        public DragHolder Owner { get; private set; }

        /// <summary>
        /// Index of the DragObject in the DropZone.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Starts the dragging process.
        /// </summary>
        public void DraggingStarted()
        {
            if (!IsDragging)
            {
                // We still have access to the former owner
                // during the callback, in case we need to use it
                if (OnDragStart != null)
                    OnDragStart(this);
                IsDragging = true;
            }

            // Since we've started dragging, we don't need an owner anymore.
            SetOwner(null);
        }

        /// <summary>
        /// Continues the dragging process.
        /// </summary>
        public void DraggingContinues()
        {
            if (IsDragging)
            {
                if (OnDragging != null)
                    OnDragging(this);
            }
        }

        /// <summary>
        /// Stops the dragging process.
        /// </summary>
        public void DraggingStops(DragEndState inState)
        {
            if (IsDragging)
            {
                if (OnDragEnd != null)
                    OnDragEnd(this, inState);

                IsDragging = false;
            }
        }

        /// <summary>
        /// Sets the owner of the object.
        /// </summary>
        public void SetOwner(DragHolder inOwner, bool inbForce = false)
        {
            // We don't need to do anything
            // if this is our owner already
            if (Owner == inOwner)
                return;

            DragHolder oldOwner = Owner;

            if (Owner != null)
            {
                Owner.RemoveObject(this, inbForce);
                Assert.True(Index == -1, "DragObject was able to be removed.");
            }

            Owner = inOwner;

            if (Owner != null)
            {
                Owner.AddObject(this, inbForce);
                Assert.True(Index >= 0, "DragObject was able to be added.");
            }

            if (OnOwnerChange != null)
            {
                OnOwnerChange(this, oldOwner);
            }
        }

        public void SetIndex(int inIndex, bool inbFromSort = false)
        {
            if (Index == inIndex)
                return;

            Index = inIndex;
            if (Index >= 0 && inbFromSort && OnSorted != null)
                OnSorted(this, Index);
        }
    }
}
