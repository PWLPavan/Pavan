using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FGUnity.Utils;

static public class AndroidHelper
{
#if UNITY_ANDROID
    static private AndroidJavaObject s_CurrentActivity = null;

    static public AndroidJavaObject GetCurrentActivity()
    {
#if !UNITY_EDITOR
        if (s_CurrentActivity == null)
        {
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            s_CurrentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
        return s_CurrentActivity;
    }
#endif

    static public bool LaunchActivity(string inActivityName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaObject currentActivity = AndroidHelper.GetCurrentActivity();
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", inActivityName);
            currentActivity.Call("startActivity", launchIntent);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogFormat("Unable to open '{0}': {1}", inActivityName, e.Message);
            return false;
        }
#else
        return false;
#endif
    }

    static public void RunOnUIThread(Action inAction)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var activity = GetCurrentActivity();
        activity.Call("runOnUiThread", new AndroidJavaRunnable(inAction));
#endif
    }

    static public void RestartActivity()
    {
        KeepAlive.DestroyEverything();
        Application.LoadLevel(0);
    }

    static public void KillActivity()
    {
        Application.Quit();
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaObject currentActivity = AndroidHelper.GetCurrentActivity();
            currentActivity.Call("finish");
        }
        catch (System.Exception e)
        {
            Debug.LogFormat("Unable to kill application: {0}", e.Message);
        }
#endif
    }

#if UNITY_ANDROID

    static public Int32 ToInt32(AndroidJavaObject inObject)
    {
        return (int)ToSingle(inObject);
    }

    static public Single ToSingle(AndroidJavaObject inObject)
    {
        string objectAsString = inObject.Call<string>("toString");
        return float.Parse(objectAsString);
    }

#endif

    static public bool IsInstalled(string inPackageName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject activity = GetCurrentActivity();
        using (AndroidJavaObject packageMgr = activity.Call<AndroidJavaObject>("getPackageManager"))
        {
            try
            {
                AndroidJavaObject packageInfo = packageMgr.Call<AndroidJavaObject>("getPackageInfo", inPackageName,
                    packageMgr.GetStatic<int>("GET_SERVICES") | packageMgr.GetStatic<int>("GET_ACTIVITIES"));
                if (packageInfo == null)
                    return false;

                Ref.Dispose(ref packageInfo);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Unable to get package: {0}", e.Message);
                return false;
            }
        }
#else
        return false;
#endif
    }

    static public string GetIntentExtra(string inKey)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject activity = GetCurrentActivity();
        AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
        if (intent.Call<bool>("hasExtra", inKey))
        {
            return intent.Call<string>("getStringExtra", inKey);
        }
        else
        {
            return string.Empty;
        }
#else
        return string.Empty;
#endif
    }
}