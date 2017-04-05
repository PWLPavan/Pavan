using System.Text;
using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using FGUnity.Utils;
using Ekstep;

namespace Ekstep
{
    public partial class Genie : LazySingletonBehavior<Genie>
    {
#if UNITY_ANDROID
        /// <summary>
        /// Wrapper around the response they give us.
        /// </summary>
        private class GenieResponseWrapper : JavaWrapper
        {
            public GenieResponseWrapper(AndroidJavaObject inObject)
                : base(inObject)
            {
            }

            public string getStatus()
            {
                return m_InternalObject.Call<string>("getStatus");
            }

            public string getError()
            {
                return m_InternalObject.Call<string>("getError");
            }

            public string getStringResult(string inKey, string inDefault = null)
            {
                return hasResult(inKey) ? m_InternalObject.Call<AndroidJavaObject>("getResult").Call<string>("get", inKey) : inDefault;
            }

            public int getIntResult(string inKey, int inDefault = 0)
            {
                if (hasResult(inKey))
                {
                    var obj = m_InternalObject.Call<AndroidJavaObject>("getResult").Call<AndroidJavaObject>("get", inKey);
                    return AndroidHelper.ToInt32(obj);
                }
                return inDefault;
            }

            public bool hasResult(string inKey)
            {
                return m_InternalObject.Call<AndroidJavaObject>("getResult").Call<bool>("containsKey", inKey);
            }

            public void WriteToBuilder(StringBuilder inBuilder)
            {
                inBuilder.AppendLine("Genie Response")
                    .AppendFormat("Status: {0}\n", getStatus())
                    .AppendFormat("Error: {0}\n", getError());
            }

            public void Log()
            {
                using (PooledStringBuilder stringBuilder = PooledStringBuilder.Create())
                {
                    this.WriteToBuilder(stringBuilder.Builder);
                    Logger.Log(stringBuilder.Builder.ToString());
                }
            }
        }

        /// <summary>
        /// Processes a response from Genie.
        /// </summary>
        private abstract class ResponseHandler : AndroidJavaProxy
        {
            public ResponseHandler()
                : base("org.ekstep.genieservices.sdks.response.IResponseHandler")
            { }

            public void onSuccess(AndroidJavaObject inObject)
            {
                if (!Genie.Exists)
                    return;

                Genie.instance.QueueAction(ResponseProcessor.Create(this.Process_Event, inObject));
            }
            
            public void onFailure(AndroidJavaObject inObject)
            {
                if (!Genie.Exists)
                    return;

                Genie.instance.QueueAction(ResponseProcessor.Create(this.Process_Event, inObject));
            }

            private void Process_Event(AndroidJavaObject inObject)
            {
                GenieResponseWrapper wrapper = new GenieResponseWrapper(inObject);
                //wrapper.Log();
                Process(wrapper);
            }

            public abstract void Process(GenieResponseWrapper inWrapper);
        }

        private class ResponseProcessor : IDisposable
        {
            public AndroidJavaObject Response;
            public Action<AndroidJavaObject> Action;
            public bool DisposeResponse;

            private ResponseProcessor() { }

            public void Process()
            {
                Action(Response);
            }

            public void Dispose()
            {
                if (Action != null)
                {
                    if (DisposeResponse)
                    {
                        Ref.Dispose(ref Response);
                        DisposeResponse = false;
                    }
                    else
                        Response = null;
                    Action = null;
                    s_Pool.Push(this);
                }
            }

            static private Pool<ResponseProcessor> s_Pool = new Pool<ResponseProcessor>(8, constructor);

            static public ResponseProcessor Create(Action<AndroidJavaObject> inAction, AndroidJavaObject inObject, bool inbDisposeResponse = true)
            {
                ResponseProcessor processor = s_Pool.Pop();
                processor.Action = inAction;
                processor.Response = inObject;
                processor.DisposeResponse = inbDisposeResponse;
                return processor;
            }

            static private ResponseProcessor constructor(Pool<ResponseProcessor> inPool)
            {
                return new ResponseProcessor();
            }
        }
#endif
    }
}