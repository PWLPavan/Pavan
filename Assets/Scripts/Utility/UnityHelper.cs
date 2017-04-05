using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FGUnity.Utils;

using UnityEngine;

static public class UnityHelper
{
    static public string GetNameInHierarchy(this Transform inTransform)
    {
        using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
        {
            StringBuilder builder = stringBuilder.Builder;
            Transform current = inTransform;
            while ((current = current.parent) != null)
            {
                builder.Insert(0, '/');
                builder.Insert(0, current.gameObject.name);
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// Safely disposes of a Unity object and sets the reference to null.
    /// </summary>
    static public void SafeDestroy(ref UnityEngine.Object inObject)
    {
        // This is to avoid calling Unity's overridden equality operator
        if (object.ReferenceEquals(inObject, null))
            return;

        // This is to see if the object hasn't been destroyed yet
        if (inObject)
        {
            UnityEngine.Object.Destroy(inObject);
        }

        inObject = null;
    }

    /// <summary>
    /// Safely disposes of the GameObject and sets
    /// the reference to null.
    /// </summary>
    static public void SafeDestroy(ref UnityEngine.GameObject inGameObject)
    {
        // This is to avoid calling Unity's overridden equality operator
        if (object.ReferenceEquals(inGameObject, null))
            return;

        // This is to see if the object hasn't been destroyed yet
        if (inGameObject)
        {
            UnityEngine.Object.Destroy(inGameObject);
        }

        inGameObject = null;
    }

    /// <summary>
    /// Safely disposes of the parent GameObject of the transform and sets
    /// the reference to null.
    /// </summary>
    static public void SafeDestroy(ref UnityEngine.Transform inTransform)
    {
        // This is to avoid calling Unity's overridden equality operator
        if (object.ReferenceEquals(inTransform, null))
            return;

        // This is to see if the object hasn't been destroyed yet
        if (inTransform && inTransform.gameObject)
        {
            UnityEngine.Object.Destroy(inTransform.gameObject);
        }

        inTransform = null;
    }

    /// <summary>
    /// Returns if the given object is null in a C# context.
    /// Avoids calling Unity's overridden equality operator.
    /// </summary>
    static public bool IsNull(this UnityEngine.Object inObject)
    {
        return System.Object.ReferenceEquals(inObject, null);
    }

    /// <summary>
    /// Performs a HitTest on a RectTransform.
    /// Determines if the given screen position is within the region.
    /// Does not take into account overlap, visibility, etc.
    /// Taken from: http://flassari.is/2015/03/unity-is-mouse-or-any-coordinates-within-ui-elements-rect/
    /// </summary>
    static public bool HitTest(this RectTransform inRectTransform, Vector2 inScreenPosition)
    {
        Vector2 localPos = inRectTransform.InverseTransformPoint(inScreenPosition);
        return inRectTransform.rect.Contains(localPos);
    }

    static public bool HitTest(this Transform inTransform, Vector2 inScreenPosition)
    {
        return HitTest((RectTransform)inTransform, inScreenPosition);
    }
}
