using System;
using System.Threading;
using Assets.Enlearn.Client;
using UnityEngine;

namespace Enlearn.Client
{
    public class JniTask
    {
        private readonly IJniCallable _jniCallable;
        private readonly float _timeoutMilliseconds;
        private bool _isDone;
        private string _result;
        private bool _timedOut;
        private Timer _timer;

        public JniTask(IJniCallable jniCallable, int timeoutMilliseconds)
        {
            _jniCallable = jniCallable;
            _timeoutMilliseconds = timeoutMilliseconds;
            _isDone = true;
        }

        public bool SafeToCall()
        {
            return _isDone;
        }

        public bool IsInProgress()
        {
            return !_isDone && !_timedOut;
        }

        public void MakeCall(string commandName, string[] arguments)
        {
            _result = "";
            _isDone = false;
            var threadStart = new ThreadStart(() =>
            {
                try
                {
                    _result = _jniCallable.Call(commandName, arguments);
                }
                catch (Exception ex)
                {
                    _result = BuildExceptionJson(ex);
                }
                finally
                {
                    _isDone = true;
                }
            });
            var t = new Thread(threadStart);
            t.Start();
            _timedOut = false;
            if (_timer != null)
                _timer.Dispose();
            _timer = new Timer(ThreadTimedOut, null, TimeSpan.FromMilliseconds(_timeoutMilliseconds), TimeSpan.FromMilliseconds(-1));

        }

        private void ThreadTimedOut(object state)
        {
            if (!_isDone)
            {
                _isDone = true;
                _timedOut = true;
            }
            _timer.Dispose();
            _timer = null;
        }

        public void CallCallbackWithResult(Action<string> callback)
        {
            if (_timer != null)
                _timer.Dispose();
            _timer = null;

            if (_timedOut)
                callback(BuildTimeoutErrorJson());
            else if (_isDone)
                callback(_result);
            else
                callback(BuildInProgressErrorJson());
        }

        private string BuildInProgressErrorJson()
        {
            return "{\"error\":\"inprogress\"}";
        }

        private static string BuildExceptionJson(Exception ex)
        {
            return "{\"error\":\"jni\", \"exception\":\"" + ex + "\" }";
        }

        private static string BuildTimeoutErrorJson()
        {
            return "{\"error\":\"timeout\"}";
        }
    }
}