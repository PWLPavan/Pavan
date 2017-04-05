using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Accepts raycasts but does not render.
/// Used to create invisible click zones. This avoids
/// the costly hack of setting a Graphic's alpha to 0.
/// </summary>
public class RaycastZone : Graphic, ICanvasRaycastFilter
{
    public bool Accepting = true;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return Accepting && rectTransform.HitTest(sp);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    { }

    protected override void UpdateGeometry()
    { }

    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        return enabled && IsRaycastLocationValid(sp, eventCamera);
    }
}
