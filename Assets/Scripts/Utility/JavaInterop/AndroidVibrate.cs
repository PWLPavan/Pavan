using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FGUnity.Utils;

static public class AndroidVibrate
{   
    static public void Vibrate(float inSeconds)
    {
#if UNITY_ANDROID
        AndroidJavaClass context = new AndroidJavaClass("android.content.Context");
        string serviceKey = context.GetStatic<string>("VIBRATOR_SERVICE");
        context.Dispose();

        AndroidJavaObject service = AndroidHelper.GetCurrentActivity().Call<AndroidJavaObject>("getSystemService", serviceKey);
        bool bHasVibrator = service.Call<bool>("hasVibrator");
        if (bHasVibrator)
        {
            service.Call("vibrate", (long)(inSeconds * 1000));
        }
        service.Dispose();
#endif
    }
}