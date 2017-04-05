using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FGUnity.Utils;

static public class AndroidToast
{
#if UNITY_ANDROID
    
    static public void Show(string inMessage, TOAST_DURATION inDuration = TOAST_DURATION.LENGTH_SHORT)
    {
        ToastWrapper wrapper = new ToastWrapper(inMessage, inDuration);
        AndroidHelper.RunOnUIThread(wrapper.ShowAndDispose);
    }

    public enum TOAST_DURATION
    {
        LENGTH_SHORT = 0x000,
        LENGTH_LONG = 0x001
    }

    private class ToastWrapper
    {
        public ToastWrapper(string inMessage, TOAST_DURATION inDuration)
        {
            m_Message = inMessage;
            m_Duration = inDuration;
        }

        private string m_Message;
        private TOAST_DURATION m_Duration;

        public void ShowAndDispose()
        {
            AndroidJavaObject context = AndroidHelper.GetCurrentActivity().Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject toastClass = new AndroidJavaClass("android.widget.Toast");

            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", context, m_Message, (int)m_Duration);
            toastObject.Call("show");

            toastClass.Dispose();
            toastObject.Dispose();
            context.Dispose();
        }
    }
#endif
}