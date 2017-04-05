using UnityEngine;
using System.Collections.Generic;
using System;

namespace Minigames
{
    /// <summary>
    /// Categories used to validate what objects can
    /// be dropped in certain places.
    /// </summary>
    [Flags]
    public enum DragObjectCategory
    {
        Plain = 0x1,
        Operator = 0x2,
        Number = 0x4
    }

    /// <summary>
    /// Indicates how a drag ended.
    /// </summary>
    public enum DragEndState
    {
        /// <summary>
        /// The drag was cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The drag ended on an empty zone.
        /// </summary>
        Empty,

        /// <summary>
        /// The drag ended on an invalid drop zone.
        /// </summary>
        InvalidZone,

        /// <summary>
        /// The drag ended on a valid drop zone.
        /// </summary>
        ValidZone
    }

    /// <summary>
    /// Indicates how DragObjects are arranged within a DragHolder
    /// </summary>
    public enum DragHolderArrangementType
    {
        /// <summary>
        /// Objects only need to stay within the bounds
        /// </summary>
        FreeRoam,

        /// <summary>
        /// Objects must return to their assigned seats
        /// </summary>
        Seats
    }

    /// <summary>
    /// Indicates type of object.
    /// </summary>
    public enum DragBaseType
    {
        Object,
        Holder
    }
}
