using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Handles DragHolders and DragObjects.
    /// </summary>
    [DisallowMultipleComponent]
    public class DragController : MonoBehaviour
    {
        #region Inspector

        /// <summary>
        /// Root from which to search for DragObjects and DragHolders.
        /// </summary>
        public Transform DragObjectRoot;

        /// <summary>
        /// If dragging is enabled.
        /// </summary>
        public bool Touchable
        {
            get { return m_Touchable; }
            set
            {
                if (!value)
                    CancelDrag();
                m_Touchable = value;
            }
        }

        private bool m_Touchable = true;

        #endregion

        #region Callbacks

        public event Action<DragObject> OnDragStart;
        public event Action<DragObject, DragEndState> OnDragEnd;

        #endregion

        #region Unity Events

        private void Awake()
        {
            Assert.True(DragObjectRoot != null, "Root is not null.", "Root from which to find dragging objects is missing.");
        }

        private void Update()
        {
            UpdateDrag();
        }

        #endregion

        #region Dragging

        public DragObject DraggingObject { get; private set; }

        /// <summary>
        /// Attempts to start dragging from the given drag space position.
        /// </summary>
        public void StartDrag(DragObject inDragObject)
        {
            Assert.True(DraggingObject == null, "Not dragging something.", "Cannot start new drag when already dragging an object.");
            if (inDragObject.AllowTouch)
            {
                DraggingObject = inDragObject;
                DraggingObject.DraggingStarted();
                if (OnDragStart != null)
                    OnDragStart(DraggingObject);
            }
        }

        /// <summary>
        /// Attempts to update the object being dragged.
        /// </summary>
        public void UpdateDrag()
        {
            if (DraggingObject != null)
            {
                if (DraggingObject.AllowTouch)
                {
                    DraggingObject.DraggingContinues();
                }
                else
                {
                    CancelDrag();
                }
            }
        }

        /// <summary>
        /// Attempts to finish a drag at the given drag space position.
        /// </summary>
        public void EndDrag(DragHolder inHolder)
        {
            if (DraggingObject != null)
            {
                DragEndState endState;
                DragObject dragObject = DraggingObject;
                DraggingObject = null;

                if (inHolder == null)
                {
                    endState = DragEndState.Empty;
                }
                else
                {
                    if (inHolder.AllowAddObject(dragObject))
                    {
                        dragObject.SetOwner(inHolder);
                        endState = DragEndState.ValidZone;
                    }
                    else
                    {
                        LastValidSeat lastSeat = dragObject.GetComponent<LastValidSeat>();
                        bool bValidSwap = lastSeat != null && lastSeat.LastValidHolder != null;
                        DragObject swappedObject = null;
                        if (bValidSwap)
                            bValidSwap = inHolder.AllowSwapObject(dragObject, out swappedObject);
                        if (bValidSwap)
                            bValidSwap = lastSeat.LastValidHolder.AllowAddObject(swappedObject);

                        if (bValidSwap)
                        {
                            swappedObject.SetOwner(lastSeat.LastValidHolder);
                            dragObject.SetOwner(inHolder);
                            endState = DragEndState.ValidZone;
                        }
                        else
                        {
                            endState = DragEndState.InvalidZone;
                        }
                    }
                }

                dragObject.DraggingStops(endState);

                if (OnDragEnd != null)
                    OnDragEnd(dragObject, endState);
            }
        }

        /// <summary>
        /// Cancels an existing drag.
        /// </summary>
        public void CancelDrag()
        {
            if (DraggingObject != null)
            {
                DragObject dragObject = DraggingObject;
                DraggingObject = null;

                dragObject.DraggingStops(DragEndState.Cancelled);
                if (OnDragEnd != null)
                    OnDragEnd(dragObject, DragEndState.Cancelled);
            }
        }

        #endregion
    }
}
