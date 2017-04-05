using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Enlearn.Client
{
    public class AndroidJniCallable : IJniCallable
    {
#if UNITY_ANDROID
        private readonly AndroidJavaObject _unityCallable;

        public AndroidJniCallable(AndroidJavaObject unityCallable)
        {
            _unityCallable = unityCallable;
        }

        public string Call(string commandName, params string[] args)
        {
            try
            {
                AndroidJNI.AttachCurrentThread();
                return _unityCallable.Call<string>(commandName, args);
            }
            finally
            {
                AndroidJNI.DetachCurrentThread();
            }
        }
#else
        public string Call(string commandName, params string[] args)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
