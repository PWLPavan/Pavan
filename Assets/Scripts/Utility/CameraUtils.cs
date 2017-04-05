using UnityEngine;
using System.Collections;

public static class CameraUtils
{
    static CameraUtils()
    {
        MessageHook.instance.OnUpdate += ResetRequest;
    }

    static private void ResetRequest()
    {
        sbRequestedCamera = false;
    }

    // We only need to calculate the camera rectangle once per frame
    static bool sbRequestedCamera = false;
    static Rect sCameraRect;

    public static Rect cameraRect
    {
        get
        {
            if (!sbRequestedCamera)
            {
                sCameraRect = GetCameraRect();
                sbRequestedCamera = true;
            }
            return sCameraRect;
        }
    }

    private static Rect GetCameraRect ()
    {
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight));
        
        return new Rect(bottomLeft.x,
                        bottomLeft.y,
                        topRight.x - bottomLeft.x,
                        topRight.y - bottomLeft.y);
    }
}
