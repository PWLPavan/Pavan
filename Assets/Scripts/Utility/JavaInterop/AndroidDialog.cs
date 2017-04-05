using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FGUnity.Utils;

static public class AndroidDialog
{
#if UNITY_ANDROID
    public class AlertDialogBuilder : JavaWrapper
    {
        public AlertDialogBuilder()
            : base(new AndroidJavaObject("android.app.AlertDialog$Builder", AndroidHelper.GetCurrentActivity()))
        {
            SetCancelable(false);
        }

        public AlertDialogBuilder SetTitle(string inTitle)
        {
            m_InternalObject.Call<AndroidJavaObject>("setTitle", inTitle);
            return this;
        }

        public AlertDialogBuilder SetMessage(string inMessage)
        {
            m_InternalObject.Call<AndroidJavaObject>("setMessage", inMessage);
            return this;
        }

        public AlertDialogBuilder SetCancelable(bool inbCancelable)
        {
            m_InternalObject.Call<AndroidJavaObject>("setCancelable", inbCancelable);
            return this;
        }

        public AlertDialogBuilder SetPositiveButton(string inMessage, Action<int> inResponse)
        {
            m_InternalObject.Call<AndroidJavaObject>("setPositiveButton", inMessage, new OnClickListener(inResponse));
            return this;
        }

        public AlertDialogBuilder SetNegativeButton(string inMessage, Action<int> inResponse)
        {
            m_InternalObject.Call<AndroidJavaObject>("setNegativeButton", inMessage, new OnClickListener(inResponse));
            return this;
        }

        public AlertDialogBuilder SetNeutralButton(string inMessage, Action<int> inResponse)
        {
            m_InternalObject.Call<AndroidJavaObject>("setNeutralButton", inMessage, new OnClickListener(inResponse));
            return this;
        }

        public AlertDialogBuilder SetItems(string[] inItems, Action<int> inResponse)
        {
            m_InternalObject.Call<AndroidJavaObject>("setItems", inItems, new OnClickListener(inResponse));
            return this;
        }

        public void ShowAndDispose()
        {
            AndroidHelper.RunOnUIThread(Show_Inner);
        }

        private void Show_Inner()
        {
            var dialog = m_InternalObject.Call<AndroidJavaObject>("create");
            dialog.Call("show");
            dialog.Dispose();
            Dispose();
        }
    }

    private class OnClickListener : AndroidJavaProxy
    {
        public OnClickListener(Action<int> inAction)
            : base("android.content.DialogInterface$OnClickListener")
        {
            Action = inAction;
        }

        public Action<int> Action;

        public void onClick(AndroidJavaObject dialog, int which)
        {
            if (Action != null)
                Action(which);
        }
    }
#endif
}