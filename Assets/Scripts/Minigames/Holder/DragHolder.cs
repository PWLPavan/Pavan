using UnityEngine;
using System.Collections.Generic;
using System;
using FGUnity.Utils;

namespace Minigames
{
    /// <summary>
    /// Container for DragObjects.
    /// </summary>
    public class DragHolder : DragBase
    {
        #region Inspector

        /// <summary>
        /// Indicates what is DragObjects are allowed to be added.
        /// </summary>
        [EnumFlag] public DragObjectCategory AllowedCategoriesAdd = DragObjectCategory.Plain;

        /// <summary>
        /// Indicates what is DragObjects are allowed to be removed.
        /// </summary>
        [EnumFlag] public DragObjectCategory AllowedCategoriesRemove = DragObjectCategory.Plain;

        /// <summary>
        /// Indicates if swapping is allowed.
        /// </summary>
        public bool AllowSwapping = true;

        /// <summary>
        /// Minimum number of allowed objects.
        /// </summary>
        [Range(0, 10)] public int MinAllowed = 0;

        /// <summary>
        /// Maximum number of allowed objects.
        /// </summary>
        [Range(0, 10)] public int MaxAllowed = 10;

        /// <summary>
        /// Contains target positions for different seats.
        /// </summary>
        public DragHolderArranger Arrangement;

        /// <summary>
        /// Whether the object list will sort itself.
        /// </summary>
        public bool AutoSort = false;

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when an object is added.
        /// </summary>
        public event Action<DragHolder, DragObject> OnObjectAdded;

        /// <summary>
        /// Called when an object is removed.
        /// </summary>
        public event Action<DragHolder, DragObject> OnObjectRemoved;

        /// <summary>
        /// Called when an object is sorted.
        /// </summary>
        public event Action<DragHolder> OnSorted;

        /// <summary>
        /// Method for sorting DragObjects.
        /// </summary>
        public Comparison<DragObject> SortMethod;

        #endregion

        #region Unity Events

        private void Start()
        {
            // We need enough seats to arrange everyone.
            Assert.True(NumSeats <= 0 || NumSeats >= MaxAllowed, "Enough seats for the max allowed DragObjects.", "Not enough seats for max {0} - only {1} provided.", MaxAllowed, NumSeats);
        }

        #endregion

        public override DragBaseType Type
        {
            get { return DragBaseType.Holder; }
        }

        private List<DragObject> m_Objects = new List<DragObject>();

        /// <summary>
        /// Returns if an object is allowed to be added.
        /// </summary>
        public bool AllowAddObject(DragObject inDragObject)
        {
            Assert.True(inDragObject != null, "DragObject is not null.", "Cannot validate null DragObject.");

            return (m_Objects.Count < MaxAllowed
                && (inDragObject.Categories & AllowedCategoriesAdd) > 0
                && AllowTouch);
        }

        /// <summary>
        /// Returns if an object is allowed to be swapped.
        /// </summary>
        public bool AllowSwapObject(DragObject inDragObject, out DragObject outSwappedObject)
        {
            Assert.True(inDragObject != null, "DragObject is not null.", "Cannot validate null DragObject.");

            if (!AllowSwapping || (inDragObject.Categories & AllowedCategoriesAdd) == 0 || !AllowTouch)
            {
                outSwappedObject = null;
                return false;
            }
            
            for(int i = Count - 1; i >= 0; --i)
            {
                DragObject dragObject = this[i];
                
                // HACK: to allow swapping when we're at the minimum
                --MinAllowed;
                bool bAllowedToRemove = AllowRemoveObject(dragObject);
                ++MinAllowed;

                if (bAllowedToRemove)
                {
                    outSwappedObject = dragObject;
                    return true;
                }
            }

            outSwappedObject = null;
            return false;
        }

        /// <summary>
        /// Returns if an object is allowed to be removed.
        /// </summary>
        public bool AllowRemoveObject(DragObject inDragObject)
        {
            Assert.True(inDragObject != null, "DragObject is not null.", "Cannot validate null DragObject.");

            return (m_Objects.Count > MinAllowed
                && (inDragObject.Categories & AllowedCategoriesRemove) > 0
                && AllowTouch);
        }

        /// <summary>
        /// Adds an object to the zone.
        /// </summary>
        public bool AddObject(DragObject inDragObject, bool inbForce = false)
        {
            if (!inbForce)
            {
                if (!AllowAddObject(inDragObject))
                    return false;
            }
            else
            {
                Assert.True(m_Objects.Count < MaxAllowed, "Valid to add this object.");
            }

            Assert.True(inDragObject != null, "DragObject is not null.", "Cannot add null DragObject to a DragZone.");
            Assert.True(!m_Objects.Contains(inDragObject), "DragObject has not been added.", "Cannot add a DragObject twice.");

            m_Objects.Add(inDragObject);
            int objectIndex = m_Objects.Count - 1;
            if (AutoSort && SortMethod != null)
            {
                m_Objects.Sort(SortMethod);
                for(int i = 0; i < m_Objects.Count; ++i)
                {
                    DragObject obj = m_Objects[i];
                    if (obj == inDragObject)
                    {
                        objectIndex = i;
                    }
                    else
                    {
                        obj.SetIndex(i, true);
                    }
                }
            }
            inDragObject.SetIndex(objectIndex, false);

            if (OnObjectAdded != null)
                OnObjectAdded(this, inDragObject);

            return true;
        }

        /// <summary>
        /// Removes an object from the zone.
        /// </summary>
        public bool RemoveObject(DragObject inDragObject, bool inbForce = false)
        {
            if (!inbForce)
            {
                if (!AllowRemoveObject(inDragObject))
                    return false;
            }
            else
            {
                Assert.True(m_Objects.Count > MinAllowed, "Valid to remove this object.");
            }

            Assert.True(inDragObject != null, "DragObject is not null.", "Cannot remove null DragObject from a DropZone.");
            Assert.True(m_Objects.Contains(inDragObject), "DragObject has not been removed.", "Cannot remove a DragObject twice.");
            
            m_Objects.Remove(inDragObject);
            inDragObject.SetIndex(-1);

            for (int i = 0; i < m_Objects.Count; ++i)
            {
                DragObject obj = m_Objects[i];
                obj.SetIndex(i, true);
            }

            if (OnObjectRemoved != null)
                OnObjectRemoved(this, inDragObject);

            return true;
        }

        /// <summary>
        /// Destroys all objects currently within the holder.
        /// </summary>
        public void DestroyObjects()
        {
            while(m_Objects.Count > 0)
            {
                DragObject obj = m_Objects[0];
                Destroy(obj.gameObject);
                m_Objects.RemoveAt(0);
            }
        }

        /// <summary>
        /// Removes an object at the given index from the zone.
        /// </summary>
        public DragObject RemoveObjectAt(int inIndex)
        {
            Assert.True(inIndex >= 0 && inIndex < m_Objects.Count, "Index is valid.", "Cannot remove DragObject at {0} - only {1} exist.", inIndex, m_Objects.Count);

            DragObject objectAt = m_Objects[inIndex];
            m_Objects.RemoveAt(inIndex);
            objectAt.SetIndex(-1);

            for (int i = 0; i < m_Objects.Count; ++i)
            {
                DragObject obj = m_Objects[i];
                obj.SetIndex(i, true);
            }

            if (OnObjectRemoved != null)
                OnObjectRemoved(this, objectAt);

            return objectAt;
        }

        /// <summary>
        /// Returns the number of objects assigned to the zone.
        /// </summary>
        public int Count { get { return m_Objects.Count; } }

        /// <summary>
        /// Returns the DragObject at the given index.
        /// </summary>
        public DragObject this[int inIndex]
        {
            get
            {
                Assert.True(inIndex >= 0 && inIndex < m_Objects.Count, "Index is valid.", "Cannot find DragObject at {0} - only {1} exist.", inIndex, m_Objects.Count);
                return m_Objects[inIndex];
            }
        }

        #region Seating

        /// <summary>
        /// What kind of arrangement should be used for this DragHolder.
        /// </summary>
        public DragHolderArrangementType ArrangementType
        {
            get { return NumSeats > 0 ? DragHolderArrangementType.Seats : DragHolderArrangementType.FreeRoam; }
        }

        public bool HasSeats
        {
            get { return ArrangementType == DragHolderArrangementType.Seats; }
        }

        /// <summary>
        /// Number of seats available for this DragHolder.
        /// </summary>
        public int NumSeats
        {
            get { return Arrangement != null ? Arrangement.Seats.Length : 0; }
        }

        /// <summary>
        /// Returns the seat for the given index.
        /// </summary>
        public Transform GetSeat(int inIndex)
        {
            Assert.True(NumSeats > 0 || (inIndex >= 0 && inIndex < NumSeats), "Index is valid.", "Cannot find seat at {0} - only {1} exist.", inIndex, NumSeats);
            return NumSeats > 0 ? Arrangement.Seats[inIndex] : null;
        }

        /// <summary>
        /// Returns the seat for the given DragObject.
        /// </summary>
        public Transform GetSeat(DragObject inDragObject)
        {
            Assert.True(inDragObject != null, "DragObject is not null.", "Cannot find seat for null DragObject.");
            Assert.True(inDragObject.Owner == this, "DragObject has this owner.", "DragObject is not assigned to this DropZone.");
            return GetSeat(inDragObject.Index);
        }

        #endregion
    }
}
