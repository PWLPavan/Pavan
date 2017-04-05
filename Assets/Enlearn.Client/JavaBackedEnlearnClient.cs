using System;
using System.Collections;
using System.Threading;
using Assets.Enlearn.Client;
using UnityEngine;

namespace Enlearn.Client
{
    public class JavaBackedEnlearnClient : IUnityEnlearnClient
    {
#if UNITY_ANDROID
        private readonly MonoBehaviour _behaviour;
        private readonly AndroidJavaObject _unityCallable;
        private readonly JniTask _jniTask;
        private readonly AndroidJniCallable _androindJniCallable;

        public JavaBackedEnlearnClient(MonoBehaviour behaviour, string gameId, UnityLogger logger, Action serviceStartedCallback)
        {
            _behaviour = behaviour;
            var unityPlayerJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityPlayerActivity = unityPlayerJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

            //Send request to start service
            _unityCallable = new AndroidJavaObject("org.enlearn.enlearnServiceClient.UnityCallable", gameId, unityPlayerActivity);
            _androindJniCallable = new AndroidJniCallable(_unityCallable);
            _jniTask = new JniTask(_androindJniCallable, 5000);
            _unityCallable.Call("StartService", new EnlearnServiceStartedListener(logger,serviceStartedCallback));
            //dispatch event back to client builder when service is started
        }

        public void GetNextProblem(Guid studentId, Action<string> callback)
        {
            _behaviour.StartCoroutine(MakeJniCall(callback, "GetNextProblem", studentId.ToString()));
        }

        private IEnumerator MakeJniCall(Action<string> callback, string commandName, params string[] arguments)
        {
            if (!_jniTask.SafeToCall())
            {
                callback("{\"error\": \"call to service already in progress\" }");
            }
            else
            {
                _jniTask.MakeCall(commandName, arguments);

                while (_jniTask.IsInProgress())
                    yield return null;

                _jniTask.CallCallbackWithResult(callback);
            }
        }

        public void LogStudentActions(Guid studentId, string studentActionString, Action<string> callback)
        {
            _behaviour.StartCoroutine(MakeJniCall(callback, "LogStudentActions", studentId.ToString(), studentActionString));

           // return _unityCallable.Call<string>("LogStudentActions", studentId.ToString(), studentActionString);
        }

        public void UpdateStudentInfo(Guid studentId, string studentInfo)
        {
            _behaviour.StartCoroutine(MakeJniCall(s => { }, "UpdateStudentInfo", studentId.ToString(), studentInfo));
        }


        public class EnlearnServiceStartedListener : AndroidJavaProxy
        {
            private readonly UnityLogger _logger;
            private readonly Action _serviceStartedCallback;
            public EnlearnServiceStartedListener(UnityLogger logger, Action serviceStartedCallback)
                : base("org.enlearn.enlearnServiceClient.UnityCallable$ServiceStartedListener")
            {
                _logger = logger;
                _serviceStartedCallback = serviceStartedCallback;
            }

            void OnServiceStart()
            {
                _logger.LogInfo("Enlearn Service Is Ready");
                _serviceStartedCallback();
            }
        }
#else
        public void GetNextProblem(Guid studentId, Action<string> callback)
        {
 	        throw new NotImplementedException();
        }

        public void LogStudentActions(Guid studentId, string studentActionString, Action<string> callback)
        {
 	        throw new NotImplementedException();
        }

        public void UpdateStudentInfo(Guid studentId, string studentInfo)
        {
 	        throw new NotImplementedException();
        }
#endif
    }
}